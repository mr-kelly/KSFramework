#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KResourceModule.cs
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
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace KEngine
{
    public enum KResourceQuality
    {
        Sd = 2,
        Hd = 1,
        Ld = 4,
    }


    public class KResourceModule : MonoBehaviour
    {
        /// <summary>
        /// 用于GetResourceFullPath函数，返回的类型判断
        /// </summary>
        public enum GetResourceFullPathType
        {
            /// <summary>
            /// 无资源
            /// </summary>
            Invalid,

            /// <summary>
            /// 安装包内
            /// </summary>
            InApp,

            /// <summary>
            /// 热更新目录
            /// </summary>
            InDocument,
        }

        public static KResourceQuality Quality = KResourceQuality.Sd;

        public static float TextureScale
        {
            get { return 1f / (float) Quality; }
        }

        public static bool LoadByQueue = false;

        #region Init

        private static KResourceModule _Instance;

        public static KResourceModule Instance
        {
            get
            {
                if (_Instance == null)
                {
                    GameObject resMgr = GameObject.Find("_ResourceModule_");
                    if (resMgr == null)
                    {
                        resMgr = new GameObject("_ResourceModule_");
                        GameObject.DontDestroyOnLoad(resMgr);
                    }
                    _Instance = resMgr.AddComponent<KResourceModule>();
                }

                return _Instance;
            }
        }

        static KResourceModule()
        {
            InitResourcePath();
        }

        /// <summary>
        /// Initialize the path of AssetBundles store place ( Maybe in PersitentDataPath or StreamingAssetsPath )
        /// </summary>
        /// <returns></returns>
        static void InitResourcePath()
        {
            string editorProductPath = EditorProductFullPath;
            BundlesPathRelative = string.Format("{0}/{1}/", AppConfig.StreamingBundlesFolderName, GetBuildPlatformName());
            string fileProtocol = GetFileProtocol;
            AppDataPathWithProtocol = fileProtocol + AppDataPath;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                {
                    if (AppConfig.ReadStreamFromEditor)
                    {
                        AppBasePath = Application.streamingAssetsPath + "/";
                        AppBasePathWithProtocol = fileProtocol + AppBasePath;
                    }
                    else
                    {
                        AppBasePath = editorProductPath + "/";
                        AppBasePathWithProtocol = fileProtocol + AppBasePath;
                    }
                }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                {
                    string path = Application.streamingAssetsPath.Replace('\\', '/');
                    AppBasePath = path + "/";
                    AppBasePathWithProtocol = fileProtocol + AppBasePath;
                }
                    break;
                case RuntimePlatform.Android:
                {
                    //文档：https://docs.unity3d.com/Manual/StreamingAssets.html
                    //注意，StramingAsset在Android平台是在apk中，无法通过File读取请使用LoadAssetsSync，如果要同步读取ab请使用GetAbFullPath
                    //NOTE 我见到一些项目的做法是把apk包内的资源放到Assets的上层res内，读取时使用 jar:file://+Application.dataPath + "!/assets/res/"，editor上则需要/../res/
                    AppBasePath = Application.dataPath + "!/assets/";
                    AppBasePathWithProtocol = fileProtocol + AppBasePath;
                }
                    break;
                case RuntimePlatform.IPhonePlayer:
                {
                    // MacOSX下，带空格的文件夹，空格字符需要转义成%20
                    // only iPhone need to Escape the fucking Url!!! other platform works without it!!!
                    AppBasePath = System.Uri.EscapeUriString(Application.dataPath + "/Raw");
                    AppBasePathWithProtocol = fileProtocol + AppBasePath;
                }
                    break;
                default:
                {
                    Debuger.Assert(false);
                }
                    break;
            }
        }

        #endregion

        #region Path Def

        /**路径说明
         * Editor下模拟下载资源：
         *     AppData:C:\xxx\xxx\Appdata
         *     StreamAsset:C:\KSFramrwork\Product
         * 真机：
         *     AppData:Android\data\com.xxx.xxx\files\
         *     StreamAsset:apk内
         */
        private static string editorProductFullPath;

        /// <summary>
        /// Product Folder Full Path , Default: C:\KSFramework\Product
        /// </summary>
        public static string EditorProductFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(editorProductFullPath))
                    editorProductFullPath = Path.GetFullPath(AppConfig.ProductRelPath);
                return editorProductFullPath;
            }
        }


        /// <summary>
        /// 安装包内的路径，移动平台为只读权限，针对Application.streamingAssetsPath进行多平台处理，以/结尾
        /// </summary>
        public static string AppBasePath { get; private set; }

        /// <summary>
        /// WWW的读取需要file://前缀
        /// </summary>
        public static string AppBasePathWithProtocol { get; private set; }


        private static string appDataPath = null;
        /// <summary>
        /// app的数据目录，有读写权限，实际是Application.persistentDataPath，以/结尾
        /// </summary>
        public static string AppDataPath
        {
            get
            {
                if (appDataPath == null) appDataPath = Application.persistentDataPath + "/";
                return appDataPath;
            }
        }

        /// <summary>
        /// file://+Application.persistentDataPath
        /// </summary>
        public static string AppDataPathWithProtocol;

        /// <summary>
        /// Bundles/Android/ etc... no prefix for streamingAssets
        /// </summary>
        public static string BundlesPathRelative { get; private set; }

        /// <summary>
        /// On Windows, file protocol has a strange rule that has one more slash
        /// </summary>
        /// <returns>string, file protocol string</returns>
        public static string GetFileProtocol
        {
            get
            {
                string fileProtocol = "file://";
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer
#if UNITY_5 || UNITY_4
                || Application.platform == RuntimePlatform.WindowsWebPlayer
#endif
                )
                    fileProtocol = "file:///";

                return fileProtocol;
            }
        }

        /// <summary>
        /// Unity Editor load AssetBundle directly from the Asset Bundle Path,
        /// whth file:// protocol
        /// </summary>
        public static string EditorAssetBundleFullPath
        {
            get
            {
                string editorAssetBundlePath = Path.GetFullPath(AppConfig.AssetBundleBuildRelPath); // for editoronly
                return editorAssetBundlePath;
            }
        }

        #endregion

        /// <summary>
        /// 获取ab文件的完整路径，做的处理：会加上ab格式的后缀，如果是在apk包体内则会加上jar:file://前缀
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns></returns>
        public static string GetAbFullPath(string path)
        {
            if (!path.EndsWith(AppConfig.AssetBundleExt)) path = path + AppConfig.AssetBundleExt;
            var _fullUrl =  GetResourceFullPath(BundlesPathRelative + path, false);
            if (!string.IsNullOrEmpty(_fullUrl))
            {
                if(Application.platform == RuntimePlatform.Android && _fullUrl.StartsWith("/data/app"))
                {
                    return  "jar:file://" + _fullUrl;//如果apk内则添加前缀，经测试unity2019.3.7f1+android6.0加在Appbase无效
                }    
            }

            return _fullUrl;
        }

        /// <summary>
        /// 资源是否存在
        /// </summary>
        /// <param name="url">相对路径</param>
        /// <param name="raiseError">文件不存在打印Error</param>
        /// <returns></returns>
        public static bool IsResourceExist(string url, bool raiseError = true)
        {
#if UNITY_EDITOR
            if (KResourceModule.IsEditorLoadAsset && !url.EndsWith(".lua") && !url.EndsWith(AppConfig.SettingExt) && !url.EndsWith(".txt"))
            {
                var editorPath = "Assets/" + KEngineDef.ResourcesBuildDir + "/" + url;
                return File.Exists(editorPath);
            }
#endif
            var pathType = GetResourceFullPath(url, false, out string fullPath, raiseError);
            return pathType != GetResourceFullPathType.Invalid;
        }

        /// <summary>
        /// 完整路径，优先级：热更目录->安装包
        /// 根路径：Product
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public static string GetResourceFullPath(string url, bool withFileProtocol = false, bool raiseError = true)
        {
            string fullPath;
            if (GetResourceFullPath(url, withFileProtocol, out fullPath, raiseError) != GetResourceFullPathType.Invalid)
                return fullPath;
            return null;
        }

        /// <summary>
        /// 根据相对路径，获取到完整路径，優先从下載資源目录找，没有就读本地資源目錄 
        /// 根路径：Product
        /// </summary>
        /// <param name="url">相对路径</param>
        /// <param name="withFileProtocol"></param>
        /// <param name="fullPath">完整路径</param>
        /// <param name="raiseError">文件不存在打印Error</param>
        /// <returns></returns>
        public static GetResourceFullPathType GetResourceFullPath(string url, bool withFileProtocol, out string fullPath, bool raiseError = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.Error("尝试获取一个空的资源路径！");
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }
            string docUrl;
            bool hasDocUrl = TryGetAppDataUrl(url, withFileProtocol, out docUrl);
            if (hasDocUrl)
            {
                fullPath = docUrl;
                return GetResourceFullPathType.InDocument;
            }
            
            string inAppUrl;
            bool hasInAppUrl = TryGetInAppStreamingUrl(url, withFileProtocol, out inAppUrl);
            if (!hasInAppUrl) // 连本地资源都没有，直接失败吧 ？？ 
            {
                if (raiseError) Log.Error($"[Not Found] StreamingAssetsPath Url Resource: {url} ,fullPath:{inAppUrl}");
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }

            fullPath = inAppUrl; // 直接使用本地資源！

            return GetResourceFullPathType.InApp;
        }
        
        /// <summary>
        /// 获取一个资源的完整路径是在apk压缩包内还是在可读写路径内
        /// </summary>
        /// <param name="fullPath"></param>
        public static GetResourceFullPathType GetResFullPathType(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                Log.Error("无法识别一个空的资源路径！");
                return GetResourceFullPathType.Invalid;
            }

            if (Application.platform == RuntimePlatform.Android)
                return fullPath.StartsWith("/data/app") ? GetResourceFullPathType.InApp : GetResourceFullPathType.InDocument;
            return fullPath.StartsWith(AppDataPath) ? GetResourceFullPathType.InApp : GetResourceFullPathType.InDocument;
        }
        
        /// <summary>
        /// use AssetDatabase.LoadAssetAtPath insead of load asset bundle, editor only
        /// </summary>
        public static bool IsEditorLoadAsset
        {
            get { return Application.isEditor && AppConfig.IsEditorLoadAsset; }
        }

        /// <summary>
        /// 可读写的目录
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetAppDataUrl(string url, bool withFileProtocol, out string newUrl)
        {
            newUrl = Path.GetFullPath((withFileProtocol ? AppDataPathWithProtocol : AppDataPath) + url);
            return File.Exists(Path.GetFullPath(AppDataPath + url));
        }

        /// <summary>
        /// StreamingAssets目录
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetInAppStreamingUrl(string url, bool withFileProtocol, out string newUrl)
        {
            newUrl = Path.GetFullPath((withFileProtocol ? AppBasePathWithProtocol : AppBasePath) + url);
            
            if (Application.isEditor)
            {
                // Editor进行文件检查
                if (!File.Exists(Path.GetFullPath(AppBasePath + url)))
                {
                    return false;
                }
            }
            else if(Application.platform == RuntimePlatform.Android) // 注意，StreamingAssetsPath在Android平台時，壓縮在apk里面，不要使用File做文件檢查
            {
                return KEngineAndroidPlugin.IsAssetExists(url);
            }

            // Windows/Editor平台下，进行大小敏感判断
            if (Application.isEditor)
            {
                var result = FileExistsWithDifferentCase(AppBasePath + url);
                if (!result)
                {
                    Log.Error("[大小写敏感]发现一个资源 {0}，大小写出现问题，在Windows可以读取，手机不行，请改表修改！", url);
                }
            }

            return true;
        }

        /// <summary>
        /// 大小写敏感地进行文件判断, Windows Only
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool FileExistsWithDifferentCase(string filePath)
        {
            if (File.Exists(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileTitle = Path.GetFileName(filePath);
                string[] files = Directory.GetFiles(directory, fileTitle);
                var realFilePath = files[0].Replace("\\", "/");
                filePath = filePath.Replace("\\", "/");
                filePath = filePath.Replace("//", "/");

                return String.CompareOrdinal(realFilePath, filePath) == 0;
            }

            return false;
        }

        private static string _unityEditorEditorUserBuildSettingsActiveBuildTarget;

        /// <summary>
        /// UnityEditor.EditorUserBuildSettings.activeBuildTarget, Can Run in any platform~
        /// </summary>
        public static string UnityEditor_EditorUserBuildSettings_activeBuildTarget
        {
            get
            {
                if (Application.isPlaying && !string.IsNullOrEmpty(_unityEditorEditorUserBuildSettingsActiveBuildTarget))
                {
                    return _unityEditorEditorUserBuildSettingsActiveBuildTarget;
                }

                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var a in assemblies)
                {
                    if (a.GetName().Name == "UnityEditor")
                    {
                        Type lockType = a.GetType("UnityEditor.EditorUserBuildSettings");
                        //var retObj = lockType.GetMethod(staticMethodName,
                        //    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                        //    .Invoke(null, args);
                        //return retObj;
                        var p = lockType.GetProperty("activeBuildTarget");

                        var em = p.GetGetMethod().Invoke(null, new object[] { }).ToString();
                        _unityEditorEditorUserBuildSettingsActiveBuildTarget = em;
                        return em;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Different platform's assetBundles is incompatible.// ex: IOS, Android, Windows
        /// KEngine put different platform's assetBundles in different folder.
        /// Here, get Platform name that represent the AssetBundles Folder.
        /// </summary>
        /// <returns>Platform folder Name</returns>
        public static string GetBuildPlatformName()
        {
            string buildPlatformName = "Windows"; // default

            if (Application.isEditor)
            {
                var buildTarget = UnityEditor_EditorUserBuildSettings_activeBuildTarget;
                //UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                switch (buildTarget)
                {
                    case "StandaloneOSXIntel":
                    case "StandaloneOSXIntel64":
                    case "StandaloneOSXUniversal":
                    case "StandaloneOSX":
                        buildPlatformName = "MacOS";
                        break;
                    case "StandaloneWindows": // UnityEditor.BuildTarget.StandaloneWindows:
                    case "StandaloneWindows64": // UnityEditor.BuildTarget.StandaloneWindows64:
                        buildPlatformName = "Windows";
                        break;
                    case "Android": // UnityEditor.BuildTarget.Android:
                        buildPlatformName = "Android";
                        break;
                    case "iPhone": // UnityEditor.BuildTarget.iPhone:
                    case "iOS":
                        buildPlatformName = "iOS";
                        break;
                    default:
                        Debuger.Assert(false);
                        break;
                }
            }
            else
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXPlayer:
                        buildPlatformName = "MacOS";
                        break;
                    case RuntimePlatform.Android:
                        buildPlatformName = "Android";
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        buildPlatformName = "iOS";
                        break;
                    case RuntimePlatform.WindowsPlayer:
#if !UNITY_5_4_OR_NEWER
                    case RuntimePlatform.WindowsWebPlayer:
#endif
                        buildPlatformName = "Windows";
                        break;
                    default:
                        Debuger.Assert(false);
                        break;
                }
            }

            if (Quality != KResourceQuality.Sd) // SD no need add
                buildPlatformName += Quality.ToString().ToUpper();
            return buildPlatformName;
        }

        /// <summary>
        /// Load file. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static byte[] LoadAssetsSync(string path)
        {
            string fullPath = GetResourceFullPath(path, false);
            if (string.IsNullOrEmpty(fullPath))
                return null;

            if (Application.platform == RuntimePlatform.Android)
            {
                return KEngineAndroidPlugin.GetAssetBytes(path);
                //TODO 通过www/webrequest读取
            }

            return ReadAllBytes(fullPath);
        }

        /// <summary>
        /// 无视锁文件，直接读bytes
        /// </summary>
        /// <param name="resPath"></param>
        public static byte[] ReadAllBytes(string resPath)
        {
            byte[] bytes;
            using (FileStream fs = File.Open(resPath, FileMode.Open, FileAccess.Read,FileShare.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int) fs.Length);
            }

            return bytes;
        }

        /// <summary>
        /// Collect all KEngine's resource unused loaders
        /// </summary>
        public static void Collect()
        {
            while (ABManager.UnUsesLoaders.Count > 0)
                ABManager.DoGarbageCollect();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        #region Unity函数

        private void Awake()
        {
            if (_Instance != null)
                Debuger.Assert(_Instance == this);
            SpriteAtlasManager.atlasRequested += ABManager.RequestAtlas;
            if (AppConfig.IsLogDeviceInfo)
            {
                //真机上输出这几个路径
                Log.Info("ResourceManager AppBasePath:{0} ,AppBasePathWithProtocol:{1}", AppBasePath,AppBasePathWithProtocol);
                Log.Info("ResourceManager AppDataPath:{0} ,AppDataPathWithProtocol:{1}", AppDataPath,AppDataPathWithProtocol);
            }
        }

        private void Update()
        {
            //NOTE 在Unity2019中有渐近式GC，而此处不会调用GC.Collect，仅仅对已加载的ab进行检查是否需要Unload
            ABManager.CheckGcCollect();
        }

        private void OnDestroy()
        {
            SpriteAtlasManager.atlasRequested -= ABManager.RequestAtlas;
        }

        #endregion
    }
}