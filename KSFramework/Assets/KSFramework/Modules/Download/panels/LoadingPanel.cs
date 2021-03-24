using System;
using System.Collections.Generic;
using KEngine;
using KEngine.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/3/4 21:09
/// Desc：loading界面，可显示真实和假进度条
/// </summary>
public class LoadingPanel : UIController
{
    private Slider Progress;
    private Text txt_center,txt_tips,txt_progress;

    public string strCenter,strTips,strProgress;
    
    public LoadingPanel()
    {
        UIName = UITemplateName = "LoadingPanel";
    }

    public override void OnInit()
    {
        Progress = gameObject.FindChild<Slider>("progress");
        txt_center = gameObject.FindChild<Text>("txt_center");
        txt_tips = gameObject.FindChild<Text>("txt_tips");
        txt_progress = gameObject.FindChild<Text>("txt_progress");
        Progress.SetValue(0.0f);
    }

    public override void OnOpen(params object[] args)
    {
        base.OnOpen(args);
        Progress.SetValue(0.0f);
        SetCenterText(strCenter);
        SetTips(strTips);
        SetProgress(strProgress);
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    
    /// <summary>
    /// 中间部分的文字
    /// </summary>
    public void SetCenterText(string str,bool hideProgress = true)
    {
        strCenter = str;
        txt_center.SetText(string.IsNullOrEmpty(str) ? "" : str);
        Progress.SetActive(hideProgress);
    }
    
    /// <summary>
    /// 进度条上方的文字
    /// </summary>
    public void SetTips(string str)
    {
        strTips = str;
        txt_tips.SetText(string.IsNullOrEmpty(str) ? "" : str);
    }
    
    /// <summary>
    /// 进度条下方的文字
    /// </summary>
    public void SetProgress(string str)
    {
        strProgress = str;
        txt_progress.SetActive(true);
        txt_progress.SetText(string.IsNullOrEmpty(str) ? "" : str);
    }
    
    /// <summary>
    /// 进度条下方的文字和进度
    /// </summary>
    public void SetProgress(string str, float progress)
    {
        strProgress = str;
        Progress.SetActive(true);
        Progress.SetValue(progress);
        txt_progress.SetText(str);
    }

    #region 假进度条，每帧走X
    
    //假的进度条，每帧固定走一段，可用在启动游戏时，比如要加载csv，解压资源等一些不确定时间的场景下
    public bool FixedProgresss;
    private float FixedAddValue = 0.05f;
    public float FixedProgresssStart;
    public float FixedProgresssEnd;
    
    public void SetFixProgress(float start, float end)
    {
        FixedProgresss = true;
        FixedAddValue = 0.1f / (float) Application.targetFrameRate;
        FixedProgresssStart = start;
        FixedProgresssEnd = end;
        UpdateFixedProgress();
    }
    
    public void UpdateFixedProgress()
    {
        if (FixedProgresss)
        {
            Progress.SetActive(true);
            txt_tips.SetText(strTips);
            if ((double) Progress.value < (double) FixedProgresssStart)
            {
                Progress.SetValue(Progress.value + FixedAddValue * 10f);
            }
            else
            {
                if ((double) Progress.value >= (double) FixedProgresssEnd)
                    return;
                Progress.SetValue(Progress.value + FixedAddValue);
            }
        }
    }
    #endregion
}