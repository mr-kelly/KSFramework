
![KSFramework](Docs/KSFramework-logo.png)

[![Build status](https://ci.appveyor.com/api/projects/status/lt34ynvl3lac62ln/branch/master?svg=true)](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master)

[**K**Engine](https://github.com/mr-kelly/KEngine) + [**S**Lua](https://github.com/mr-kelly/slua)|[XLua](https://github.com/Tencent/xLua)+ **Framework** = KSFramework

**[KSFramework](https://github.com/mr-kelly/KSFramework)是一个整合KEngine、SLua/XLua 的Unity 5 Asset Bundle开发框架，并为程序、美术、策划、运营提供辅助工具集。**

---------------------

**热重载**是KSFramework的开发重点——在不重启游戏的前提下，重载代码、配置表可立刻看到修改效果，最大限度的提升开发、调试的速度，方便运营阶段热更新。

对于程序人员，可以使用AssetBundle加载与打包、脚本化的UI、配置表代码自动生成、下载更新等基础功能模块，大大减少游戏周边基础功能的工作量；

对于策划人员，使用Excel进行编辑，可以在编辑过程中添加注释、图标、预编译指令，KSFramework会根据配置内容自动生成代码供程序使用。

对于美术人员，只需将项目需要用到资源放到指定目录，将会自动的生成Asset Bundle；程序加载Asset Bundle跟Resources.Load一样方便。


对于运营人员，利用KSFramework的热重载特性，可以针对运营需求，在项目运行过程中配置表、脚本代码在用户无知觉的情况下进行热更新。

# 基础知识

## 方式1，下载即用

以下方法任选其一：

方法一：把源码**git clone**到本地（推荐）：  https://github.com/mr-kelly/KSFramework.git

方法二：在项目页面点击 **Clone or download**  选择 **Download ZIP** 

**更新说明：**

如果需要单独更新KEngine或者xlua/slua，可以手动删除原有目录，再拷贝新的目录过去。

原有目录如下：

```c#
KSFramework\Assets\KSFramework\KEngine
KSFramework\Assets\xLua
KSFramework\Assets\slua
```



## 方式2，从产品包安装

（这种方式获得的版本非最新版本），您可以从[KSFramework Release](https://github.com/mr-kelly/KSFramework/releases)页面下载最新版本的产品包。

解压后直接用Unity打开KSFramework目录，或直接双击场景KSFramework/Assets/Game.unity。

> 如遇到无法下载的网络问题, 备选下载站:
>
> - [KSFramework Appveyor Artifacts](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master/artifacts): 包含每次提交的构建结果
> - [KSFramework OSChina镜像](http://git.oschina.net/mrkelly/KSFramework)): 国内的镜像Git


## 编辑器下无需打包资源

在Editor下，无需打包AB资源即可运行，没有打包时间，提高工作效率。

如果需要调试AB相关信息，请打包AB。

## 工程打包方法
1. 点击菜单栏 **KEngine** - **AutoBuilder** - **Android**/**iOS**/**Windows**
2. Unity会执行生成安装包，放在**Product/Apps/Android/**KSFramework.apk `[根据平台区分目录]`
3. 打包期间会自动Link AB资源到**StreamingAssets**目录下

注：如果是xlua，打包前请先生成代码。其它热更新方案，请参考框架使用说明。

## Unity3D版本支持

| Unity3D版本  | 支持情况 | 备注                        |
| ---------- | ---- | ------------------------- |
| Unity 4.X  | 支持   |                           |
| Unity 5.x  | 支持   | 部分版本API有差异，建议使用5.3.7及以上版本 |
| Unity 2017 | 支持   |                           |
| Unity 2018 | 支持   | 2018.2.7f1已测试             |


## 其它热更新方案

理论上只需要修改LuaModule的部分代码，就可以支持其它任意的热更新方案。

- `KSFramework/Modules/LuaModule/LuaBehaviour.cs`  
- `KSFramework/Modules/LuaModule/LuaModule.cs`

更多版本信息可查看：https://mr-kelly.github.io/KSFramework/overview/environment-other-hotfix-solution/

## 错误解决

下载后如果运行出错，或者使用其它版本的Unity打开，尝试以下方法：

1. 重新生成Assetbundle，方法如下：点击菜单项 **KEngine** - **AssetBundle** - **Bulld All**
2. 删除xlua或slua的生成代码，重新生成，生成的代码不同的Unity版本有差异
3. 上传报错信息，提issuse。如果能自己调试解决，那更棒，同时欢迎pull 到仓库中。

# 教程

- [**KSFramework: Unity3D开发辅助框架快速入门**](http://www.jianshu.com/p/ccb491ed4260)
- [KEngine策划指南: 配置表格的编辑与编译](http://www.jianshu.com/p/ead1a148b504)
- [KEngine: 资源的打包、加载、调试监控](http://www.jianshu.com/p/ce3b5d0bdf8c)
- [KSFramework常见问题：Lua脚本热重载，内存状态数据会不会丢失？](http://www.jianshu.com/p/eebd5cfce87f)
- [KSFramework常见问题：Excel如何进行SVN协作、差异比较？](http://www.jianshu.com/p/2ea5468e9d5b)
- [KEngine配置表：扩展表格解析类型](http://www.jianshu.com/p/722c5856166f)
- [KEngine:配置表的条件编译](http://www.jianshu.com/p/cb7ddfab23ba)

# 文档

- [【查看完整文档】](https://mr-kelly.github.io/KSFramework/)
- [功能特性](https://mr-kelly.github.io/KSFramework/overview/features/)
- [策划指南：配置表的使用](https://mr-kelly.github.io/KSFramework/setting/guide/)

...

# 结构组成

![KSFramework由KEngine和SLua结合组成](Docs/Structure.png)



# 涉及第三方库

- [xLua is a lua programming solution for C# ( Unity, .Net, Mono) , it supports android, ios, windows, linux, osx, etc.](https://github.com/Tencent/xLua), master分支已切换为默认使用xLua
- [SLua:基于Unity的Lua引擎，也可用于C#独立程序](https://github.com/pangweiwei/slua) 较少维护
- [ILRuntime项目为基于C#的平台（例如Unity）提供了一个纯C#实现，快速、方便且可靠的IL运行时，使得能够在不支持JIT的硬件环境（如iOS）能够实现代码的热更新](https://github.com/Ourpalm/ILRuntime) , 待续
- [KEngine:AssetBundle打包加载框架](https://github.com/mr-kelly/KEngine)
  - [ini-parser:Ini配置文件解析器，支持多文件合并](https://github.com/rickyah/ini-parser)
  - [Premake:VS工程生成](https://github.com/premake/premake-core)
- [TableML:表格标记语言，运行时与编译器](https://github.com/mr-kelly/TableML)
  - [NPOI:强大的Excel读写库](http://npoi.codeplex.com/)
    - [ISharpZipLib:Zip格式读写库](https://github.com/icsharpcode/SharpZipLib)
  - [DotLiquid:模板语言引擎](https://github.com/dotliquid/dotliquid)
