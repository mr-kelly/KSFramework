local UIBase = import("UI/UIBase")

if not Cookie then
    Cookie = CS.KSFramework.Cookie
end

if not I18N then
    I18N = CS.KSFramework.I18N -- use slua reflection mode
end

if not Log then
    Log = CS.KEngine.Log
end

local UIMain = {}
extends(UIMain, UIBase)

-- Maybe you have many `UIMain` instance? create a new function!
-- Always write a New function is best practices
function UIMain.New(controller)
    local newUINews = new(UIMain)
    newUINews.Controller = controller
    return newUINews
end

-- controller also pass to OnInit function
function UIMain:OnInit(controller)

    Log.Info("================================ UIMain:OnInit ============================")

    self.TitleText.text = ' ++ form Main.lua'

    local btnBack = self.BackBtn

    print(string.format("Controller type: %s, Button type full name: %s", type(self.Controller), btnBack:GetType().FullName))

    btnBack.onClick:RemoveAllListeners()
    btnBack.onClick:AddListener(function()
        UIModule.Instance:CloseWindow("Main")
        UIModule.Instance:OpenWindow("Login","name:user1,pwd:123")
    end)
    print('Success bind back button onClick event!')

end

function UIMain:OnOpen(num1)
    print("UIMain:OnOpen, arg1: " .. tostring(num1))

    local openCount
    openCount= Cookie.Get('UIMain.OpenCount')
    if not openCount then
        openCount = 0
    end
    openCount = openCount + 1
    Cookie.Set('UIMain.OpenCount', openCount)

    Log.Info('Test cookie, Reload UI use (Ctrl+Alt+Shift+R)... UI Open Count: ' .. tostring(openCount))
end

return UIMain
