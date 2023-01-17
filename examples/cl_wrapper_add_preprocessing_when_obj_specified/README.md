# MSVC cl wrapper
Since the compiler does not know how to save intermediate files during compilation, the wrapper before generating .obj file with /Fo argument saves source after preprocessing

It may be useful for working with large projects

It was used for static analysis of dotnet/runtime
