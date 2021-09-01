@echo off
cd %~dp0
set bak_path=%~dp0
set dst_path=%~dp0..\KSFramework\Product\Bundles\Windows\
@echo on
python gen_filelist.py %dst_path% %bak_path% "Windows"
REM pause