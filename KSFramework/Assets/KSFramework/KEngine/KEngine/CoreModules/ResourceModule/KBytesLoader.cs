#region Copyright (c) Kingsoft Xishanju

// KEngine - Asset Bundle framework for Unity3D
// ===================================
// 
// Filename: KBytesLoader.cs
// Date:        2016/01/20
// Author:     Kelly
// Email:       23110388@qq.com
// Github:     https://github.com/mr-kelly/KEngine
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
using System.IO;
using KEngine;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// 读取字节，调用WWW, 会自动识别Product/Bundles/Platform目录和StreamingAssets路径
    /// </summary>
    public class KBytesLoader : AbstractResourceLoader
    {
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// 异步模式中使用了WWWLoader
        /// </summary>
        private KWWWLoader _wwwLoader;

        private LoaderMode _loaderMode;
        
        public static KBytesLoader Load(string path, LoaderMode loaderMode)
        {
            var newLoader = AutoNew<KBytesLoader>(path, null, false, loaderMode);
            return newLoader;
        }
        
        public override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            _loaderMode = (LoaderMode)args[0];
            KResourceModule.Instance.StartCoroutine(CoLoad(url));
        }
        
        private IEnumerator CoLoad(string url)
        {
            if (_loaderMode == LoaderMode.Sync)
            {
                Bytes = KResourceModule.LoadAssetsSync(url);
            }
            else
            {
                string _fullUrl;
                var getResPathType = KResourceModule.GetResourceFullPath(url, _loaderMode == LoaderMode.Async, out _fullUrl);
                if (getResPathType == KResourceModule.GetResourceFullPathType.Invalid)
                {
                    Log.Error("[HotBytesLoader]Error Path: {0}", url);
                    OnFinish(null);
                    yield break;
                }
                _wwwLoader = KWWWLoader.Load(_fullUrl);
                while (!_wwwLoader.IsCompleted)
                {
                    Progress = _wwwLoader.Progress;
                    yield return null;
                }

                if (!_wwwLoader.IsSuccess)
                {
                    Log.Error("[HotBytesLoader]Error Load WWW: {0}", url);
                    OnFinish(null);
                    yield break;
                }
#if UNITY_2018_1_OR_NEWER //TODO 换成WebRequst
                //Bytes = _wwwLoader.Www.downloadHandler.data;
                Bytes = _wwwLoader.Www.bytes;
#else
                Bytes = _wwwLoader.Www.bytes;
#endif
            }

            OnFinish(Bytes);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            if (_wwwLoader != null)
            {
                _wwwLoader.Release();
            }
        }
    }

}
