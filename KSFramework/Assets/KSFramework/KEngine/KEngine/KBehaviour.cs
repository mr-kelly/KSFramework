#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KBehaviour.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using KEngine;
using UnityEngine;

/// <summary>
/// KEngine MonoBehaivour
/// Without Update, With some cache
/// </summary>
public abstract class KBehaviour : MonoBehaviour
{
    private Transform _cachedTransform;

    public Transform CachedTransform
    {
        get { return _cachedTransform ?? (_cachedTransform = transform); }
    }

    private GameObject _cacheGameObject;

    public GameObject CachedGameObject
    {
        get { return _cacheGameObject ?? (_cacheGameObject = gameObject); }
    }

    protected bool IsDestroyed = false;
    public static event System.Action<KBehaviour> StaticDestroyEvent;
    public event System.Action<KBehaviour> DestroyEvent;

    private static bool _isApplicationQuited = false; // 全局标记, 程序是否退出状态

    public static bool IsApplicationQuited
    {
        get { return _isApplicationQuited; }
    }

    public static System.Action ApplicationQuitEvent;

    private float _TimeScale = 1f; // TODO: In Actor, Bullet,....

    public virtual float TimeScale
    {
        get { return _TimeScale; }
        set { _TimeScale = value; }
    }

    public virtual void Delete()
    {
        Delete(0);
    }

    /// <summary>
    /// GameObject.Destory对象
    /// </summary>
    public virtual void Delete(float time)
    {
        if (!IsApplicationQuited)
            UnityEngine.Object.Destroy(gameObject, time);
    }

    // 只删除自己这个Component
    public virtual void DeleteSelf()
    {
        UnityEngine.Object.Destroy(this);
    }

    // 继承CBehaivour必须通过Delete删除
    // 程序退出时会强行Destroy所有，这里做了个标记
    protected virtual void OnDestroy()
    {
        IsDestroyed = true;
        if (DestroyEvent != null)
            DestroyEvent(this);
        if (StaticDestroyEvent != null)
            StaticDestroyEvent(this);
    }

    private void OnApplicationQuit()
    {
        if (!_isApplicationQuited)
            Log.Info("OnApplicationQuit!");

        _isApplicationQuited = true;

        if (ApplicationQuitEvent != null)
            ApplicationQuitEvent();
    }
}