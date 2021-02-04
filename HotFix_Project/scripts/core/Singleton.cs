using System;
using System.Collections.Generic;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2021/2/4 17:27
/// Desc：单例类
/// <typeparam name="T">需要单例的类</typeparam>
public class Singleton<T> where T : class, new()
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