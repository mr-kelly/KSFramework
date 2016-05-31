local UIBase = import("KSFramework/UIBase")

local UILogin = {}
extends(UILogin, UIBase)

-- Maybe you have many `UILogin` instance? create a new function!
-- Always write a New function is best practices
function UILogin.New(controller)
    local newUILogin = new(UILogin)
    newUILogin.Controller = controller
    print(newUILogin)
    return newUILogin
end

-- controller also pass to OnInit function
function UILogin:OnInit(controller)

    local text = self:GetUIText("Login")
    text.text = 'Login Interface !!!'

    local btn = self:GetUIButton("Button")
    print(string.format("Controller type: %s, Button type full name: %s", type(self.Controller), btn:GetType().FullName))

    if UnityEngine and  UnityEngine.Vector3 then -- static code binded!
        btn.onClick:RemoveAllListeners()
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

    for config in foreach(AppSettings.GameConfigSettings.GetAll()) do
        print(string.format("Lua Read Config, Id: %s, Value: %s", config.Id, config.Value))
    end
end

return UILogin
