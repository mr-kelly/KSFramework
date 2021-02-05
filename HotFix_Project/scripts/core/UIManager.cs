using System;
using System.Collections.Generic;
using KEngine.UI;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/2/4 17:26
/// Desc：热更层的UIManager
///         因为从ILRuntime调用主工程中的泛型方法，传过去的type为：ILRuntimeAdapter.UIControllerAdapter+Adapter
///问题：
///         如果热更中的UI类中调用了base.OnInit或其它base的方法，则在主工程中会调用两次OnInit且第二次是个null值
///         因为在主工程ILRuntimeBase中调用了base.OnInit，而热更中的代码再次调用base.OnInit就重复调用了，热更中不要调用base.xxx，也是会执行主工程中的base.xxx
///待定方案：
///        1.修改UIAdapter中的代码，只执行else中的方法
///           if (mOnInit_0.CheckShouldInvokeBase(this.instance))
///                base.OnInit();
///           else
///                mOnInit_0.Invoke(this.instance);
///       2.在主工程中的ILRuntimeBase中不调用base.xx，转而由热更中调用
/// 
/// </summary>
public class UIManager : Singleton<UIManager>
{
    public UILoadState OpenWindow<T>(params object[] args) where T : UIController
    {
        return UIModule.Instance.OpenWindow(typeof(T).Name.Remove(0, 2), args);
    }

    //隐藏时打开，打开时隐藏
    public void ToggleWindow<T>(params object[] args)
    {
        string uiName = typeof(T).Name.Remove(0, 2);
        ToggleWindow(uiName, args);
    }

    public void ToggleWindow(string uiName, params object[] args)
    {
        if (UIModule.Instance.IsOpen(uiName))
        {
            UIModule.Instance.CloseWindow(uiName);
        }
        else
        {
            UIModule.Instance.OpenWindow(uiName, args);
        }
    }

    public void CloseWindow<T>() where T : UIController
    {
        UIModule.Instance.CloseWindow(typeof(T).Name.Remove(0, 2));
    }
}