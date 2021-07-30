---@type UILogin
local UILogin = {}
extends(UILogin, UIBase)

-- Maybe you have many `UILogin` instance? create a new function!
-- Always write a New function is best practices
function UILogin.New(controller)
    local newUILogin = new(UILogin)
    newUILogin.Controller = controller
    return newUILogin
end

---测试场景数组
local scenes = { "Scene/Scene1001/Scene1001", "Scene/Scene1002/Scene1002" }

-- controller also pass to OnInit function
function UILogin:OnInit(controller)
    self.sceneIndex = 1
    print("================================ UILogin:OnInit ============================")

    ---多语言示例，从语言表中读取
    -- self.LoginText from LuaOutlet
    --self.LoginText.text = I18N.Str("UILogin.LoginDescText")

    Tools.SetButton(self.btnSwithScene, function()
        self.sceneIndex = self.sceneIndex == 1 and 2 or 1
        SceneLoader.Load(scenes[self.sceneIndex], function(isOK)
            if not isOK then
                return print("SceneLoader.Load faild")
            end
            print("switch scene success")
        end, LoaderMode.Async)
    end)
    Tools.SetButton(self.btnBillboard, function()
        print('Click the button!!!')
        UIModule.Instance:OpenWindow("UIBillboard")
    end)

    Tools.SetButton(self.btnSwithUI, function()
        UIModule.Instance:CloseWindow("UILogin")
        UIModule.Instance:OpenWindow("UIMain", "user1")
    end)
    Tools.SetButton(self.btnLoadSprite, function()
        UIModule.Instance:OpenWindow("UILoadUISprite")
    end)
    
end

function UILogin:OnOpen(num1)
    print("UILogin:OnOpen, arg1: " .. tostring(num1))
    UIModule.Instance:OpenWindow("UINavbar")
    UIModule.Instance:OpenWindow("UIMain", 888);
    
    if AppSettings then
        using("AppSettings") -- namespace all
        print("================================ Read settings throught Lua ============================")
        for config in foreach(GameConfigSettings.GetAll()) do
            print(string.format("Lua Read Setting, Id: %s, Value: %s", config.Id, config.Value))
        end
    else
        print("Not found AppSettings, maybe no static code generate yet")
    end

    local openCount
    openCount = Cookie.Get('UILogin.OpenCount')
    if not openCount then
        openCount = 0
    end
    openCount = openCount + 1
    Cookie.Set('UILogin.OpenCount', openCount)
    self:TestJson()
    print('Test cookie, Reload UI use (Ctrl+Alt+Shift+R)... UI Open Count: ' .. tostring(openCount))
end

function UILogin:OnClose()
    print(string.format("btnBillboard=%s ,sceneIndex=%s",self.btnBillboard,self.sceneIndex))
end

function UILogin:TestJson()
    --table转成json
    local lua_table = {
        a = "hello",
        b = "ksframework",
        c = 2020,
        d = "2020", }
    local json_str = table.table2JsonString(lua_table)
    print("lua table to json string ---->"..json_str)
    
    local t = table.jsonStr2Table("{\"name\":\"ksframework\",\"year\":2020}")
    print("json string to lua table----> table.name:"..t.name)
    t.a = 456
    
end
return UILogin
