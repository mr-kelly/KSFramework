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
using KEngine;
using UnityEngine;

namespace KEngine.UI
{

    /// <summary>
    /// Abstract class of all UI Script
    /// </summary>
    public class UIController : KBehaviour
    {
        /// <summary>
        /// Set from KUIModule, Resource Name
        /// </summary>
        public string UITemplateName = "";

        /// <summary>
        /// Set from KUIModule, InstanceName
        /// </summary>
        public string UIName = "";

        public virtual void OnInit()
        {
        }

        public virtual void BeforeOpen(object[] onOpenArgs, Action doOpen)
        {
            doOpen();
        }

        public virtual void OnOpen(params object[] args)
        {
        }

        public virtual void OnClose()
        {
        }

        /// <summary>
        /// 输入uri搜寻控件
        /// findTrans默认参数null时使用this.transform
        /// </summary>
        public T GetControl<T>(string uri, Transform findTrans = null, bool isLog = true) where T : UnityEngine.Object
        {
            return (T)GetControl(typeof(T), uri, findTrans, isLog);
        }

        public object GetControl(Type type, string uri, Transform findTrans = null, bool isLog = true)
        {
            if (findTrans == null)
                findTrans = transform;

            Transform trans = findTrans.Find(uri);
            if (trans == null)
            {
                if (isLog)
                    Log.Error("Get UI<{0}> Control Error: " + uri, this);
                return null;
            }

            if (type == typeof(GameObject))
                return trans.gameObject;

            return trans.GetComponent(type);
        }

        /// <summary>
        /// 默认在当前transfrom下根据Name查找子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T FindControl<T>(string name) where T : Component
        {
            GameObject obj = DFSFindObject(transform, name);
            if (obj == null)
            {
                Log.Error("Find UI Control Error: " + name);
                return null;
            }

            return obj.GetComponent<T>();
        }

        public GameObject FindGameObject(string name)
        {
            GameObject obj = DFSFindObject(transform, name);
            if (obj == null)
            {
                Log.Error("Find GemeObject Error: " + name);
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 从parent下根据Name查找
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject DFSFindObject(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; ++i)
            {
                Transform node = parent.GetChild(i);
                if (node.name == name)
                    return node.gameObject;

                GameObject target = DFSFindObject(node, name);
                if (target != null)
                    return target;
            }

            return null;
        }

        /// <summary>
        /// 清除一个GameObject下面所有的孩子
        /// </summary>
        /// <param name="go"></param>
        public void DestroyGameObjectChildren(GameObject go)
        {
            KTool.DestroyGameObjectChildren(go);
        }

        /// <summary>
        /// Shortcuts for UIModule's Open Window
        /// </summary>
        protected void OpenWindow(string uiName, params object[] args)
        {
            UIModule.Instance.OpenWindow(uiName, args);
        }

        /// <summary>
        /// Shortcuts for UIModule's Close Window
        /// </summary>
        /// <param name="uiName"></param>
        protected void CloseWindow(string uiName = null)
        {
            UIModule.Instance.CloseWindow(uiName ?? UIName);
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
        public static void CallUI<T>(Action<T> callback) where T : UIController
        {
            KUIModule.Instance.CallUI<T>(callback);
        }
    }

}