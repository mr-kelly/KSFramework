
local UIBase = import('UI/UIBase')
---@type UIRoleInfo
local UIRoleInfo = {}
extends(UIRoleInfo, UIBase)

-- create a ui instance
function UIRoleInfo.New(controller)
    local newUI = new(UIRoleInfo)
    newUI.Controller = controller
    return newUI
end

function UIRoleInfo:OnInit(controller)
    Tools.SetButton(self.btn_close,function()
        print("destory UIRoleInfo res")
        UIModule.Instance:CloseWindow("UIRoleInfo")
        UIModule.Instance:DestroyWindow("UIRoleInfo")
    end)
end

function UIRoleInfo:OnOpen()
   
end

return UIRoleInfo
