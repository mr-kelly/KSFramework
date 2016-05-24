UIBase = {}
UIBase.__index = UIBase

function UIBase:GetControl(typeName, uri)
    return self.Controller:GetControl(typeName, uri)
end

return UIBase
