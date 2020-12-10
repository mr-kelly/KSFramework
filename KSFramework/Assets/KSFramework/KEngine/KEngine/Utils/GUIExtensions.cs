using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KEngine;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;


/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2020/12/3 15:17
/// Desc：Extension Unity's gui
/// </summary>
public static class GUIExtensions
{
    public static void SetText(this GameObject go, string strText, bool riase_error = true)
    {
        if (!go)
            return;
        var text = go.FindChild("");
    }


    public static void SetText(this Text text, string strText, bool riase_error = true)
    {
        if (!text)
        {
            if(riase_error) Log.LogError("SetText failed ,text is null");
            return;
        }

        if (text.text != strText)
            text.text = strText;
    }
}