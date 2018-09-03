#region  Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Asset Bundle framework for Unity3D
// ===================================
// 
// Filename: KAssetVersionControl.cs
// Date:     2016/01/21
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
// License along with this library

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KEngine.Editor
{
    [Obsolete("Please use AssetVersionControl instead!")]
    public class KAssetVersionControl : AssetVersionControl
    {
    }

    /// <summary>
    /// 基于Tab表格的纯文本简单资源差异管理器
    /// </summary>
    public class AssetVersionControl : IDisposable
    {
        public static AssetVersionControl Current;

        private bool _isRebuild = false;

        /// <summary>
        /// 持久化，硬盘的
        /// </summary>
        private Dictionary<string, BuildRecord> StoreBuildVersion = new Dictionary<string, BuildRecord>();

        private Dictionary<string, BuildRecord> InstanceBuildVersion = new Dictionary<string, BuildRecord>();

        /// <summary>
        /// Getter获取所有记录的字典, 对这个字典的修改，将会影响写入文件的结果
        /// </summary>
        public Dictionary<string, BuildRecord> BuildRecords
        {
            get { return StoreBuildVersion; }
        } 

        /// <summary>
        /// 资源打包周期版本管理
        /// </summary>
        /// <param name="rebuild">如果是rebuild，将无视之前的差异打包信息</param>
        public AssetVersionControl(bool rebuild = false)
        {
            if (Current != null)
            {
                Log.Error("New a KAssetVersionControl, but already has annother instance using! Be careful!");
            }

            Current = this;

            _isRebuild = rebuild;

            Log.Warning("================== KAssetVersionControl Begin ======================");

            SetupHistory();

            //KDependencyBuild.Clear();
#if UNITY_4
            BuildTools.AfterBuildAssetBundleEvent += OnAfterBuildAssetBundleEvent;
#endif
        }

        public void Dispose()
        {
            WriteVersion();
            if (BuildedList.Count > 0)
            {
                //ProductMd5_CurPlatform();
                Log.Info("一共打包了{0}個資源:\n{1}", BuildedList.Count, string.Join("\n", BuildedList.ToArray()));
            }
            else
                Log.Info("没有任何需要打包的资源！");

            //KDependencyBuild.SaveBuildAction();

            Current = null;
#if UNITY_4
            BuildTools.AfterBuildAssetBundleEvent -= OnAfterBuildAssetBundleEvent;
#endif
            //KDependencyBuild.Clear();
        }

        private void OnAfterBuildAssetBundleEvent(Object arg1, string arg2, string arg3)
        {
            //BuildedList.Add(arg3);
        }

#region 资源版本管理相关

        public class BuildRecord
        {
            public string MD5;
            public int ChangeCount;
            public string DateTime;

            public BuildRecord()
            {
                MD5 = null;
                DateTime = System.DateTime.Now.ToString();
                ChangeCount = 0;
            }

            public BuildRecord(string md5, string dt, int changeCount)
            {
                MD5 = md5;
                DateTime = dt;
                ChangeCount = changeCount;
            }

            public void Mark(string md5)
            {
                MD5 = md5;
                DateTime = System.DateTime.Now.ToString();
                ChangeCount++;
            }
        }

        public void WriteVersion()
        {
            string path = GetBuildVersionTab();
            // MakeSureExportPath(VerCtrlInfo.VerFile, EditorUserBuildSettings.activeBuildTarget);
            KTabFile tabFile = new KTabFile();
            tabFile.NewColumn("AssetPath");
            tabFile.NewColumn("AssetMD5");
            tabFile.NewColumn("AssetDateTime");
            tabFile.NewColumn("ChangeCount");

            foreach (var node in StoreBuildVersion)
            {
                int row = tabFile.NewRow();
                tabFile.SetValue(row, "AssetPath", node.Key);
                tabFile.SetValue(row, "AssetMD5", node.Value.MD5);
                tabFile.SetValue(row, "AssetDateTime", node.Value.DateTime);
                tabFile.SetValue(row, "ChangeCount", node.Value.ChangeCount);
            }

            tabFile.Save(path);
        }

        private void SetupHistory()
        {
            BuildedList.Clear();

            string verFile = GetBuildVersionTab();
            //MakeSureExportPath(VerCtrlInfo.VerFile, EditorUserBuildSettings.activeBuildTarget);
            KTabFile tabFile;
            if (File.Exists(verFile))
            {
                tabFile = KTabFile.LoadFromFile(verFile);

                foreach (KTabFile.RowInterator row in tabFile)
                {
                    var assetPath = row.GetString("AssetPath");
                    StoreBuildVersion[assetPath] =
                        new BuildRecord(
                            row.GetString("AssetMD5"),
                            row.GetString("AssetDateTime"),
                            row.GetInteger("ChangeCount"));
                }
            }
        }

        public string GetAssetLastBuildMD5(string assetPath)
        {
            BuildRecord md5;
            if (StoreBuildVersion.TryGetValue(assetPath, out md5))
            {
                return md5.MD5;
            }

            return "";
        }

        /// <summary>
        /// 处理过的build文件列表
        /// </summary>
        private List<string> BuildedList = new List<string>(); // 累计Build了多少次，用于版本控制时用的

        // Prefab Asset打包版本號記錄
        public static string GetBuildVersionTab()
        {
            return Application.dataPath + "/../" + KEngineDef.ResourcesBuildInfosDir + "/ArtBuildResource_" +
                   KResourceModule.BuildPlatformName + ".txt";
        }

        public static bool TryCheckFileBuild(string filePath)
        {
            if (Current == null)
                return true;

            return Current.DoCheckBuild(filePath, true);
        }


        /// <summary>
        /// 只标记，无MD5比较
        /// </summary>
        /// <param name="strKey"></param>
        public static void TryMarkRecord(string strKey)
        {
            if (Current == null) return;
            Current.MarkRecord(strKey);
        }

        /// <summary>
        /// 不进行文件比较，只判断是否存在
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public static bool TryCheckExistRecord(string strKey)
        {
            if (Current == null)
                return false;
            return Current.DoCheckExistRecord(strKey);
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <returns></returns>
        public static bool TryCheckNeedBuildWithMeta(params string[] sourceFiles)
        {
            if (Current == null)
                return true;

            return Current.CheckFileBuildWithMeta(sourceFiles);
        }

        ///// <summary>
        ///// 判断路径，并且尝试判断meta文件
        ///// </summary>
        public bool CheckFileBuildWithMeta(params string[] sourceFiles)
        {
            foreach (string file in sourceFiles)
            {
                if (DoCheckBuild(file, true) || DoCheckBuild(file + ".meta", false))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 只检查是否存在记录，不进行文件检查
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        private bool DoCheckExistRecord(string strKey, out BuildRecord assetMd5)
        {
            return StoreBuildVersion.TryGetValue(strKey, out assetMd5);
        }

        private bool DoCheckExistRecord(string strKey)
        {
            BuildRecord assetMd5;
            return StoreBuildVersion.TryGetValue(strKey, out assetMd5);
        }

        /// <summary>
        /// 检查是否需要build，
        /// 文件，要进行MD5校验
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isFile"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        private bool DoCheckBuild(string filePath, bool log = true)
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
#else
            var currentScene = EditorApplication.currentScene;
#endif
            BuildRecord assetMd5;
            if (!File.Exists(filePath))
            {
                if (log)
                    Log.Error("[DoCheckBuild]Not Found 无法找到文件 {0}", filePath);

                if (filePath.Contains("unity_builtin_extra"))
                {
                    Log.Error(
                        "[DoCheckBuild]Find unity_builtin_extra resource to build!! Please check it! current scene: {0}",
                        currentScene);
                }
                return false;
            }

            if (InstanceBuildVersion.ContainsKey(filePath)) // 本次打包已经处理过，就不用重复处理了
                return false;

            if (_isRebuild) // 所有rebuild，不用判断，直接需要build, 保留change count的正确性
                return true;

            // 不存在记录，则需要打包吧
            if (!DoCheckExistRecord(filePath, out assetMd5))
                return true;

            if (KTool.MD5_File(filePath) != assetMd5.MD5)
                return true; // different

            return false;
        }

        /// <summary>
        /// 只标记，不做MD5比较
        /// </summary>
        /// <param name="strKey"></param>
        public void MarkRecord(string strKey)
        {
            StoreBuildVersion[strKey] = InstanceBuildVersion[strKey] = new BuildRecord(); // ensure in dict
            Current.BuildedList.Add(strKey);
        }

        /// <summary>
        /// 标记一个路径为打包
        /// </summary>
        public void MarkBuildVersion(params string[] sourceFiles)
        {
            if (sourceFiles == null || sourceFiles.Length == 0)
                return;

            foreach (string file in sourceFiles)
            {
                //StoreBuildVersion[file] = GetAssetVersion(file);
                BuildRecord theRecord;
                var nowMd5 = KTool.MD5_File(file);
                if (!StoreBuildVersion.TryGetValue(file, out theRecord))
                {
                    theRecord = new BuildRecord();
                    theRecord.Mark(nowMd5);
                }
                else
                {
                    if (nowMd5 != theRecord.MD5) // 只有改变时才会mark，所以可能会出现情况，rebuild时，change count不改变
                    {
                        theRecord.Mark(nowMd5);
                    }
                }
                StoreBuildVersion[file] = InstanceBuildVersion[file] = theRecord; // ensure in dict

                string metaFile = file + ".meta";
                if (File.Exists(metaFile))
                {
                    BuildRecord theMetaRecord;
                    var nowMetaMd5 = KTool.MD5_File(metaFile);
                    if (!StoreBuildVersion.TryGetValue(metaFile, out theMetaRecord))
                    {
                        theMetaRecord = new BuildRecord();
                        theMetaRecord.Mark(nowMetaMd5);
                    }
                    else
                    {
                        if (nowMetaMd5 != theMetaRecord.MD5)
                            theMetaRecord.Mark(nowMetaMd5);
                    }

                    StoreBuildVersion[metaFile] = InstanceBuildVersion[metaFile] = theMetaRecord; // ensure in dict
                }
                // meta不记录
                Current.BuildedList.Add(file);
            }
        }

        public static void TryMarkBuildVersion(params string[] sourceFiles)
        {
            if (Current == null)
                return;
            Current.MarkBuildVersion(sourceFiles);
        }

#endregion

        public static bool TryIsRebuild()
        {
            if (Current == null)
                return true;

            return Current._isRebuild;
        }
    }
}