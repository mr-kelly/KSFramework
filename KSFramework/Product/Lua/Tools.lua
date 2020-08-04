json = require('rapidjson')

---@type Tools
local Tools = {}

function Tools.SetButton (button, func)
    if button and button.onClick then
        button.onClick:RemoveAllListeners()
        button.onClick:AddListener(func)
    else
        print("set click event faild ,type not button")
    end
end

---@param tb table
function table.table2JsonString(tb)
    if (not tb or type(tb) ~= "table" ) then
        warn("to json string fail,not table or nil")
        return "{}"
    end
    --local newTb = table.tosimple(tb)
    return json.encode(tb)
end

---
---@param jsonStr string
---@return table
function table.jsonStr2Table(jsonStr)
    if (not jsonStr or jsonStr == "") then
        return nil
    end
    return json.decode(jsonStr)
end

return Tools
