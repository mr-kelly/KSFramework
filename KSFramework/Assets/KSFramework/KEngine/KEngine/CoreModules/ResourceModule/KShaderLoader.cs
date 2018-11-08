#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KShaderLoader.cs
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

using System.Collections;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// Shader加载器
    /// </summary>
    public class ShaderLoader : AbstractResourceLoader
    {
        public delegate void ShaderLoaderDelegate(bool isOk, Shader shader);

        public Shader ShaderAsset
        {
            get { return ResultObject as Shader; }
        }

        public static ShaderLoader Load(string path, ShaderLoaderDelegate callback = null)
        {
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk, obj as Shader);
            }
            return AutoNew<ShaderLoader>(path, newCallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);
            KResourceModule.Instance.StartCoroutine(CoLoadShader());
        }

        private IEnumerator CoLoadShader()
        {
            var loader = AssetBundleLoader.Load(Url);
            while (!loader.IsCompleted)
            {
                Progress = loader.Progress;
                yield return null;
            }

            var shader = loader.Bundle.mainAsset as Shader;
            Debuger.Assert(shader);

            Desc = shader.name;

            if (Application.isEditor)
                KResoourceLoadedAssetDebugger.Create("Shader", Url, shader);

            loader.Release();

            OnFinish(shader);
        }


        protected override void DoDispose()
        {
            base.DoDispose();

            GameObject.Destroy(ShaderAsset);
        }
    }
}