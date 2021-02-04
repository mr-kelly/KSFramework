using System;
using System.Collections.Generic;
using KEngine.UI;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/2/4 17:26
/// Desc：热更层的UIManager
///         因为从ILRuntime调用主工程中的泛型方法，传过去的type为：ILRuntimeAdapter.UIControllerAdapter+Adapter
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