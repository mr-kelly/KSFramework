#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: IUIBridge.cs
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
using KSFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

namespace KEngine.UI
{
    public class GUIBridge
    {
        public EventSystem EventSystem;
        private GameObject gameObject;

        public void InitBridge()
        {
            gameObject = new GameObject("EventSystem");
            EventSystem = gameObject.AddComponent<EventSystem>();
            var inputModule = gameObject.AddComponent<StandaloneInputModule>();
#if UNITY_4
            gameObject.AddComponent<TouchInputModule>();
#else
            inputModule.forceModuleActive = true;
#endif
        }

        /// <summary>
        /// CreateUIController
        /// </summary>
        /// <returns></returns>
        public UIController CreateUIController(GameObject uiObj, string uiTemplateName)
        {
            UIController uiBase = null;
#if xLua || SLUA
            uiBase = new LuaUIController();
#elif ILRuntime
            uiBase = new ILRuntimeUIBase();
#else
            var type = System.Type.GetType("UI" + uiTemplateName + ", Assembly-CSharp");
            uiBase = Activator.CreateInstance(type) as UIController;
#endif
            KEngine.Debuger.Assert(uiBase);
            uiBase.gameObject = uiObj;
            uiBase.transform = uiObj.transform;
            uiBase.UIName = uiBase.UITemplateName = uiTemplateName;
            return uiBase;
        }

        public IEnumerator LoadUIAsset(UILoadState loadState, UILoadRequest request)
        {
            float beginTime = Time.realtimeSinceStartup;
            string path = string.Format("ui/{0}", loadState.TemplateName);
            var loader = AssetBundleLoader.Load(path);
            while (!loader.IsCompleted)
                yield return null;
#if UNITY_EDITOR
            if (KResourceModule.IsEditorLoadAsset)
            {
                var go = GameObject.Instantiate(loader.ResultObject as GameObject);
                request.Asset = go;
                var windowAsset = go.GetComponent<UIWindowAsset>();
                if (windowAsset)
                {
                    //TODO unity2019.3.7f1 图片已设置上去了，但无法显示，勾选atlas中的Include In Build就可以正常显示，但打包AB时需要强制去掉勾选
                    windowAsset.IsUIEditor = true;
                    windowAsset.InitEvent();//监听atlasRequested事件
                    if (!string.IsNullOrEmpty(windowAsset.Atals_arr))
                    {
                        string[] arr = windowAsset.Atals_arr.Split(',');
                        int sprite_count = 0;
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string atlas_name = arr[i].ToLower();
                            if (!UIModule.Instance.CommonAtlases.Contains(atlas_name)) sprite_count++;
                        }

                        if (sprite_count > windowAsset.MAX_Atlas)
                            Log.LogError($"UI:{loadState.TemplateName}包括过多图集({windowAsset.Atals_arr})会减慢加载速度，请处理");
                    }
                }
            }
            else
#endif            
            {
                if (AppConfig.IsLogAbLoadCost)
                    Log.Info("{0} Load AB, cost:{1:0.000}s", loadState.TemplateName,Time.realtimeSinceStartup - beginTime);
                if (AppConfig.IsSaveCostToFile)
                    LogFileManager.WriteUILog(loadState.TemplateName, LogState.LoadAB,Time.realtimeSinceStartup - beginTime);
                if (loader.Bundle == null)
                {
                    yield break;
                }
                
                beginTime = Time.realtimeSinceStartup;
                var req = loader.Bundle.LoadAssetAsync<GameObject>(loadState.TemplateName);
                while (!req.isDone)
                    yield return null;
                request.Asset = GameObject.Instantiate(req.asset);

                //管理图集
                var go = req.asset as GameObject;
                if (go)
                {
                    var windowAsset = go.GetComponent<UIWindowAsset>();
                    if (windowAsset && !string.IsNullOrEmpty(windowAsset.Atals_arr))
                    {
                        string[] arr = windowAsset.Atals_arr.Split(',');
                        int sprite_count = 0;
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string atlas_name = arr[i].ToLower();
                            if (!UIModule.Instance.CommonAtlases.Contains(atlas_name)) sprite_count++;
                            var atlas = loader.Bundle.LoadAsset<SpriteAtlas>(atlas_name);
                            if (atlas != null) ABManager.SpriteAtlases[atlas_name] = atlas;
                        }

                        if (sprite_count > windowAsset.MAX_Atlas)
                            Log.LogError($"UI:{loadState.TemplateName}包括过多图集({windowAsset.Atals_arr})会减慢加载速度，请处理");
                    }
                }
            }

            loadState.UIResourceLoader = loader;
            if (AppConfig.IsLogAbLoadCost)
            {
                var cost = Time.realtimeSinceStartup - beginTime;
                Log.Info($"Load Asset from {0}, cost:{1:0.###} s", loadState.TemplateName, cost);
                LogFileManager.WriteUILog(loadState.TemplateName,LogState.LoadAsset, cost);
         
            }
        }
    }
}