
## Unity编辑器强化

- UI编辑器，一键生成APK
- 封装Unity编辑器的各种事件，如编译前、播放前、暂停时等

## 快捷窗口

通过点击 KEngine - Open Quick Window 可以打开一个窗口，里面集成了KSFramework常用的功能

![快捷窗口](../images/advanced/quick_window.png)

### 快速重载当前打开的顶层UI脚本

在运行期间，在下拉列表中选择界面类型，会自动把选中类型中的最顶层UI名字填充到要重载的输入框内，再点击重载Lua脚本，就可以重载特定的脚本啦

![重载顶层UI](../images/advanced/reload_topui.png)

## 键盘快捷键

- Ctrl+Alt+E: 在编辑UI场景时，导出UI成AssetBundle
- Ctrl+Alt+R: 在运行时，热重载所有LuaUIController
- Ctrl+Alt+Shift+R: 在运行时，热重载所有LuaUIController，并且把所有打开状态UI关闭后重新开启
- Ctrl+Alt+I: 在编辑器，打开Game.unity主运行场景
- Ctrl+Alt+O: 在编辑器，打开Ctrl+Alt+I前的一个场景
