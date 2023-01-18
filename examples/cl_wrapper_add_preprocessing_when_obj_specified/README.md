# MSVC cl wrapper
Since the compiler does not know how to save intermediate files during compilation, the wrapper before generating .obj file with /Fo argument saves source after preprocessing

It may be useful for working with large projects

It was used for static analysis of dotnet/runtime

When build dotnet/runtime clr, it can sometimes cause errors during preprocessing for midl. This is solved by restarting the build or adding a [wrapper for midl](../midl_wrapper_for_cl_wrapper)