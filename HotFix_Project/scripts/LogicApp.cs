using System;
using System.Collections.Generic;
using KEngine;
using KEngine.UI;
using UnityEngine;

/// <summary>
/// 热更工程的入口
/// </summary>
public class LogicApp
{
    public static Action UpdateEvent;
    public static Action UpdatePer1sEvent;
    public static Action UpdatePer500msEvent;
    private static float time_per_1s;
    private static float time_per_500ms;

    private static float begin;

    public static void Start()
    {
        UIModule.Instance.OpenWindow("UIBillboard","hotfix");
        UIManager.Instance.OpenWindow<UIMain>("aabb");
        AppEngine.UpdateEvent += Update;
    }

    public static void Update()
    {
        UpdateEvent?.Invoke();
        if (Time.time >= time_per_1s)
        {
            UpdatePer1sEvent?.Invoke();
            time_per_1s = Time.time + 1.0f;
            //Log.Info("fire per1s event");
        }

        if (Time.time >= time_per_500ms)
        {
            UpdatePer500msEvent?.Invoke();
            time_per_500ms = Time.time + 0.5f;
        }
    }
}