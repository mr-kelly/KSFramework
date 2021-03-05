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
using System.Text;
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
        /// 标记，是否正在打开提示配置变更对话框
        /// </summary>
        private static bool _isPopUpConfirm = false;

        static SettingModuleEditor()
        {
            if (Directory.Exists(AppConfig.SettingSourcePath))
            {
                // when build app, ensure compile ALL settings
                KUnityEditorEventCatcher.OnBeforeBuildAppEvent -= CompileSettings;
                KUnityEditorEventCatcher.OnBeforeBuildAppEvent += CompileSettings;
                // when play editor, ensure compile settings
                KUnityEditorEventCatcher.OnWillPlayEvent -= QuickCompileSettings;
                KUnityEditorEventCatcher.OnWillPlayEvent += QuickCompileSettings;

                // watch files, when changed, compile settings
                new KDirectoryWatcher(AppConfig.SettingSourcePath, (o, args) =>
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
                Debug.Log("[SettingModuleEditor]Watching directory: " + AppConfig.SettingSourcePath);
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
        /// do compile settings
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
            
            List<TableCompileResult> results = null;
            if (AppConfig.IsUseLuaConfig)
            {
                Log.Info("Start Compile to lua");
                var genParam = new GenParam()
                {
                    settingCodeIgnorePattern = AppConfig.SettingCodeIgnorePattern,
                    genCSharpClass = false, genCodeFilePath = null, forceAll = true, ExportLuaPath = AppConfig.ExportLuaPath
                };
                var compilerParam = new CompilerParam() {CanExportTsv = false, ExportTsvPath = AppConfig.ExportTsvPath, ExportLuaPath = AppConfig.ExportLuaPath};
                results = new BatchCompiler().CompileAll(AppConfig.SettingSourcePath, AppConfig.ExportLuaPath, genParam,compilerParam);     
            }
            else
            {
                if (string.IsNullOrEmpty(AppConfig.ExportTsvPath))
                {
                    Log.Error("Need to KEngineConfig: ExportTsvPath");
                    return;
                }
                Log.Info("Start Compile to c#+tsv");
                
                var template = force ? (forceTemplate ?? DefaultTemplate.GenCodeTemplateOneFile) : null; 
                var genParam = new GenParam(){forceAll = force, genCSharpClass = true,genCodeFilePath = AppConfig.ExportCSharpPath,
                    genCodeTemplateString = template,changeExtension = AppConfig.SettingExt,
                    settingCodeIgnorePattern = AppConfig.SettingCodeIgnorePattern,nameSpace = "AppSettings"};
                var compilerParam = new CompilerParam() {CanExportTsv = true, ExportTsvPath = AppConfig.ExportTsvPath, ExportLuaPath = null};
                results = new BatchCompiler().CompileAll(AppConfig.SettingSourcePath, AppConfig.ExportTsvPath, genParam,compilerParam);
            }
            
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