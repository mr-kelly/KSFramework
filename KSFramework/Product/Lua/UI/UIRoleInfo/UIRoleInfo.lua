
local UIBase = import('UI/UIBase')
---@type UIRoleInfo
local UIRoleInfo = {}
extends(UIRoleInfo, UIBase)
local atlasLoader
-- create a ui instance
function UIRoleInfo.New(controller)
    local newUI = new(UIRoleInfo)
    newUI.Controller = controller
    return newUI
end

function UIRoleInfo:OnInit(controller)
    Tools.SetButton(self.btn_close,function()
        print("destory UIRoleInfo res")
        UIModule.Instance:CloseWindow("UIRoleInfo")
        UIModule.Instance:DestroyWindow("UIRoleInfo")
    end)
    Tools.SetButton(self.btn_img,function()
        print("change sprite from atlas")
        atlasLoader = KSpriteAtlasLoader.Load("uiatlas/atlas_common", "btn_win_close", function(isOk, loadSprite)
            if (isOk) then
                self.img.sprite = loadSprite;
                self.img:SetNativeSize();
                print("图片从图集加载完成");
            end
        end);
    end)
end

function UIRoleInfo:OnOpen()
   
end

function UIRoleInfo:OnClose()
    print("release loader")
    if atlasLoader then
        atlasLoader:Release(true);
    end
end

return UIRoleInfo
