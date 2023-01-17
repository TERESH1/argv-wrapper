@echo off
FOR /F "tokens=*" %%g IN ('where cl') do (SET cl_path=%%g)
set cl_dir=%cl_path:cl.exe=%
move "%cl_path%" "%cl_dir%cl_orig.exe"
copy cl_wrapper.exe "%cl_path%"
