# Submodule update
# git submodule update --recursive
SRC_ASSETS_PATH=$PWD/KEngine/KEngine.UnityProject/Assets
DST_ASSETS_PATH=$PWD/KSFramework/Assets
DST_PLUGIN_PATH=$PWD/KSFramework/Assets/Plugins/KEngine
DST_EDITOR_PATH=$PWD/KSFramework/Assets/Plugins/Editor/KEngineEditor


# Resources

mkdir $DST_ASSETS_PATH/../Product/SettingSource
mkdir $DST_ASSETS_PATH/../Product/Bundles

#  Codes

echo $SRC_ASSETS_PATH
echo $DST_PLUGIN_PATH

mkdir $DST_PLUGIN_PATH 2>nul
rm $DST_PLUGIN_PATH/KEngine
ln -s $SRC_ASSETS_PATH/KEngine $DST_PLUGIN_PATH/KEngine 
rm $DST_PLUGIN_PATH/KEngine.Lib
ln -s $SRC_ASSETS_PATH/KEngine.Lib $DST_PLUGIN_PATH/KEngine.Lib 
rm $DST_PLUGIN_PATH/KEngine.UI
ln -s $SRC_ASSETS_PATH/KEngine.UI $DST_PLUGIN_PATH/KEngine.UI 

mkdir $DST_EDITOR_PATH 2>nul
rm $DST_EDITOR_PATH/KEngine.Editor
ln -s $SRC_ASSETS_PATH/KEngine.Editor/Editor $DST_EDITOR_PATH/KEngine.Editor 
rm $DST_EDITOR_PATH/KEngine.EditorTools
ln -s $SRC_ASSETS_PATH/KEngine.EditorTools/Editor $DST_EDITOR_PATH/KEngine.EditorTools 
rm $DST_EDITOR_PATH/KEngine.UI.Editor
ln -s $SRC_ASSETS_PATH/KEngine.UI.Editor/Editor $DST_EDITOR_PATH/KEngine.UI.Editor 

mkdir -p KSFramework/Assets/Plugins/libs/ 2>nul
cp -rf $SRC_ASSETS_PATH/Plugins/Android/libs/KEngine.Android.jar KSFramework/Assets/Plugins/Android/libs/KEngine.Android.jar

# SLua
mkdir KSFramework/Assets/SLua
mkdir KSFramework/Assets/SLua/Resources
mkdir KSFramework/Assets/SLua/Editor
cp -rf slua/Assets/SLua/Editor/CustomEditor.cs KSFramework/Assets/SLua/Editor
cp -rf slua/Assets/SLua/Editor/LuaCodeGen.cs KSFramework/Assets/SLua/Editor

rm -rf KSFramework/Assets/Plugins/Slua_Managed 
ln -s $PWD/slua/Assets/Plugins/Slua_Managed KSFramework/Assets/Plugins/Slua_Managed 

cp -rf slua/Assets/Plugins/Android/* KSFramework/Assets/Plugins/Android
cp -rf slua/Assets/Plugins/iOS/* KSFramework/Assets/Plugins/iOS
cp -rf slua/Assets/Plugins/slua.bundle/* KSFramework/Assets/Plugins/slua.bundle
cp -rf slua/Assets/Plugins/x64/* KSFramework/Assets/Plugins/x64
cp -rf slua/Assets/Plugins/x86/* KSFramework/Assets/Plugins/x86

echo Finish!

