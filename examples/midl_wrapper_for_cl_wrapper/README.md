# MIDL Compiler wrapper
MIDI uses MSVC cl as a preprocessor and does not work with [cl wrapper](../cl_wrapper_add_preprocessing_when_obj_specified)

It adds ```/cpp_cmd cl_orig.exe``` to command line
