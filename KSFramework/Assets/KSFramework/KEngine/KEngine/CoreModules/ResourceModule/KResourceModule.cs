#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

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
using Object = UnityEngine.Object;

namespace KEngine
{
    public enum KResourceQuality
    {
        Sd = 2,
        Hd = 1,
        Ld = 4,
    }

    /// <summary>
    /// 资源路径优先级，优先使用
    /// </summary>
    public enum KResourcePathPriorityType
    {
        Invalid,

        /// <summary>
        /// 忽略PersitentDataPath, 优先寻找Resources或StreamingAssets路径 (取决于ResourcePathType)
        /// </summary>
        InAppPathPriority,

        /// <summary>
        /// 尝试在Persistent目錄尋找，找不到再去StreamingAssets,
        /// 这一般用于进行热更新版本号判断后，设置成该属性
        /// </summary>
        PersistentDataPathPriority,
    }

    public class KResourceModule : MonoBehaviour
    {
        static KResourceModule()
        {
            InitResourcePath();
        }

        public delegate void AsyncLoadABAssetDelegate(Object asset, object[] args);

        public enum LoadingLogLevel
        {
            None,
            ShowTime,
            ShowDetail,
        }

        public static KResourceQuality Quality = KResourceQuality.Sd;

        public static float TextureScale
        {
            get { return 1f / (float)Quality; }
        }

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

        public static bool LoadByQueue = false;
        public static int LogLevel = (int)LoadingLogLevel.None;

        public static string BuildPlatformName
        {
            get { return GetBuildPlatformName(); }
        } // ex: IOS, Android, AndroidLD

        public static string FileProtocol
        {
            get { return GetFileProtocol(); }
        } // for WWW...with file:///xxx

        /// <summary>
        /// Product Folder's Relative Path   -  Default: ../Product,   which means Assets/../Product
        /// </summary>
        public static string ProductRelPath
        {
            get { return KEngine.AppEngine.GetConfig(KEngineDefaultConfigs.ProductRelPath); }
        }

        /// <summary>
        /// Product Folder Full Path , Default: C:\xxxxx\xxxx\../Product
        /// </summary>
        public static string EditorProductFullPath
        {
            get { return Path.GetFullPath(ProductRelPath); }
        }

        /// <summary>
        /// StreamingAssetsPath/Bundles/Android/ etc.
        /// WWW的读取，是需要Protocol前缀的
        /// </summary>
        public static string ProductPathWithProtocol { get; private set; }

        public static string ProductPathWithoutFileProtocol { get; private set; }

        /// <summary>
        /// Bundles/Android/ etc... no prefix for streamingAssets
        /// </summary>
        public static string BundlesPathRelative { get; private set; }

        public static string ApplicationPath { get; private set; }

        public static string DocumentResourcesPathWithoutFileProtocol
        {
            get
            {
                return string.Format("{0}/", GetAppDataPath()); // 各平台通用
            }
        }

        public static string DocumentResourcesPath;

        /// <summary>
        /// 是否優先找下載的資源?還是app本身資源優先. 优先下载的资源，即采用热更新的资源
        /// </summary>
        public static KResourcePathPriorityType ResourcePathPriorityType =
            KResourcePathPriorityType.PersistentDataPathPriority;

        public static System.Func<string, string> CustomGetResourcesPath; // 自定义资源路径。。。

        /// <summary>
        /// 统一在字符串后加上.box, 取决于配置的AssetBundle后缀
        /// </summary>
        /// <param name="path"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static string GetAssetBundlePath(string path, params object[] formats)
        {
            return string.Format(path + KEngine.AppEngine.GetConfig("KEngine", "AssetBundleExt"), formats);
        }

        // 检查资源是否存在
        public static bool ContainsResourceUrl(string resourceUrl)
        {
            string fullPath;
            return GetResourceFullPath(resourceUrl, false, out fullPath, false) != GetResourceFullPathType.Invalid;
        }

