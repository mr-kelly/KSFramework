@echo off
::Des： 1.调用msbuild生成dll
::		2.把开发目录下的dll拷贝到unity的工程目录
::		3.用于dll开发模式(仅限于android上热更dll)
::		4.仅限于windows平台，如果其它平台请使用visual studio开发		
::Time: 2021/1/31 23:05
::Author: qingqing-zhao(569032731@qq.com)


cd %~dp0 

set dll_path=%~dp0\HotFix_Project\Temp\bin\Release\HotFix_Project.dll
set mdb_path=%~dp0\HotFix_Project\Temp\bin\Release\HotFix_Project.mdb

set to_dll_path=%~dp0\KSFramework\Product\Bundles\Windows\HotFix_Project.dll
set to_mdb_path=%~dp0\KSFramework\Product\Bundles\Windows\HotFix_Project.mdb

rem hh用来解决取小时可能出现空格的问题(凌晨1点到早上9点%time:~0,2%都会出现空格)
set h=%time:~0,2% 
set hh=%h: =0%
set log_file=%cd%"\HotFix_Project\build_logs\build_dll_log_%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%%hh%%time:~3,2%.log

set codePath=%~dp0\HotFix_Project\HotFix_Project.csproj

set msbuild_path="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\"
c:
REM set msbuild_path="D:\Program Files\JetBrains\JetBrains Rider 2020.3.4\tools\MSBuild\Current\Bin\"
REM cd d:\
cd %msbuild_path%
@echo on
msbuild /m %codePath% /t:clean;Rebuild /p:Configuration=Release  /fl /flp:logfile=%log_file%;verbosity=diagnostic

copy %dll_path% %to_dll_path% /y


c:
cd "C:\Program Files\Unity\Editor\Data\MonoBleedingEdge\"
set mono_path="C:\Program Files\Unity\Editor\Data\MonoBleedingEdge\bin\mono.exe"
set pdb2mdb_path="C:\Program Files\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.5\pdb2mdb.exe"

%mono_path% %pdb2mdb_path% %dll_path%
copy %mdb_path% %to_mdb_path% /y

pause
