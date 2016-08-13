
## 资源在0引用时不会立即释放

在KEngine的资源模块中，需要手工的对资源进行释放，减少引用。
但是当引用为0后，会有一个延迟时间，才进行真正的资源释放。

这是因为，实际项目开发中，清理掉所有的现有资源，可能同时重新初始化这样的一个资源，即引用为0的同一帧内，可能引用立刻又会改变。

因此，引用为0，释放资源，显然不符合项目真实情况。

事实上，Unity的GameObject.Destroy也有类似的机制，Destroy并非立刻触发的。

## 间隔时间

目前默认在编辑器模式下，间隔时间是1秒后进行资源清理；非编辑器模式，调试模式时5秒，正式环境时10秒。

```csharp

// KAbstractResourceLoader.cs文件代码

/// <summary>
/// 间隔多少秒做一次GC(在AutoNew时)
/// </summary>
public static float GcIntervalTime
{
    get
    {
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor)
            return 1f;

        return Debug.isDebugBuild ? 5f : 10f;
    }
}
```
