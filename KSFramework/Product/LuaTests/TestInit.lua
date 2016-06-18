
print("LuaTests begin...")

assert(type(extends), 'function')
assert(type(foreach), 'function')
assert(type(new), 'function')

local TestBase = {}
function TestBase.BaseFunc()

end


local TestClass = {}
extends(TestClass, TestBase)
