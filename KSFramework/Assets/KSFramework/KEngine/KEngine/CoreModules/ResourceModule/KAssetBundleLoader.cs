#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KAssetBundleLoader.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KEngine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KEngine
{
    /// <summary>
    /// 加载模式，同步或异步
    /// </summary>
    public enum LoaderMode
    {
        Async,
        Sync,
    }

    // 調用WWWLoader
    public class AssetBundleLoader : AbstractResourceLoader
    {
        public delegate void CAssetBundleLoaderDelegate(bool isOk, AssetBundle ab);

        public static Action<string> NewAssetBundleLoaderEvent;
        public static Action<AssetBundleLoader> AssetBundlerLoaderErrorEvent;

        private KWWWLoader _wwwLoader;
        private KAssetBundleParser BundleParser;
        //private bool UnloadAllAssets; // Dispose时赋值
        public AssetBundle Bundle
        {
            get { return ResultObject as AssetBundle; }
        }

        private string RelativeResourceUrl;
        private List<UnityEngine.Object> _loadedAssets;

        /// <summary>
        /// AssetBundle加载方式
        /// </summary>
        private LoaderMode _loaderMode;

        /// <summary>
        /// AssetBundle读取原字节目录
        /// </summary>
        //private KResourceInAppPathType _inAppPathType;

        public static AssetBundleLoader Load(string url, CAssetBundleLoaderDelegate callback = null,
            LoaderMode loaderMode = LoaderMode.Async)
        {

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            url = url.ToLower();
#endif
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk, obj as AssetBundle);
            }
            var newLoader = AutoNew<AssetBundleLoader>(url, newCallback, false, loaderMode);


            return newLoader;
        }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        private static bool _hasPreloadAssetBundleManifest = false;
        private static AssetBundle _mainAssetBundle;
        private static AssetBundleManifest _assetBundleManifest;
        /// <summary>
        /// Unity5下，使用manifest进行AssetBundle的加载
        /// </summary>
        static void PreLoadManifest()
        {
            if (_hasPreloadAssetBundleManifest)
                return;

            _hasPreloadAssetBundleManifest = true;
            //            var mainAssetBundlePath = string.Format("{0}/{1}/{1}", KResourceModule.BundlesDirName,KResourceModule.BuildPlatformName);
            HotBytesLoader bytesLoader = HotBytesLoader.Load(KResourceModule.BundlesPathRelative + KResourceModule.BuildPlatformName, LoaderMode.Sync);//string.Format("{0}/{1}", KResourceModule.BundlesDirName, KResourceModule.BuildPlatformName), LoaderMode.Sync);

            _mainAssetBundle = AssetBundle.LoadFromMemory(bytesLoader.Bytes);//KResourceModule.LoadSyncFromStreamingAssets(mainAssetBundlePath));
            _assetBundleManifest = _mainAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

            Debuger.Assert(_mainAssetBundle);
            Debuger.Assert(_assetBundleManifest);
        }
#endif

        protected override void Init(string url, params object[] args)
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            PreLoadManifest();
#endif

            base.Init(url);

            _loaderMode = (LoaderMode)args[0];

            if (NewAssetBundleLoaderEvent != null)
                NewAssetBundleLoaderEvent(url);

            RelativeResourceUrl = url;
            KResourceModule.LogRequest("AssetBundle", RelativeResourceUrl);
            KResourceModule.Instance.StartCoroutine(LoadAssetBundle(url));
        }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        /// <summary>
        /// 依赖的AssetBundleLoader
        /// </summary>
        private AssetBundleLoader[] _depLoaders;
#endif

        private IEnumerator LoadAssetBundle(string relativeUrl)
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            // Unity 5 Manifest中管理了依赖
            var abPath = relativeUrl.ToLower();
            var deps = _assetBundleManifest.GetAllDependencies(abPath);
            _depLoaders = new AssetBundleLoader[deps.Length];
            for (var d = 0; d < deps.Length; d++)
            {
                var dep = deps[d];
                _depLoaders[d] = AssetBundleLoader.Load(dep, null, _loaderMode);
            }
            for (var l = 0; l < _depLoaders.Length; l++)
            {
                var loader = _depLoaders[l];
                while (!loader.IsCompleted)
                {
                    yield return null;
                }
            }
#endif

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            // Unity 5 AssetBundle自动转小写
            relativeUrl = relativeUrl.ToLower();
#endif
            var bytesLoader = HotBytesLoader.Load(KResourceModule.BundlesPathRelative + relativeUrl, _loaderMode);
            while (!bytesLoader.IsCompleted)
            {
                yield return null;
            }
            if (!bytesLoader.IsSuccess)
            {
                if (AssetBundlerLoaderErrorEvent != null)
                {
                    AssetBundlerLoaderErrorEvent(this);
                }
                Log.Error("[AssetBundleLoader]Error Load Bytes AssetBundle: {0}", relativeUrl);
                OnFinish(null);
                yield break;
            }

            byte[] bundleBytes = bytesLoader.Bytes;
            Progress = 1 / 2f;
            bytesLoader.Release(); // 字节用完就释放

            BundleParser = new KAssetBundleParser(RelativeResourceUrl, bundleBytes);
            while (!BundleParser.IsFinished)
            {
                if (IsReadyDisposed) // 中途释放
                {
                    OnFinish(null);
                    yield break;
                }
                Progress = BundleParser.Progress / 2f + 1 / 2f; // 最多50%， 要算上WWWLoader的嘛
                yield return null;
            }

            Progress = 1f;
            var assetBundle = BundleParser.Bundle;
            if (assetBundle == null)
                Log.Error("WWW.assetBundle is NULL: {0}", RelativeResourceUrl);

            OnFinish(assetBundle);

            //Array.Clear(cloneBytes, 0, cloneBytes.Length);  // 手工释放内存

            //GC.Collect(0);// 手工释放内存
        }

        protected override void OnFinish(object resultObj)
        {
            if (_wwwLoader != null)
            {
                // 释放WWW加载的字节。。释放该部分内存，因为AssetBundle已经自己有缓存了
                _wwwLoader.Release();
                _wwwLoader = null;
            }
            base.OnFinish(resultObj);
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            if (BundleParser != null)
                BundleParser.Dispose(false);
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            if (_depLoaders != null && _depLoaders.Length > 0)
            {
                foreach (var depLoader in _depLoaders)
                {
                    depLoader.Release();
                }
            }

            _depLoaders = null;
#endif

            if (_loadedAssets != null)
            {
                foreach (var loadedAsset in _loadedAssets)
                {
                    Object.DestroyImmediate(loadedAsset, true);
                }
                _loadedAssets.Clear();
            }
        }

        public override void Release()
        {
            if (Application.isEditor)
            {
                if (Url.Contains("Arial"))
                {
                    Log.Error("要释放Arial字体！！错啦！！builtinextra:{0}", Url);
                    //UnityEditor.EditorApplication.isPaused = true;
                }
            }

            base.Release();
        }

        /// 舊的tips~忽略
        /// 原以为，每次都通过getter取一次assetBundle会有序列化解压问题，会慢一点，后用AddWatch调试过，发现如果把.assetBundle放到Dictionary里缓存，查询会更慢
        /// 因为，估计.assetBundle是一个纯Getter，没有做序列化问题。  （不保证.mainAsset）
        public void PushLoadedAsset(Object getAsset)
        {
            if (_loadedAssets == null)
                _loadedAssets = new List<Object>();
            _loadedAssets.Add(getAsset);
        }
    }

}
