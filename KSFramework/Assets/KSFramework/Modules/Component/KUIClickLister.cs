using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/2/4 11:32
/// Desc：UGUI的事件
/// </summary>
public class KUIClickLister
{
    public Action<MonoBehaviour> ClickEvent;
    private MonoBehaviour go;
    
    public static KUIClickLister Get(Button go, Action<MonoBehaviour> clickEvent)
    {
        if (go == null)
        {
            Debug.LogError("bind Event faild ! go is null");
            return null;
        }

        var lister =  new KUIClickLister();
        lister.go = go;
        lister.ClickEvent = clickEvent;
        go.onClick.RemoveListener(lister.OnClick);
        go.onClick.AddListener(lister.OnClick);
        return lister;
    }

    public  void OnClick()
    {
        if (ClickEvent != null) ClickEvent(go);
    }
}