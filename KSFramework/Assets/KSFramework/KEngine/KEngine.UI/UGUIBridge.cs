#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
//
// Filename: UGUIBridge.cs
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
using KEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KEngine.UI
{
    /// <summary>
    /// Unity UGUI Bridge
    /// </summary>
    public class UGUIBridge : IUIBridge
    {
        public EventSystem EventSystem;

        /// <summary>
        /// Init the UI Bridge, necessary
        /// </summary>
        public void InitBridge()
        {
            EventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            EventSystem.gameObject.AddComponent<StandaloneInputModule>();
#if UNITY_5 || !UNITY_2017_1_OR_NEWER
            EventSystem.gameObject.AddComponent<TouchInputModule>();
#endif
        }

        public UIController CreateUIController(GameObject uiObj, string uiTemplateName)
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            UIController uiBase = uiObj.AddComponent(System.Type.GetType("KUI" + uiTemplateName + ", Assembly-CSharp")) as UIController;
#else
            UIController uiBase = uiObj.AddComponent("KUI" + uiTemplateName) as UIController;
#endif
            KEngine.Debuger.Assert(uiBase);
            return uiBase;
        }

        /// <summary>
        /// Some stuff for the loaded GameObject and UICtroller
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="uiObject"></param>
        public void UIObjectFilter(UIController ui, GameObject uiObject)
        {
        }

        public IEnumerator LoadUIAsset(UILoadState loadState, UILoadRequest request)
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            string path = string.Format("UI/{0}.prefab", loadState.TemplateName);
#else
            string path = string.Format("UI/{0}_UI", loadState.TemplateName);
#endif
            var assetLoader = StaticAssetLoader.Load(path);
            loadState.UIResourceLoader = assetLoader; // 基本不用手工释放的
            while (!assetLoader.IsCompleted)
                yield return null;

            request.Asset = assetLoader.TheAsset;

        }
    }

}
