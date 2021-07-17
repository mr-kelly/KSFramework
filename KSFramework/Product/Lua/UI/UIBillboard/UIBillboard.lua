
---@type UIBillboard
local UIBillboard = {}

extends(UIBillboard, UIBase)

function UIBillboard:OnInit(controller)
    print("================================ UIBillboard:OnInit ============================")
    self.Controller = controller
    if not self.txtTitle then
        self.txtTitle = self:GetUIText('txtTitle')
    end
    if not self.txtContent then
        self.txtContent = self:GetUIText('txtContent')
    end
    Tools.SetButton(self.btn_close, function()
        print('Click the button!!!')
        UIModule.Instance:CloseWindow("UIBillboard")
        --UIModule.Instance:OpenWindow("Login")
        --尝试从Billboard.lua中读取配置数据
        ---@type Billboard
        local config = Billboard["Billboard1"]
        print(config.Id)
    end)
end

function UIBillboard:OnOpen()
    print("================================ UIBillboard:OnOpen ============================")
    local rand
    --rand = Cookie.Get('UIBillboard.RandomNumber')
    --if not rand then
    rand = math.random(1, 3)
    --    Cookie.Set('UIBillboard.RandomNumber', rand)
    --end
    local billboardSetting = BillboardSettings.Get('Billboard' .. tostring(rand))
    ---@type Billboard
    --local billboardSetting = Billboard['Billboard1']
    self.txtTitle.text = billboardSetting.Title
    self.txtContent.text = billboardSetting.Content

    --尝试从Billboard.lua中读取配置数据
    print("from lua config ,id:",billboardSetting.Id)
end

function UIBillboard:OnClose()
    print("================================ UIBillboard:OnClose ============================")
end
return UIBillboard
