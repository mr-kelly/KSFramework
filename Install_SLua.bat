@echo off
:: Submodule update
::git submodule update --recursive

set SRC_ASSETS_PATH=KEngine\KEngine.UnityProject\Assets
set DST_ASSETS_PATH=KSFramework\Assets
set DST_PLUGIN_PATH=KSFramework\Assets\Plugins\KEngine
set DST_EDITOR_PATH=KSFramework\Assets\Plugins\Editor\KEngineEditor


:: Resources
::xcopy %SRC_ASSETS_PATH%\Resources\KEngineConfig.txt %DST_ASSETS_PATH%\Resources\KEngineConfig.txt /S/Y/R/I

mkdir %DST_ASSETS_PATH%\..\Product\SettingSource
mkdir %DST_ASSETS_PATH%\..\Product\Bundles

:: Codes
::xcopy %SRC_ASSETS_PATH%\KEngine %DST_PLUGIN_PATH%\KEngine /S/Y/R/I
::xcopy %SRC_ASSETS_PATH%\KEngine.Lib %DST_PLUGIN_PATH%\KEngine.Lib /S/Y/R/I
::xcopy %SRC_ASSETS_PATH%\KEngine.Editor\Editor %DST_EDITOR_PATH%\KEngine.Editor /S/Y/R/I
::xcopy %SRC_ASSETS_PATH%\KEngine.EditorTools\Editor %DST_EDITOR_PATH%\KEngine.EditorTools /S/Y/R/I

echo %SRC_ASSETS_PATH%
echo %DST_PLUGIN_PATH%

mkdir %DST_PLUGIN_PATH% 2>nul
rmdir %DST_PLUGIN_PATH%\KEngine
mklink /J %DST_PLUGIN_PATH%\KEngine %SRC_ASSETS_PATH%\KEngine
rmdir %DST_PLUGIN_PATH%\KEngine.Lib
mklink /J %DST_PLUGIN_PATH%\KEngine.Lib %SRC_ASSETS_PATH%\KEngine.Lib
rmdir %DST_PLUGIN_PATH%\KEngine.Lib.UI
mklink /J %DST_PLUGIN_PATH%\KEngine.UI %SRC_ASSETS_PATH%\KEngine.UI

mkdir %DST_EDITOR_PATH% 2>nul
rmdir %DST_EDITOR_PATH%\KEngine.Editor
mklink /J %DST_EDITOR_PATH%\KEngine.Editor %SRC_ASSETS_PATH%\KEngine.Editor\Editor
rmdir %DST_EDITOR_PATH%\KEngine.EditorTools
mklink /J %DST_EDITOR_PATH%\KEngine.EditorTools %SRC_ASSETS_PATH%\KEngine.EditorTools\Editor
rmdir %DST_EDITOR_PATH%\KEngine.UI.Editor
mklink /J %DST_EDITOR_PATH%\KEngine.UI.Editor %SRC_ASSETS_PATH%\KEngine.UI.Editor\Editor

mkdir KSFramework\Assets\Plugins\libs\ 2>nul
xcopy %SRC_ASSETS_PATH%\Plugins\Android\libs\KEngine.Android.jar KSFramework\Assets\Plugins\Android\libs\KEngine.Android.jar /S/Y/R/I

:: SLua
mkdir KSFramework\Assets\SLua
mkdir KSFramework\Assets\SLua\Resources
mkdir KSFramework\Assets\SLua\Editor
xcopy slua\Assets\SLua\Editor\CustomEditor.cs KSFramework\Assets\SLua\Editor /S/Y/R/I
xcopy slua\Assets\SLua\Editor\LuaCodeGen.cs KSFramework\Assets\SLua\Editor /S/Y/R/I

rmdir KSFramework\Assets\Plugins\Slua_Managed
mklink /J KSFramework\Assets\Plugins\Slua_Managed slua\Assets\Plugins\Slua_Managed

xcopy slua\Assets\Plugins\Android\* KSFramework\Assets\Plugins\Android /S/Y/R/I
xcopy slua\Assets\Plugins\iOS\* KSFramework\Assets\Plugins\iOS /S/Y/R/I
xcopy slua\Assets\Plugins\slua.bundle\* KSFramework\Assets\Plugins\slua.bundle /S/Y/R/I
xcopy slua\Assets\Plugins\x64\* KSFramework\Assets\Plugins\x64 /S/Y/R/I
xcopy slua\Assets\Plugins\x86\* KSFramework\Assets\Plugins\x86 /S/Y/R/I

echo Finish!
ping -n 5 127.0.0.1>nul 

