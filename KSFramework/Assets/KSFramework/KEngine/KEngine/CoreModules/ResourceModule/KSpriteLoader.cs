#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: SpriteLoader.cs
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
    public class SpriteLoader : AbstractResourceLoader
    {
        public Sprite Asset
        {
            get { return ResultObject as Sprite; }
        }

        public delegate void CSpriteLoaderDelegate(bool isOk, Sprite tex);

        private AssetFileLoader AssetFileBridge;

        public override float Progress
        {
            get { return AssetFileBridge.Progress; }
        }

        public string Path { get; private set; }

        public static SpriteLoader Load(string path, CSpriteLoaderDelegate callback = null)
        {
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) =>
                {
//                    var t2d = obj as UnityEngine.Texture2D;
//                    var sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
//                    callback(isOk, sp as Sprite);
                    callback(isOk, obj as Sprite);
                };
            }
            return AutoNew<SpriteLoader>(path, newCallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);
            Path = url;
            AssetFileBridge = AssetFileLoader.Load(Path, OnAssetLoaded);
        }

        private void OnAssetLoaded(bool isOk, UnityEngine.Object obj)
        {
            OnFinish(obj);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            AssetFileBridge.Release(); // all, Texture is singleton!
        }
    }

}