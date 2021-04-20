## 代码放置目录

### SLua

KSFramework中SLua采用其默认的目录结构，即C#运行时代码放置在Assets/Plugins/Slua_Managed中，编辑器相关代码放置在Assets/SLua中。

### XLua

XLua放置在Assets/XLua目录下，结构和官方保持

### KEngine

KEngine的所有运行时代码放置在**Assets/KSFramework/KEngine**中，相关的编辑器代码放置在**Assets/KSFramework/KEngine/Editor**中。

### KSFramework

对比KEngine新加的功能，所有运行时代码放置在**Assets/KSFramework**中，相关的编辑器代码放置在**Assets/KSFramework/Editor**中。

### 为什么都放在Plugins中？

脚本的修改，会触发Unity的重新编译。一般的项目逻辑，都在非Plugins目录进行的，随着项目的发展，非Plugins目录的代码越来越多，编译越来越慢。把代码放进Plugins目录，可以节约一定的编译时间，改善开发效率。

注：对于unity2018及更高的版本中可以对不同的目录设置Assemly。

## 资源目录约定

- **Assets/BundleEdting**: 一些编辑性的、需要配合编译过程的美术资源。比如UI在保存时自动导出Prefab到Assets/BundleResources目录下
- **Assets/BundleResources:** Asset Bundle全自动化导出的关键，此目录下所有的文件都会设置上abName，然后进行导出，并使用Unity管理依赖。
- **Product/**: 默认的产品导出目录，包括Asset Bundle、Lua脚本、配置表，统一放置在这个目录。 编译客户端时，这些资源将被拷贝到StreamingAssets目录后才进行编译打包。

BundleEdting和BundleResources的名称可自定义，修改AppConfig.cs和KEngineDef.cs的路径定义即可。

## UI资源

所有的UI都放在根目录(**Assets/BundleEdting/UI**)下，KSFramrwork建议的分类如下：

<pre>
-atlas_common/(整个游戏的公共图集)
-系统A/
​	-atlas/(在导出UI时会自动把此目录的小图打包为：系统名.spriteatlas)
​	-面板1
​	-面板2
​	...更多面板
-系统B
-更多系统...


</pre>

上述UI场景在保存时，会导出为prefab放到**Assets/BundleResources/UI**下：

<pre>
-系统A的面板1.prefab
-系统A的面板2.prefab
-系统A的面板xxx.prefab
-系统A.spriteatlas
======== 每个面板打包成一个ab，atlas单独打包到一个ab中 ==========



-系统A只有一个同名面板.prefab
-系统A.spriteatlas
======= 这两个文件会打包到一个ab中 ========
</pre>

## 场景资源

所有需要打包成ab的场景文件都放在**Assets/BundleResources/Scene**下

如果多个场景都有用到的共用小物件，建议这些小物件打到共用prefab中

<pre>
    -场景1001.unity
    	-navmesh.asset
    	-lightmap/文件夹
    	-物件a.prefab
    	-物件b.prefab

</pre>

如果非editor下直接加载场景，则无需将sene.unity添加到Build Settings - Scenes In Build下

## 角色/NPC/怪物资源

所有的角色资源放在BundleEditing/Character/下，每个角色一个文件夹，一个角色中包含几个子文件夹，并且把制作好的prefab放到BundleResources目录下

<pre>
	-职业A/
		-mesh/
		-anim/
		-tex/
		-xx.prefab放到BundleResources下，如果有共用资源也放到BundleResources目录下
	-职业B/
	-怪物或BOSS/
	-....
</pre>


对于大多数单个的npc，怪物等，可以将贴图，材质，动作，Mesh打包到一个ab中(只对prefab设置abName即可)

## 特效资源

所有的特效资源放在BundleEditing/Effect/下，建议将单个effect打包一个ab，如果effect中包含的贴图和材质在多个prefab中用到，则把这些共用的放在Assets/BundleResources/Effect/common下

<pre>
    -character/(所有的角色特效,包括战斗特效、武器上的特效)
    	-mesh/
    	-anim/
    	-tex/
    	-fx_xxx.prefab ->放到BundleResources下，建议单个特效会打成一个ab，mesh,anim,texture等不进行拆分
    -buff/(所有的buff特效)
    -pet、魂器、圣物、坐骑、法宝等文件夹
    -monster/(所有的怪物/boss特效)
    -scene/(场景内的特效)
    -ui/(ui特效资源)
    -common/(公共资源)
</pre>
## Shader资源

KSFramework中建议把所有的shader放在BundleEditing/Shader下，在打包时会把整个目录的shader打包到一个ab中，这样就不会有shader冗余。

TODO 计划：在游戏启动时进行shader预热

如果需要自定义规则：`BaseImporter.cs OnPostprocessAssets`

## 声音资源

KSFramework中建议把所有的声音放在BundleEditing/Sounds下，然后在打包时除BGM目录外，其它每个子目录中的声音文件会打包到一个ab中

如果是某个界面特有的音效可以和界面打包到一个ab中，BGM(背景音乐)目录下的音频会每个单独打成一个ab包

如果需要自定义规则：`BaseImporter.cs OnPreprocessAudio`

## AB粒度划分

在KSFramework中是通过LoadFromFile来加载ab，所以我们不建议对ab折分地过细。

根据出现频率和同时加载峰值来划分，尽可能把逻辑上同时出现(非公共部分)、小而细碎的资源（贴图、Material、动画、音效）尽可能打包在一起，并通过

LoadAll来进行加载，这样会带来更好的加载效率，最终我们的结果是减小内存峰值和加快加载速度。



可以参考《[AssetBundle 粒度规划](https://answer.uwa4d.com/question/58e5bd96e042a5c92c3484ec)》