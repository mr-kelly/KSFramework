-- global variables / functions

function import(filename)
    return CS.KSFramework.LuaModule.Instance:Import(filename)

end

-- simple class extends
function extends(class, base)
    base.__index = base
    setmetatable(class, base)
end

-- simple new table to a object
function new(table, ctorFunc)
    assert(table ~= nil)

    table.__index = table

    local tb = {}
    setmetatable(tb, table)

    if ctorFunc then
        ctorFunc(tb)
    end

    return tb
end
---NOTE xlua中访问C#代码需要写完整的namespace
---@type KSFramework.Cookie
Cookie = CS.KSFramework.Cookie
---@type KEngine.Log
Log = CS.KEngine.Log
---@type KSFramework.I18N
I18N = CS.KSFramework.I18N
---@type KEngine.UI.UIModule
UIModule = CS.KEngine.UI.UIModule
---@type KEngine.SceneLoader
SceneLoader = CS.KEngine.SceneLoader
---@type KEngine.LoaderMode
LoaderMode = CS.KEngine.LoaderMode
---@type AppSettings.BillboardSettings
BillboardSettings = CS.AppSettings.BillboardSettings
---@type AppSettings.GameConfigSettings
GameConfigSettings = CS.AppSettings.GameConfigSettings
---@type AppSettings.TestSettings
TestSettings = CS.AppSettings.TestSettings

UIBase = import("UI/UIBase")
Tools 		= import("Tools")
import("CSharpBinding")

--import lua config
Billboard = import("configs/Billboard")
print("Init.lua script finish!")
--emmylua debug
if Application.isEditor then
    package.cpath = package.cpath .. ';C:/Users/qing/AppData/Roaming/JetBrains/IdeaIC2020.1/plugins/intellij-emmylua/classes/debugger/emmy/windows/x64/?.dll'
    local dbg = require('emmy_core')
    dbg.tcpListen('localhost', 9966)
end 