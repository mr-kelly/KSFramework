
rm -rf ./Assets/StreamingAssets/Bundles/$1
rm -rf ./Assets/StreamingAssets/Lua
rm -rf ./Assets/StreamingAssets/Setting

#if link fail, use full path to test
ln -s ~/KSFramework/Product/Bundles/$1 ./Assets/StreamingAssets/Bundles/$1
ln -s ~/KSFramework/Product/Lua ./Assets/StreamingAssets/Lua
ln -s ~/KSFramework/Product/Setting ./Assets/StreamingAssets/Setting