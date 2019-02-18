using UnityEngine;
using System.Collections;
using System.IO;
using AppSettings;
using KEngine;
using KEngine.Editor;
using KEngine.UI;
using KSFramework.Editor;
using UnityEditor;

public class KQuickWindowEditor : EditorWindow
{
    [MenuItem("KEngine/Open Quick Window %&Q")]
    static void DoIt()
    {
        //note 在Windows Editor下Scree.Width,Screen.Height获取的并不是真实的显示器宽度和高度
        KQuickWindowEditor window = EditorWindow.GetWindow<KQuickWindowEditor>();
        window.titleContent = new GUIContent("快捷工具窗");
        window.position = new Rect(400, 100, 640, 480);
        window.Show();
    }

    public void OnGUI()
    {
        DrawKEngineInit();
        GUILayout.Space(20);

        DrawHotReLoadUI();
        GUILayout.Space(20);

        DrawKEngineUI();
        GUILayout.Space(20);

        DrawAssetBundleUI();
        GUILayout.Space(20);

        DrawBuildUI();
        GUILayout.Space(20);
    }


    public void DrawKEngineInit()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== KEngine ==");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("初始化AB资源链接", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            ResourcesSymbolLinkHelper.SymbolLinkResource();
        }

        GUILayout.EndHorizontal();
    }

    public void DrawHotReLoadUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== 热重载 ==");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重载配置表", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            //仅仅是重载编译后的配置表
            SettingsManager.AllSettingsReload();
        }
        if (GUILayout.Button("快速编译配置表", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            SettingModuleEditor.QuickCompileSettings();
        }
        if (GUILayout.Button("编译配置表并生成代码", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            SettingModuleEditor.CompileSettings();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重载UI的Lua代码", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadLuaCache();
        }
        if (GUILayout.Button("重新打开UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadUILua();
        }
        if (GUILayout.Button("重新加载并打开UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadUI();
        }
        GUILayout.EndHorizontal();
    }

    public void DrawKEngineUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== UI相关辅助 ==");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.CreateNewUI();
        }
        if (GUILayout.Button("为当前UI创建Lua脚本", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
             KSFrameworkEditor.AutoMakeUILuaScripts();
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("导出当前UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.ExportCurrentUI();
        }
        if (GUILayout.Button("导出全部UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.ExportAllUI();
        }
        GUILayout.EndHorizontal();
    }

    public void DrawAssetBundleUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== Assetbundle相关 ==");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打包Assetbundle（增量）", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            BuildTools.BuildAllAssetBundles();
        }
        if (GUILayout.Button("重新打包Assetbundle（先删除再重打）", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            BuildTools.ReBuildAllAssetBundles();
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("为Prefab设置Assetbundle name(BoundleResources)", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            BuildTools.MakeAssetBundleNames();
        }
        if (GUILayout.Button("清理冗余资源", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            BuildTools.CleanAssetBundlesRedundancies();
        }

        GUILayout.EndHorizontal();

    }

    void DrawBuildUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== 常用的功能键 ==");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打PC版", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(20)))
        {
            KAutoBuilder.PerformWinBuild();
        }
        if (GUILayout.Button("打Android包", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(20)))
        {
            KAutoBuilder.PerformAndroidBuild();
        }
     
        if (GUILayout.Button("打IOS版", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(20)))
        {
            KAutoBuilder.PerformiOSBuild();
        }
        if (GUILayout.Button("打开安装包目录", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(20)))
        {
            var path = KResourceModule.ProductRelPath + "/Apps/" + KResourceModule.GetBuildPlatformName();
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath) == false)
            {
                Log.Debug("{0} 目录不存在，定位到父目录。", fullPath);

                DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
                fullPath = directoryInfo.Parent.FullName;
            }
            Log.Debug("open: {0}", fullPath);
            System.Diagnostics.Process.Start("explorer.exe", fullPath);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
       
        
        GUILayout.EndHorizontal();
    }
}
