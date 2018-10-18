
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

Cookie = CS.KSFramework.Cookie
Log = CS.KEngine.Log
I18N = CS.KSFramework.I18N
UIModule = CS.KEngine.UI.UIModule
SceneLoader = CS.KEngine.SceneLoader
LoaderMode = CS.KEngine.LoaderMode
BillboardSettings = CS.AppSettings.BillboardSettings
GameConfigSettings = CS.AppSettings.GameConfigSettings
TestSettings = CS.AppSettings.TestSettings

UIBase = import("UI/UIBase")
Tools 		= import("Tools")
import("CSharpBinding")
print("Init.lua script finish!")
