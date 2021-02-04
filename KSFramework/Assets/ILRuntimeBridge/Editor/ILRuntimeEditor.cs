#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using KEngine;
using KEngine.Editor;
using KEngine.UI;
using KSFramework;

[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeEditor
{
    private static string genPath = "Assets/ILRuntimeBridge/Generated";
    private static string adapterPath = "Assets/ILRuntimeBridge/Adapter/";

    [MenuItem("KEngine/ILRuntime/通过自动分析热更DLL生成CLR绑定")]
    public static void GenerateCLRBindingByAnalysis()
    {
        if (!Directory.Exists(genPath)) Directory.CreateDirectory(genPath);
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        var dllPath = $"Assets/StreamingAssets/{AppConfig.StreamingBundlesFolderName}/{KResourceModule.GetBuildPlatformName()}/HotFix_Project.dll";
        using (System.IO.FileStream fs = new System.IO.FileStream(dllPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            ILRuntimeModule.Instance.InitBinders(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, genPath);
        }
        //NOTE 如果出现报错 TypeLoadException: Cannot find Adaptor for:xxx，这是ILRuntime提醒你需要编写Adapter
        AssetDatabase.Refresh();
    }


    [MenuItem("KEngine/ILRuntime/生成跨域继承适配器")]
    public static void GenerateCrossbindAdapter()
    {
        //由于跨域继承特殊性太多，自动生成无法实现完全无副作用生成，所以这里提供的代码自动生成主要是给大家生成个初始模版，简化大家的工作
        //大多数情况直接使用自动生成的模版即可，如果遇到问题可以手动去修改生成后的文件，因此这里需要大家自行处理是否覆盖的问题
        Dictionary<string, Type> filename2type = new Dictionary<string, Type>();
        filename2type.Add("UIControllerAdapter.cs", typeof(UIController));
        filename2type.Add("ILRuntimeUIBaseAdapter.cs", typeof(ILRuntimeUIBase));
        //filename2type.Add("KBehaviourAdapter.cs", typeof(KBehaviour));
        //NOTE IEnumerator和IEnumerable生成的缺少几个方法，手写补充，不生成
        if (!Directory.Exists(adapterPath)) Directory.CreateDirectory(adapterPath);
        foreach (KeyValuePair<string, Type> kv in filename2type)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(adapterPath + kv.Key))
            {
                sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(kv.Value, "ILRuntimeAdapter"));
            }
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("KEngine/ILRuntime/删除生成的代码")]
    public static void ClearGenCode()
    {
        var files = Directory.GetFiles(genPath, "*.cs");
        foreach (var path in files)
        {
            if (path.Contains("CLRBindings.cs"))
                continue;
            File.Delete(path);
        }

        string codeContent = @"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        { 
        }
        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}";
        File.WriteAllText(genPath + "/CLRBindings.cs", codeContent);
        AssetDatabase.Refresh();
    }


    [MenuItem("KEngine/ILRuntime/生成Hotfix的dll")]
    public static void GenDll()
    {
        var path = Path.GetFullPath(Application.dataPath + "/../..//build_tools/build_ilruntime_dll.bat");
        if (!File.Exists(path))
        {
            Log.LogError($"打包脚本找不到,请检查路径path:{path}");
            return;
        }

        Process.Start(path);
    }

    [MenuItem("KEngine/ILRuntime/生成Hotfix的dll为ab")]
    public static void BuildDllToAb()
    {
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetNames = new string[] {"hotfix"};
        build.addressableNames = new string[] {"hotfix"};
        //TODO 待测试：理论上来说在IOS通过加载byte也可以，我的实际项目中是把dll放在prefab中打成ab
        //不能单独打包dll，提示：No AssetBundle has been set for this build.需要把dll放到prefab中，可参考ET中的ReferenceCollector
        build.assetNames = new string[]
        {
            Path.GetFullPath(Application.dataPath + "/ILRuntimeBridge/Dll/HotFix_Project.dll"+AppConfig.AssetBundleExt),
        };
        var outputPath = BuildTools.GetExportPath();
        var opt = BuildAssetBundleOptions.DeterministicAssetBundle; //| BuildAssetBundleOptions.ChunkBasedCompression;
        BuildPipeline.BuildAssetBundles(outputPath, new AssetBundleBuild[] {build}, opt, EditorUserBuildSettings.activeBuildTarget);
        
        build.assetNames = new string[]
        {
            Path.GetFullPath(Application.dataPath + "/ILRuntimeBridge/Dll/HotFix_Project.pdb"+AppConfig.AssetBundleExt),
        };
        BuildPipeline.BuildAssetBundles(outputPath, new AssetBundleBuild[] {build}, opt, EditorUserBuildSettings.activeBuildTarget);
        Log.Info("build dll ab finish.");
    }
}
#endif