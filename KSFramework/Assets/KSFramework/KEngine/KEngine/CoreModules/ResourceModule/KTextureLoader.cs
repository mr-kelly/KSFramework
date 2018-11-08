#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: TextureLoader.cs
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

namespace KEngine
{
    public class TextureLoader : AbstractResourceLoader
    {
        public Texture Asset
        {
            get { return ResultObject as Texture; }
        }

        public delegate void CTextureLoaderDelegate(bool isOk, Texture tex);

        private AssetFileLoader AssetFileBridge;

        public override float Progress
        {
            get { return AssetFileBridge.Progress; }
        }

        public string Path { get; private set; }

        public static TextureLoader Load(string path, CTextureLoaderDelegate callback = null)
        {
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk, obj as Texture);
            }
            return AutoNew<TextureLoader>(path, newCallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            Path = url;
            AssetFileBridge = AssetFileLoader.Load(Path, OnAssetLoaded);
        }

        private void OnAssetLoaded(bool isOk, UnityEngine.Object obj)
        {
            if (!isOk)
            {
                Log.Error("[TextureLoader:OnAssetLoaded]Is not OK: {0}", this.Url);
            }

            OnFinish(obj);

            if (isOk)
            {
                var tex = Asset as Texture2D;

                string format = tex != null ? tex.format.ToString() : "";
                Desc = string.Format("{0}*{1}-{2}-{3}", Asset.width, Asset.height, Asset.width * Asset.height, format);
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            AssetFileBridge.Release(); // all, Texture is singleton!
        }
    }

}
