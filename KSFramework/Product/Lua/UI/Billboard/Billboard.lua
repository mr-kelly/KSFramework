---@type UIBillboard
local UIBillboard = {}
extends(UIBillboard, UIBase)

function UIBillboard:OnInit(controller)
    self.Controller = controller
    if not self.txtTitle then
        self.txtTitle = self:GetUIText('txtTitle')
    end
    if not self.txtContent then
        self.txtContent = self:GetUIText('txtContent')
    end
    Tools.SetButton(self.btnBack, function()
        print('Click the button!!!')
        UIModule.Instance:CloseWindow("Billboard")
        UIModule.Instance:OpenWindow("Login")
    end)
end

function UIBillboard:OnOpen()
    local rand
    --rand = Cookie.Get('UIBillboard.RandomNumber')
    --if not rand then
    rand = math.random(1, 3)
    --    Cookie.Set('UIBillboard.RandomNumber', rand)
    --end
    local billboardSetting = BillboardSettings.Get('Billboard' .. tostring(rand))
    self.txtTitle.text = billboardSetting.Title
    self.txtContent.text = billboardSetting.Content
end

return UIBillboard
