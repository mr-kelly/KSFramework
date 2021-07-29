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
using UnityEngine.U2D;

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
#if xLua || SLUA
        modules.Add(LuaModule.Instance);
#elif ILRuntime
        modules.Add(ILRuntimeModule.Instance);
#endif
        return modules;
    }

    /// <summary>
    /// Before Init Modules, coroutine
    /// </summary>
    /// <returns></returns>
    public override IEnumerator OnBeforeInit()
    {
        var loader = AssetBundleLoader.Load($"uiatlas/{UIModule.Instance.CommonAtlases[0]}", (isOk, ab) =>
        {
            if (isOk && ab)
            {
                var atlas = ab.LoadAsset<SpriteAtlas>("atlas_common");
                ABManager.SpriteAtlases["atlas_common"] = atlas;
            }
        });
        while (!loader.IsCompleted)
        {
            yield return null;
        }
        if (AppConfig.IsDownloadRes)
        {
            yield return  StartCoroutine(DownloadManager.Instance.CheckDownload());
            if (DownloadManager.Instance.ErrorType != UpdateErrorType.None)
            {
                UIMsgBoxInfo info = new UIMsgBoxInfo().GetDefalut(I18N.Get("download_error", DownloadManager.Instance.ErrorType), null,I18N.Get("common_ignore"), I18N.Get("common_exit"));
                info.OkCallback = () => { DownloadManager.Instance.DownloadFinish = true;};
                info.CancelCallback = KTool.ExitGame;
                var panel = UIModule.Instance.GetOrCreateUI<KUIMsgBox>();
                panel.info = info;
                panel.DisPlay(true);
            }
        }
        else
        {
            DownloadManager.Instance.DownloadFinish = true;
        }
    }

    /// <summary>
    /// After Init Modules, coroutine
    /// </summary>
    /// <returns></returns>
    public override IEnumerator OnGameStart()
    {
        WaitForSeconds wait  = new WaitForSeconds(1);
        while (DownloadManager.Instance.DownloadFinish == false)
        {
            yield return wait;
        }
        Log.Info(I18N.Get("btn_billboard"));
        // Print AppConfigs
        // Log.Info("======================================= Read Settings from C# =================================");
        foreach (BillboardSetting setting in BillboardSettings.GetAll())
        {
            Debug.Log(string.Format("C# Read Setting, Key: {0}, Value: {1}", setting.Id, setting.Title));
        }
        
        UIModule.Instance.OpenWindow("UILogin", 888);

        // Test Load a scene in asset bundle
        SceneLoader.Load("Scene/Scene1001/Scene1001");
        
        //预加载公告界面
        // UIModule.Instance.PreLoadUIWindow("Billboard");
        //UIModule.Instance.OpenWindow("Billboard");
         // 测试Collect函数，立即回收所有资源
        var path = "ui/UIRoleInfo";
        var assetLoader = AssetBundleLoader.Load(path); 
        while (!assetLoader.IsCompleted)             
            yield return null;
        yield return new WaitForSeconds(1);
        assetLoader.Release();
  
        KResourceModule.Collect();
    }

    private void OnApplicationQuit()
    {
#if ILRuntime
        ILRuntimeModule.Instance.OnDestroy();
#endif
    }
}
