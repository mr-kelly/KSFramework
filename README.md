
![KSFramework](Docs/KSFramework-logo.png)

[![Build status](https://ci.appveyor.com/api/projects/status/lt34ynvl3lac62ln/branch/master?svg=true)](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master)

[**K**Engine](https://github.com/mr-kelly/KEngine) + [**S**Lua](https://github.com/mr-kelly/slua)+ **Framework** = KSFramework

KSFramework是一个整合KEngine、SLua的Unity 5开发框架，并为程序、美术、策划、运营提供辅助工具集。

**热重载**是KSFramework的开发重点——在不重启游戏的前提下，重载代码、配置表可立刻看到修改效果，最大限度的提升开发、调试的速度，方便运营阶段热更新。

# 安装

## 从产品包安装

您可以从[KSFramework Release](https://github.com/mr-kelly/KSFramework/releases)页面下载最新版本的产品包。

解压后直接用Unity打开KSFramework目录，或直接双击场景KSFramework/Assets/Game.unity。

> 如遇到无法下载的网络问题, 备选下载站:
> - [KSFramework Appveyor Artifacts](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master/artifacts): 包含每次提交的构建结果
> - [KSFramework OSChina镜像](http://git.oschina.net/mrkelly/KSFramework)): 国内的镜像Git


## 从源码安装

获取到源码后，需要通过git submodule命令获取KEngine和SLua
```shell
git submodule init
git submodule update
```
拉取submodule后，Windows下双击执行源码Install.bat进行安装，把KEngine和SLua相关代码链接到KSFramework各目录，然后用Unity打开

# 教程

- [**KSFramework: Unity3D开发辅助框架快速入门**](http://www.jianshu.com/p/ccb491ed4260)
- [KEngine策划指南: 配置表格的编辑与编译](http://www.jianshu.com/p/ead1a148b504)
- [KEngine: 资源的打包、加载、调试监控](http://www.jianshu.com/p/ce3b5d0bdf8c)
- [KSFramework常见问题：Lua脚本热重载，内存状态数据会不会丢失？](http://www.jianshu.com/p/eebd5cfce87f)
- [KSFramework常见问题：Excel如何进行SVN协作、差异比较？](http://www.jianshu.com/p/2ea5468e9d5b)
- [KEngine配置表：扩展表格解析类型](http://www.jianshu.com/p/722c5856166f)
- [KEngine:配置表的条件编译](http://www.jianshu.com/p/cb7ddfab23ba)

# 结构组成

![KSFramework由KEngine和SLua结合组成](Docs/Structure.png)
[View on ProcessOn](https://www.processon.com/view/link/57634e3ce4b07fa2f3bb0ee8)

# 功能特性

## 资源模块

- Unity 5中一键打包Asset Bundle
- AssetBundle加载器，加载时自动处理依赖关系
- 资源路径约定，含热更新资源机制
- 手动的、引用计数的资源释放策略
- ~~资源运行时重载（减引用计数）~~

## 配置表模块

- 自动编译Excel，支持在表中添加注释
- Excel表头，可设置数据类型（如int, array的声明）
- 自动生成配置表读取代码
- 支持惰式加载，无初始化的时间消耗
- 支持热重载，运行时修改配置表无需重启

## UI模块

- 约定优于配置式的UI框架
- 快速导出当前编辑的UI
- 支持热重载，运行时修改UI脚本无需重启

## 脚本模块

- 路径约定，通过import函数进行加载
- 缓存机制配合import函数，可实现所有脚本的热重载
- Lua新增using函数类似于C#中的using，暴露使用table中的属性为全局使用
- 可以在编辑器非运行模式下执行Lua脚本，支持简单Lua单元测试

## 多语言模块

- 基于配置表模块
- 约定好多语言字符串的机制
- ~~多语言字符串收集器~~

## Unity编辑器强化

- 编辑代码后，返回正在运行的游戏，强制停到正在运行的游戏，避免崩溃的出现
- 封装Unity编辑器的各种事件，如编译前、播放前、暂停时等

# 工程建议

建议创建两个Unity工程：code和art，一个用于代码编辑，一个用于美术编辑并导出AssetBundle。
这样code的Unity工程，只带了代码和AssetBundle，没有资源加载的缓慢过程，让Unity开发更畅快；同时也对代码部分做了保密，防止其他人员外泄。

# 键盘快捷键

- Ctrl+Alt+E: 在编辑UI场景时，导出UI成AssetBundle
- Ctrl+Alt+R: 在运行时，热重载所有LuaUIController
- Ctrl+Alt+Shift+R: 在运行时，热重载所有LuaUIController，并且把所有打开状态UI关闭后重新开启
- Ctrl+Alt+I: 在编辑器，打开Game.unity主运行场景
- Ctrl+Alt+O: 在编辑器，打开Ctrl+Alt+I前的一个场景


## KEngine和KSFramework

### 定位不一样

KEngine:为了减低Unity 4.x中AssetBundle的加载、打包复杂度；

KSFramework:一站式的开发框架，可以开箱即用，整合KEngine和SLua。只支持Unity 5。

### 提供的模块不同

KEngine: 提供基础的资源加载（ResourceModule）功能，并以之为基础，增加配置表（SettingModule）、UI模块（UIModule）这两个核心模块；另外还有针对Unity 4.x的资源依赖打包模块。

KSFramework：基于KEngine的资源、UI、配置表模块，实现更直接的、面向具体项目的常用功能模块，并搭配SLua。
