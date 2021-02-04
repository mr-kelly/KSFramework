using System;
using KEngine;
using KEngine.UI;
using KSFramework;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/2/4 9:30
/// Desc：功能页
/// </summary>
public class UILogin : ILRuntimeUIBase
{
    string[] scenes = new string[] {"Scene/Scene1001/Scene1001", "Scene/Scene1002/Scene1002"};
    private int sceneIndex = 0;

    public override void OnInit()
    {
        UIClickLister.Get(gameObject.FindChild<Button>("btnBillboard"), OnClick);
        UIClickLister.Get(gameObject.FindChild<Button>("btnSwithScene"), OnClick);
        UIClickLister.Get(gameObject.FindChild<Button>("btnSwithUI"), OnClick);
    }

    public override void BeforeOpen(object[] onOpenArgs, Action doOpen)
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
        //TODO 理想的事件处理：不需要FindChild，重定向当前界面的点击事件到这个函数
        var id = behaviour.name.Trim();
        if (id == "btnBillboard")
        {
            UIManager.Instance.OpenWindow<UIBillboard>();
        }
        else if (id == "btnSwithScene")
        {
            sceneIndex = sceneIndex > scenes.Length - 1 ? 0 : sceneIndex;
            SceneLoader.Load(scenes[sceneIndex], (isOK) =>
            {
                sceneIndex++;
                if (isOK) Log.Info("load scene success.");
            }, LoaderMode.Async);
        }
        else if (id == "btnSwithUI")
        {
            UIManager.Instance.OpenWindow<UIMain>("user1");
        }
    }
}