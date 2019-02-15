--- 常驻界面，导航条

local UIBase = import('UI/UIBase')

---@type UINavbar
local UINavbar = {}
extends(UINavbar, UIBase)

-- create a ui instance
function UINavbar.New(controller)
    local newUI = new(UINavbar)
    newUI.Controller = controller
    return newUI
end

function UINavbar:OnInit(controller)
    Tools.SetButton(self.btnBack,function()
        UIModule.Instance:CloseAllWindows()
        UIModule.Instance:OpenWindow("Login","name:user1,pwd:123")
    end)
end

function UINavbar:OnOpen()

end

return UINavbar
