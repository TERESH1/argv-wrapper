using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArgvWrapper {
   public static class MIDLWrapper {
      const string CompilerName = "midl_orig.exe";
      static bool Verbose = false;

      static int Main(string[] args) {
         if (Environment.GetEnvironmentVariable("MIDLWrapperVerbose") == "1")
            Verbose = true;
         if (Verbose)
            Console.WriteLine(Environment.CommandLine);
         List<string> Argv = WrapperUtills.CommandLineToArgvOriginal(Environment.CommandLine);
         Argv.Insert(0, "/cpp_cmd");
         Argv.Insert(1, "cl_orig.exe");
         return WrapperUtills.StartProc(Verbose, CompilerName, Argv);
      }
   }
}
