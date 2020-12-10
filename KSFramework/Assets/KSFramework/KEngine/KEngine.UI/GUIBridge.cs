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

using System.Collections;
using KSFramework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KEngine.UI
{
    public class GUIBridge
    {
        /// <summary>
        /// 使用Lua编写UI代码or原生C#
        /// </summary>
        public bool IsLuaBridge = false;

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
            if (IsLuaBridge)
            {
                uiBase = uiObj.AddComponent<LuaUIController>();
            }
            else
            {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                uiBase = uiObj.AddComponent(System.Type.GetType("KUI" + uiTemplateName + ", Assembly-CSharp")) as UIController;
#else
                uiBase = uiObj.AddComponent("KUI" + uiTemplateName) as UIController;
#endif
            }

            KEngine.Debuger.Assert(uiBase);
            return uiBase;
        }

        public void UIObjectFilter(UIController controller, GameObject uiObject)
        {
        }

        public IEnumerator LoadUIAsset(UILoadState loadState, UILoadRequest request)
        {
            float beginTime = Time.realtimeSinceStartup;
            string path = string.Format("ui/{0}.prefab", loadState.TemplateName);
            var loader = AssetBundleLoader.Load(path);
            while (!loader.IsCompleted)
                yield return null;
            if (AppConfig.IsLogAbLoadCost)
                Log.Info("{0} Load AB, cost:{1:0.000}s", loadState.TemplateName, Time.realtimeSinceStartup - beginTime );
            if(AppConfig.IsSaveAbLoadCost) 
                LogFileRecorder.WriteUILog(loadState.TemplateName, LogFileRecorder.UIState.LoadAB,Time.realtimeSinceStartup - beginTime);
            if (loader.Bundle == null)
            {
                yield break;
            }

            beginTime = Time.realtimeSinceStartup;
            var req = loader.Bundle.LoadAssetAsync<GameObject>(loadState.TemplateName);
            while (!req.isDone)
                yield return null;
            request.Asset = GameObject.Instantiate(req.asset);
            loadState.UIResourceLoader = loader;
            if (AppConfig.IsLogAbLoadCost)
            {
                var cost = Time.realtimeSinceStartup - beginTime;
                Log.Info($"Load Asset from {0}, cost:{1:0.000} s", loadState.TemplateName, cost);
                LogFileRecorder.WriteUILog(loadState.TemplateName,LogFileRecorder.UIState.LoadAsset, cost);
         
            }
        }
    }
}