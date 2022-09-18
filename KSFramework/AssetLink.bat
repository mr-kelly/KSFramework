set path=%~dp0
set platform=%~1%
rd %path%\Assets\StreamingAssets\Bundles\%platform% /S /Q
rd %path%\Assets\StreamingAssets\Lua /S /Q
rd %path%\Assets\StreamingAssets\Setting /S /Q

mklink /j %path%\Assets\StreamingAssets\Bundles\%platform% %path%\Product\Bundles\%platform%
mklink /j %path%\Assets\StreamingAssets\Lua %path%\Product\Lua
mklink /j %path%\Assets\StreamingAssets\Setting %path%\Product\Setting

pause