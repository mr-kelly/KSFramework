---
--- Created by zhaoqingqing. 569032731@qq.com
--- DateTime: 2018/12/8
---
--require("Behaviour/ItemComponent")
import("Behaviour/Hello")

local RequireTest = {}
function RequireTest.Start()
    print("from RequireTest.lua")
end
return RequireTest
