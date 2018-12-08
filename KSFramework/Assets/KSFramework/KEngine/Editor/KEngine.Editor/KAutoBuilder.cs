#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KAutoBuilder.cs
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
using KUnityEditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KEngine.Editor
{
    public class KAutoBuilder
    {
        private static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            return s[s.Length - 2];
        }

        private static string[] GetScenePaths()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }

            return scenes;
        }

        private static void ParseArgs(ref BuildOptions opt, ref string outputpath)
        {
            string[] args = System.Environment.GetCommandLineArgs();

            string productPath = KEngine.AppEngine.GetConfig("KEngine", "ProductRelPath");

            if (!Directory.Exists(productPath))
            {
                Directory.CreateDirectory(productPath);
            }

            if (args.Length >= 2)
            {
                CommandArgs commandArg = CommandLine.Parse(args);
                //List<string> lparams = commandArg.Params;
                Dictionary<string, string> argPairs = commandArg.ArgPairs;

                foreach (KeyValuePair<string, string> item in argPairs)
                {
                    switch (item.Key)
                    {
                        case "BundleVersion":
                            PlayerSettings.bundleVersion = item.Value;
                            break;
                        case "AndroidVersionCode":
                            PlayerSettings.Android.bundleVersionCode = System.Int32.Parse(item.Value);
                            break;
                        case "AndroidKeyStoreName":
                            PlayerSettings.Android.keystoreName = item.Value;
                            break;
                        case "AndroidKeyStorePass":
                            PlayerSettings.Android.keystorePass = item.Value;
                            break;
                        case "AndroidkeyAliasName":
                            PlayerSettings.Android.keyaliasName = item.Value;
                            break;
                        case "AndroidKeyAliasPass":
                            PlayerSettings.Android.keyaliasPass = item.Value;
                            break;
                        case "BuildOptions":
                        {
                            opt = BuildOptions.None;
                            string[] opts = item.Value.Split('|');
                            foreach (string o in opts)
                            {
                                opt = opt | (BuildOptions) System.Enum.Parse(typeof (BuildOptions), o);
                            }
                        }
                            break;
                        case "Outputpath":
                            outputpath = item.Value;
                            break;
                    }
                    UnityEngine.Debug.Log(item.Key + " : " + item.Value);
                }
            }
        }

        /// <summary>
        /// return full path or build
        /// </summary>
        /// <param name="outputpath"></param>
        /// <param name="tag"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        private static string PerformBuild(string outputpath, BuildTarget tag, BuildOptions opt)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(tag);

            ParseArgs(ref opt, ref outputpath);

            string fullPath = System.IO.Path.Combine(KEngine.AppEngine.GetConfig("KEngine", "ProductRelPath"),
                outputpath);

            string fullDir = System.IO.Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            Log.Info("Start Build Client {0} to: {1}", tag, Path.GetFullPath(fullPath));

            //NOTE xlua打包前生成Lua绑定代码
#if xLua
            //先clear，再gen，避免同一个class修改后，再gen会报错
                        CSObjectWrapEditor.Generator.ClearAll();
                        CSObjectWrapEditor.Generator.GenAll();
#endif
            var buildResult = BuildPipeline.BuildPlayer(GetScenePaths(), fullPath, tag, opt);
#if xLua
                        CSObjectWrapEditor.Generator.ClearAll();
