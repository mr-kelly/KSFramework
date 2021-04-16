新版本中已经把配置项放在Appconfig.cs中，因为这些设置项不会经常改动，把它放在代码中减少读取配置的时间。

## 配置项讲解

### Assetbundle文件格式(默认.k)

如果需要修改打包后ab的文件格式，则修改配置：

```ini
AssetBundleExt = .k
或
AssetBundleExt = .ab
```

### IsEditorLoadAsset

编辑器开发环境下无需打包，修改后直接加载

```ini
IsEditorLoadAsset = 0 ;编辑器也加载ab
或
IsEditorLoadAsset = 1  ;编辑器通过AssetDataBase加载资源
```



### 使用Lua还是纯C#编写UI代码

每个界面对应一个相同名字的脚本，如果是xlua或slua则会加载lua脚本，而ILRuntime则会从hotfix.dll中加载cs脚本。

PS.如果想在xlua或slua中使用c#来编写UI功能，请在`GUIBridge.CreateUIControlle`修改下实现方法，大概代码如下：

```c#
        public UIController CreateUIController(GameObject uiObj, string uiTemplateName)
        {
            UIController uiBase = null;
#if xLua || SLUA
            uiBase = new LuaUIController();
#elif ILRuntime
            uiBase = new ILRuntimeUIBase();
#else
            var type = System.Type.GetType("UI" + uiTemplateName + ", Assembly-CSharp");
            uiBase = Activator.CreateInstance(type) as UIController;
#endif
        }
```



### 多语言配置

```ini
;当前要使用的语言
LangId = en
```



### Lua源代码路径

相对于Product目录的相对路径，默认是放在Product/Lua下

```ini
LuaPath = Lua
```

## 计划

TODO 通过Appconfig.txt来重载Appconfig.cs中的数据