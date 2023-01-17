using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArgvWrapper {
   public static class MSVCWrapper {
      
      const string CompilerName = "cl_orig.exe";
      static bool Verbose = false;

      static int Main(string[] args) {
         if (Environment.GetEnvironmentVariable("MSVCWrapperVerbose") == "1")
            Verbose = true;
         if (Verbose)
            Console.WriteLine(Environment.CommandLine);
         List<string> Argv = WrapperUtills.CommandLineToArgvOriginal(Environment.CommandLine);
         int FoArgIndex = Argv.FindIndex(s => s.StartsWith("/Fo"));
         if (FoArgIndex != -1) {
            List<string> ArgvMod = new List<string>(Argv);
            StringBuilder strb_with_fo = new StringBuilder(ArgvMod[FoArgIndex]);
            strb_with_fo[2] = 'i';
            int ObjExtIndex = ArgvMod[FoArgIndex].LastIndexOf(".obj");
            if (ObjExtIndex == -1) {
               strb_with_fo.Append(".i");
            } else {
               strb_with_fo[ObjExtIndex + 1] = 'i';
               strb_with_fo.Remove(ObjExtIndex + 2, 2);
            }
            ArgvMod[FoArgIndex] = strb_with_fo.ToString();
            ArgvMod.Insert(FoArgIndex, "/P");
            ArgvMod.Remove("/FS");
            ArgvMod.RemoveAll(s => s.StartsWith("/Fd"));
            ArgvMod.Remove("/Zi");
            WrapperUtills.StartProc(Verbose, CompilerName, ArgvMod, false);
         }
         return WrapperUtills.StartProc(Verbose, CompilerName, Argv);
      }
   }
}