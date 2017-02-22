#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AppEngineInspector.cs
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
using KEngine;
using KEngine.UI;
namespace KSFramework
{
    /// <summary>
    /// KSFramework base game entry,  create your class extends from this
    /// </summary>
    public abstract partial class KSGame : MonoBehaviour, IAppEntry
    {
        /// <summary>
        /// KSGame 单例引用对象
        /// </summary>
        public static KSGame Instance { get; private set; }

        /// <summary>
        /// Module/Manager of Lua 
        /// </summary>
        public LuaModule LuaModule { get; private set; }

        /// <summary>
        /// Create Module, with new some class inside
        /// </summary>
        /// <returns></returns>
        protected virtual IList<IModuleInitable> CreateModules()
        {
            return new List<IModuleInitable>
            {
                LuaModule,
            };
        }

        /// <summary>
        /// Unity `Awake`
        /// </summary>
        protected virtual void Awake()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
            Instance = this;
            LuaModule = LuaModule.Instance;
            AppEngine.New(gameObject, this, CreateModules());
        }



        /// <summary>
        /// Before KEngine init modules
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator OnBeforeInit();

        /// <summary>
        /// After KEngine inited all module, make the game start!
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator OnGameStart();
    }
}
