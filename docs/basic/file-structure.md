# 代码放置目录

## SLua

KSFramework中SLua采用其默认的目录结构，即C#运行时代码放置在Assets/Plugins/Slua_Managed中，编辑器相关代码放置在Assets/SLua中。

## XLua

XLua放置在Assets/XLua目录下，结构和官方保持

## KEngine

KEngine的所有运行时代码放置在**Assets/KSFramework/KEngine**中，相关的编辑器代码放置在**Assets/KSFramework/KEngine/Editor**中。

## KSFramework

对比KEngine新加的功能，所有运行时代码放置在**Assets/KSFramework**中，相关的编辑器代码放置在**Assets/KSFramework/Editor**中。

## 为什么都放在Plugins中？

脚本的修改，会触发Unity的重新编译。一般的项目逻辑，都在非Plugins目录进行的，随着项目的发展，非Plugins目录的代码越来越多，编译越来越慢。把代码放进Plugins目录，可以节约一定的编译时间，改善开发效率。

注：对于unity2018及更新的版本中可以对不同的目录设置Assemly。

## 资源目录约定

- **Assets/BundleEdting**: 一些编辑性的、需要配合编译过程的美术资源。比如UI场景，放置在这里，保存是，自动导出Prefab到Assets/BundleResources/UI目录
- **Assets/BundleResources:** Asset Bundle全自动化导出的关键，KSFramework的AssetBundle打包，按照这个目录的结构，完整导出。
- **Product/**: 默认的产品导出目录，包括Asset Bundle、Lua脚本、配置表，统一放置在这个目录。 编译客户端时，这些资源将被拷贝到StreamingAssets目录后才进行编译打包。

路径定义放在AppConfig.cs或KEngineDef.cs