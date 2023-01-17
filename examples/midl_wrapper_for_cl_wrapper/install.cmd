@echo off
FOR /F "tokens=*" %%g IN ('where midl') do (SET midl_path=%%g)
set midl_dir=%midl_path:midl.exe=%
move "%midl_path%" "%midl_dir%midl_orig.exe"
copy midl_wrapper.exe "%midl_path%"
