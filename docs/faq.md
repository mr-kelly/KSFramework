本篇介绍一些常见的问题及解答

### OnInit调用两次

默认OnInit在Onit会被调用两次，设计用于热重载。

在OnOpen中也会调用一次OnInit，用于热重载之后，对数据进行初始化。

如果你不希望被调用两次，在**LuaUIController.OnOpen**中注释掉这两行

```c#
//if (!CheckInitScript())       //去掉热重载，因此只需要在OnInit中加入这步检查
//    return;
```

### 配置表生成C#代码，如何热更？

配置部分可以修改生成Lua代码，或读取tsv，而不使用C#代码。



### build之后，运行报错

请检查是否有生成xlua/slua代码，根据具体的报错日志进行排查。



### LuaBehaviour 

xlua分支，在C#中调用self.ABC为空，已修复

```c#
TestLuaBehaivour = {}

function TestLuaBehaivour:Awake()
print("Test Lua Behaivour Awake!")
self.ABC={}
self.ABC["name"]="test" 
end

function TestLuaBehaivour:Start()
    print("Test Lua Behaivour start!")
print(self.ABC)
print(self.ABC["name"])
end

return TestLuaBehaivour
```



### 资源相关

Q：webstrom一直没有释放？

A：资源是否没有打包成assetbundle



Q：部分安卓机型报：local reference table overflow (max=512)

A：在同学反馈用Unity2017打包，在红米部分机型会出现此错误，已添加宏处理Unity版本。



Q：打包后无法加载资源？

A：如果在Editor下正常，请先尝试更新KEngine。

1. Android7出现资源加载失败（小米黑鲨游戏手机 ），把Android的SDK Version改到25以下。

    来自群友：pabble(405706175) 



Q：Lua如何打包？

A：可以打包成zip，游戏启动时，进行解压



### loadfromfile

lzma也可以loadfromfile



### xlua相关

提示：code has not been genrate, may be not work in phone!

这是因为xlua通过 DelegateBridge.Gen_Flag 来判断是否生成代码，但我们在打包过程中，先生成代码，然后删除生成后的代码，并没有设置此flag，但功能是正常的。



更多xlua相关的知识，可以查看xlua的官方文档