
KSFramework中使用Ini格式的文件作为配置的基础。它有一个基础的ini+自定义的ini两个文件合并而成。

基础的ini配置约定，在代码EngineConfigs.cs文件中可以查看。

自定义的ini文件，放置在Assets/Resources/AppConfigs.txt中。


## 读取配置

在KSFramework初始化时，会把两个ini配置文件合并。要想获取到配置的值，只需简单的执行静态方法：

```csharp
AppEngine.GetConfig("Section", "ConfigKey")
```

你可以放入自己的自定义参数并读取。
