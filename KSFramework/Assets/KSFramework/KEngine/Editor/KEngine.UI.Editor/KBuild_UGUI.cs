#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: CBuild_UGUI.cs
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
using System.IO;
using KEngine.UI;
using KUnityEditorTools;
using UnityEditor;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KEngine.Editor
{
    [InitializeOnLoad]
    public class KUGUIBuilder
#if UNITY_4
        : KBuild_Base
#endif
    {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
        /// <summary>
        /// 是否在保存场景的时候，自动判断场景中是否有UI对象，自动导出Prefab？
        /// 如果嫌弃保存自动导出，卡顿明显，可以设置这里为false，手动从菜单执行
        /// </summary>
        public static bool AutoUIPrefab = true;

#endif
        static KUGUIBuilder()
        {
            KUnityEditorEventCatcher.OnWillPlayEvent -= OnWillPlayEvent;
            KUnityEditorEventCatcher.OnWillPlayEvent += OnWillPlayEvent;
            KUnityEditorEventCatcher.OnSaveSceneEvent -= OnSaveScene;
            KUnityEditorEventCatcher.OnSaveSceneEvent += OnSaveScene;
            KUnityEditorEventCatcher.OnBeforeBuildPlayerEvent -= OnBeforeBuildPlayerEvent;
            KUnityEditorEventCatcher.OnBeforeBuildPlayerEvent += OnBeforeBuildPlayerEvent;
        }

        private static void OnSaveScene()
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            if (AutoUIPrefab && !Application.isPlaying)
            {
                var scenePath = EditorSceneManager.GetActiveScene().path;
                if (!scenePath.Contains("Assets/" + KEngineDef.ResourcesEditDir + "/UI") &&
                    !scenePath.Contains("Assets/" + KEngineDef.ResourcesBuildDir + "/UI"))
                    return;

                // Unity 5模式，自动把需要打包的资源，转成Prefab放置到UI下
                Debug.Log("Save Scene... " + EditorSceneManager.GetActiveScene().path);
                UISceneToPrefabs();
            }
#endif
        }
#if UNITY_5 || UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Unity 5下，将场景中的UI对象转成Prefab
        /// </summary>

        [MenuItem("KEngine/UI(UGUI)/UIScene -> Prefabs")]
        public static void UISceneToPrefabs()
        {
            var windowAssets = GetUIWIndoeAssetsFromCurrentScene();
            var uiPrefabDir = "Assets/" + KEngineDef.ResourcesBuildDir + "/UI";
            if (!Directory.Exists(uiPrefabDir))
                Directory.CreateDirectory(uiPrefabDir);

            foreach (var windowAsset in windowAssets)
            {
                var uiPrefabPath = uiPrefabDir + "/" + windowAsset.name + ".prefab";
                //if (File.Exists(uiPrefabPath))
                //{
                //    var srcPrefab = AssetDatabase.LoadAssetAtPath(uiPrefabPath, typeof (UnityEngine.Object));
                //    var newPrefab = PrefabUtility.ReplacePrefab(windowAsset.gameObject, srcPrefab, ReplacePrefabOptions.Default);
                //    EditorUtility.SetDirty(newPrefab);
                //}
                //else
                {
                    var prefab = PrefabUtility.CreatePrefab(uiPrefabPath, windowAsset.gameObject, ReplacePrefabOptions.Default);
                    EditorUtility.SetDirty(prefab);
                }

                AssetDatabase.ImportAsset(uiPrefabPath, ImportAssetOptions.ForceSynchronousImport);
                Debug.Log("Create UIWindowAsset to prfab: " + uiPrefabPath);
            }
            AssetDatabase.SaveAssets();
        }

#endif


        private static void OnBeforeBuildPlayerEvent()
        {
            // Auto Link resources when play!
            if (!Directory.Exists(ResourcesSymbolLinkHelper.GetLinkPath()))
            {
                Log.Warning("Auto Link Bundle Resources Path... {0}", ResourcesSymbolLinkHelper.GetLinkPath());
                ResourcesSymbolLinkHelper.SymbolLinkResource();
            }
        }

        private static void OnWillPlayEvent()
        {
        }

        [MenuItem("KEngine/UI(UGUI)/Export Current UI %&e")]
        public static void ExportCurrentUI()
        {
            if (EditorApplication.isPlaying)
            {
                Log.Error("Cannot export in playing mode! Please stop!");
                return;
            }
#if UNITY_4

            var windowAssets = GetUIWIndoeAssetsFromCurrentScene();

            foreach(var windowAsset in windowAssets)
            {
                BuildTools.BuildAssetBundle(windowAsset.gameObject, GetBuildRelPath(windowAsset.name));
            }
#else
            UISceneToPrefabs();
            BuildTools.BuildAllAssetBundles();
#endif
        }

        static UIWindowAsset[] GetUIWIndoeAssetsFromCurrentScene()
        {

            //var UIName = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
            var windowAssets = GameObject.FindObjectsOfType<UIWindowAsset>();
            if (windowAssets.Length <= 0)
            {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                var currentScene = EditorSceneManager.GetActiveScene().path;
#else
                var currentScene = EditorApplication.currentScene;
#endif
                Log.Error("Not found UIWindowAsset in scene `{0}`", currentScene);
            }

            return windowAssets;
        }

        [MenuItem("KEngine/UI(UGUI)/Export All UI")]
        public static void ExportAllUI()
        {
            if (Application.isPlaying)
            {
                Log.Error("Cannot export in playing mode! Please stop!");
                return;
            }
            var uiPath = Application.dataPath + "/" + KEngineDef.ResourcesEditDir + "/UI";
            var uiScenes = Directory.GetFiles(uiPath, "*.unity", SearchOption.AllDirectories);
            foreach (string uiScene in uiScenes)
            {
                Log.Info("begin export {0}", uiScene);
                EditorSceneManager.OpenScene(uiScene);
                KUGUIBuilder.UISceneToPrefabs();
            }
            BuildTools.BuildAllAssetBundles();
        }
        public static string GetBuildRelPath(string uiName)
        {
            return string.Format("UI/{0}_UI{1}", uiName, KEngine.AppEngine.GetConfig("KEngine", "AssetBundleExt"));
        }

        [MenuItem("KEngine/UI(UGUI)/Create UI(UGUI)")]
        public static void CreateNewUI()
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            var currentScene = EditorSceneManager.GetActiveScene().path;
#else
            var currentScene = EditorApplication.currentScene;
#endif
            GameObject mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
                GameObject.DestroyImmediate(mainCamera);

            var uiName = Path.GetFileNameWithoutExtension(currentScene);
            if (string.IsNullOrEmpty(uiName) || GameObject.Find(uiName) != null) // default use scene name, if exist create random name
            {
                uiName = "NewUI_" + Path.GetRandomFileName();
            }
            GameObject uiObj = new GameObject(uiName);
            uiObj.layer = (int)UnityLayerDef.UI;
            uiObj.AddComponent<UIWindowAsset>();

            var uiPanel = new GameObject("Image").AddComponent<Image>();
            uiPanel.transform.parent = uiObj.transform;
            KTool.ResetLocalTransform(uiPanel.transform);

            var canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = uiObj.AddComponent<CanvasScaler>();
            uiObj.AddComponent<GraphicRaycaster>();
            var uiSize = new Vector2(1280,720);
            var uiResolution = AppEngine.GetConfig("KEngine.UI", "UIResolution");
            if (!string.IsNullOrEmpty(uiResolution))
            {
                var sizeArr = uiResolution.Split(',');
                if (sizeArr.Length >= 2) { uiSize=new Vector2(sizeArr[0].ToInt32(),sizeArr[1].ToInt32()); }
            }
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = uiSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            if (GameObject.Find("EventSystem") == null)
            {
                var evtSystemObj = new GameObject("EventSystem");
                evtSystemObj.AddComponent<EventSystem>();
                evtSystemObj.AddComponent<StandaloneInputModule>();
#if UNITY_4
                evtSystemObj.AddComponent<TouchInputModule>();
#endif

            }

            if (GameObject.Find("Camera") == null)
            {
                GameObject cameraObj = new GameObject("Camera");
                cameraObj.layer = (int)UnityLayerDef.UI;

                Camera camera = cameraObj.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.depth = 0;
                camera.backgroundColor = Color.grey;
                camera.cullingMask = 1 << (int)UnityLayerDef.UI;
                camera.orthographicSize = 1f;
                camera.orthographic = true;
                camera.nearClipPlane = -2f;
                camera.farClipPlane = 2f;

                camera.gameObject.AddComponent<AudioListener>();

            }

            Selection.activeGameObject = uiObj;
        }

#if UNITY_4
        public override void Export(string path)
        {
            EditorApplication.OpenScene(path);
            ExportCurrentUI();
        }

        public override string GetDirectory()
        {
            return "UI";
        }

        public override string GetExtention()
        {
            return "*.unity";
        }
#endif
    }
}