        /// <summary>
        /// 完整路径，www加载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="inAppPathType"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="isLog"></param>
        /// <returns></returns>
        public static string GetResourceFullPath(string url, bool withFileProtocol = true, bool isLog = true)
        {
            string fullPath;
            if (GetResourceFullPath(url, withFileProtocol, out fullPath, isLog) != GetResourceFullPathType.Invalid)
                return fullPath;

            return null;
        }

        /// <summary>
        /// 用于GetResourceFullPath函数，返回的类型判断
        /// </summary>
        public enum GetResourceFullPathType
        {
            Invalid,
            InApp,
            InDocument,
        }

        /// <summary>
        /// 根据相对路径，获取到StreamingAssets完整路径，或Resources中的路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fullPath"></param>
        /// <param name="inAppPathType"></param>
        /// <param name="isLog"></param>
        /// <returns></returns>
        public static GetResourceFullPathType GetResourceFullPath(string url, bool withFileProtocol, out string fullPath,
             bool isLog = true)
        {
            if (string.IsNullOrEmpty(url))
                Log.Error("尝试获取一个空的资源路径！");

            string docUrl;
            bool hasDocUrl = TryGetDocumentResourceUrl(url, withFileProtocol, out docUrl);

            string inAppUrl;
            bool hasInAppUrl  = TryGetInAppStreamingUrl(url, withFileProtocol, out inAppUrl);

            if (ResourcePathPriorityType == KResourcePathPriorityType.PersistentDataPathPriority) // 優先下載資源模式
            {
                if (hasDocUrl)
                {
                    if (Application.isEditor)
                        Log.Warning("[Use PersistentDataPath] {0}", docUrl);
                    fullPath = docUrl;
                    return GetResourceFullPathType.InDocument;
                }
                // 優先下載資源，但又沒有下載資源文件！使用本地資源目錄 
            }

            if (!hasInAppUrl) // 连本地资源都没有，直接失败吧 ？？ 沒有本地資源但又遠程資源？竟然！!?
            {
                if (isLog)
                    Log.Error("[Not Found] StreamingAssetsPath Url Resource: {0}", url);
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }

            fullPath = inAppUrl; // 直接使用本地資源！

            return GetResourceFullPathType.InApp;
        }

        /// <summary>
        /// 獲取app數據目錄，可寫，同Application.PersitentDataPath，但在windows平台時為了避免www類中文目錄無法讀取問題，單獨實現
        /// </summary>
        /// <returns></returns>
        public static string GetAppDataPath()
        {
            // Windows 时使用特定的目录，避免中文User的存在 
            // 去掉自定义PersistentDataPath, 2015/11/18， 务必要求Windows Users是英文
            //if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer)
            //{
            //    string dataPath = Application.dataPath + "/../Library/UnityWinPersistentDataPath";
            //    if (!Directory.Exists(dataPath))
            //        Directory.CreateDirectory(dataPath);
            //    return dataPath;
            //}

            return Application.persistentDataPath;
        }


        /// <summary>
        /// (not android ) only! Android资源不在目录！
        /// Editor返回文件系统目录，运行时返回StreamingAssets目录
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetInAppStreamingUrl(string url, bool withFileProtocol, out string newUrl)
        {
            if (withFileProtocol)
                newUrl = ProductPathWithProtocol + url;
            else
                newUrl = ProductPathWithoutFileProtocol + url;

            // 注意，StreamingAssetsPath在Android平台時，壓縮在apk里面，不要做文件檢查了
            if (!Application.isEditor && Application.platform == RuntimePlatform.Android)
            {
                if (!KEngineAndroidPlugin.IsAssetExists(url))
                    return false;
            }
            else
            {
                // Editor, 非android运行，直接进行文件检查
                if (!File.Exists(ProductPathWithoutFileProtocol + url))
                {
                    return false;
                }
            }

            // Windows/Edtiro平台下，进行大小敏感判断
            if (Application.isEditor)
            {
                var result = FileExistsWithDifferentCase(ProductPathWithoutFileProtocol + url);
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

                return String.CompareOrdinal(realFilePath, filePath) == 0;
            }
            return false;
        }

