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
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AppSettings;
using KEngine;
using KEngine.UI;
using KSFramework;

public class Game : KSGame
{
    /// <summary>
    /// Add Your Custom Initable(Coroutine) Modules Here...
    /// </summary>
    /// <returns></returns>
    protected override IList<IModuleInitable> CreateModules()
    {
        var modules = base.CreateModules();

        // TIP: Add Your Custom Module here
        //modules.Add(new Module());

        return modules;
    }

    /// <summary>
    /// Before Init Modules, coroutine
    /// </summary>
    /// <returns></returns>
    public override IEnumerator OnBeforeInit()
    {
        // Do Nothing
        yield break;
    }

    /// <summary>
    /// After Init Modules, coroutine
    /// </summary>
    /// <returns></returns>
    public override IEnumerator OnGameStart()
    {

        // Print AppConfigs
        Log.Info("======================================= Read Settings from C# =================================");
        foreach (GameConfigSetting setting in GameConfigSettings.GetAll())
        {
            Debug.Log(string.Format("C# Read Setting, Key: {0}, Value: {1}", setting.Id, setting.Value));
        }

        yield return null;

        Log.Info("======================================= Open Window 'Login' =================================");
        UIModule.Instance.OpenWindow("Login", 888);

        // Test Load a scene in asset bundle
        SceneLoader.Load("Scene/Scene1001/Scene1001.unity");

        // 开始加载我们的公告界面！
        //UIModule.Instance.OpenWindow("Billboard");


        // 测试Collect函数，立即回收所有资源
        var path = "ui/billboard.prefab";
        var assetLoader = InstanceAssetLoader.Load(path); 
        while (!assetLoader.IsCompleted)             
            yield return null;         
        var assetLoader2 = InstanceAssetLoader.Load(path);       
        while (!assetLoader2.IsCompleted)             
            yield return null;         
        assetLoader2.Release();
		assetLoader.Release();

        KResourceModule.Collect();
    }

}
