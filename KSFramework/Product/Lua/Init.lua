
-- global variables / functions

function import(filename)
    return Slua.GetClass('KSFramework.LuaModule').Instance:Import(filename)

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



if not Cookie then
    Cookie = Slua.GetClass('KSFramework.Cookie')
end

if not I18N then
    I18N = Slua.GetClass('KSFramework.I18N') -- use slua reflection mode
end

if not UIModule then
    UIModule = Slua.GetClass('KEngine.UI.UIModule')
end

if not Log then
    Log = Slua.GetClass('KEngine.Log')
end

print("Init.lua script finish!")
