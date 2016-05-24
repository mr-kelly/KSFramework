
-- global variables / functions



-- simple class extends
function extends(class, base)
    base.__index = base
    setmetatable(class, base)
end

-- simple new table to a object
function new(table, ctorFunc)
    if not table then
        assert(table ~= nil)
    end

    table.__index = table

    local tb = {}
    setmetatable(tb, table)

    if ctorFunc then
        ctorFunc(tb)
    end

    return tb
end

print("Init.lua script finish!")
