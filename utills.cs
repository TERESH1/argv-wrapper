using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ArgvWrapper {
   public static class WrapperUtills {
      public static int StartProc(bool Verbose, string ExeName, List<string> Argv, bool redirect = true) {
         string args = String.Join(" ", Argv);
         int result = 0;
         if (Verbose)
            Console.WriteLine("Process arguments: {0}", args);
         using (Process proc = new Process()) {
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = ExeName;
            if (redirect) {
               proc.StartInfo.RedirectStandardError = true;
               proc.StartInfo.RedirectStandardOutput = true;
               proc.StartInfo.RedirectStandardInput = true;
            }
            proc.Start();
            
            if (redirect) {
               StreamPipe pout = new StreamPipe(proc.StandardOutput.BaseStream, Console.OpenStandardOutput());
               StreamPipe perr = new StreamPipe(proc.StandardError.BaseStream, Console.OpenStandardError());
               StreamPipe pin = new StreamPipe(Console.OpenStandardInput(), proc.StandardInput.BaseStream);

               pin.Connect();
               pout.Connect();
               perr.Connect();
            }
            proc.WaitForExit();
            result = proc.ExitCode;
         }
         if (Verbose)
            Console.WriteLine("Return code: {0}", result);
         return result;
      }

      public static List<string> CommandLineToArgvOriginal(string cmdline) {
         int argc = 0;
         IntPtr argv = CommandLineToArgvWMod(cmdline, out argc);
         //Console.WriteLine("ARGC = {0}", argc);
         //Console.WriteLine("ARGV[0] = {0}", Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, 0)));
         List<string> result = new List<string>();
         for (int i = 1; i < argc; i++) {
            //Console.WriteLine("ARGV[{0}] = {1}", i, Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, 0)));
            result.Add(Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size)));
         }
         return result;
      }
      
      [DllImport("CommandLineToArgvWMod.dll")]
      static extern IntPtr CommandLineToArgvWMod([MarshalAs(UnmanagedType.LPWStr)] string cmdline, out int numargs);
      
      // From https://gist.github.com/antopor/5515bed636c3d99395ea
      private class StreamPipe {
         private const Int32 BufferSize = 4096;

         public Stream Source { get; protected set; }
         public Stream Destination { get; protected set; }

         private CancellationTokenSource _cancellationToken;
         private Task _worker;

         public StreamPipe(Stream source, Stream destination) {
            Source = source;
            Destination = destination;
         }

         public StreamPipe Connect() {
            _cancellationToken = new CancellationTokenSource();
            _worker = Task.Run(() => {
               byte[] buffer = new byte[BufferSize];
               while (true) {
                  _cancellationToken.Token.ThrowIfCancellationRequested();
                  var count = Source.Read(buffer, 0, BufferSize);
                  if (count <= 0)
                     break;
                  if (count == BufferSize)
                     Environment.SetEnvironmentVariable("MSVCWrapperOverflow","1");
                  if (count == BufferSize - 1)
                     Environment.SetEnvironmentVariable("MSVCWrapperOverflow","2");
                  Destination.Write(buffer, 0, count);
                  Destination.Flush();
               }
            }, _cancellationToken.Token);
            return this;
         }

         public void Disconnect() {
            _cancellationToken.Cancel();
         }
      }
   }
}
