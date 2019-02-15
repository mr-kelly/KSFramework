---@type UIMain
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
    print(string.format("Controller type: %s, Button type full name: %s", type(self.Controller), self.BackBtn:GetType().FullName))

    Tools.SetButton(self.BackBtn,function()
        UIModule.Instance:CloseAllWindows()
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
