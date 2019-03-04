本篇介绍一些常见的问题及解答

### 编辑器下无需打包资源

在Editor下，无需打包AB资源即可运行，减少打包时间，提高工作效率。

如果需要调试AB资源泄漏问题，请打包AB。

### 如何打包APK？

1. 点击菜单栏 **KEngine** - **AutoBuilder** - **Android**/**iOS**/**Windows**
2. Unity会执行生成安装包，放在**Product/Apps/Android/**KSFramework.apk `[根据平台区分目录]`
3. 打包期间会自动Link AB资源到**StreamingAssets**目录下

注：如果是xlua，打包前请先生成代码。其它热更新方案，请参考框架使用说明。

### 如何更新仓库？

如果需要单独更新KEngine或者xlua/slua，可以手动删除原有目录，再拷贝新的目录过去。

原有目录如下：

```c#
KSFramework\Assets\KSFramework\KEngine
KSFramework\Assets\xLua
KSFramework\Assets\slua
```

### OnInit会调用两次？

默认OnInit在Onit会被调用两次，设计用于热重载。

在OnOpen中也会调用一次OnInit，用于热重载之后，对数据进行初始化。

如果你不希望被调用两次，在**LuaUIController.OnOpen**中注释掉这两行

```c#
//if (!CheckInitScript())       //去掉热重载，因此只需要在OnInit中加入这步检查
//    return;
```

### 配置表生成C#代码，如何热更？

可以把配置文件生成Lua代码，或读取tsv，或配置文件插入到数据库，等等方式。

C#代码是配置表的字段，在开发期可以直接使用Class类的字段，而不用打开配置表查字段。

可以参考 tableml 的修改版，改成sqlite：https://www.cnblogs.com/zhaoqingqing/p/7440867.html



### build之后，运行报错

请检查是否有生成xlua/slua代码，根据具体的报错日志进行排查。



### LuaBehaviour 

xlua分支，在C#中调用self.ABC为空，已修复

```c#
TestLuaBehaivour = {}

function TestLuaBehaivour:Awake()
print("Test Lua Behaivour Awake!")
self.ABC={}
self.ABC["name"]="test" 
end

function TestLuaBehaivour:Start()
    print("Test Lua Behaivour start!")
print(self.ABC)
print(self.ABC["name"])
end

return TestLuaBehaivour
```



### 资源相关

Q：webstrom一直没有释放？

A：资源是否没有打包成assetbundle



Q：部分安卓机型报：local reference table overflow (max=512)

A：在同学反馈用Unity2017打包，在红米部分机型会出现此错误，已添加宏处理Unity版本。



Q：打包后无法加载资源？

A：如果在Editor下正常，请先尝试更新KEngine。

1. Android7出现资源加载失败（小米黑鲨游戏手机 ），把Android的SDK Version改到25以下。

    来自群友：pabble(405706175) 



Q：Lua如何打包？

A：可以打包成zip，游戏启动时，进行解压



### loadfromfile

lzma也可以loadfromfile

### 其它热更新方案

理论上只需要修改LuaModule的部分代码，就可以支持其它任意的热更新方案。

- `KSFramework/Modules/LuaModule/LuaBehaviour.cs`  
- `KSFramework/Modules/LuaModule/LuaModule.cs`

更多版本信息可查看：https://mr-kelly.github.io/KSFramework/overview/environment-other-hotfix-solution/

### xlua相关

提示：code has not been genrate, may be not work in phone!

这是因为xlua通过 DelegateBridge.Gen_Flag 来判断是否生成代码，但我们在打包过程中，先生成代码，然后删除生成后的代码，并没有设置此flag，但功能是正常的。



更多xlua相关的知识，可以查看xlua的官方文档

## 遇到报错

> 问题：TargetException: Non-static field reqires a target 
>
> 调用堆栈 KEngine.Log.Logger
>
> 解决办法：游戏启动时，打开Unity的日志窗口(Console界面)，因为添加了双击日志跳转到Lua文件中

> 如果游戏启动报，资源加载的报错

解决办法：

1. 重新生成Assetbundle，方法如下：点击菜单项 **KEngine** - **AssetBundle** - **Bulld All**
2. 删除xlua或slua的生成代码，重新生成，生成的代码不同的Unity版本有差异
3. 如果前两步不能解决问题，请上传报错信息，提issuse。