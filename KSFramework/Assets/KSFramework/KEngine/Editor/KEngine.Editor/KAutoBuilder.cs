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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KUnityEditorTools;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

//using Unity.EditorCoroutines.Editor; //package:com.unity.editorcoroutines ,package中勾选:Show preview packages

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

        /// <summary>
        /// 命令行给unity传参数 示例：-BundleVersion=1.0.1 -AndroidKeyStoreName=KSFramework
        /// Unity.exe -batchmode -projectPath %codePath%\ -nographics -executeMethod BuildTest.PerformAndroidBuild -BundleVersion=1.0.1 -AndroidKeyStoreName=KSFramework -logFile %~dp0\build.log -quit
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="outputpath"></param>
        private static void ParseArgs(ref BuildOptions opt, ref string outputpath)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            if (!Directory.Exists(AppConfig.ProductRelPath))
            {
                Directory.CreateDirectory(AppConfig.ProductRelPath);
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
                    UnityEngine.Debug.Log("parse arg -> " + item.Key + " : " + item.Value);
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
        private static string PerformBuild(string outputpath, BuildTargetGroup buildTargetGroup,BuildTarget tag, BuildOptions opt)
        {
#if UNITY_2018_1_OR_NEWER
            EditorUserBuildSettings.SwitchActiveBuildTarget( buildTargetGroup, tag);
#else
			EditorUserBuildSettings.SwitchActiveBuildTarget(tag);
#endif
            //for release build min log output ,can modify 
            PlayerSettings.SetStackTraceLogType(LogType.Error,StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Exception,StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Warning,StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Assert,StackTraceLogType.ScriptOnly);
            //建议所有的log.info日志前都要添加 if(AppConfig.isEditor)的判断，上线产品中关闭log。PS.不判断的话关闭输出后还有调用log接口传递字符串拼接的消耗
            PlayerSettings.SetStackTraceLogType(LogType.Log,StackTraceLogType.ScriptOnly /*StackTraceLogType.None*/);
            
            if(AppConfig.IsDownloadRes && !File.Exists(AppConfig.VersionTextPath))
            {
                Log.LogError("打包失败，可下载更新的包，需要先生成vresion.txt");
                return null;
            }
            //OnBeforeBuildPlayerEvent Unity引擎的打包前事件
            ParseArgs(ref opt, ref outputpath);
            string fullPath = System.IO.Path.Combine(AppConfig.ProductRelPath,outputpath);
            string fullDir = System.IO.Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            Log.Info("Start Build Client {0} to: {1}", tag, Path.GetFullPath(fullPath));
#if xLua
         	// NOTE xlua在编辑器开发模式不生成代码，因为.NET Standard 2.0不支持emit，会导致某些CSharpCallLua注册失败，Api要改成.Net4.X，在打包时如果有需要再修改回
         	// 需要先clear，再gen，避免同一个class修改后，覆盖gen会报错
            XLua.DelegateBridge.Gen_Flag = true;
            CSObjectWrapEditor.Generator.ClearAll();
            CSObjectWrapEditor.Generator.GenAll();
#elif ILRuntime
            ILRuntimeEditor.GenerateCLRBindingByAnalysis();
            ILRuntimeEditor.GenerateCrossbindAdapter();
#endif
            var buildResult = BuildPipeline.BuildPlayer(GetScenePaths(), fullPath, tag, opt);
#if xLua
            if(buildResult.summary.result == BuildResult.Succeeded) CSObjectWrapEditor.Generator.ClearAll();
#endif
            Log.Info("Build App result:{0} ,errors:{1}",buildResult.summary.result ,buildResult.summary.totalErrors);
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

        [MenuItem("KEngine/AutoBuilder/WindowsX86 Dev")] 
        public static void PerformWinBuild()
        {
            PerformBuild("Apps/Windows_Dev/KSFramework_Dev.exe", BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows,
                BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler);
        }

        [MenuItem("KEngine/AutoBuilder/WindowsX86")]
        public static void PerformWinReleaseBuild()
        {
        	PerformBuild("Apps/Windows/KSFramework.exe", BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows, BuildOptions.None);
        }
        
        [MenuItem("KEngine/AutoBuilder/Windows IL2CPP")]
        public static void PerformWinIL2CPPBuild()
        {
            //TODO install checklist: 1. il2cpp support 2.win7/win10 sdk (vs setup.exe install c++ desktop dev env)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.IL2CPP);
            PerformBuild("Apps/WindowsIL2CPP/KSFramework_IL2CPP.exe", BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows, BuildOptions.None);
        }

        [MenuItem("KEngine/AutoBuilder/iOS")]
        public static void PerformiOSBuild()
        {
            //apple store need x64
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS,ManagedStrippingLevel.Disabled);//不裁剪代码
            PlayerSettings.applicationIdentifier = "com.github.ksframework";//todo read from config or param to fill appid
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;

            //todo custom you project render settings
            #if UNITY_2017_1_OR_NEWER
            //PlayerSettings.colorSpace = ColorSpace.Gamma;
            //PlayerSettings.gpuSkinning = true;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS,new GraphicsDeviceType[]{UnityEngine.Rendering.GraphicsDeviceType.Metal});
            UnityEditor.Rendering.TierSettings ts = new TierSettings();
            ts.hdr = true;
            ts.hdrMode = CameraHDRMode.R11G11B10;
            ts.renderingPath = RenderingPath.Forward;
            ts.realtimeGICPUUsage = RealtimeGICPUUsage.Low;
            UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.iOS,UnityEngine.Rendering.GraphicsTier.Tier3,ts);
            #endif
            
            PerformiOSBuild("KSFramework",false);
        }

        public static string PerformiOSBuild(string ipaName, bool isDevelopment = true)
        {
            //增量生成xcode project
            BuildOptions opt = isDevelopment
                ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler | BuildOptions.AcceptExternalModificationsToPlayer)
                : BuildOptions.AcceptExternalModificationsToPlayer;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            return PerformBuild("Apps/IOSProjects/" + ipaName, BuildTargetGroup.iOS,BuildTarget.iOS, opt);
