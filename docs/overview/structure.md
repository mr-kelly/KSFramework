
![KSFramework模块的组织架构](../images/Structure.png)

KSFramework中，总体分成三个部分：SLua、KEngine、KSFramework自定义模块。

KEngine中，有Resource、Setting、UI三个核心模块和其它一些辅助类，做一些比较基础的通用功能。

SLua中，提供Lua脚本引擎。

KSFramework中，主要把SLua和KEngine中的UI模块进行整合，并提供UI热重载的功能。
