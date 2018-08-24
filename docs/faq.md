本篇介绍一些常见的问题及解答

### OnInit调用两次

默认OnInit在Onit会被调用两次，设计用于热重载。

在OnOpen中也会调用一次OnInit，用于热重载之后，对数据进行初始化。

如果你不希望被调用两次，在OnOpen中注释掉这两行

```c#
//if (!CheckInitScript())       //去掉热重载，因此只需要在OnInit中加入这步检查
//    return;
```

### 配置表生成C#代码，如何热更？



### build之后，运行报错

打包之前，请先生成xlua代码



### LuaBehaviour 

xlua分支，在C#中调用self.ABC为空，已修复，待提交

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





webstrom一直没有释放？

资源是否没有打包成assetbundle



### 打包后无法加载资源？

如果在Editor下正常，请先尝试更新KEngine。

1. Android7出现资源加载失败（小米黑鲨游戏手机 ），把Android的SDK Version改到25以下。

    来自群友：pabble(405706175) 

### loadfromfile

lzma也可以loadfromfile