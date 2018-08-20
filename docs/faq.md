本篇介绍一些常见的问题及解答

### OnInit调用两次

默认OnInit在Onit会被调用两次，设计用于热重载。

在OnOpen中也会调用一次OnInit，用于热重载之后，对数据进行初始化。

如果你不希望被调用两次，在OnOpen中注释掉这两行

```c#
//if (!CheckInitScript())       //去掉热重载，因此只需要在OnInit中加入这步检查
//    return;
```

