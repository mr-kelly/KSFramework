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
        var text = go.GetComponent<Text>();
        if (!text) text = go.FindChild<Text>("Text");
        if (!text) text = go.FindChild<Text>("text");
        if (text)
        {
            if (text.text != strText) text.text = strText;
        }
        else
        {
            if(riase_error) Log.LogError($"SetText failed ,{go.name} not find text");
        }
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
    
    public static void SetValue(this Slider text, float value, bool riase_error = true)
    {
        if (!text)
        {
            if(riase_error) Log.LogError("SetValue failed ,go is null");
            return;
        }

        if (Math.Abs(text.value - value) > 0.00000000001f)
            text.value = value;
    }
}