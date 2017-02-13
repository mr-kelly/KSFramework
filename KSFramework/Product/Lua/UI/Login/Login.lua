local UIBase = import("UI/UIBase")

if not Cookie then
    Cookie = Slua.GetClass('KSFramework.Cookie')
end

if not I18N then
    I18N = Slua.GetClass('KSFramework.I18N') -- use slua reflection mode
end

if not UIModule then
    UIModule = Slua.GetClass('KEngine.UI.UIModule')
end

if not Log then
    Log = Slua.GetClass('KEngine.Log')
end

local UILogin = {}
extends(UILogin, UIBase)

-- Maybe you have many `UILogin` instance? create a new function!
-- Always write a New function is best practices
function UILogin.New(controller)
    local newUILogin = new(UILogin)
    newUILogin.Controller = controller
    return newUILogin
end

-- controller also pass to OnInit function
function UILogin:OnInit(controller)

    Log.Info("================================ UILogin:OnInit ============================")

    --local text = self:GetUIText("Login")
    --text.text = I18N.Str("UILogin.LoginDescText")
    -- read LoginText from Outlet
    self.LoginText.text = I18N.Str("UILogin.LoginDescText")

    self.LoginButtonText.text = I18N.Str('UILogin.LoginButtonText')
    local btn = self.LoginButton

    print(string.format("Controller type: %s, Button type full name: %s", type(self.Controller), btn:GetType().FullName))

    if UnityEngine and  UnityEngine.Vector3 then -- static code binded!
        btn.onClick:RemoveAllListeners()
        btn.onClick:AddListener(function()
            print('Click the button!!!')
        end)
        print('Success bind button OnClick!')
    else
        Log.Warning("Not found UnityEngine static code! No AddListener to the button")
    end
	
    -- this button click to load new UI
    local btnMain = self.BtnMain
    if UnityEngine and  UnityEngine.Vector3 then -- static code binded!
        btnMain.onClick:RemoveAllListeners()
        btnMain.onClick:AddListener(function()
			UIModule.Instance:CloseWindow("Login")
			UIModule.Instance:OpenWindow("Main","user1")
        end)
        print('Success bind button OnClick!')
    else
        Long.Warning('MainButton need Slua static code.')
    end

    -- test LuaBehaivour
    if not LuaBehaviour then LuaBehaviour = Slua.GetClass('KSFramework.LuaBehaviour') end
    LuaBehaviour.Create(controller.CachedGameObject, 'Behaviour/TestLuaBehaviour')
end

function UILogin:OnOpen(num1)
    print("UILogin:OnOpen, arg1: " .. tostring(num1))

    if AppSettings then
        using("AppSettings") -- namespace all
        Log.Info("================================ Read settings throught Lua ============================")
        for config in foreach(GameConfigSettings.GetAll()) do
            print(string.format("Lua Read Setting, Id: %s, Value: %s", config.Id, config.Value))
        end
    else
        print("Not found AppSettings, maybe no static code generate yet")
    end

    local openCount
    openCount= Cookie.Get('UILogin.OpenCount')
    if not openCount then
        openCount = 0
    end
    openCount = openCount + 1
    Cookie.Set('UILogin.OpenCount', openCount)

    Log.Info('Test cookie, Reload UI use (Ctrl+Alt+Shift+R)... UI Open Count: ' .. tostring(openCount))
end

return UILogin
