using UnityEngine;
using System.Collections;
using KEngine.Editor;
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
        DrawKEngineUI();
        GUILayout.Space(20);

        DrawAssetBundleUI();
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
            //            KSFrameworkEditor.AutoMakeUILuaScripts();
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
}
