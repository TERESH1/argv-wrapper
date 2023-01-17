using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArgvWrapper {
   public static class WrapperUtills {
      static void STDOutputHandler(object proc, DataReceivedEventArgs line) {
         //if (!String.IsNullOrEmpty(line.Data))
            Console.WriteLine(line.Data);
      }

      static void STDErrorHandler(object proc, DataReceivedEventArgs line) {
         if (!String.IsNullOrEmpty(line.Data))
            Console.Error.WriteLine(line.Data);
      }

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
               proc.OutputDataReceived += new DataReceivedEventHandler(STDOutputHandler);
               proc.ErrorDataReceived += new DataReceivedEventHandler(STDErrorHandler);
            }
            proc.Start();
            if (redirect) {
               proc.BeginOutputReadLine();
               proc.BeginErrorReadLine();
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
   }
}