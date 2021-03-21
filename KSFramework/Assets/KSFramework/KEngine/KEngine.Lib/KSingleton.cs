using System;
using System.Collections.Generic;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/3/2 17:58
/// Desc：单例类
/// </summary>
public class KSingleton<T> where T : class, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = new T();
            return _instance;
        }
    }
}