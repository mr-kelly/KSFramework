#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KResourceBuilder.cs
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
using KEngine;
using UnityEditor;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace KEngine.Editor
{
    public abstract class KBuild_Base
    {
        public virtual string GetResourceBuildDir()
        {
            return KEngineDef.ResourcesBuildDir;
        }

        public virtual void BeforeExport()
        {
        }

        public abstract void Export(string path);

        public virtual void AfterExport()
        {
        }

        public abstract string GetDirectory();
        public abstract string GetExtention();

        /// <summary>
        /// 可以针对path进行过滤是否需要
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual bool FilterPath(string path)
        {
            return true;
        }
    }

    /// <summary>
    /// 项目资源导出器
    /// 根据策略，导出_ResourceBuild_的东西
    /// </summary>
    public partial class KResourceBuilder
    {
        /// <summary>
        /// 导出系列打包器
        /// </summary>
        /// <param name="exports"></param>
        public static void ProductExport(KBuild_Base[] exports)
        {
            foreach (var export in exports)
            {
                ProductExport(export);
            }
        }

        public static void ProductExport(KBuild_Base export)
        {
            Log.Info("Start Auto Build... {0}", export.GetType().Name);

            var time = DateTime.Now;
            try
            {
                string ext = export.GetExtention();
                string[] itemArray;

                if (ext.StartsWith("dir:")) // 目錄下的所有文件，包括子文件夾
                {
                    string newExt = ext.Replace("dir:", "");
                    itemArray = Directory.GetFiles("Assets/" + export.GetResourceBuildDir() + "/" + export.GetDirectory(),
                        newExt, SearchOption.AllDirectories);
                }
                else if (ext == "dir")
                    itemArray =
                        Directory.GetDirectories("Assets/" + export.GetResourceBuildDir() + "/" + export.GetDirectory());
                else if (ext == "")
                    itemArray = new string[0];
                else
                    itemArray = Directory.GetFiles("Assets/" + export.GetResourceBuildDir() + "/" + export.GetDirectory(),
                        export.GetExtention()); // 不包括子文件夾

                export.BeforeExport();
                int okItemCount = 0;
                foreach (string item in itemArray)
                {
                    var cleanPath = item.Replace("\\", "/");

                    if (!export.FilterPath(cleanPath))
                        continue;
                    okItemCount++;
                    EditorUtility.DisplayCancelableProgressBar("[ProductExport]", cleanPath, .5f);
                    try
                    {
                        export.Export(cleanPath);
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }

                    GC.Collect();
                    Resources.UnloadUnusedAssets();
                }
                export.AfterExport();


                Log.Info("Finish Auto Build: {0}, ItemsCount: {1}, Used Time: {2}", export.GetType().Name,
                    okItemCount, DateTime.Now - time);
            }
            catch (Exception e)
            {
                Log.Error("[Fail] Auto Build... {0}, Exception: {1}, Used Time: {2}, CurrentScene: {3}, Stack: {4}",
                    export.GetType().Name,
                    e.Message + "," + (e.InnerException != null ? e.InnerException.Message : ""), DateTime.Now - time,
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                    EditorSceneManager.GetActiveScene().path,
#else
                    EditorApplication.currentScene,
#endif
                    e.StackTrace);
            }

            GC.Collect();
        }
    }

}