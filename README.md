# KSFramework
**K**Engine + **S**Lua + **Framework** = KSFramework

KSFramework是一个整合KEngine、SLua和一些开发组件组成的全功能Unity 5开发框架，适合有一定规模的团队使用。

目前正在开发中...


## Session会话模块

### 为什么要用Session？
TODO

## UI模块

- 快速导出当前编辑的UI
- 支持运行时热重载UI脚本
- UISession（会话）支持

# 工程建议

- 建议创建两个工程：code和product，一个用于代码编辑，一个用于美术编辑导出AssetBundle。

# 键盘快捷键

- Ctrl+Alt+E: 在编辑UI场景时，导出UI成AssetBundle
- Ctrl+Alt+R: 在运行时，热重载所有LuaUIController
- Ctrl+Alt+I: 在编辑器，打开Game.unity主运行场景
- Ctrl+Alt+O: 在编辑器，打开Ctrl+Alt+I前的一个场景

## KEngine和KSFramework

随着KEngine复杂程度、项目适配的程度增加，变得越来越臃肿了。

完全不符合最初的简单、解耦的设计方式，越来越多的特定模块被加入进去, 如KResourceDep、KAssetDep两个模块，两种不同的思路来对Unity 4.x的AssetBundle依赖打包进行处理。然而Unity 5的新AssetBundle打包系统的存在，让它们已经没有实际意义了。

KSFramework希望能简化这些事情，做到简单、易用。 并且KSFramework的最大不同是，适当的增加部分耦合组件，来做到一站式的开发框架，可以开箱即用，因此把SLua也引入其中。只支持Unity 5。
