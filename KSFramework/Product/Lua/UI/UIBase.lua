UIBase = {}
UIBase.__index = UIBase

function UIBase:FindChild(typeName, uri)
    return self.Controller:FindChild(typeName, uri)
end

function UIBase:GetUIText(uri)
    local text = self:FindChild("UnityEngine.UI.Text", uri)
    return text
end

function UIBase:GetUIButton(uri)
    local btn = self:FindChild("UnityEngine.UI.Button", uri)
    return btn
end

return UIBase
