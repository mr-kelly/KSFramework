
UILogin = {}

function UILogin:OnInit(controller)
    self.Controller = controller

    local btn = self.Controller:GetControl("UnityEngine.UI.Button", "Button")
    print(string.format("Controller type: %s, Button type full name: %s", type(controller), btn:GetType().FullName))
end


return UILogin
