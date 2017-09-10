

## 运行环境及其它热更新方案

目前经过测试的u部分Unity3D版本、操作系统版本、移动平台信息如下：

**Unity3D版本**

- Unity5.6.0

- Unity5.4.0

- Unity5.3.7

  ​

Unity4.x的版本

- Unity4.6.9




**操作系统**

- Windows 7 x64 sp1

- Windows 10 x64 专业版/企业版本

- Mac OS X 10.x




**Android 平台**

- Android 7.0

- Android 4.3.3




**IOS平台**

- IOS 10.3.3

- IOS 8.3





## Unity3D版本

目前支持Unity4.X及Unity5.x的所有版本，Unity2017在近期将会进行升级适配。



## SLua方案

支持SLua截止目前的最新版本，理论上KSFramework可以支持任意版本的SLua，因为KSFramework封装了一层LuaModule、LuaBehaviour，执行代码逻辑，对于开发者使用那种Lua热更热方案是没有直接影响的，可以兼容适配。



PS.github上的slua版本较旧，大家可自己更新slua的版本，并执行安装。



## XLua方案

支持XLua截止目前的最新版本，理论上KSFramework可以支持任意版本的XLua，因为KSFramework封装了一层LuaModule、LuaBehaviour，执行代码逻辑，对于开发者使用那种Lua热更热方案是没有直接影响的，可以兼容适配。



## ILRuntime方案

目前基于ILRuntime的最新版本，在ILRuntime分支进行测试，正在进行中。



## 其它热更新方案

理论上只需要修改LuaModule的部分代码，就可以支持其它任意的热更新方案。

- `KSFramework/Modules/LuaModule/LuaBehaviour.cs`  
- `KSFramework/Modules/LuaModule/LuaModule.cs`