
-- global variables / functions

function import(filename)
    return CS.KSFramework.LuaModule.Instance:Import(filename)

end



-- simple class extends
function extends(class, base)
    base.__index = base
    setmetatable(class, base)
end

-- foreach C# ienumerable
function foreach(csharp_ienumerable)
    return Slua.iter(csharp_ienumerable)
end

-- simple new table to a object
function new(table, ctorFunc)
    assert(table ~= nil)

    table.__index = table

    local tb = {}
    setmetatable(tb, table)

    if ctorFunc then
        ctorFunc(tb)
    end

    return tb
end

print("Init.lua script finish!")
