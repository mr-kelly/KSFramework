#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KResourceLoaderDebuggers.cs
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

namespace KEngine
{
    /// <summary>
    /// 只在编辑器下出现，分别对应一个Loader~生成一个GameObject对象，为了方便调试！
    /// </summary>
    public class KResourceLoaderDebugger : MonoBehaviour
    {
        public AbstractResourceLoader TheLoader;
        public int RefCount;
        public float FinishUsedTime; // 参考，完成所需时间
        public static bool IsApplicationQuit = false;

        public static KResourceLoaderDebugger Create(string type, string url, AbstractResourceLoader loader)
        {
            if (IsApplicationQuit) return null;

            const string bigType = "ResourceLoaderDebuger";

            Func<string> getName = () => string.Format("{0}-{1}-{2}", type, url, loader.Desc);

            var newHelpGameObject = new GameObject(getName());
            KDebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);
            var newHelp = newHelpGameObject.AddComponent<KResourceLoaderDebugger>();
            newHelp.TheLoader = loader;

            loader.SetDescEvent += (newDesc) =>
            {
                if (loader.RefCount > 0)
                    newHelpGameObject.name = getName();
            };


            loader.DisposeEvent += () =>
            {
                if (!IsApplicationQuit)
                    KDebuggerObjectTool.RemoveFromParent(bigType, type, newHelpGameObject);
            };


            return newHelp;
        }

        private void Update()
        {
            RefCount = TheLoader.RefCount;
            FinishUsedTime = TheLoader.FinishUsedTime;
        }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

    }

}
