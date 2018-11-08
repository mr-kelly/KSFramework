#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AudioLoader.cs
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

using UnityEngine;

namespace KEngine
{
    public class AudioLoader : AbstractResourceLoader
    {
        private AudioClip ResultAudioClip
        {
            get { return ResultObject as AudioClip; }
        }

        private AssetFileLoader AssetFileBridge;

        public override float Progress
        {
            get { return AssetFileBridge.Progress; }
        }

        public static AudioLoader Load(string url, System.Action<bool, AudioClip> callback = null)
        {
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk, obj as AudioClip);
            }
            return AutoNew<AudioLoader>(url, newCallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            AssetFileBridge = AssetFileLoader.Load(url, (bool isOk, UnityEngine.Object obj) => { OnFinish(obj); });
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            AssetFileBridge.Release();
        }
    }

}