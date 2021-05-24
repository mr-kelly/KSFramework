using KSFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/5/24 18:39
/// Desc：关闭就销毁的面板
/// </summary>
public class UIRoleInfo : ILRuntimeUIBase
{
    public override void OnInit()
    {
        UIClickLister.Get(gameObject.FindChild<Button>("btn_close"), OnClick);
    }

    public override void BeforeOpen(object[] onOpenArgs)
    {
    }

    public override void OnOpen(params object[] args)
    {
    }

    public override void OnClose()
    {
    }

    void OnClick(MonoBehaviour behaviour)
    {
        var id = behaviour.name.Trim();
        if (id == "btn_close")
        {
            DisPlay(false);
        }
    }
}