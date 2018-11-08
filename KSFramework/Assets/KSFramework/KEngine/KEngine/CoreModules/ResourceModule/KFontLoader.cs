#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: FontLoader.cs
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
using UnityEngine;

namespace KEngine
{
    public class FontLoader : AbstractResourceLoader
    {
        private AssetFileLoader _bridge;

        public override float Progress
        {
            get { return _bridge.Progress; }
        }

        public static FontLoader Load(string path, Action<bool, Font> callback = null)
        {
            LoaderDelgate realcallback = null;
            if (callback != null)
            {
                realcallback = (isOk, obj) => callback(isOk, obj as Font);
            }

            return AutoNew<FontLoader>(path, realcallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            _bridge = AssetFileLoader.Load(Url, (_isOk, _obj) => { OnFinish(_obj); });
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            _bridge.Release();
        }
    }
}