#endif
            Log.Info("Build Client Finish.");
            return fullPath;
        }

        //public static int GetProgramVersion()
        //{
        //    var oldVersion = 0;
        //    if (File.Exists(GetProgramVersionFullPath()))
        //        oldVersion = File.ReadAllText(GetProgramVersionFullPath()).ToInt32();

        //    return oldVersion;
        //}

        //public static string GetProgramVersionFullPath()
        //{
        //    string programVersionFile = string.Format("{0}/Resources/ProgramVersion.txt", Application.dataPath);
        //    return programVersionFile;
        //}

        [MenuItem("KEngine/AutoBuilder/WindowsX86 Dev")] // 注意，PC版本放在不一样的目录的！
        public static void PerformWinBuild()
        {
            PerformBuild("KSFramework_Dev.exe", BuildTarget.StandaloneWindows,
                BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler);
        }

        [MenuItem("KEngine/AutoBuilder/WindowsX86")]
        static void PerformWinReleaseBuild()
        {
        	PerformBuild("KSFramework.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        }

        [MenuItem("KEngine/AutoBuilder/iOS")]
        public static void PerformiOSBuild()
        {
            PerformiOSBuild("KSFramework",false);
        }

        public static string PerformiOSBuild(string ipaName, bool isDevelopment = true)
        {
            //增量生成xcode project
            BuildOptions opt = isDevelopment
                ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler | BuildOptions.AcceptExternalModificationsToPlayer)
                : BuildOptions.AcceptExternalModificationsToPlayer;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            return PerformBuild("Apps/IOSProjects/" + ipaName, BuildTarget.iOS, opt);
#else
            return PerformBuild("Apps/IOSProjects/" + ipaName, BuildTarget.iOS, opt);
#endif
        }

        [MenuItem("KEngine/AutoBuilder/Android")]
        public static void PerformAndroidBuild()
        {
            PerformAndroidBuild("KSFramework",false);
        }

        public static string PerformAndroidBuild(string apkName, bool isDevelopment = true)
        {
            BuildOptions opt = isDevelopment
                ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler)
                : BuildOptions.None;
            var path = string.Format("Apps/{0}/{1}.apk", "Android", apkName);
            return PerformBuild(path, BuildTarget.Android, opt);
        }

        [MenuItem("KEngine/Clear PC PersistentDataPath")]
        public static void ClearPersistentDataPath()
        {
            foreach (string dir in Directory.GetDirectories(KResourceModule.GetAppDataPath()))
            {
                Directory.Delete(dir, true);
            }
            foreach (string file in Directory.GetFiles(KResourceModule.GetAppDataPath()))
            {
                File.Delete(file);
            }
        }

        [MenuItem("KEngine/Open PC PersistentDataPath Folder")]
        public static void OpenPersistentDataPath()
        {
            System.Diagnostics.Process.Start(KResourceModule.GetAppDataPath());
        }

        [MenuItem("KEngine/Clear Prefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            BuildTools.ShowDialog("Prefs Cleared!");
        }

   }

    public class ResourcesSymbolLinkHelper
    {
        public static string AssetBundlesLinkPath
        {
            get
            {
                // StreamingAssetsPath
                return "Assets/StreamingAssets/" +
                       KEngine.AppEngine.GetConfig(KEngineDefaultConfigs.StreamingBundlesFolderName);
                        // hold asset bundles
            }
        }

        public static string GetLinkPath()
        {
            if (!Directory.Exists(AssetBundlesLinkPath))
            {
                Directory.CreateDirectory(AssetBundlesLinkPath);
                Log.Info("Create StreamingAssets Bundles Director {0}", AssetBundlesLinkPath);
            }
            return AssetBundlesLinkPath + "/" + KResourceModule.BuildPlatformName + "/";
        }

        public static string GetResourceExportPath()
        {
            var resourcePath = BuildTools.GetExportPath(EditorUserBuildSettings.activeBuildTarget,
                KResourceModule.Quality);
            return resourcePath;
        }

        [MenuItem("KEngine/Symbol Link Resources/Link Builded Resource -> StreamingAssets or Resources")]
        public static void SymbolLinkResource()
        {
            KSymbolLinkHelper.DeleteAllLinks(ResourcesSymbolLinkHelper.AssetBundlesLinkPath);

            var exportPath = GetResourceExportPath();
            var linkPath = GetLinkPath();

            KSymbolLinkHelper.SymbolLinkFolder(exportPath, linkPath);

            AssetDatabase.Refresh();
        }
		[MenuItem("KEngine/Symbol Link Resources/Remove StreamingAssets or Resources links")]
		public static void RemoveSymbolLinkResource()
		{
			KSymbolLinkHelper.DeleteAllLinks(ResourcesSymbolLinkHelper.AssetBundlesLinkPath);
			Debug.Log ("Remove " + ResourcesSymbolLinkHelper.AssetBundlesLinkPath);

			AssetDatabase.DeleteAsset(ResourcesSymbolLinkHelper.AssetBundlesLinkPath);

			AssetDatabase.Refresh ();
		}
    }
}