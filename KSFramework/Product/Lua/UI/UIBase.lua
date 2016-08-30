UIBase = {}
UIBase.__index = UIBase

function UIBase:GetControl(typeName, uri)
    return self.Controller:GetControl(typeName, uri)
end

function UIBase:GetUIText(uri)
    local text = self:GetControl("UnityEngine.UI.Text", uri)
    return text
end

function UIBase:GetUIButton(uri)
    local btn = self:GetControl("UnityEngine.UI.Button", uri)
    return btn
end

return UIBase
