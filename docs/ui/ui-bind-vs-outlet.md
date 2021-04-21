## 两种方式对比

在KSFramework中，提供两种方式进行控件绑定：

   1. 在代码里写FindChild查找控件，进行绑定
   2. 在编辑UI时，拖拽控件进行绑定(LuaOutlet)

在KSFramework的demo中两种绑定方式都有，大家可以根据项目情况进行选择，本篇对这两种绑定方式进行对比。

代码绑定

- ​	 优点：无须打开UI，在代码中FindChild
- ​	缺点：FindChild路径过深会有性能消耗

拖拽绑定

- ​	优点： 1. 没有FindChild消耗  2.可视化查看引用是否丢失
- ​	缺点：手动拖拽控件

如果是从代码的可读性来讲，那么工程师喜欢使用代码手工实现控件绑定。

如果对于策划或美术同学的可视化操作来讲，那么策划师和设计师喜欢拖拽实现控件绑定。

## 手工代码绑定

技术同学在代码中完成控件绑定，而不需要在UI中手动拖拽，但注意路径不要过深，深的节点可以先找到一个父节点，再找子节点

```lua
--Path
local button = self:GetUIButton('path1/path2/button')
--实现代码在UIBase.lua
```



##  拖拽绑定控件

在Editor中编辑UI的时候拖拽绑定控件，然后在UI代码中使用self.xxx就可以访问控件了

比如：self.txtTips，就指向txtTips这个Object，类型为UnityEngine.UI.Text

```lua
self.txtTips.text = "这是给文本赋值"
```



![](../images/ui/luaoutlet.png)



### 可视化查找引用丢失

在代码中通过路径查找控件，而如果这个控件后来因于UI的结构修改而被删除了，或者美术、策划调整了UI层级结构，这个路径就找不到对应的控件，变量就变成了Null， 程序对变量的访问就会引发NullReferenceException。 如果经常出现这种问题的话，让技术同学去检查路径会浪费不必要的时间。

而outlet则提供可视化的方法，在Inspect面板会**以红色标识丢失的引用，同时会出提示同名的变量**。

![outlet-error](../images/ui/outlet-error.png)
