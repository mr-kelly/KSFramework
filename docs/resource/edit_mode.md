#### Editor下直接加载资源无需打包

如何在Editor下直接加载资源，而不用打包ab呢？

在**Resources\AppConfigs.txt**中配置**IsEditorLoadAsset=1** ，在Editor模式下就会使用AssetDatabase进行加载资源，对于大型项目可以减少打包时间，实现代码可查看 AssetFileLoader.cs。

注意事项：默认是需要打包成assetbundle加载资源，在游戏正式发布的时候也是通过assetbundle方式来加载资源的。

如果是分了两个工程，比如代码工程，美术资源工程，则无法使用此功能。