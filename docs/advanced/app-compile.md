
当您在菜单中Build Settings进行客户端的Build时，KSFramework在后边默默的做了一些额外处理：

## 将资源拷贝到StreamingAssets

开发过程中你会发现，无论是Lua脚本、配置表、Asset Bundle，这些资源都放在了配置的自定义项Product目录。

在Unity的标准的中，有三种加载文件的方式，分别是Resources加载、WWW加载StreamingAssets、PersitentDataPath加载。 按照标准，资源文件应该要放置在以上目录之一中，而KSFramework中资源文件的存放路径是自定义的。这是为什么————

- 编辑器模式，KSFramework的加载类会自动读取定义的Product目录
- 编译时，Product目录的Bundles目录、Lua目录、Setting目录会被拷贝到StreamingAssets目录
- 非编辑器模式（如iOS、Android客户端），读取的StreamingAssets目录


### 为什么要这样设计？
 
首先，在文件放置在StreamingAssets时，Unity会自动生成meta文件。而这些StreamingAssets中的meta文件对开发时没有作用的，在版本管理（如SVN提交）时，会令人迷惑这到底要不要提交。 保持文件结构的整洁，对开发效率的提升是有作用的。

其次，实际的项目开发中，除了客户端，还有服务端。部分资源，如配置表、脚本，是服务端-客户端共用的，如果它们放置在Unity的StreamingAssets中，服务端就需要进入到客户端的目录去读取。


此外，Unity中Asset Bundle是平台独立的，即iOS的AssetBundle不能在Android中使用。因此，在编译时，KSFramework会判断编译的客户端所属的平台，自动将指定平台的AssetBundle目录导入。比如现在要编译Android客户端，那么KSFramework会将Product/Bundles/Android目录做一个软连接，链到StreamingAssets。 同时不处理Products/Bundles中的其它目录，否则，一些没用资源就进入到最终的APK包了。
