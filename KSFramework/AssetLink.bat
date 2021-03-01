set path=%~dp0
rd %path%\Assets\StreamingAssets\Lua /S /Q
rd %path%\Assets\StreamingAssets\Setting /S /Q

mklink /j %path%\Assets\StreamingAssets\Lua %path%\Product\Lua
mklink /j %path%\Assets\StreamingAssets\Setting %path%\Product\Setting

pause