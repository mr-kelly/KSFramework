local UIBase = import("UI/UIBase")

UILogin = {}
setmetatable(UILogin, UIBase)

-- Maybe you have many `UILogin` instance? create a new function!
-- Always write a New function is best practices
function UILogin.New(controller)
    return new(UILogin, function(tb)
        tb.Controller = controller
    end)
end

-- controller also pass to OnInit function
function UILogin:OnInit(controller)

    local text = self:GetControl("UnityEngine.UI.Text", "Login")
    text.text = 'Login Interface !!!'

    local btn = self:GetControl("UnityEngine.UI.Button", "Button")
    print(string.format("Controller type: %s, Button type full name: %s", type(self.Controller), btn:GetType().FullName))

    if UnityEngine and  UnityEngine.Vector3 then -- static code binded!
        btn.onClick:AddListener(function()
            print('Click the button!!!')
        end)
        print('Success bind button OnClick!')
    else
        print("Not found UnityEngine static code! No AddListener to the button")

    end
end


function UILogin:OnOpen(num1)
    print("UILogin:OnOpen, arg1: " .. tostring(num1))
end

return UILogin
