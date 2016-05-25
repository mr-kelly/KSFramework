@echo off
:: Submodule update
::git submodule update --recursive

set SRC_ASSETS_PATH=KEngine\KEngine.UnityProject\Assets
set DST_ASSETS_PATH=KSFramework\Assets
set DST_PLUGIN_PATH=KSFramework\Assets\Plugins\KEngine
set DST_EDITOR_PATH=KSFramework\Assets\Plugins\Editor\KEngineEditor


:: Resources
::xcopy %SRC_ASSETS_PATH%\Resources\KEngineConfig.txt %DST_ASSETS_PATH%\Resources\KEngineConfig.txt /S/Y/R

mkdir %DST_ASSETS_PATH%\..\Product\SettingSource
mkdir %DST_ASSETS_PATH%\..\Product\Bundles

:: Codes
::xcopy %SRC_ASSETS_PATH%\KEngine %DST_PLUGIN_PATH%\KEngine /S/Y/R
::xcopy %SRC_ASSETS_PATH%\KEngine.Lib %DST_PLUGIN_PATH%\KEngine.Lib /S/Y/R
::xcopy %SRC_ASSETS_PATH%\KEngine.Editor\Editor %DST_EDITOR_PATH%\KEngine.Editor /S/Y/R
::xcopy %SRC_ASSETS_PATH%\KEngine.EditorTools\Editor %DST_EDITOR_PATH%\KEngine.EditorTools /S/Y/R

echo %SRC_ASSETS_PATH%
echo %DST_PLUGIN_PATH%

mkdir %DST_PLUGIN_PATH%
mklink /J %DST_PLUGIN_PATH%\KEngine %SRC_ASSETS_PATH%\KEngine
mklink /J %DST_PLUGIN_PATH%\KEngine.Lib %SRC_ASSETS_PATH%\KEngine.Lib
mklink /J %DST_PLUGIN_PATH%\KEngine.UI %SRC_ASSETS_PATH%\KEngine.UI

mkdir %DST_EDITOR_PATH%
mklink /J %DST_EDITOR_PATH%\KEngine.Editor %SRC_ASSETS_PATH%\KEngine.Editor\Editor
mklink /J %DST_EDITOR_PATH%\KEngine.EditorTools %SRC_ASSETS_PATH%\KEngine.EditorTools\Editor
mklink /J %DST_EDITOR_PATH%\KEngine.UI.Editor %SRC_ASSETS_PATH%\KEngine.UI.Editor\Editor

:: SLua
mkdir KSFramework\Assets\SLua
mkdir KSFramework\Assets\SLua\Resources
mkdir KSFramework\Assets\SLua\Editor
xcopy slua\Assets\SLua\Editor\CustomEditor.cs KSFramework\Assets\SLua\Editor /S/Y/R
xcopy slua\Assets\SLua\Editor\LuaCodeGen.cs KSFramework\Assets\SLua\Editor /S/Y/R

mklink /J KSFramework\Assets\Plugins\Slua_Managed slua\Assets\Plugins\Slua_Managed

xcopy slua\Assets\Plugins\Android KSFramework\Assets\Plugins\Android /S/Y/R
xcopy slua\Assets\Plugins\iOS KSFramework\Assets\Plugins\iOS /S/Y/R
xcopy slua\Assets\Plugins\slua.bundle KSFramework\Assets\Plugins\slua.bundle /S/Y/R
xcopy slua\Assets\Plugins\x64 KSFramework\Assets\Plugins\x64 /S/Y/R
xcopy slua\Assets\Plugins\x86 KSFramework\Assets\Plugins\x86 /S/Y/R


pause
