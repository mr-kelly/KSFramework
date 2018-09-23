#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: SettingModuleEditor.cs
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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KSFramework
{
    [RequireComponent(typeof(KEngine.UI.UIWindowAsset))]
    [DisallowMultipleComponent]
    public class UILuaOutlet : MonoBehaviour
    {
        public bool FillByObjectName = false;
        /// Outlet info, serialize
        /// </summary>
        [System.Serializable]
        public class OutletInfo
        {
            /// <summary>
            /// Lua Property Name
            /// </summary>
            public string Name;

            /// <summary>
            /// Component type 's full name (with namespace)
            /// </summary>
            public string ComponentType;

            /// <summary>
            /// UI Control Object
            /// </summary>
            public UnityEngine.Object Object;
        }
        /// <summary>
        /// Serialized outlet infos
        /// </summary>
        public List<OutletInfo> OutletInfos = new List<OutletInfo>();
    }
}
