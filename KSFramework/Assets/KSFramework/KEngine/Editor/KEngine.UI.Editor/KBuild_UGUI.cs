#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using KEngine.UI;
using KUnityEditorTools;
using UnityEditor;
using UnityEditor.U2D;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
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
            KUnityEditorEventCatcher.OnBeforeBuildAppEvent -= OnBeforeBuildPlayerEvent;
            KUnityEditorEventCatcher.OnBeforeBuildAppEvent += OnBeforeBuildPlayerEvent;
            KUnityEditorEventCatcher.OnPostBuildPlayerEvent -= OnAfterBuildPlayerEvent;
            KUnityEditorEventCatcher.OnPostBuildPlayerEvent += OnAfterBuildPlayerEvent;
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
                windowAsset.IsUIEditor = false;
                BeforeExportUIPrefab(windowAsset);
                var uiPrefabPath = uiPrefabDir + "/" + windowAsset.name + ".prefab";
#if UNITY_2018_1_OR_NEWER
                var prefab = PrefabUtility.SaveAsPrefabAsset(windowAsset.gameObject, uiPrefabPath);
#else
                var prefab = PrefabUtility.CreatePrefab(uiPrefabPath, windowAsset.gameObject, ReplacePrefabOptions.Default);
#endif

                EditorUtility.SetDirty(prefab);
                windowAsset.IsUIEditor = true;
                //NOTE 有同学反馈在unity2019.3.4下这里会导致unity卡死(我在2019.3.7未遇到)，如出现问题可注释这行
                AssetDatabase.ImportAsset(uiPrefabPath, ImportAssetOptions.ForceSynchronousImport);
                Debug.Log("Create UIWindowAsset to prfab: " + uiPrefabPath);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// TODO 导出UI前的工作
        ///     生成图集
        ///     检查只能使用一个图集，Text不勾选bestFit，不使用空的Image接受事件
        /// </summary>
        /// <param name="asset"></param>
        private static void BeforeExportUIPrefab(UIWindowAsset asset)
        {
            /* 目录结构：
             *     UIHello.unity
             *     atlas
             *         sprite1.png
             *         ....
             * 每个目录下的atlas中存放当前界面用到的图片，atlas文件夹所有图打到一个图集中
             */
            var scene = EditorSceneManager.GetActiveScene();
            var src_path = scene.path.Replace(scene.name+".unity","").Replace(Application.dataPath,"Assets/") + "atlas";
            var dst_path = "Assets/" + KEngineDef.ResourcesBuildDir + "/UI/";
            CreateSpriteAtlas(src_path, dst_path, asset.name);
            List<string> atlasNames = new List<string>();
            var images = asset.GetComponentsInChildren<Image>(false);
            foreach (Image image in images)
            {
                if (image.sprite != null)
                {
                    var atlasName = GetSpriteAtlasName(image);
                    if (!string.IsNullOrEmpty(atlasName) && !atlasNames.Contains(atlasName))
                    {
                        atlasNames.Add(atlasName);
                    }
                }
            }

            asset.Atals_arr = string.Join(",", atlasNames);
        }

        private static string GetSpriteAtlasName(Image img)
        {
            if (img == null || img.sprite == null)
                return null;
            string[] array = AssetDatabase.FindAssets("t:spriteatlas");
            foreach (string guid in array)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                //Get Packed sprites in atlas.
                SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
                var sprites = Enumerable.Range(0, spPackedSprites.arraySize)
                    .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
                    .OfType<Sprite>();

                    if (sprites.Contains(img.sprite))
                    {
                        return atlas.name;
                    }
                
            }

            return null;
        }
