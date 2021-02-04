using System;
using UnityEngine;
using UnityEngine.UI;
using KEngine;
using KEngine.UI;
using KSFramework;


/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/2/3 21:05
/// Desc：功能演示
public class UIBillboard : ILRuntimeUIBase
{
    public Button btn_close;

    public override void OnInit()
    {
        Log.Info($"base:{base.ToString()},UIName:{UIName}, gameObject:{gameObject}");
        btn_close = gameObject.FindChild<Button>("btn_close");
        Log.Info($"btn_close={btn_close}");
        btn_close.onClick.AddListener(OnClick);
    }

    public override void BeforeOpen(object[] onOpenArgs, Action doOpen)
    {
        
    }

    public override void OnOpen(params object[] args)
    {
        string t = args != null && args.Length > 0 ? args[0].ToString() : "null";
        Log.Info($"UIBillboard args:{t}");
    }

    public override void OnClose()
    {
        Log.Info("OnClose from hotfix");
    }


    void OnClick()
    {
        DisPlay(false);
    }
}