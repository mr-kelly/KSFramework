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
            get { return 1f / (float)Quality; }
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
                    // GameObject resMgr = GameObject.Find("_ResourceModule_");
                    // if (resMgr == null)
                    // {
                        var resMgr = new GameObject("_ResourceModule_");
                        GameObject.DontDestroyOnLoad(resMgr);
                    // }

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
            DocumentResourcesPath = fileProtocol + DocumentResourcesPathWithoutFileProtocol;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    {
                        ApplicationPath = string.Format("{0}{1}", fileProtocol, editorProductPath);
                        ProductPathWithProtocol = fileProtocol + editorProductPath + "/";
                        ProductPathWithoutFileProtocol = editorProductPath + "/";
                        // Resources folder
                    }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    {
                        string path = Application.streamingAssetsPath.Replace('\\', '/');//Application.dataPath.Replace('\\', '/');
                        ApplicationPath = string.Format("{0}{1}", fileProtocol, Application.dataPath);
                        ProductPathWithProtocol = string.Format("{0}{1}/", fileProtocol, path);
                        ProductPathWithoutFileProtocol = string.Format("{0}/", path);
                        // Resources folder
                    }
                    break;
                case RuntimePlatform.Android:
                    {
                        ApplicationPath = string.Concat("jar:", fileProtocol, Application.dataPath, "!/assets");
                        ProductPathWithProtocol = string.Concat(ApplicationPath, "/");
                        ProductPathWithoutFileProtocol = string.Concat(Application.dataPath,  "!/assets/");
                        // 注意，StramingAsset在Android平台中，是在壓縮的apk里，不做文件檢查
                        // Resources folder
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    {
                        // MacOSX下，带空格的文件夹，空格字符需要转义成%20
                        ApplicationPath = System.Uri.EscapeUriString(fileProtocol + Application.streamingAssetsPath); 

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
        #endregion        

        #region Path Def

        private static string editorProductFullPath;
        /// <summary>
        /// Product Folder Full Path , Default: C:\xxxxx\xxxx\Product
        /// </summary>
        public static string EditorProductFullPath
        {
            get
            {
                if(string.IsNullOrEmpty(editorProductFullPath))  
                    editorProductFullPath =  Path.GetFullPath(AppConfig.ProductRelPath);
                return editorProductFullPath;
            }
        }

        /// <summary>
        /// WWW的读取需要Protocol前缀，比如：
        ///     StreamingAssets/Bundles/Windows/ 
        ///     StreamingAssets/Bundles/Android/
        /// </summary>
        public static string ProductPathWithProtocol { get; private set; }

        public static string ProductPathWithoutFileProtocol { get; private set; }

        /// <summary>
        /// Bundles/Android/ etc... no prefix for streamingAssets
        /// </summary>
        public static string BundlesPathRelative { get; private set; }

        public static string ApplicationPath { get; private set; }
        /// <summary>
        /// 获取app的数据目录，可写入
        /// </summary>
        public static string GetAppDataPath()
        {
            return Application.persistentDataPath;
        }
        /// <summary>
        /// Application.persistentDataPath
        /// </summary>
        public static string DocumentResourcesPathWithoutFileProtocol
        {
            get
            {
                return string.Format("{0}/", GetAppDataPath()); // 各平台通用
            }
        }
        /// <summary>
        /// Application.persistentDataPath
        /// </summary>
        public static string DocumentResourcesPath;
        #endregion

        /// <summary>
        /// 给路径加上ab文件的后缀, 取决于配置的AssetBundle后缀
        /// </summary>
        /// <param name="path"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static string GetAbFullPath(string path)
        {
            if (!path.EndsWith(AppConfig.AssetBundleExt)) path = path + AppConfig.AssetBundleExt;
            return GetResourceFullPath(BundlesPathRelative + path, false);
        }

        /// <summary>
        /// 完整路径，优先级：热更目录->安装包
        /// 根路径：Product
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="isLog"></param>
        /// <returns></returns>
        public static string GetResourceFullPath(string url, bool withFileProtocol = false, bool isLog = true)
        {
            string fullPath;
            if (GetResourceFullPath(url, withFileProtocol, out fullPath, isLog) != GetResourceFullPathType.Invalid)
                return fullPath;

            return null;
        }
        
        /// <summary>
        /// 资源是否存在
        /// </summary>
        /// <param name="url">相对路径</param>
        /// <param name="raiseError">文件不存在打印Error</param>
        /// <returns></returns>
        public static bool IsResourceExist(string url, bool raiseError = true)
        {
            var pathType = GetResourceFullPath(url, false, out string fullPath,raiseError);
            return pathType != GetResourceFullPathType.Invalid;
        }

        /// <summary>
        /// 根据相对路径，获取到完整路径
        /// </summary>
        /// <param name="url">相对路径</param>
        /// <param name="withFileProtocol"></param>
        /// <param name="fullPath">完整路径</param>
        /// <param name="raiseError">文件不存在打印Error</param>
        /// <returns></returns>
        public static GetResourceFullPathType GetResourceFullPath(string url, bool withFileProtocol, out string fullPath,bool raiseError = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.Error("尝试获取一个空的资源路径！");
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }

            string docUrl;
            bool hasDocUrl = TryGetDocumentResourceUrl(url, withFileProtocol, out docUrl);
            if (hasDocUrl)
            {
                fullPath = docUrl;
                return GetResourceFullPathType.InDocument;
            }
            // 優先下載資源，但又沒有下載資源文件！使用本地資源目錄 
            string inAppUrl;
            bool hasInAppUrl  = TryGetInAppStreamingUrl(url, withFileProtocol, out inAppUrl);
            if (!hasInAppUrl) // 连本地资源都没有，直接失败吧 ？？ 沒有本地資源但又遠程資源？竟然！!?
            {
                if (raiseError) Log.Error($"[Not Found] StreamingAssetsPath Url Resource: {url} ,fullPath:{inAppUrl}");
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }

            fullPath = inAppUrl; // 直接使用本地資源！

            return GetResourceFullPathType.InApp;
        }
        
        /// <summary>
        /// use AssetDatabase.LoadAssetAtPath insead of load asset bundle, editor only
        /// </summary>
        public static bool IsEditorLoadAsset
        {
            get
            {
                return Application.isEditor && AppConfig.IsEditorLoadAsset ;
            }
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
            if (AppConfig.UseAppPath)
            {
                newUrl = Path.GetFullPath(withFileProtocol ? GetFileProtocol : "" + Application.streamingAssetsPath + "/" + url);
            }
            else
            {
                newUrl = Path.GetFullPath((withFileProtocol ? ProductPathWithProtocol : ProductPathWithoutFileProtocol) + url);
            }
            
            // 注意，StreamingAssetsPath在Android平台時，壓縮在apk里面，不要做文件檢查了
            if (!Application.isEditor && Application.platform == RuntimePlatform.Android)
            {
                if (!KEngineAndroidPlugin.IsAssetExists(url))
                    return false;
            }
            else if(Application.isEditor)
            {
                // Editor, 非android运行，直接进行文件检查
                if (!File.Exists(Path.GetFullPath(ProductPathWithoutFileProtocol+ url)))
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
                filePath = filePath.Replace("//", "/");

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
            if (AppConfig.UseAppPath)
            {
                newUrl = Path.GetFullPath((withFileProtocol ? DocumentResourcesPath :DocumentResourcesPathWithoutFileProtocol) + url);
                return File.Exists(Path.GetFullPath(DocumentResourcesPathWithoutFileProtocol + url));
            }
            else
            {
                newUrl = Path.GetFullPath((withFileProtocol ? ProductPathWithProtocol : ProductPathWithoutFileProtocol) + url);
                return File.Exists(Path.GetFullPath(ProductPathWithoutFileProtocol + url));
            }
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
        

        /// <summary>
        /// Load file. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static byte[] LoadAssetsSync(string path)
        {
            string fullPath = GetResourceFullPath(path,false);
            if (string.IsNullOrEmpty(fullPath))
                return null;

            if (Application.platform == RuntimePlatform.Android)
                return KEngineAndroidPlugin.GetAssetBytes(fullPath);
            
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
                Log.Info("ResourceManager ApplicationPath: {0}", ApplicationPath);
                Log.Info("ResourceManager ProductPathWithProtocol: {0}", ProductPathWithProtocol);
                Log.Info("ResourceManager ProductPathWithoutProtocol: {0}", ProductPathWithoutFileProtocol);
                Log.Info("ResourceManager DocumentResourcesPath: {0}", DocumentResourcesPath);
                Log.Info("================================================================================");
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
