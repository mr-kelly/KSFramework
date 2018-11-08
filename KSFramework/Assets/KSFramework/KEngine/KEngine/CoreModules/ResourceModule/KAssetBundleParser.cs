#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KAssetBundleParser.cs
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
using KEngine;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// AssetBundle字节解析器
    /// </summary>
    public class KAssetBundleParser
    {
        public enum CAssetBundleParserMode
        {
            Async,
            Sync,
        }

        /// <summary>
        /// 是异步解析，还是同步解析
        /// </summary>
        public static CAssetBundleParserMode Mode = CAssetBundleParserMode.Sync;

        private bool IsDisposed = false;
        private bool UnloadAllAssets; // Dispose时赋值

        private readonly Action<AssetBundle> Callback;
        public bool IsFinished;
        public AssetBundle Bundle;

        public static Func<string, byte[], byte[]> BundleBytesFilter = null; // 可以放置資源加密函數

        private static int _autoPriority = 1;

        private readonly AssetBundleCreateRequest CreateRequest;

        public float Progress
        {
            get { return CreateRequest.progress; }
        }

        public string RelativePath;

        private readonly float _startTime = 0;

        public KAssetBundleParser(string relativePath, byte[] bytes, Action<AssetBundle> callback = null)
        {
            if (Debug.isDebugBuild)
            {
                _startTime = Time.realtimeSinceStartup;
            }

            Callback = callback;
            RelativePath = relativePath;

            var func = BundleBytesFilter ?? DefaultParseAb;
            var abBytes = func(relativePath, bytes);
            switch (Mode)
            {
                case CAssetBundleParserMode.Async:
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                    CreateRequest = AssetBundle.LoadFromMemoryAsync(abBytes);
#else
					CreateRequest = AssetBundle.CreateFromMemory(abBytes);
#endif
                    CreateRequest.priority = _autoPriority++; // 后进先出, 一个一个来
                    KResourceModule.Instance.StartCoroutine(WaitCreateAssetBundle(CreateRequest));
                    break;
                case CAssetBundleParserMode.Sync:
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                    OnFinish(AssetBundle.LoadFromMemory(abBytes));
#else
					OnFinish(AssetBundle.CreateFromMemoryImmediate(abBytes));
#endif
                    break;
                default:
                    throw new Exception("Error CAssetBundleParserMode: " + Mode);
            }
        }

        private void OnFinish(AssetBundle bundle)
        {
            IsFinished = true;
            Bundle = bundle;

            if (IsDisposed)
                DisposeBundle();
            else
            {
                if (Callback != null)
                    Callback(Bundle);
            }

            if (Application.isEditor && Debug.isDebugBuild)
            {
                var useTime = Time.realtimeSinceStartup - _startTime;
                var timeLimit = Mode == CAssetBundleParserMode.Async ? 1f : .3f;
                if (useTime > timeLimit) // 超过一帧时间肯定了
                {
                    Log.Warning("[KAssetBundleParser] Parse Too long time: {0},  used time: {1}", RelativePath,
                        useTime);
                }
            }
        }

        private IEnumerator WaitCreateAssetBundle(AssetBundleCreateRequest req)
        {
            float startTime = Time.time;

            while (!req.isDone)
            {
                yield return null;
            }

            if (Application.isEditor)
            {
                const float timeout = 5f;
                if (Time.time - startTime > timeout)
                {
                    Log.Warning("[CAssetBundlerParser]{0} 解压/读取Asset太久了! 花了{1}秒, 超过 {2}秒", RelativePath,
                        Time.time - startTime, timeout);
                }
            }
            OnFinish(req.assetBundle);
        }


        private static byte[] DefaultParseAb(string relativePath, byte[] bytes)
        {
            return bytes;
        }

        private void DisposeBundle()
        {
            Bundle.Unload(UnloadAllAssets);
        }

        public void Dispose(bool unloadAllAssets)
        {
            UnloadAllAssets = unloadAllAssets;
            if (Bundle != null)
                DisposeBundle();
            IsDisposed = true;
        }
    }

}
