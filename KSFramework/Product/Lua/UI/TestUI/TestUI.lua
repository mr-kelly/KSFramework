require("Behaviour.Hello")
local UITestUI = {}
extends(UITestUI, UIBase)

-- create a ui instance
function UITestUI.New(controller)
    local newUI = new(UITestUI)
    newUI.Controller = controller
    return newUI
end

function UITestUI:OnInit(controller)
    Log.Info('UITestUI OnInit, do controls binding')
end

function UITestUI:OnOpen()
    Log.Info('UITestUI OnOpen, do your logic')
end

return UITestUI
