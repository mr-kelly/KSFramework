#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KDebuggerObjectTool.cs
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
using System.Collections.Generic;
using KEngine;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// 专门用于资源Debugger用到的父对象自动生成
    /// DebuggerObject - 用于管理虚拟对象（只用于显示调试信息的对象）
    /// </summary>
    public class KDebuggerObjectTool
    {
        private static readonly Dictionary<string, Transform> Parents = new Dictionary<string, Transform>();
        private static readonly Dictionary<string, int> Counts = new Dictionary<string, int>(); // 数量统计...

        private static string GetUri(string bigType, string smallType)
        {
            var uri = string.Format("{0}/{1}", bigType, smallType);
            return uri;
        }

        /// <summary>
        /// 设置某个物件，在指定调试组下
        /// </summary>
        /// <param name="bigType"></param>
        /// <param name="smallType"></param>
        /// <param name="obj"></param>
        public static void SetParent(string bigType, string smallType, GameObject obj)
        {
            var uri = GetUri(bigType, smallType);
            Transform theParent = GetParent(bigType, smallType);

            int typeCount;
            if (!Counts.TryGetValue(uri, out typeCount))
            {
                Counts[uri] = 0;
            }
            typeCount = ++Counts[uri];

            try
            {
                KTool.SetChild(obj, theParent.gameObject);
            }
            catch (Exception e)
            {
                Log.Error(string.Format("[SetParent]{0}->{1}->{2}", bigType, smallType, e.Message));
            }

            theParent.gameObject.name = GetNameWithCount(smallType, typeCount);
        }

        public static void RemoveFromParent(string bigType, string smallType, GameObject obj)
        {
            if (!KBehaviour.IsApplicationQuited)
            {
                if (obj != null)
                    GameObject.Destroy(obj);

                var newCount = --Counts[GetUri(bigType, smallType)];

                var parent = GetParent(bigType, smallType);
                parent.name = GetNameWithCount(smallType, newCount);
            }
        }

        /// <summary>
        /// 设置Parent名字,带有数量
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="smallType"></param>
        /// <param name="count"></param>
        protected static string GetNameWithCount(string smallType, int count)
        {
            return string.Format("{0}({1})", smallType, count);
        }

        protected static Transform GetParent(string bigType, string smallType)
        {
            var uri = GetUri(bigType, smallType);
            Transform theParent;

            if (!Parents.TryGetValue(uri, out theParent))
            {
                var bigTypeObjName = string.Format("__{0}__", bigType);
                var bigTypeObj = GameObject.Find(bigTypeObjName) ?? new GameObject(bigTypeObjName);
                GameObject.DontDestroyOnLoad(bigTypeObj);
                bigTypeObj.transform.SetAsFirstSibling();

                theParent = new GameObject(smallType).transform;
                KTool.SetChild(theParent, bigTypeObj.transform);
                Parents[uri] = theParent;
            }
            return theParent;
        }
    }

}
