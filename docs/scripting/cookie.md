
Web开发中，Cookie是指某些网站为了辨别用户身份、进行session跟踪而储存在用户本地终端上的数据。

KSFramework中，参考Web开发，取了`Cookie`一词，作为其客户端状态缓存的模块名字。

代码热重载的一大难题，就是代码执行期间产生的内存状态数据。在服务器开发中，缓存层是非常常见的，因而我们在网络上的各种开源服务器框架，很多都支持脚本代码热重载。而Cookie则模仿这种缓存层。

## 将状态数据迁移到缓存

Cookie的本质就是缓存层。将尽可能多的状态信息放到缓存层，数据对运行时代码的依赖就越低。

状态数据与代码逻辑分离是代码热重载的重要要求。

## 利用Cookie脚本重载

查看章节：[利用Cookie脚本重载](../advanced/script-reload)

## 使用Cookie

Cookie的实现本质，是一个C#端的key-value存取模块：

```Lua

Cookie.Set('TestString', 'hi i am cookie')

print(Cookie.Get('TestString'))
-- output: hi i am cookie
```

当Lua代码进行重载，C#层的Cookie的完整键值数据都会完整保留。
