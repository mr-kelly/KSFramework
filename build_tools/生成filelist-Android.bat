@echo off
cd %~dp0
set bak_path=%~dp0
set dst_path=%~dp0..\KSFramework\Product\Bundles\Android\
@echo on
python gen_filelist.py %dst_path% %bak_path% "Android"
REM pause