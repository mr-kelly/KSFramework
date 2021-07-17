---@type UILoadUISprite
local UILoadUISprite = {}
extends(UILoadUISprite, UIBase)

-- create a ui instance
function UILoadUISprite.New(controller)
    local newUI = new(UILoadUISprite)
    newUI.Controller = controller
    return newUI
end

function UILoadUISprite:OnInit(controller)
    Log.Info('UILoadUISprite OnInit, do controls binding')
end

function UILoadUISprite:OnOpen()
    Log.Info('UILoadUISprite OnOpen, do your logic')
end

return UILoadUISprite
