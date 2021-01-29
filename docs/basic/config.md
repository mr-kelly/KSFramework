
KSFramework中使用Ini格式的文件作为配置的基础。它有一个基础的ini+自定义的ini两个文件合并而成。

基础的ini配置约定，在代码EngineConfigs.cs文件中可以查看。

自定义的ini文件，放置在Assets/Resources/AppConfigs.txt中，或搜索**AppConfigs.txt**

NOTE：新版本中已经把配置项放在Appconfig.cs中，因为这些设置项不会经常改动，把它放在代码中减少读取配置。

## 读取配置

在KSFramework初始化时，会把两个ini配置文件合并。要想获取到配置的值，只需简单的执行静态方法：

```ini
AppEngine.GetConfig("Section", "ConfigKey")
```

你可以放入自己的自定义参数并读取。

## 配置项讲解

### Assetbundle文件格式(默认.k)

如果需要修改打包后ab的文件格式，则修改配置：

```ini
AssetBundleExt = .k
或
AssetBundleExt = .ab
```

### IsEditorLoadAsset

```ini
IsEditorLoadAsset = 0 ;编辑器也加载ab
或
IsEditorLoadAsset = 1  ;编辑器通过AssetDataBase加载资源
```



### 使用Lua还是纯C#编写UI代码

每个界面对应一个相同名字的脚本，UGUISLuaBridge会加载lua脚本，UGUIBridge会绑定同名脚本

```ini
UIModuleBridge = KSFramework.UGUISLuaBridge ;使用lua编写UI代码
或
UIModuleBridge = KSFramework.UGUIBridge ;使用C#编辑UI代码
```



### 多语言配置

```ini
;当前要使用的语言
I18N = en
;如果没有配置I18N则取第1个为默认语言
I18NLanguages = cn,en
```



### Lua源代码路径

相对于Product目录的相对路径，默认是放在Product/Lua下

```ini
LuaPath = Lua
```

