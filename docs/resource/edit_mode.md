#### Editor加载无需打包

在**Resources\AppConfigs.txt**中配置**IsEditorLoadAsset=1** ，在Editor模式下就会使用AssetDatabase进行加载资源，对于大型项目可以减少打包时间，实现代码可查看 AssetFileLoader.cs。

注：默认是需要打包成assetbundle加载资源，在游戏正式发布的时候也是通过assetbundle方式来加载资源的。