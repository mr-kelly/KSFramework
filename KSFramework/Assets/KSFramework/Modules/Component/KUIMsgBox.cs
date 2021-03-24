using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KEngine;
using KEngine.UI;
using KSFramework;
using UnityEngine.UI;


public class UIMsgBoxInfo
{
    // 窗口标题,消息内容
    public string Title, Message;

    // 按钮文字，如果没有指定则不显示按钮
    public string strOk, strCancel;

    public float TimeOut = 0f;
    /// <summary>
    /// 是否需要遮罩
    /// </summary>
    public bool AlphaMask = false;
    /// <summary>
    /// 点击遮罩关闭MsgBox
    /// </summary>
    public bool ClickMaskHide = true;

    public Action OkCallback;
    public Action CancelCallback;

    public UIMsgBoxInfo GetDefalut(string msg, string strTitle = null, string strOk = null, string strCancel = null)
    {
        var info = new UIMsgBoxInfo();
        info.Title = strTitle == null ? I18N.Get("common_title_tips") : strTitle;
        info.Message = msg;
        info.strOk = strOk == null ? I18N.Get("common_ok") : strOk;
        info.strCancel = strCancel == null ? I18N.Get("common_cancel") : strCancel;
        return info;
    }

    //隐式的用户定义类型转换
    public static implicit operator UIMsgBoxInfo(string msg)
    {
        var info = new UIMsgBoxInfo();
        info.Message = msg;
        return info;
    }
}


/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/3/4 11:21
/// Desc：消息框
/// </summary>
public class KUIMsgBox : UIController
{
    private Text labelTitle, labelMsgContent, labelCountDown;
    private Button btnOk, btnCancel, ColorQuadMask;
    private Text btnOkText, btnCancelText;

    public UIMsgBoxInfo info;

    public KUIMsgBox()
    {
        UIName = UITemplateName = "MsgBox";
    }

    public override void OnInit()
    {
        base.OnInit();
        labelTitle = gameObject.FindChild<Text>("txtTitle");
        labelMsgContent = gameObject.FindChild<Text>("txtContent");
        labelCountDown = gameObject.FindChild<Text>("txtCountDown");
        labelCountDown.SetActive(false);
        ColorQuadMask = gameObject.FindChild<Button>("EmptyImage");

        btnOk = gameObject.FindChild<Button>("btnOk");
        btnOkText = gameObject.FindChild<Text>("btnOk/Text");
        btnCancel = gameObject.FindChild<Button>("btnCancel");
        btnCancelText = gameObject.FindChild<Text>("btnCancel/Text");
        KUIClickLister.Get(btnOk, OnClick);
        KUIClickLister.Get(btnCancel, OnClick);
        KUIClickLister.Get(ColorQuadMask, OnClick);
    }

    /// <summary>
    /// 打开时，设置UI
    /// </summary>
    /// <param name="args"></param>
    public override void OnOpen(params object[] args)
    {
        base.OnOpen(args);
        if (info == null)
        {
            Log.LogError("消息框数据为空，无法显示");
            DisPlay(false);
            return;
        }

        if (info.TimeOut > 0.0f)
            KSGame.Instance.StartCoroutine(WaitForTimeout()); //开启倒计时，并且倒计时完了窗口会自动关闭
        RefreshUI(info);
    }

    private void RefreshUI(UIMsgBoxInfo info)
    {
        ColorQuadMask.SetActive(info.AlphaMask);
        labelTitle.SetText(info.Title);
        labelMsgContent.SetText(info.Message);
        if (!string.IsNullOrEmpty(info.strOk))
        {
            btnOk.SetActive(true);
            btnOkText.SetText(info.strOk);
        }
        else
        {
            btnOk.SetActive(false);
        }

        if (!string.IsNullOrEmpty(info.strCancel))
        {
            btnCancel.SetActive(true);
            btnCancelText.SetText(info.strCancel);
        }
        else
        {
            btnCancel.SetActive(false);
        }
    }


    public override void OnClose()
    {
        base.OnClose();
        KSGame.Instance.StopCoroutine(WaitForTimeout());
        labelCountDown.SetActive(false);
        info.OkCallback = null;
        info.CancelCallback = null;
        info = null;
    }

    void OnClick(MonoBehaviour behaviour)
    {
        if (behaviour == btnOk)
        {
            if (info.OkCallback != null)
            {
                info.OkCallback?.Invoke();
                info.OkCallback = null;
            }

            DisPlay(false);
        }
        else if (behaviour == btnCancel)
        {
            if (info.CancelCallback != null)
            {
                info.CancelCallback?.Invoke();
                info.CancelCallback = null;
            }

            DisPlay(false);
        }
        else if (behaviour == ColorQuadMask)
        {
            if (info.ClickMaskHide)
            {
                DisPlay(false);
            }
        }
    }

    public IEnumerator WaitForTimeout()
    {
        if (info.TimeOut <= 0.0f)
            yield break;
        var timeOut = Time.time + info.TimeOut;
        labelCountDown.SetActive(true);
        var wait = new WaitForSeconds(1);
        while (Time.time < timeOut)
        {
            labelCountDown.SetText((timeOut - Time.time).ToString());
            yield return wait;
        }

        DisPlay(false);
    }
}
