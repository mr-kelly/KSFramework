
在KSFramework中，目前并没有提供资源包更新的具体功能。

因资源包更新同时还涉及到资源文件服务器的整合、资源包的制作这些耦合的功能。

您可以参考作者的另一个项目，[resources_packer](https://github.com/mr-kelly/resources_packer)提供资源差异包的制作功能。


要实现更新的机制非常之多，最常见的更新莫过于启动时进行更新。但是基于KSFramework热重载的特性，建议可以更多的考虑运行时静默的下载更新，静默重载。


