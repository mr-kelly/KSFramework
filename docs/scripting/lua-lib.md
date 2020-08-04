为xlua集成了lua的常见的第三方库，来源自xlua作者的repo：https://github.com/chexiongsheng/build_xlua_with_libs

使用方法：在lua中使用require导入，然后使用

```lua
json = require('rapidjson')
local lua_table = {
	a = "hello",
	b = "ksframework",
	c = 2020 }
local json_str = json.encode(lua_table)
```

目前集成了这些库

## lua-protobuf

https://github.com/starwing/lua-protobuf

ps：如果需要用pbc的，可以修改编译参数指定改为用pbc，以window 64位为例，打开make_win64_lua53.bat，找到这行：

~~~bash
cmake -G "Visual Studio 14 2015 Win64" ..
~~~

修改为

~~~bash
cmake -DPBC=ON -G "Visual Studio 14 2015 Win64" ..
~~~

## LuaSocket

xLua默认集成库。

## RapidJson

json处理，特点是Rapid。

## LPeg

模式匹配库。

## FFI for lua53

基于这个项目的裁剪：https://github.com/facebookarchive/luaffifb

裁剪掉函数调用部分，这部分需要用到jit，有些系统行不同（比如ios），故裁剪掉。