#else
            return PerformBuild("Apps/IOSProjects/" + ipaName, BuildTarget.iOS, opt);
#endif
        }

        [MenuItem("KEngine/AutoBuilder/Android")]
        public static void PerformAndroidBuild()
        {
            PerformAndroidBuild("KSFramework",false);
        }
        
        [MenuItem("KEngine/AutoBuilder/Android IL2CPP")]
        public static void PerformAndroidIL2CPPBuild()
        {
            PerformAndroidBuild("KSFrameworkProject",false,true,true);
        }
        
        public static string PerformAndroidBuild(string apkName, bool isDevelopment = true,bool isIL2CPP = false,bool exportASProject = false)
        {
            BuildOptions opt = BuildOptions.None;
            if (isDevelopment)
            {
                opt = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;
            }
            if (isIL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, isDevelopment ? Il2CppCompilerConfiguration.Debug : Il2CppCompilerConfiguration.Release);
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP );
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            }
            else
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
                //x86是平板和安卓模拟器
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
            }

            if (exportASProject)
            {
                opt = opt | BuildOptions.AcceptExternalModificationsToPlayer;
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            }
            //export android studio project,use gradle to build apk
            EditorUserBuildSettings.exportAsGoogleAndroidProject = exportASProject;

            //settings
            EditorUserBuildSettings.development = isDevelopment;
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
            PlayerSettings.Android.forceSDCardPermission = true;//申请工信部版号时，可关闭
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.renderOutsideSafeArea = true; //刘海屏
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android,ManagedStrippingLevel.Disabled);

            //render settings
#if UNITY_2017_1_OR_NEWER
            //PlayerSettings.colorSpace = ColorSpace.Linear;
            //PlayerSettings.gpuSkinning = true;
            //opengl 3
            /*PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new GraphicsDeviceType[]{UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3});
            PlayerSettings.openGLRequireES31 = false;
            PlayerSettings.openGLRequireES31AEP = false;*/
            
            UnityEditor.Rendering.TierSettings ts = new TierSettings();
            ts.hdr = true;
            ts.hdrMode = CameraHDRMode.R11G11B10;
            ts.renderingPath = RenderingPath.Forward;
            ts.realtimeGICPUUsage = RealtimeGICPUUsage.Low;
            UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Android,UnityEngine.Rendering.GraphicsTier.Tier3,ts);
