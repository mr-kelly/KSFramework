#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: UIController.cs
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

using System;
using System.Reflection;
using KEngine;
using UnityEngine;

namespace KEngine.UI
{

    /// <summary>
    /// Abstract class of all UI Script
    ///     如果需要FindChild请使用FindEx中的扩展方法，可以忽略节点层级且性能好
    ///     NOTE: in xlua' lua script can't call C# [Obsolete] method
    /// </summary>
    public class UIController 
    {
        // TODO 默认情况下脚本和UI资源名是一样的，后续支持：多个脚本对应同一个UI界面资源

        /// <summary>
        /// Set from KUIModule, Resource Name
        /// </summary>
        public string UITemplateName;

        /// <summary>
        /// Set from KUIModule, InstanceName
        /// </summary>
        public string UIName;


        public GameObject gameObject;//NOTE 无法为属性，在ILRuntime中get失败，而字段则可以
        public Transform transform;
        private Canvas _canvas;
        /// <summary>
        /// 除HUD外，每个界面都有一个Canvas
        /// </summary>
        public Canvas Canvas
        {
            get
            {
                if (_canvas == null && gameObject)
                {
                    _canvas = gameObject.GetComponent<Canvas>();
                }

                return _canvas;
            }
        }
        private UIWindowAsset _windowAsset;
        /// <summary>
        /// 除HUD外，每个界面都有一个UIWindowAsset
        /// </summary>
        public UIWindowAsset WindowAsset
        {
            get
            {
                if (_windowAsset == null && gameObject)
                {
                    _windowAsset = gameObject.GetComponent<UIWindowAsset>();
                }

                return _windowAsset;
            }
        }
        private bool isVisiable;
        /// <summary>
        /// 是否显示，对于一些外部调用，如果未显示，则不调用
        /// </summary>
        public bool IsVisiable
        {
            get { return isVisiable; }
            set { isVisiable = value; }
        }
        /// <summary>
        /// 放在主工程，不热更的UI，目前有特殊处理
        /// </summary>
        public bool IsGameBaseUI = false;
        
        public virtual void OnInit()
        {
        }

        public virtual void BeforeOpen(object[] onOpenArgs)
        {
            
        }

        public virtual void OnOpen(params object[] args)
        {
            IsVisiable = true;
        }

        public virtual void OnClose()
        {
            IsVisiable = false;
        }
        
        /// <summary>
        /// 从数组获取参数，并且不报错，返回null, 一般用于OnOpen, OnClose的可变参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="openArgs"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected T GetFromArgs<T>(object[] openArgs, int offset, bool isLog = true)
        {
            return openArgs.Get<T>(offset, isLog);
        }

        [Obsolete("Use CallUI(stringName) insted!")]
        public void CallUI<T>(Action<T> callback) where T : UIController
        {
            UIModule.Instance.CallUI<T>(callback);
        }

        public virtual void OnDestroy()
        {
            ClearHeapValues();
            if (this.IsGameBaseUI) UIModule.Instance.dict.Remove(this.GetType());
        }

        /// <summary>
        /// 释放堆区的成员变量，清空UnityEngine.Gameobject的引用
        /// </summary>
        void ClearHeapValues()
        {
            var type = this.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.FieldType == typeof(GameObject) || field.FieldType.IsSubclassOf(typeof(Component)))
                {
                    //field.GetCustomAttributes("")//可添加自定义属性不清空
                    field.SetValue(this,null);    
                }
            }
        }

        public virtual void DisPlay(bool visiable)
        {
            if (IsGameBaseUI)
            {
                if (visiable)
                {
                    gameObject.SetActiveX(true);
                    UIModule.Instance.SetUIOrder(this);
                    OnOpen();
                }
                else
                {
                    Canvas.enabled = false;
                    OnClose();
                }
            }
            else
            {
                if (visiable)
                {
                    UIModule.Instance.OpenWindow(UIName);
                }
                else
                {
                    UIModule.Instance.CloseWindow(UIName);
                }
            }
        }
        
        public T FindChild<T>(string uri)
        {
            return FindChild<T>( uri, null, true);
        }
        
        public T FindChild<T>(string uri, Transform findTrans)
        {
            return FindChild<T>( uri, findTrans);
        }
        
        public T FindChild<T>( string uri, Transform findTrans, bool raise_error)
        {
            if (findTrans == null)
                findTrans = transform;

            Transform trans = findTrans.FindChildX(uri,false,raise_error);
            if (trans == null)
            {
                return default(T);
            }

            return trans.GetComponent<T>();
        }
    }

}