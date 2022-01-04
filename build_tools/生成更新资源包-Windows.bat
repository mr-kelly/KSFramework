@echo off
cd %~dp0
set start_path=%~dp0
@echo on
python gen_hotfix_res.py %start_path% "Windows"
REM pause