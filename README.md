# argv-wrapper
Utills with examples for wrapping arguments for original application

You should replace original app with your wrapper and use new name/path in wrapper

You can use this tamplate:
```cs
namespace ArgvWrapper {
   public static class ExampleWrapper {
      const string AppName = "example_orig.exe";
      static bool Verbose = false;

      static int Main(string[] args) {
         if (Environment.GetEnvironmentVariable("ExampleWrapperVerbose") == "1")
            Verbose = true;
         if (Verbose)
            Console.WriteLine(Environment.CommandLine);
         List<string> Argv = WrapperUtills.CommandLineToArgvOriginal(Environment.CommandLine);
         //
         // Do something with Argv list
         //
         return WrapperUtills.StartProc(Verbose, AppName, Argv);
      }
   }
}
```
