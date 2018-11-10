

# UI的打开、关闭、调用都是异步的

类似资源模块，UI模块是接口，大部分异步的， 主要采用CPS异步回调的风格。常用接口：

```csharp

// 异步打开UI，向UI控制器的OnOpen函数传入3个参数, 分别是1,2,3
UIModule.Instance.OpenWindow("ExampleUI", 1, 2, 3)

// 异步打开UI，与普通OpenWindow不一样的是，这是拷贝一个UI进行打开。对一些重复性的UI，比如角色血条，有重要作用。
UIModule.Instance.OpenDynamicWindow("ExampleUI", "ExampleUI-Clone", 1, 2, 3)

// 同步关闭UI,
UIModule.Instance.CloseWindow("ExampleUI")
```

UIModule暴露的外部接口不多，KSFramework中建议提高UI逻辑的独立性、内聚性，减少外部的调用。

## OpenWindow

异步打开UI

## OpenDynamicWindow

异步打开UI，与普通OpenWindow不一样的是，这是拷贝一个UI进行打开。对一些重复性的UI，比如角色血条，有重要作用

## 获取并调用UI控制器内部方法

要获取一个UI并执行其内部方法，也是通过异步的方法来获取：

```csharp
UIModule.Instance.CallUI("ExampleUI", (uiController) =>{
    // 异步获取到UI控制器，带有异步加载的过程，完成后，回调方法。
    uiController.DoSomething();
});
```

当一个UI已经加载过，CallUI方法将会同步进入回调。
