+++
weight = 10
date = "2016-08-09T09:12:09+08:00"
draft = false
title = "快速入门"

+++

[https://github.com/mr-kelly/KSFramework](https://github.com/mr-kelly/KSFramework)

KSFramework是一个整合KEngine、SLua和一些开发组件组成的全功能Unity 5开发框架，适合有一定规模的团队使用。

热重载是KSFramework的开发重点——在不重启游戏的前提下，重载代码、配置表可立刻看到修改效果，最大限度的提升开发、调试的速度，并且在运营阶段方便的进行产品热更新。


## 看看Demo！

双击打开Assets/Game.unity场景，点击播放。

![开始Game.unity后的日志输出](http://upload-images.jianshu.io/upload_images/1835687-b5116d08675b5b0d.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

这时候KSFramework的默认Demo开始，做了这些事情：

- 基础模块的启动
- Lua模块的启动
- 尝试读取并打印Excel表格GameConfig.xlsx内容
- 加载UI界面Login
- 执行UI界面Login的Lua脚本
- Lua脚本绑定UI控件、设置UI控件
- Lua脚本读取并打印Excel表格GameConfig.xlsx内容

总而言之，这个Demo囊括了KSFramework中的几个核心模块的使用：

- KEngine中的Setting模块的使用
- KEngine中的UI的资源加载
- SLua脚本引擎与UI的联合

接下来，将模仿这个Demo，创建/加载一个新的UI界面、创建/读取一个新的配置表格。

## 尝试做一个公告界面Billboard

接下来，我们将创建一个UI公告牌（Billboard），使用Lua脚本，并从配置表读取公告内容。

### 创建UI资源
![创建New Scene](http://upload-images.jianshu.io/upload_images/1835687-78c33d2dbb2d34fb.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![KEngine-UI-Create UI创建UI布局](http://upload-images.jianshu.io/upload_images/1835687-25dffd06037757b8.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![点击Create UI后，默认随机名字，把UI名字修改为Billboard](http://upload-images.jianshu.io/upload_images/1835687-07a7a61c381e89b4.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![修改UI名字为Billboard，UI界面右边带有黄色UI标识](http://upload-images.jianshu.io/upload_images/1835687-b3737213af89a494.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![编辑一下UI场景，一个背景Image，两个Label](http://upload-images.jianshu.io/upload_images/1835687-25819cee7bfd0b98.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

![保存一下场景，保存到Assets/BundleEditing/UI/Billboard.unity](http://upload-images.jianshu.io/upload_images/1835687-6d4dae99358884d1.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![导出——打包AssetBundle，快捷键Ctrl+Alt+E](http://upload-images.jianshu.io/upload_images/1835687-ae25ea2b174732cd.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

### 加载UI界面

好了，Billboard界面创建好了，也导出成了AssetBundle。
接下来，我们通过代码打开界面。

![编辑Assets/Code/Game.cs](http://upload-images.jianshu.io/upload_images/1835687-f6ff1f85b4c72c0e.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

在OnFinishInitModules函数的末端，加上这样的一句：
```csharp
// 开始加载我们的公告界面！
UIModule.Instance.OpenWindow("Billboard");
```

完成。 打开场景Assets/Game.unity，点击播放按钮：

![我们的UI通过AssetBundle打开了，弹出提示找不到UI Lua脚本，接下来我们创建Lua脚本吧](http://upload-images.jianshu.io/upload_images/1835687-e7f38d6b85378bc3.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

[我们的UI通过AssetBundle打开了，弹出提示找不到UI Lua脚本，接下来我们创建Lua脚本吧。

### 创建Lua脚本


![在目录Product/Lua/UI中新建一个lua文件](http://upload-images.jianshu.io/upload_images/1835687-e2a07506af3ba947.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![写一段Lua代码：UIBillboard的执行逻辑](http://upload-images.jianshu.io/upload_images/1835687-fb72415afd39a73a.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

```lua
local UIBase = import("KSFramework/UIBase")

local UIBillboard = {}
extends(UIBillboard, UIBase)

function UIBillboard:OnInit(controller)
    self.Controller = controller
    self.TitleLabel = self:GetUIText('Title')
    self.ContentLabel = self:GetUIText('Content')
end

function UIBillboard:OnOpen()
    self.TitleLabel.text = "This is a title"
    self.ContentLabel.text = "Here is content!"
end

return UIBillboard
```
这段lua中，创建了一个Table叫UIBillboard，这个table必须有OnInit(controller)函数。它通过代码设置了UI中的文字。

好了，接下来，我们要为策划准备配置表了。

### 创建配置表

打开Product/SettingSource目录，复制一份StringsTable.xlsx，并改名叫Billboard.xlsx吧

![复制一份StringsTable.xlsx](http://upload-images.jianshu.io/upload_images/1835687-b854b326a8046966.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

用Excel打开我们新的Billboard.xlsx，编辑我们的公告。我们大概定一下需求，我们假设写入3条公告，每次打开公告随机显示其中一条。每个公告，我们给它一个英文ID，一列中文标题，一列中文内容。

Excel表修改如下：

![增加公告内容](http://upload-images.jianshu.io/upload_images/1835687-78cbcf4e26f0da94.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![回到Unity，监测到Excel变动。点击OK。](http://upload-images.jianshu.io/upload_images/1835687-037c1801a8f40556.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![上一步监测到变动，只编译Excel表，手动执行一些重新编译，并生成配置表代码](http://upload-images.jianshu.io/upload_images/1835687-d83b8167e76faddd.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

![这时候，打开AppSettings.cs代码文件，我们可以发现，已经生成名叫BillboardSettings的类了](http://upload-images.jianshu.io/upload_images/1835687-b254b8c1f1b64224.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![因为我们要在Lua使用BillboardSettings读取配置表，这里需要重新生成一下SLua的静态代码](http://upload-images.jianshu.io/upload_images/1835687-a9bdf5d0c5c410db.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![接下来修改Lua代码，随机读取一条公告，并设置Content、Title](http://upload-images.jianshu.io/upload_images/1835687-1bb462980f923226.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![运行播放Game.unity，我们的公告界面完成了](http://upload-images.jianshu.io/upload_images/1835687-9603f6d7c2f1b679.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

公告界面完成了。我们创建了一个UI、写了C#和Lua代码加载它、然后创建了一张配置表，再从Lua读取配置表，设置UI的显示。

## 玩玩热重载

### 热重载Lua
接着我们刚才运行的Game.unity。 我们尝试一下对Lua热重载：在不重启游戏的情况，快速重载Lua代码，方便程序调试。


![菜单KSFramework->UI->Reload+ReOpen：热重载Lua脚本，并重新打开目前处在打开状态的UI界面](http://upload-images.jianshu.io/upload_images/1835687-efd4bb9bf9496776.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

我们可以从菜单执行热重载并重新打开UI界面，或者使用快捷键Ctrl+Alt+Shift+R。
由于我们的Lua脚本中，每次执行随机获取一条公告。因此可以看到公告内容在不停的变动着。

热重载的实现，主要依赖于每个lua脚本的最后一行——return Table；C#层在执行完Lua脚本后，会把这个返回的Table缓存起来，每次需要使用table时，直接从缓存拿；而热重载，实际就是把这个缓存table删掉，这样当需要用到这个table时，就会重新执行Lua文件，来获取Table，这样就达到了热重载得目的。

### 热重载Excel表格

我们保持运行刚刚的Game.unity，不要停止掉。这时候我们去修改Excel表格。

![修改Excel表格](http://upload-images.jianshu.io/upload_images/1835687-a5aa9b523ad7c4b6.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

保存后，回到Unity，提示表格有改动。

![发现表格有变动，点击OK编译表](http://upload-images.jianshu.io/upload_images/1835687-790e24f195b85f44.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![从菜单中心一下重载配置表格吧](http://upload-images.jianshu.io/upload_images/1835687-3e3bf9af96089576.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![Ctrl+Alt+Shift+R刷新Lua](http://upload-images.jianshu.io/upload_images/1835687-2622e03d3d1c7ea9.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

重载Lua，我们的新修改的配置表内容生效了。

至此，我们的Lua和配置表的改动，都可以在不重启、不重新转菊花的情况下快速修改。



---------------------------


[KEngine策划指南：配置表格的编辑与编译](http://www.jianshu.com/p/ead1a148b504)

[KEngine资源的打包、加载、调试监控](http://www.jianshu.com/p/ce3b5d0bdf8c)