        /// <summary>
        /// 可被WWW读取的Resource路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetDocumentResourceUrl(string url, bool withFileProtocol, out string newUrl)
        {
            if (withFileProtocol)
                newUrl = DocumentResourcesPath + url;
            else
                newUrl = DocumentResourcesPathWithoutFileProtocol + url;

            if (File.Exists(DocumentResourcesPathWithoutFileProtocol + url))
            {
                return true;
            }

            return false;
        }

        private void Awake()
        {
            if (_Instance != null)
                Debuger.Assert(_Instance == this);

            //InvokeRepeating("CheckGcCollect", 0f, 3f);
            if (Debug.isDebugBuild)
            {
                Log.Info("ResourceManager ApplicationPath: {0}", ApplicationPath);
                Log.Info("ResourceManager ProductPathWithProtocol: {0}", ProductPathWithProtocol);
                Log.Info("ResourceManager ProductPathWithoutProtocol: {0}", ProductPathWithoutFileProtocol);
                Log.Info("ResourceManager DocumentResourcesPath: {0}", DocumentResourcesPath);
                Log.Info("================================================================================");
            }
        }

        private void Update()
        {
            AbstractResourceLoader.CheckGcCollect();
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
        /// Different platform's assetBundles is incompatible.
        /// CosmosEngine put different platform's assetBundles in different folder.
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
        /// On Windows, file protocol has a strange rule that has one more slash
        /// </summary>
        /// <returns>string, file protocol string</returns>
        public static string GetFileProtocol()
        {
            string fileProtocol = "file://";
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer
#if !UNITY_5_4_OR_NEWER
                || Application.platform == RuntimePlatform.WindowsWebPlayer
#endif
)
                fileProtocol = "file:///";

            return fileProtocol;
        }

        public static string BundlesDirName
        {
            get { return KEngine.AppEngine.GetConfig(KEngineDefaultConfigs.StreamingBundlesFolderName); }
        }

        /// <summary>
        /// Unity Editor load AssetBundle directly from the Asset Bundle Path,
        /// whth file:// protocol
        /// </summary>
        public static string EditorAssetBundleFullPath
        {
            get
            {
                string editorAssetBundlePath = Path.GetFullPath(KEngine.AppEngine.GetConfig(KEngineDefaultConfigs.AssetBundleBuildRelPath)); // for editoronly

                return editorAssetBundlePath;
            }
        }

        /// <summary>
        /// Load Async Asset Bundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback">cps style async</param>
        /// <returns></returns>
        public static AbstractResourceLoader LoadBundleAsync(string path, AssetFileLoader.AssetFileBridgeDelegate callback = null)
        {
            var request = AssetFileLoader.Load(path, callback);
            return request;
        }

        /// <summary>
        /// load asset bundle immediatly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AbstractResourceLoader LoadBundle(string path, AssetFileLoader.AssetFileBridgeDelegate callback = null)
        {
            var request = AssetFileLoader.Load(path, callback, LoaderMode.Sync);
            return request;
        }

        /// <summary>
        /// check file exists of streamingAssets. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static bool IsStreamingAssetsExists(string path)
        {
            if (Application.platform == RuntimePlatform.Android)
                return KEngineAndroidPlugin.IsAssetExists(path);

            var fullPath = Path.Combine(Application.streamingAssetsPath, path);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Load file from streamingAssets. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static byte[] LoadSyncFromStreamingAssets(string path)
        {
            if (!IsStreamingAssetsExists(path))
                throw new Exception("Not exist StreamingAssets path: " + path);

            if (Application.platform == RuntimePlatform.Android)
                return KEngineAndroidPlugin.GetAssetBytes(path);

            var fullPath = Path.Combine(Application.streamingAssetsPath, path);
            return ReadAllBytes(fullPath);
        }

        public static bool IsPersistentDataExist(string path)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, path);
            return File.Exists(fullPath);
        }