#endif


        private static void OnBeforeBuildPlayerEvent()
        {
            // Auto Link resources when play! //NOTE 打包ab时不link资源，在Editor可以从磁盘中直接读取
            //if (!Directory.Exists(ResourcesSymbolLinkHelper.GetABLinkPath()))
            {
                Log.Info("Auto Link Bundle Resources Path... {0}", ResourcesSymbolLinkHelper.GetABLinkPath());
                ResourcesSymbolLinkHelper.SymbolLinkResource();
            }
        }
        
        private static void OnAfterBuildPlayerEvent(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (Directory.Exists(ResourcesSymbolLinkHelper.GetABLinkPath()))
            {
                ResourcesSymbolLinkHelper.RemoveSymbolLinkResource();
            }

            if (buildTarget == BuildTarget.Android)
            {
                if (!pathToBuiltProject.EndsWith(".apk"))
                {
                    //android studio project: create gradle script
                    Log.Info($"Android Studio工程导出完成，创建Gradle打包脚本到{pathToBuiltProject}");
                    KAutoBuilder.CreateGradleBatScript(pathToBuiltProject);
                }
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
        }
        
        [MenuItem("KEngine/UI(UGUI)/Create Main Panel")]
        public static void CreateMainUI()
        {
            CreateNewUI(PanelType.MainUI);
        }
        
        [MenuItem("KEngine/UI(UGUI)/Create Normal Panel")]
        public static void CreateNormalUI()
        {
            CreateNewUI(PanelType.NormalUI);
        }
        
        [MenuItem("KEngine/UI(UGUI)/Create Tips Panel")]
        public static void CreateTipsUI()
        {
            CreateNewUI(PanelType.TipsUI);
        }
        
        [MenuItem("KEngine/UI(UGUI)/Create HUD Panel")]
        public static void CreateHUDUI()
        {
            CreateNewUI(PanelType.HeadInfoUI);
        }
        
        public static void CreateNewUI(PanelType panelType)
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
                uiName = "UI" + Path.GetRandomFileName();
            }

            GameObject uiObj = new GameObject(uiName);
            uiObj.layer = (int) UnityLayerDef.UI;
            var windowAsset = uiObj.AddComponent<UIWindowAsset>();
            windowAsset.IsUIEditor = true;
            windowAsset.PanelType = panelType;
            if (panelType != PanelType.NormalUI)
            {
                windowAsset.MoneyBar = MoneyBarType.None;
                windowAsset.IsShowTabBar = false;
                windowAsset.TabBarId = 0;
            }

            var uiPanel = new GameObject("Image").AddComponent<Image>();
            uiPanel.transform.SetParent(uiObj.transform);
            KTool.ResetLocalTransform(uiPanel.transform);

            var canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            CanvasScaler canvasScaler = uiObj.AddComponent<CanvasScaler>();
            uiObj.AddComponent<GraphicRaycaster>();

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = AppConfig.UIResolution;
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

            Camera camera;
            var go = GameObject.Find("UICamera");
            if (go == null)
            {
                GameObject cameraObj = new GameObject("UICamera");
                cameraObj.layer = (int) UnityLayerDef.UI;

                camera = cameraObj.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.depth = 0;
                camera.backgroundColor = Color.grey;
                camera.cullingMask = 1 << (int) UnityLayerDef.UI;
                camera.orthographicSize = 1f;
                camera.orthographic = true;
                camera.nearClipPlane = 0.3f;
                camera.farClipPlane = 1000f;

                camera.gameObject.AddComponent<AudioListener>();
            }
            else
            {
                camera = go.GetComponent<Camera>();
            }

            canvas.worldCamera = camera;
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

        #region SpriteAtlas

        //https://docs.unity3d.com/ScriptReference/U2D.SpriteAtlasExtensions.html
        private static bool GetTagIfTexture(string Path, ref Dictionary<string, List<string>> dict)
        {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(Path);
            foreach (Object o in data)
            {
                Texture2D s = o as Texture2D;
                if (s != null)
                {
                    TextureImporter ti = AssetImporter.GetAtPath(Path) as TextureImporter;
                    List<string> spritesWithTag;
                    if (!dict.ContainsKey(Path))
                    {
                        spritesWithTag = new List<string>();
                        dict[ti.spritePackingTag] = spritesWithTag;
                    }
                    else
                    {
                        spritesWithTag = dict[ti.spritePackingTag];
                    }

                    spritesWithTag.Add(Path);
                    return true;
                }
            }

            return false;
        }

        private static Texture2D GetTexture(string Path)
        {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(Path);
            foreach (Object o in data)
            {
                Texture2D s = (Texture2D) o;
                if (s != null)
                    return s;
            }

            return null;
        }

        private static bool CacheSpriteAtlasSprites(string Path, ref SortedSet<string> SpritesInAtlas)
        {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(Path);
            foreach (Object o in data)
            {
                SpriteAtlas sa = o as SpriteAtlas;
                if (sa != null)
                {
                    Sprite[] sprites = new Sprite[sa.spriteCount];
                    sa.GetSprites(sprites);

                    foreach (Sprite sprite in sprites)
                    {
                        SpritesInAtlas.Add(AssetDatabase.GetAssetPath(sprite));
                    }
                }

                return true;
            }

            return false;
        }

        [MenuItem("Assets/Create SpriteAtlas for selected Sprites.")]
        public static void CreateAtlasForSelectedSprites()
        {
            SpriteAtlas sa = new SpriteAtlas();
            AssetDatabase.CreateAsset(sa, "Assets/sample.spriteatlas");
            foreach (var obj in Selection.objects)
            {
                Object o = obj as Sprite;
                if (o != null)
                    SpriteAtlasExtensions.Add(sa, new Object[] {o});
            }

            AssetDatabase.SaveAssets();
        }

        //[MenuItem("Assets/SpriteAtlas Migrate")]
        public static void SpriteAtlasMigrator()
        {
            List<string> TexturesList = new List<string>();
            List<string> SpriteAtlasList = new List<string>();
            Dictionary<string, List<string>> spriteTagMap = new Dictionary<string, List<string>>();
            SortedSet<string> SpritesInAtlas = new SortedSet<string>();

            foreach (string s in AssetDatabase.GetAllAssetPaths())
            {
                if (s.StartsWith("Packages") || s.StartsWith("ProjectSettings") || s.Contains("scene"))
                    continue;
                bool hasSprite = GetTagIfTexture(s, ref spriteTagMap);
                if (hasSprite)
                {
                    TexturesList.Add(s);
                }
                else if (s.Contains("spriteatlas"))
                {
                    bool hasSpriteAtlas = CacheSpriteAtlasSprites(s, ref SpritesInAtlas);
                    if (hasSpriteAtlas)
                        SpriteAtlasList.Add(s);
                }
            }

            foreach (KeyValuePair<string, List<string>> tag in spriteTagMap)
            {
                bool found = SpriteAtlasList.Contains(tag.Key);
                if (!found)
                {
                    string atlasPath = "Assets/" + tag.Key + ".spriteatlas";
                    SpriteAtlas sa = new SpriteAtlas();
                    AssetDatabase.CreateAsset(sa, atlasPath);
                    sa.name = tag.Key;
                    List<string> ss = tag.Value;
                    foreach (string s in ss)
                    {
                        Object o = GetTexture(s);
                        SpriteAtlasExtensions.Add(sa, new Object[] {o});
                    }

                    AssetDatabase.SaveAssets();
                }
            }
        }

        [MenuItem("KEngine/UI(UGUI)/Export Common Atlas")]
        public static void BuildCommonAtlas()
        {
            var assetPath = Application.dataPath + "/";
            var dirs = Directory.GetDirectories(assetPath + KEngineDef.ResourcesEditDir + "/UI/", "atlas_*", SearchOption.TopDirectoryOnly);
            foreach (string dir in dirs)
            {
                var arr = dir.Split('/');
                var name = dir.Replace(assetPath, "");
                CreateSpriteAtlas("Assets/" + name, "Assets/" + KEngineDef.ResourcesBuildDir + "/uiatlas/", arr[arr.Length - 1],false);
            }
        }

        public static void CreateSpriteAtlas(string src_path, string dst_path, string atlas_name,bool includeInBuild = true)
        {
            if (string.IsNullOrEmpty(src_path) || string.IsNullOrEmpty(dst_path) || string.IsNullOrEmpty(atlas_name))
            {
                Log.LogError($"无法创建图集,路径为空. src_path={src_path} ,dst_path={dst_path} ,atlas_name={atlas_name}");
                return;
            }

            if (!src_path.EndsWith("/")) src_path = src_path + "/";
            var full_path = Path.GetFullPath(src_path);
            if (!Directory.Exists(full_path))
            {
                Log.Info($"不创建图集,{src_path}下无atlas文件夹");
                return;
            }
            var assets = Directory.GetFiles(full_path, "*.png"); //这里无法过滤两种类型 *.png|*.jpg
            if (assets == null || assets.Length == 0)
            {
                Log.Info($"{src_path}下无图片，不生成图集");
                return;
            }

            SpriteAtlas spriteAtlas = new SpriteAtlas();
            if (!dst_path.EndsWith("/")) dst_path = dst_path + "/";
            if (!Directory.Exists(dst_path)) Directory.CreateDirectory(dst_path);
            AssetDatabase.CreateAsset(spriteAtlas, dst_path + atlas_name + ".spriteatlas");
            foreach (var path in assets)
            {
                var t = src_path + Path.GetFileName(path);
                var o = AssetDatabase.LoadAssetAtPath<Sprite>(t);
                if (o != null)
                    SpriteAtlasExtensions.Add(spriteAtlas, new Object[] {o});
            }

            //TODO 根据平台设置图集格式 //EditorUserBuildSettings.activeBuildTarget
            var atlasSetting = spriteAtlas.GetPlatformSettings(KResourceModule.GetBuildPlatformName());
            atlasSetting.maxTextureSize = 2048;
            atlasSetting.textureCompression = TextureImporterCompression.Compressed;
            atlasSetting.format = TextureImporterFormat.Automatic;
            spriteAtlas.SetIncludeInBuild(includeInBuild);
            spriteAtlas.SetPlatformSettings(atlasSetting);

            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] {spriteAtlas}, EditorUserBuildSettings.activeBuildTarget, false);
            AssetDatabase.SaveAssets();
            Log.Info($"创建图集{spriteAtlas.name}完成，包含{spriteAtlas.spriteCount}个图片");
        }

        #endregion
    }
}