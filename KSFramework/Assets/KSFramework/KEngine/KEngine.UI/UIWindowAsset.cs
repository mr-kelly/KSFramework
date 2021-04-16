#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KUIWindowAsset.cs
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KEngine.UI
{
    public enum PanelType
    {
        MainUI, //main panel ui 
        NormalUI, //normal panel ui
        HeadInfoUI //hud ui
    }

    /// <summary>
    /// Mark for build UI window
    /// </summary>
    [AddComponentMenu("KEngine/KUIWindowAsset")]
    public class UIWindowAsset : MonoBehaviour
    {
        public string StringArgument;
        public PanelType PanelType = PanelType.NormalUI;
        public bool IsUIEditor = true;
        public string atals_arr = "";
        
#if UNITY_EDITOR
        public void InitEvent()
        {
            if (IsUIEditor)
            {
                SpriteAtlasManager.atlasRequested += this.RequestAtlas;
            }
        }

        void Awake()
        {
            InitEvent();
        }
        
        private void OnDestroy()
        {
            if (IsUIEditor)
            {
                SpriteAtlasManager.atlasRequested -= this.RequestAtlas;
            }
        }

        private void RequestAtlas(string tag, Action<SpriteAtlas> callback)
        {
            var findAssets = AssetDatabase.FindAssets("t:SpriteAtlas", new string[] {"Assets\\" + KEngineDef.ResourcesBuildDir});
            if (findAssets != null && findAssets.Length >= 1)
            {
                string find_path = null;
                foreach (string guid in findAssets)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.Contains(tag))
                    {
                        find_path = assetPath;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(find_path))
                {
                    var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(find_path);
                    callback?.Invoke(atlas);
                }
                else
                {
                    Log.LogError($"找不到名字为{tag}的SpriteAtlas");
                }
            }
            else
            {
                Log.LogError($"找不到名字为{tag}的SpriteAtlas");
            }
        }
#endif
    }
}