        public static byte[] LoadSyncFromPersistentDataPath(string path)
        {
            if (!IsPersistentDataExist(path))
                throw new Exception("Not exist PersistentData path: " + path);

            var fullPath = Path.Combine(Application.persistentDataPath, path);
            return ReadAllBytes(fullPath);
        }

        /// <summary>
        /// 无视锁文件，直接读bytes
        /// </summary>
        /// <param name="resPath"></param>
        public static byte[] ReadAllBytes(string resPath)
        {
            byte[] bytes;
            using (FileStream fs = File.Open(resPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
            }
            return bytes;
        }

        /// <summary>
        /// Initialize the path of AssetBundles store place ( Maybe in PersitentDataPath or StreamingAssetsPath )
        /// </summary>
        /// <returns></returns>
        static void InitResourcePath()
        {

            string editorProductPath = EditorProductFullPath;

            BundlesPathRelative = string.Format("{0}/{1}/", BundlesDirName, GetBuildPlatformName());
            DocumentResourcesPath = FileProtocol + DocumentResourcesPathWithoutFileProtocol;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    {
                        ApplicationPath = string.Format("{0}{1}", GetFileProtocol(), editorProductPath);
                        ProductPathWithProtocol = GetFileProtocol() + EditorProductFullPath + "/";
                        ProductPathWithoutFileProtocol = EditorProductFullPath + "/";
                        // Resources folder
                    }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    {
                        string path = Application.streamingAssetsPath.Replace('\\', '/');//Application.dataPath.Replace('\\', '/');
                        //                        path = path.Substring(0, path.LastIndexOf('/') + 1);
                        ApplicationPath = string.Format("{0}{1}", GetFileProtocol(), Application.dataPath);
                        ProductPathWithProtocol = string.Format("{0}{1}/", GetFileProtocol(), path);
                        ProductPathWithoutFileProtocol = string.Format("{0}/", path);
                        // Resources folder
                    }
                    break;
                case RuntimePlatform.Android:
                    {
                        ApplicationPath = string.Concat("jar:", GetFileProtocol(), Application.dataPath, "!/assets");
                        ProductPathWithProtocol = string.Concat(ApplicationPath, "/");
                        ProductPathWithoutFileProtocol = string.Concat(Application.dataPath,
                            "!/assets/");
                        // 注意，StramingAsset在Android平台中，是在壓縮的apk里，不做文件檢查
                        // Resources folder
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    {
                        ApplicationPath =
                            System.Uri.EscapeUriString(GetFileProtocol() + Application.streamingAssetsPath); // MacOSX下，带空格的文件夹，空格字符需要转义成%20

                        ProductPathWithProtocol = string.Format("{0}/", ApplicationPath);
                        // only iPhone need to Escape the fucking Url!!! other platform works without it!!! Keng Die!
                        ProductPathWithoutFileProtocol = Application.streamingAssetsPath + "/";
                        // Resources folder
                    }
                    break;
                default:
                    {
                        Debuger.Assert(false);
                    }
                    break;
            }
        }

        public static void LogRequest(string resType, string resPath)
        {
            if (LogLevel < (int)LoadingLogLevel.ShowDetail)
                return;

            Log.Info("[Request] {0}, {1}", resType, resPath);
        }

        public static void LogLoadTime(string resType, string resPath, System.DateTime begin)
        {
            if (LogLevel < (int)LoadingLogLevel.ShowTime)
                return;

            Log.Info("[Load] {0}, {1}, {2}s", resType, resPath, (System.DateTime.Now - begin).TotalSeconds);
        }

        /// <summary>
        /// Collect all KEngine's resource unused loaders
        /// </summary>
        public static void Collect()
        {
            while (AbstractResourceLoader.UnUsesLoaders.Count > 0)
                AbstractResourceLoader.DoGarbageCollect();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();

        }
    }

}
