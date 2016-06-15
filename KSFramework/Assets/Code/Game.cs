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
    public override IEnumerator OnBeforeInitModules()
    {
        // Do Nothing
        yield break;
    }

    /// <summary>
    /// After Init Modules, coroutine
    /// </summary>
    /// <returns></returns>
    public override IEnumerator OnFinishInitModules()
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

        // 开始加载我们的公告界面！
        //UIModule.Instance.OpenWindow("Billboard");
    }
}
