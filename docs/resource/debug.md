

# 资源的调试监控

在Unity Editor模式下，所有的XXXLoader加载类实例，都会伴随住一个GameObject的产生，而这个GameObject，只用于进行调试、内存信息查看：


![资源加载调试信息：Loader、加载的对象](../images/resource/debug-1.png)
> 资源加载调试信息：Loader、加载的对象

![每一个Loader的引用计数信息，都可以通过面板来进行实时查看](../images/resource/debug-2.png)

如上图所示，通过KEngine的资源调试器，可以方便的找到加载的AssetBundle的资源对象、监控内存占用的大小、Loader加载消耗的时间、Loader当前引用计数等信息。对比Unity原生的Profiler，这些信息是即时的。 开发人员可以非常方便的寻找资源泄露问题，优化内存占用。