#endif

            string path = exportASProject ? $"Apps/Android/{apkName}" : $"Apps/Android/{apkName}.apk";
            return PerformBuild(path,BuildTargetGroup.Android, BuildTarget.Android, opt);
        }

        public static void CreateGradleBatScript(string asPrjectPath)
        {
            //NOTE unity2019之后版本导出的android studio工程结构与2018有差异。see:https://www.cnblogs.com/zhaoqingqing/p/14968513.html
            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine("cd %~dp0");
            cmd.AppendLine("gradle assembleRelease");
            //gradle build success后面的cmd没有执行???
            cmd.AppendLine("start \"\" explorer \"launcher\\build\\outputs\\apk\\release\"");
            cmd.AppendLine("pause");

            string savePath = asPrjectPath + "/" + "build_apk.bat";
            File.WriteAllText(savePath, cmd.ToString(), Encoding.Default);
            
            //折中方法:再创建一个bat open build apk folder
            cmd.Clear();
            cmd.AppendLine("cd %~dp0");
            cmd.AppendLine("start \"\" explorer \"launcher\\build\\outputs\\apk\\release\"");
            savePath = asPrjectPath + "/" + "open_apk.bat";
            File.WriteAllText(savePath, cmd.ToString(), Encoding.Default);
        }

        [MenuItem("KEngine/Clear PersistentDataPath",false,99)]
        public static void ClearPersistentDataPath()
        {
            foreach (string dir in Directory.GetDirectories(KResourceModule.AppDataPath))
            {
                Directory.Delete(dir, true);
            }
            foreach (string file in Directory.GetFiles(KResourceModule.AppDataPath))
            {
                File.Delete(file);
            }
        }

        [MenuItem("KEngine/Open PersistentDataPath Folder",false,98)]
        public static void OpenPersistentDataPath()
        {
            System.Diagnostics.Process.Start(KResourceModule.AppDataPath);
        }

        [MenuItem("KEngine/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            BuildTools.ShowDialog("Prefs Cleared!");
        }

   }

    public class ResourcesSymbolLinkHelper
    {
        public static string StreamingPath = "Assets/StreamingAssets/";
        public static string AssetBundlesLinkPath = StreamingPath + AppConfig.StreamingBundlesFolderName;
        public static string LuaLinkPath = StreamingPath + AppConfig.LuaPath ;//NOTE mac os下不需要/结尾
        public static string SettingLinkPath = StreamingPath + AppConfig.SettingResourcesPath ;
        //WeakReference ins
        //public static object ins;
        
        public static string GetABLinkPath()
        {
            if (!Directory.Exists(AssetBundlesLinkPath))
            {
                Directory.CreateDirectory(AssetBundlesLinkPath);
                Log.Info("Create StreamingAssets Bundles Director {0}", AssetBundlesLinkPath);
            }
            return AssetBundlesLinkPath + "/" + KResourceModule.GetBuildPlatformName() + "/";
        }

        public static string GetResourceExportPath()
        {
            var resourcePath = BuildTools.GetExportPath(KResourceModule.Quality);
            return resourcePath;
        }

        [MenuItem("KEngine/Symbol Link Resources/Link Builded Resource -> StreamingAssets or Resources")]
        public static void SymbolLinkResource()
        {
            KSymbolLinkHelper.DeleteAllLinks(AssetBundlesLinkPath);
            var exportPath = GetResourceExportPath();
            var linkPath = GetABLinkPath();
            KSymbolLinkHelper.SymbolLinkFolder(exportPath, linkPath);
            //NOTE 特别无解，无法同步link这两个目录，使用协程处理后目录内容是空，如果2018及以下版本无EditorCoroutine使用脚本进行link
            /*Log.Info("Add Symbol Link Assetbundle.");
            ins = new object();
            EditorCoroutineUtility.StartCoroutine(LinkLua(), ins);
            Log.Info("Add Symbol Link Lua.");
            EditorCoroutineUtility.StartCoroutine(LinkSettings(), ins);
            Log.Info("Add Symbol Link Settings.");*/

            var linkFile = Application.dataPath + "/../AssetLink.sh";
            if (System.Environment.OSVersion.ToString().Contains("Windows"))
            {
                linkFile = Application.dataPath + "/../AssetLink.bat";
            }
            KTool.ExecuteFile(linkFile);
            var  dstPath = Application.streamingAssetsPath + "/" + AppConfig.VersionTxtName;
            if (File.Exists(dstPath)) File.Delete(dstPath);
            File.Copy(AppConfig.VersionTextPath, dstPath);
            Log.Info($"拷贝version.txt完成,File.Exists:{File.Exists(dstPath)}");
            AssetDatabase.Refresh();
        }

        private static IEnumerator LinkLua()
        {
            KSymbolLinkHelper.DeleteAllLinks(LuaLinkPath);
            yield return new WaitForSeconds(1.0f);
            var exportPath = KResourceModule.AppBasePath + AppConfig.LuaPath + "/";
            if (!Directory.Exists(LuaLinkPath)) Directory.CreateDirectory(LuaLinkPath);
            KSymbolLinkHelper.SymbolLinkFolder(exportPath, LuaLinkPath);
        }

        private static IEnumerator LinkSettings()
        {
            KSymbolLinkHelper.DeleteAllLinks(SettingLinkPath);
            yield return new WaitForSeconds(1.0f);
           var exportPath = KResourceModule.AppBasePath + AppConfig.SettingResourcesPath + "/";
            if (!Directory.Exists(SettingLinkPath)) Directory.CreateDirectory(SettingLinkPath);
            KSymbolLinkHelper.SymbolLinkFolder(exportPath, SettingLinkPath);
        }
       
		[MenuItem("KEngine/Symbol Link Resources/Remove StreamingAssets or Resources links")]
		public static void RemoveSymbolLinkResource()
		{
			KSymbolLinkHelper.DeleteAllLinks(AssetBundlesLinkPath);
            AssetDatabase.DeleteAsset(AssetBundlesLinkPath);
            
            KSymbolLinkHelper.DeleteAllLinks(LuaLinkPath);
            KSymbolLinkHelper.DeleteAllLinks(SettingLinkPath);
            var dstPath = Application.streamingAssetsPath + $"/{AppConfig.VersionTxtName}";
            if (File.Exists(dstPath)) File.Delete(dstPath);
            Debug.Log ("Remove Symbol LinkPath.");
			AssetDatabase.Refresh();
        }

        /// <summary>
        /// Assets/xx -> E:\Code\KSFramework\xxx
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string AssetPathToFullPath(string assetPath)
        {
            assetPath = assetPath.Replace("\\", "/");
            return Path.GetFullPath(Application.dataPath + "/" + assetPath.Remove(0,"Assets/".Length));
        }
    }
}