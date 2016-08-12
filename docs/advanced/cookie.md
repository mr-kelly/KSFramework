KSFramework中，所有的UI Lua脚本是可以重载的。脚本中的一些内存数据，在重载后会丢失，比如：

```lua

-- 记录一个UI界面被打开了多少次
local openCount = 0

function UILogin:OnOpen()
    openCount = openCount + 1
end

return UILogin
```
如上，每一次的脚本Reload，都是对openCount变量重新初始化为0，这与实际需求不符。

为此，KSFramework中引入了Cookie机制——把状态值存起来，避免被脚本重载所影响，以上代码用加入Cookie机制：

```lua
function UILogin:OnOpen()
    local openCount= Cookie.Get('UILogin.OpenCount')
    if not openCount then
        openCount = 0
    end
    openCount = openCount + 1
    Cookie.Set('UILogin.OpenCount', openCount)
end

return UILogin
```

# Cookie是什么？

cookie常见于http开发中，网站为了辨别用户身份而储存在用户本地终端上的数据，可以叫做浏览器缓存。
http是一种无状态协议，比如在用php语言开发http网站时，开发者对代码的改动只需刷新浏览器就可以立刻看到自己的改动，无需进行进程的启停操作，开发起来十分方便。这也是php语言大热的其中一个原因。

KSFramework采用Lua来进行UI开发，支持热重载来迅速修改代码；对Lua代码的热重载最重要的考虑因素就是Lua运行内存状态会丢失。

因此，KSFramework参考将HTTP领域的Cookie机制引入游戏开发，所有的本地状态值，都存放在Cookie中，逻辑与状态分离。写代码的过程即逻辑的过程，并不会影响当前的状态。

Cookie的具体实现非常的简单，它只不过是一个Hashtable，进行get/set操作，获取或设置任意的对象：
![Cookie的代码实现](http://upload-images.jianshu.io/upload_images/1835687-e2fc7337ce013701.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


以(快速入门)的随机公告为例子：每一次重载lua脚本，都会重新进行随机。 有什么办法，让这个例子中，首次加载进行随机出1~3的数字，这个数字保存到Cookie。

在我们对脚本逻辑修改后，进行LUA脚本重载，这时候从Cookie中拿回之前随机的值进行使用。

```lua
    -- 当不存在Cookie时，进行随机；存在Cookie，直接取值
    local rand = Cookie.Get('UIBillboard.RandomNumber')
    if not rand then
        rand = math.random(1,3)
        Cookie.Set('UIBillboard.RandomNumber')
    end

```

简而言之——**把状态信息保存到Cookie中，与逻辑代码分离**。

当然了，这里说的Cookie，跟HTTP的Cookie是不同的，仅仅是名称的借用，来解决类似的问题。
