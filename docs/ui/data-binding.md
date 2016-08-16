
UI模块提供一种简单的、利用Cookie进行单向数据绑定的技巧 —— 当一个脚本变量的变化，会直接改变UI控件的属性。  

比如常用的UI中，角色生命值对应一个数值变量，当数值变量改变后，传统做法是额外一个刷新函数（如Refresh）来对整个界面的所有相关控件进行刷新。但如果使用了数据绑定，将变量和UI控件绑定一起后，当变量的数据改变，无需任何额外操作、且会针对单独的控件进行刷新，节省额外的不必要开销。

数据绑定技术在ASP.net、Angular JS、Cocoa Touch等等Web、UI开发中非常的普遍存在。


## 数据绑定的实现代码

因为Unity UGUI的存在，实现数据绑定，主要从我们的代码脚本着手。其本质，是设置Setter——当变量被改变时，触发连带的函数。

```Lua
-- 绑定变量TestNumber, 当数值改变时，设置UI的文字
Cookie.AddSetListener('TestNumber', function(setValue)
    uiText.text = setValue
end)
Cookie.Set('TestNumber', 567) -- 同一时间，UI改变
```
