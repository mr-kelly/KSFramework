

## UI约定

在KSFramework中，每个UI界面都是一个以UI开头的场景文件，在保存场景时会自动把绑定UIWindowAsset节点下的所有内容另存为prefab保存到BundleResource目录下，所以请不要修改BundleResource下的UI Prefab，而是修改UI场景然后保存场景，就会自动更新Prefab。

每个UI对应一个同名的Lua文件，可通过 **KEngine** - **UI(UGUI)** - **Auto Make UI Lua Scripts(Current Scene)** 创建模版脚本

UI的生命周期有三个函数：**OnInit** ，**OnOpen**，**OnClose**，并没有使用Unity的MonoBehaviour来管理生命周期

## UI的打开、关闭、调用都是异步的

类似资源模块，UI模块是接口，大部分异步的， 主要采用CPS异步回调的风格。常用接口：

```csharp
// 异步打开UI，向UI控制器的OnOpen函数传入3个参数, 分别是1,2,3
UIModule.Instance.OpenWindow("ExampleUI", 1, 2, 3)

// 异步打开UI，与普通OpenWindow不一样的是，这是拷贝一个UI进行打开。对一些重复性的UI，比如角色血条，有重要作用。
UIModule.Instance.OpenDynamicWindow("ExampleUI", "ExampleUI-Clone", 1, 2, 3)

// 同步关闭UI,
UIModule.Instance.CloseWindow("ExampleUI")
```

UIModule暴露给外部接口不多，KSFramework中建议提高UI逻辑的独立性、内聚性，减少外部的调用。

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

## 禁用Canvas代替SetActive

对一个gameobject进行SetActive(true)和SetActive(false)会调用Monobehaviour的OnEnable和OnDisable函数，会造成一定的堆内存分配和耗时较高，根据多个项目的经验，在KSFramework中处理UI的显示和隐藏，我们是通过禁用和启用canvas来实现的，具体实现代码在UIModule.OnOpen

```c#
private void OnOpen(UILoadState uiState, params object[] args)
{
	uiBase.gameObject.SetActiveX(true);
	if (uiBase.Canvas)
	{
		uiBase.Canvas.enabled = true;
		if (sortOrder >= int.MaxValue)
			sortOrder = 0;

		uiBase.Canvas.sortingOrder = sortOrder++;
	}

	uiBase.OnOpen(args);
}
```

说明：SetActiveX是对SetActive的扩展，如果gameobject已经是active状态则不会重复设置