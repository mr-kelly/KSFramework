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
        /// 主界面类型，比如：摇杆，任务，聊天，活动/穿戴提示，主界面入口图标等界面
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
        /// tips层级，在最顶层显示，比如：系统飘字提示，系统公告/广播，停服维护等
        /// </summary>
        TipsUI,
    }
    
    #region 枚举配置定义

    /// <summary>
    /// 比如在rpg游戏中，标识当前界面是否会挡住主界面
    /// </summary>
    public enum PanelSize
    {
        /// <summary>
        /// 非全屏界面，不会挡住主界面，比如穿戴提示，活动推送，领取红包
        /// </summary>
        SmallPanel,
        /// <summary>
        /// 遮挡了主界面的80%，上下和两边的空隙可利用截屏一张图后，就可以隐藏Main Camera
        /// </summary>
        SinglePanel,
        /// <summary>
        /// 处于全屏界面，可以禁用Main Camera，减少开销
        /// </summary>
        FullScreen, 
    }
    
    /// <summary>
    /// 通用的货币栏，可配置位置和样式
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
    
    /// <summary>
    /// Mark for build UI window
    /// 界面配置文件，尽可能地把一些样式或通用配置放在UI编辑器中配置而非写在代码中，方便把界面拼接工作交接给策划和美术拼接。
    /// </summary>
    [AddComponentMenu("KEngine/KUIWindowAsset")]
    public class UIWindowAsset : MonoBehaviour
    {
        public string StringArgument;
        [HideInInspector]
        public PanelType PanelType = PanelType.NormalUI;
        /// <summary>
        /// 是否为全屏界面
        /// </summary>
        public PanelSize panelSize = PanelSize.SmallPanel;
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
        [HideInInspector] //另一种方法不显示在Inspector，把字段改成property
        public bool IsShowTabBar = false;
        [HideInInspector]
        public int TabBarId = 0;
        [Range(1,5)]
        public int MAX_Atlas = 3;
        /// <summary>
        /// 当前界面包含的图集，在导出UI时会自动赋值，不需要处理
        /// </summary>
        [HideInInspector]
        public string Atals_arr = "";

        #region Editor下预览图片

#if UNITY_EDITOR
        #region SpriteAtlas Preview In Editor
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
        #endregion
        
        [InitializeOnLoadMethod]
        static void OnUpdatePrefab()
        {
            //TODO 当直接更改Bundle目录下Prefab时发出提示，请修改UI/xx.Scene
            PrefabUtility.prefabInstanceUpdated = delegate(GameObject instance)
            {
                Debug.Log($"prefab be update {instance.name}");
            };
        }
#endif
        #endregion
        
        
    }

}