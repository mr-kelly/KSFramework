#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: BuildTools.cs
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KEngine;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KEngine.Editor
{
    [Obsolete("Please use BuildTools instead")]
    public class KBuildTools : BuildTools
    {
    }

    public partial class BuildTools
    {
#if UNITY_4
		private static int PushedAssetCount = 0;

		public static event Action<UnityEngine.Object, string, string> BeforeBuildAssetBundleEvent;
		public static event Action<UnityEngine.Object, string, string> AfterBuildAssetBundleEvent;
#endif


#if UNITY_5 || UNITY_2017_1_OR_NEWER
        static string ResourcesBuildDir
        {
            get
            {
                var dir = "Assets/" + KEngineDef.ResourcesBuildDir + "/";
                return dir;
            }
        }

        [MenuItem("KEngine/AssetBundle/Clear assetBundleName exclude BundleResources")]
        public static void ClearOtherAssetBundleNames()
        {
            // Check marked asset bundle whether real
            foreach (var assetGuid in AssetDatabase.FindAssets(""))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                var bundleName = assetImporter.assetBundleName;
                if (string.IsNullOrEmpty(bundleName))
                {
                    continue;
                }
                if (!assetPath.StartsWith(ResourcesBuildDir))
                {
                    assetImporter.assetBundleName = null;
                }
            }
        }

        /// <summary>
        /// Unity 5新AssetBundle系统，需要为打包的AssetBundle配置名称
        /// 
        /// 直接将KEngine配置的BundleResources目录整个自动配置名称，因为这个目录本来就是整个导出
        /// </summary>
        [MenuItem("KEngine/AssetBundle/Make Names from [BundleResources]")]
        public static void MakeAssetBundleNames()
        {
            var dir = ResourcesBuildDir;
            
            // set BundleResources's all bundle name
            foreach (var filepath in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
            {
                if (filepath.EndsWith(".meta")) continue;

                var importer = AssetImporter.GetAtPath(filepath);
                if (importer == null)
                {
                    Log.Error("Not found: {0}", filepath);
                    continue;
                }
                var bundleName = filepath.Substring(dir.Length, filepath.Length - dir.Length);
                importer.assetBundleName = bundleName + AppEngine.GetConfig(KEngineDefaultConfigs.AssetBundleExt);
            }

            Log.Info("Make all asset name successs!");
        }

        /// <summary>
        /// 清理冗余，即无此资源，却有AssetBundle的, Unity 5 only
        /// </summary>
        [MenuItem("KEngine/AssetBundle/Clean Redundancies")]
        public static void CleanAssetBundlesRedundancies()
        {
            var platformName = KResourceModule.BuildPlatformName;
            var outputPath = GetExportPath(EditorUserBuildSettings.activeBuildTarget);
            var srcList = new List<string>(Directory.GetFiles(ResourcesBuildDir, "*.*", SearchOption.AllDirectories));
            for (var i = srcList.Count - 1; i >= 0; i--)
            {
                if (srcList[i].EndsWith(".meta"))
                    srcList.RemoveAt(i);
                else
                    srcList[i] = srcList[i].Replace(ResourcesBuildDir, "").ToLower();
            }

            var toListMap = new Dictionary<string, string>();
            var toList = new List<string>(Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories));
            for (var i = toList.Count - 1; i >= 0; i--)
            {
                var filePath = toList[i];

                if (toList[i].EndsWith((".meta")) || toList[i].EndsWith(".manifest"))
                {
                    toList.RemoveAt(i);
                }
                else
                {
                    var rName = toList[i].Replace(outputPath, "");
                    if (rName == platformName || // 排除AB 平台总索引文件,
                        rName == (platformName + ".manifest") ||
                        rName == (platformName + ".meta") ||
                        rName == (platformName + ".manifest.meta"))
                    {
                        toList.RemoveAt(i);
                    }
                    else
                    {
//                        AppEngine.GetConfig(KEngineDefaultConfigs.AssetBundleExt
                        // 去掉扩展名，因为AssetBundle额外扩展名
                        toList[i] = Path.ChangeExtension(rName, "");// 会留下最后句点
                        toList[i] = toList[i].Substring(0, toList[i].Length - 1); // 去掉句点

                        toListMap[toList[i]] = filePath;
                    }
                }
            }

            // 删文件和manifest
            for (var i = 0; i < toList.Count; i++)
            {
                if (!srcList.Contains(toList[i]))
                {
                    var filePath = toListMap[toList[i]];
                    var manifestPath = filePath + ".manifest";
                    File.Delete(filePath);
                    Debug.LogWarning("Delete... " + filePath);
                    if (File.Exists(manifestPath))
                    {
                        File.Delete(manifestPath);
                        Debug.LogWarning("Delete... " + manifestPath);
                    }

                    
                }
            }
        }

        [MenuItem("KEngine/AssetBundle/Build All to All Platforms")]
        public static void BuildAllAssetBundlesToAllPlatforms()
        {
            var platforms = new List<BuildTarget>()
            {
                BuildTarget.iOS,
                BuildTarget.Android,
                BuildTarget.StandaloneWindows,
                BuildTarget.StandaloneOSXIntel,
            };

            // Build all support platforms asset bundle
            var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            platforms.Remove(currentBuildTarget);
            BuildAllAssetBundles();

            foreach (var platform in platforms)
            {
                if (EditorUserBuildSettings.SwitchActiveBuildTarget(platform))
                    BuildAllAssetBundles();
            }

            // revert platform 
            EditorUserBuildSettings.SwitchActiveBuildTarget(currentBuildTarget);
        }

        [MenuItem("KEngine/AssetBundle/ReBuild All")]
        public static void ReBuildAllAssetBundles()
        {
            var outputPath = GetExportPath(EditorUserBuildSettings.activeBuildTarget);
            Directory.Delete(outputPath, true);

            Debug.Log("Delete folder: " + outputPath);

            BuildAllAssetBundles();
        }

        [MenuItem("KEngine/AssetBundle/Build All %&b")]
        public static void BuildAllAssetBundles()
        {
            if (EditorApplication.isPlaying)
            {
                Log.Error("Cannot build in playing mode! Please stop!");
                return;
            }
            MakeAssetBundleNames();
            var outputPath = GetExportPath(EditorUserBuildSettings.activeBuildTarget);
            Log.Info("Asset bundle start build to: {0}", outputPath);
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
            Log.Info("Asset bundle build success.");
        }

#endif
        #region 打包功能

        /// <summary>
        /// 获取完整的打包路径，并确保目录存在
        /// </summary>
        /// <param name="path"></param>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static string MakeSureExportPath(string path, BuildTarget buildTarget, KResourceQuality quality)
        {
            path = BuildTools.GetExportPath(buildTarget, quality) + path;

            path = path.Replace("\\", "/");

            string exportDirectory = path.Substring(0, path.LastIndexOf('/'));

            if (!System.IO.Directory.Exists(exportDirectory))
                System.IO.Directory.CreateDirectory(exportDirectory);

            return path;
        }

        /// <summary>
        /// Extra Flag ->   ex:  Android/  AndroidSD/  AndroidHD/
        /// </summary>
        /// <param name="platfrom"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static string GetExportPath(BuildTarget platfrom, KResourceQuality quality = KResourceQuality.Sd)
        {
            string basePath =
                Path.GetFullPath(KEngine.AppEngine.GetConfig(KEngineDefaultConfigs.AssetBundleBuildRelPath));

            if (File.Exists(basePath))
            {
                BuildTools.ShowDialog("路径配置错误: " + basePath);
                throw new System.Exception("路径配置错误");
            }
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string path = null;
            var platformName = KResourceModule.BuildPlatformName;
            if (quality != KResourceQuality.Sd) // SD no need add
                platformName += quality.ToString().ToUpper();

            path = basePath + "/" + platformName + "/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static void ClearConsole()
        {
#if UNITY_2018_1_OR_NEWER
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            var type = assembly.GetType("UnityEditor.LogEntries");
#else
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            System.Type type = assembly.GetType("UnityEditorInternal.LogEntries");
#endif
 
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(null, null);
        }

        public static bool ShowDialog(string msg, string title = "提示", string button = "确定")
        {
            return EditorUtility.DisplayDialog(title, msg, button);
        }

        public static void ShowDialogSelection(string msg, Action yesCallback)
        {
            if (EditorUtility.DisplayDialog("确定吗", msg, "是!", "不！"))
            {
                yesCallback();
            }
        }

#if UNITY_4
		public static void PushAssetBundle (Object asset, string path)
		{
			BuildPipeline.PushAssetDependencies ();
			BuildAssetBundle (asset, path);
			PushedAssetCount++;
		}

		public static void PopAllAssetBundle ()
		{
			for (int i = 0; i < PushedAssetCount; ++i) {
				BuildPipeline.PopAssetDependencies ();
			}
			PushedAssetCount = 0;
		}

		public static void PopAssetBundle ()
		{
			BuildPipeline.PopAssetDependencies ();
			PushedAssetCount--;
		}

#endif

        #endregion

        public static void BuildError(string fmt, params string[] args)
        {
            fmt = "[BuildError]" + fmt;
            Debug.LogError(string.Format(fmt, args));
        }

#if UNITY_4
		public static uint BuildAssetBundle (Object asset, string path)
		{
			return BuildAssetBundle (asset, path, EditorUserBuildSettings.activeBuildTarget, KResourceModule.Quality);
		}

		public static uint BuildAssetBundle (Object asset, string path, BuildTarget buildTarget, KResourceQuality quality)
		{
			if (asset == null || string.IsNullOrEmpty (path)) {
				BuildError ("BuildAssetBundle: {0}", path);
				return 0;
			}

			var assetNameWithoutDir = asset.name.Replace ("/", "").Replace ("\\", ""); // 防止多重目录...
			string tmpPrefabPath = string.Format ("Assets/{0}.prefab", assetNameWithoutDir);

			PrefabType prefabType = PrefabUtility.GetPrefabType (asset);

			string relativePath = path;
			path = MakeSureExportPath (path, buildTarget, quality);

			var assetPath = AssetDatabase.GetAssetPath (asset);
			CheckAndLogDependencies (assetPath);

			uint crc = 0;
			if (asset is Texture2D) {
				if (!string.IsNullOrEmpty (assetPath)) { // Assets内的纹理
					// Texutre不复制拷贝一份
					_DoBuild (out crc, asset, null, path, relativePath, buildTarget);
				} else {
					// 内存的图片~临时创建Asset, 纯正的图片， 使用Sprite吧
					var memoryTexture = asset as Texture2D;
					var memTexName = memoryTexture.name;

					var tmpTexPath = string.Format ("Assets/Tex_{0}_{1}.png", memoryTexture.name, Path.GetRandomFileName ());

					Log.Warning ("【BuildAssetBundle】Build一个非Asset 的Texture: {0}", memoryTexture.name);

					File.WriteAllBytes (tmpTexPath, memoryTexture.EncodeToPNG ());
					AssetDatabase.ImportAsset (tmpTexPath,
						ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
					var tmpTex = (Texture2D)AssetDatabase.LoadAssetAtPath (tmpTexPath, typeof(Texture2D));

					asset = tmpTex;
					try {
						asset.name = memTexName;

						_DoBuild (out crc, asset, null, path, relativePath, buildTarget);
					} catch (Exception e) {
						Log.LogException (e);
					}

					File.Delete (tmpTexPath);
					if (File.Exists (tmpTexPath + ".meta"))
						File.Delete (tmpTexPath + ".meta");
				}
			} else if ((prefabType == PrefabType.None && assetPath == string.Empty) ||
			           (prefabType == PrefabType.ModelPrefabInstance)) { // 非prefab对象
				Object tmpInsObj = (GameObject)GameObject.Instantiate (asset); // 拷出来创建Prefab
				Object tmpPrefab = PrefabUtility.CreatePrefab (tmpPrefabPath, (GameObject)tmpInsObj,
					                   ReplacePrefabOptions.Default);
				CheckAndLogDependencies (tmpPrefabPath);
				asset = tmpPrefab;

				_DoBuild (out crc, asset, null, path, relativePath, buildTarget);

				GameObject.DestroyImmediate (tmpInsObj);
				AssetDatabase.DeleteAsset (tmpPrefabPath);
			} else if (prefabType == PrefabType.PrefabInstance) {
				var prefabParent = PrefabUtility.GetPrefabParent (asset);
				_DoBuild (out crc, prefabParent, null, path, relativePath, buildTarget);
			} else {
				//Log.Error("[Wrong asse Type] {0}", asset.GetType());
				_DoBuild (out crc, asset, null, path, relativePath, buildTarget);
			}
			return crc;
		}
#endif

        /// <summary>
        /// 检查如果有依赖，报出
        /// 检查prefab中存在prefab依赖，进行打散！
        /// </summary>
        /// <param name="assetPath"></param>
        public static void CheckAndLogDependencies(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return;

            // 输出依赖
            var depSb = new StringBuilder();
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            var depsArray = EditorUtility.CollectDependencies(new[] { asset });
            if (depsArray != null && depsArray.Length > 0)
            {
                if (depsArray.Length == 1 && depsArray[0] == asset)
                {
                    // 自己依赖自己的忽略掉
                }
                else
                {
                    foreach (var depAsset in depsArray)
                    {
                        var depAssetPath = AssetDatabase.GetAssetPath(depAsset);
                        depSb.AppendLine(string.Format("{0} --> {1} <{2}>", depAssetPath, depAsset.name, depAsset.GetType()));
                    }
                    Log.Info("[BuildAssetBundle]Asset: {0} has dependencies: \n{1}", assetPath, depSb.ToString());
                }
            }
        }

        [MenuItem("Assets/Print Aseet Dependencies", false, 100)]
        public static void MenuCheckAndLogDependencies()
        {
            var obj = Selection.activeObject;
            if (obj == null)
            {
                Debug.LogError("No selection object");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(obj);
            BuildTools.CheckAndLogDependencies(assetPath);
        }

#if UNITY_4
		private static void _DoBuild (out uint crc, Object asset, Object[] subAssets, string path, string relativePath,
		                              BuildTarget buildTarget)
		{
			if (BeforeBuildAssetBundleEvent != null)
				BeforeBuildAssetBundleEvent (asset, path, relativePath);

			if (subAssets == null) {
				subAssets = new[] { asset };
			} else {
				var listSubAsset = new List<Object> (subAssets);
				if (!listSubAsset.Contains (asset)) {
					listSubAsset.Add (asset);
				}
				subAssets = listSubAsset.ToArray ();
			}

			var time = DateTime.Now;
			BuildPipeline.BuildAssetBundle (
				asset,
				subAssets,
				path,
				out crc,
				BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets |
				BuildAssetBundleOptions.DeterministicAssetBundle,
				buildTarget);

			Log.Info ("生成文件： {0}, 耗时: {1:F5}", path, (DateTime.Now - time).TotalSeconds);

			if (AfterBuildAssetBundleEvent != null)
				AfterBuildAssetBundleEvent (asset, path, relativePath);
		}

		public static uint BuildScriptableObject<T> (T scriptObject, string path) where T : ScriptableObject
		{
			return BuildScriptableObject (scriptObject, path, EditorUserBuildSettings.activeBuildTarget,
				KResourceModule.Quality);
		}

		public static uint BuildScriptableObject<T> (T scriptObject, string path, BuildTarget buildTarget,
		                                             KResourceQuality quality) where T : ScriptableObject
		{
			const string tempAssetPath = "Assets/~Temp.asset";
			AssetDatabase.CreateAsset (scriptObject, tempAssetPath);
			T tempObj = (T)AssetDatabase.LoadAssetAtPath (tempAssetPath, typeof(T));

			if (tempObj == null) {
				throw new System.Exception ();
			}

			uint crc = BuildTools.BuildAssetBundle (tempObj, path, buildTarget, quality);
			AssetDatabase.DeleteAsset (tempAssetPath);

			return crc;
		}

#endif
        public static void CopyFolder(string sPath, string dPath)
        {
            if (!Directory.Exists(dPath))
            {
                Directory.CreateDirectory(dPath);
            }

            DirectoryInfo sDir = new DirectoryInfo(sPath);
            FileInfo[] fileArray = sDir.GetFiles();
            foreach (FileInfo file in fileArray)
            {
                if (file.Extension != ".meta")
                    file.CopyTo(dPath + "/" + file.Name, true);
            }

            DirectoryInfo[] subDirArray = sDir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirArray)
            {
                CopyFolder(subDir.FullName, dPath + "/" + subDir.Name);
            }
        }

        /// <summary>
        /// 是否有指定宏呢
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool HasDefineSymbol(string symbol)
        {
            string symbolStrs =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> symbols =
                new List<string>(symbolStrs.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            return symbols.Contains(symbol);
        }

        /// <summary>
        /// 移除指定宏
        /// </summary>
        /// <param name="symbol"></param>
        public static void RemoveDefineSymbols(string symbol)
        {
            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                List<string> symbols =
                    new List<string>(symbolStr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                if (symbols.Contains(symbol))
                    symbols.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols.ToArray()));
            }
        }

        /// <summary>
        /// 添加指定宏（不重复）
        /// </summary>
        /// <param name="symbol"></param>
        public static void AddDefineSymbols(string symbol)
        {
            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                List<string> symbols =
                    new List<string>(symbolStr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                if (!symbols.Contains(symbol))
                {
                    symbols.Add(symbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols.ToArray()));
                }
            }
        }

        public static bool IsWin32
        {
            get
            {
                var os = Environment.OSVersion;
                return os.ToString().Contains("Windows");
            }
        }

        // 执行Python文件！获取返回值
        public static string ExecutePyFile(string pyFileFullPath, string arguments)
        {
            string pythonExe = null;
            if (IsWin32)
            {
                var guids = AssetDatabase.FindAssets("py");
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (Path.GetFileName(assetPath) == "py.exe")
                    {
                        pythonExe = assetPath; // Python地址
                        break;
                    }
                }
            }
            else
            {
                pythonExe = "python"; // linux or mac
            }


            if (string.IsNullOrEmpty(pythonExe))
            {
                Log.Error("无法找到py.exe, 或python指令");
                return "Error: Not found python";
            }

            string allOutput = null;
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = pythonExe;
                process.StartInfo.Arguments = pyFileFullPath + " " + arguments;
                //process.StartInfo.UseShellExecute = false;
                ////process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                var tips = string.Format("ExecutePython: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                Log.Info(tips);
                EditorUtility.DisplayProgressBar("Python...", tips, .5f);

                process.Start();

                allOutput = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                Log.Info("PyExecuteResult: {0}", allOutput);

                EditorUtility.ClearProgressBar();

                return allOutput;
            }
        }

        /* TODO: CFolderSyncTool
            public static void DeleteLink(string linkPath)
            {
                var os = Environment.OSVersion;
                if (os.ToString().Contains("Windows"))
                {
                    CFolderSyncTool.ExecuteCommand(string.Format("rmdir \"{0}\"", linkPath));
                }
                else if (os.ToString().Contains("Unix"))
                {
                    CFolderSyncTool.ExecuteCommand(string.Format("rm -Rf \"{0}\"", linkPath));
                }
                else
                {
                    Log.Error("[SymbolLinkFolder]Error on OS: {0}", os.ToString());
                }
            }

            public static void SymbolLinkFolder(string srcFolderPath, string targetPath)
            {
                var os = Environment.OSVersion;
                if (os.ToString().Contains("Windows"))
                {
                    CFolderSyncTool.ExecuteCommand(string.Format("mklink /J \"{0}\" \"{1}\"", targetPath, srcFolderPath));
                }
                else if (os.ToString().Contains("Unix"))
                {
                    var fullPath = Path.GetFullPath(targetPath);
                    if (fullPath.EndsWith("/"))
                    {
                        fullPath = fullPath.Substring(0, fullPath.Length - 1);
                        fullPath = Path.GetDirectoryName(fullPath);
                    }
                    CFolderSyncTool.ExecuteCommand(string.Format("ln -s {0} {1}", Path.GetFullPath(srcFolderPath), fullPath));
                }
                else
                {
                    Log.Error("[SymbolLinkFolder]Error on OS: {0}", os.ToString());
                }
            }
            */

        //[Obsolete("Please use KAssetVersionControl")]
        //public static bool CheckNeedBuildWithMeta(params string[] assetPath)
        //{
        //    return KAssetVersionControl.TryCheckNeedBuildWithMeta(assetPath);
        //}
        //[Obsolete("Please use KAssetVersionControl")]
        //public static void MarkBuildVersion(string assetPath)
        //{
        //    KAssetVersionControl.TryMarkBuildVersion(assetPath);
        //}
    }

}
