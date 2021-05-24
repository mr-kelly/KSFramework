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
        /// <summary>
        /// 主界面，一般像摇杆，任务，聊天，活动提示等界面
        /// </summary>
        MainUI, 
        /// <summary>
        /// 最常用的类型，绝大多数界面都属于此层级
        /// </summary>
        NormalUI, 
        /// <summary>
        /// 仅仅给头顶文字使用
        /// </summary>
        HeadInfoUI,
        /// <summary>
        /// tips 在最顶层显示，像系统飘字提示，系统公告/广播，停服维护等
        /// </summary>
        TipsUI,
    }
    
    /// <summary>
    /// Mark for build UI window
    /// 可扩展更多UI的配置项，供UI编辑器
    /// </summary>
    [AddComponentMenu("KEngine/KUIWindowAsset")]
    public class UIWindowAsset : MonoBehaviour
    {
        public string StringArgument;
        public PanelType PanelType = PanelType.NormalUI;
        /// <summary>
        /// 是否为全屏界面
        /// </summary>
        public PanelSizeType SizeType = PanelSizeType.Ignore;
        public bool IsUIEditor = true;
        /// <summary>
        /// 切换场景时是否关闭当前界面
        /// </summary>
        public bool IsHidenWhenLeaveScene = true;
        /// <summary>
        /// 当前界面是否显示货币栏，只有NormalUI有效
        /// </summary>
        public MoneyBarType MoneyBar = MoneyBarType.InRight;
        /// <summary>
        /// 当前界面是否有左侧切页栏，只有NormalUI有效
        /// </summary>
        public bool IsShowTabBar = true;
        public int TabBarId = 0;
        /// <summary>
        /// 当前界面包含的图集，在导出UI时会自动赋值
        /// </summary>
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

    #region 枚举定义

    /// <summary>
    /// 比如在mmo中，常用来判断当前是否停在主界面，也就是没有打开全屏界面
    /// </summary>
    public enum PanelSizeType
    {
        Ignore,
        FullScreen, 
    }
    
    /// <summary>
    /// 货币栏类型
    /// </summary>
    public enum MoneyBarType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// 货币栏在左侧
        /// </summary>
        InLeft, 
        /// <summary>
        /// 货币栏在右侧
        /// </summary>
        InRight, 
    }
    #endregion
}