---
--- Created by zhaoqingqing. 569032731@qq.com
--- DateTime: 2018/12/8
---
--import("Behaviour/Hello")
json = import('rapidjson')
--import("Behaviour/Hello")
--json = require('rapidjson')

local RequireTest = {}

--table转成json
local lua_table = {
    a = "hello",
    b = "world",
    c = 123456,
    d = "123456",}
local json_str = table.table2JsonString(lua_table)
print(json_str)
print(lua_table["a"])
print(lua_table["b"])

----json转成table
local json_object = "{\"name\":\"lua\",\"other\":\"csharp,\",\"num\":100}"
local tb = table.jsonStr2Table(json_object)
print(tb["name"])

function RequireTest.Start()
    print("from RequireTest.lua")
end

return RequireTest
