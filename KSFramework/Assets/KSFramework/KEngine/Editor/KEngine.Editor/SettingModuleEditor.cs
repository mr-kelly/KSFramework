#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: SettingModuleEditor.cs
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid;
using KUnityEditorTools;
using TableML.Compiler;
using UnityEditor;
using UnityEngine;

namespace KEngine.Editor
{
    /// <summary>
    /// For SettingModule
    /// </summary>
    [InitializeOnLoad]
    public class SettingModuleEditor
    {
        /// <summary>
        /// 是否自动在编译配置表时生成静态代码，如果不需要，外部设置false
        /// </summary>
        public static bool AutoGenerateCode = true;

        /// <summary>
        /// 当生成的类名，包含数组中字符时，不生成代码
        /// </summary>
        /// <example>
        /// GenerateCodeFilesFilter = new []
        /// {
        ///     "SubdirSubSubDirExample3",
        /// };
        /// </example>
        public static string[] GenerateCodeFilesFilter = null;

        /// <summary>
        /// 条件编译变量
        /// </summary>
        public static string[] CompileSettingConditionVars;

        /// <summary>
        /// 可以为模板提供额外生成代码块！返回string即可！
        /// 自定义[InitializeOnLoad]的类并设置这个委托
        /// </summary>
        public static CustomExtraStringDelegate CustomExtraString;
        public delegate string CustomExtraStringDelegate(TableCompileResult tableCompileResult);

        /// <summary>
        /// 编译出的后缀名, 可修改
        /// </summary>
        public static string SettingExtension
        {
            get
            {
                return AppEngine.GetConfig(KEngineDefaultConfigs.SettingExt);
            }
        }

        /// <summary>
        /// 生成代码吗？它的路径配置
        /// </summary>
        public static string SettingCodePath
        {
            get
            {
                var compilePath = AppEngine.GetConfig("KEngine.Setting", "SettingCompileCodePath", false);
                if (string.IsNullOrEmpty(compilePath))
                {
                    return "Assets/AppSettings.cs"; // default value
                }
                return compilePath;
            }
        }

        /// <summary>
        /// 标记，是否正在打开提示配置变更对话框
        /// </summary>
        private static bool _isPopUpConfirm = false;

        static SettingModuleEditor()
        {
            var path = SettingSourcePath;
            if (Directory.Exists(path))
            {
                // when build app, ensure compile ALL settings
                KUnityEditorEventCatcher.OnBeforeBuildPlayerEvent -= CompileSettings;
                KUnityEditorEventCatcher.OnBeforeBuildPlayerEvent += CompileSettings;
                // when play editor, ensure compile settings
                KUnityEditorEventCatcher.OnWillPlayEvent -= QuickCompileSettings;
                KUnityEditorEventCatcher.OnWillPlayEvent += QuickCompileSettings;

                // watch files, when changed, compile settings
                new KDirectoryWatcher(path, (o, args) =>
                {
                    if (_isPopUpConfirm) return;

                    _isPopUpConfirm = true;
                    KEditorUtils.CallMainThread(() =>
                    {
                        EditorUtility.DisplayDialog("Excel Setting Changed!", "Ready to Recompile All!", "OK");
                        QuickCompileSettings();
                        _isPopUpConfirm = false;
                    });
                });
                Debug.Log("[SettingModuleEditor]Watching directory: " + SettingSourcePath);
            }
        }

        static string SettingSourcePath
        {
            get
            {
                var sourcePath = AppEngine.GetConfig("KEngine.Setting", "SettingSourcePath");
                return sourcePath;
            }
        }

        [MenuItem("KEngine/Settings/Force Compile Settings + Code")]
        public static void CompileSettings()
        {
            DoCompileSettings(true);
        }
        [MenuItem("KEngine/Settings/Quick Compile Settings")]
        public static void QuickCompileSettings()
        {
            DoCompileSettings(false);
        }

        /// <summary>
        /// Custom the monitor trigger compile settings behaviour
        /// </summary>
        public static Action CustomCompileSettings;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="force">Whether or not,check diff.  false will be faster!</param>
        /// <param name="genCode">Generate static code?</param>
        public static void DoCompileSettings(bool force = true, string forceTemplate = null, bool canCustom = true)
        {
            if (canCustom && CustomCompileSettings != null)
            {
                CustomCompileSettings();
                return;
            }

            var sourcePath = SettingSourcePath;//AppEngine.GetConfig("SettingSourcePath");
            if (string.IsNullOrEmpty(sourcePath))
            {
                Log.Error("Need to KEngineConfig: SettingSourcePath");
                return;
            }
            var compilePath = AppEngine.GetConfig("KEngine.Setting", "SettingCompiledPath");
            if (string.IsNullOrEmpty(compilePath))
            {
                Log.Error("Need to KEngineConfig: SettingCompiledPath");
                return;
            }

            var bc = new BatchCompiler();

            var settingCodeIgnorePattern = AppEngine.GetConfig("KEngine.Setting", "SettingCodeIgnorePattern", false);
            var template = force ? (forceTemplate ?? DefaultTemplate.GenCodeTemplate) : null; // 
            var results = bc.CompileTableMLAll(sourcePath, compilePath, SettingCodePath, template, "AppSettings", SettingExtension, settingCodeIgnorePattern, force);

            //            CompileTabConfigs(sourcePath, compilePath, SettingCodePath, SettingExtension, force);
            var sb = new StringBuilder();
            foreach (var r in results)
            {
                sb.AppendLine(string.Format("Excel {0} -> {1}", r.ExcelFile, r.TabFileRelativePath));
            }
            Log.Info("TableML all Compile ok!\n{0}", sb.ToString());
            // make unity compile
            AssetDatabase.Refresh();
        }
    }

}