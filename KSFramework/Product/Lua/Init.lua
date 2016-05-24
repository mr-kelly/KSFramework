
-- global variables / functions



-- simple new table to a object
function new(table, ctorFunc)
    table.__index = table
    local tb = {}
    setmetatable(tb, table)

    if ctorFunc then
        ctorFunc(tb)
    end

    return tb
end

print("Init.lua script finish!")
