local UIBase = import("UI/UIBase")
--if not Cookie then
--    Cookie = Slua.GetClass('KSFramework.Cookie')
--end

local UIBillboard = {}
extends(UIBillboard, UIBase)

function UIBillboard:OnInit(controller)
    self.Controller = controller
    self.TitleLabel = self:GetUIText('Title')
    self.ContentLabel = self:GetUIText('Content')
end

function UIBillboard:OnOpen()
    local rand
    --rand = Cookie.Get('UIBillboard.RandomNumber')
    --if not rand then
        rand = math.random(1,3)
    --    Cookie.Set('UIBillboard.RandomNumber', rand)
    --end
    local billboardSetting = AppSettings.BillboardSettings.Get('Billboard' .. tostring(rand))
    -- self.TitleLabel.text = "This is a title"
    -- self.ContentLabel.text = "Here is content!"
    self.TitleLabel.text = billboardSetting.Title
    self.ContentLabel.text = billboardSetting.Content
end

return UIBillboard
