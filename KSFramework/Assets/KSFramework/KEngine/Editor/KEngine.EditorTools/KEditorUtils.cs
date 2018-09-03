#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KEngineUtils.cs
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace KUnityEditorTools
{
    /// <summary>
    /// Shell / cmd / 等等常用Editor需要用到的工具集
    /// </summary>
    public class KEditorUtils
    {
        /// <summary>
        /// 用于非主线程里执行主线程的函数
        /// </summary>
        internal static Queue<Action> _mainThreadActions = new Queue<Action>();

        static KEditorUtils()
        {
            KUnityEditorEventCatcher.OnEditorUpdateEvent -= OnEditorUpdate;
            KUnityEditorEventCatcher.OnEditorUpdateEvent += OnEditorUpdate;
        }

        /// <summary>
        /// 捕获Unity Editor update事件
        /// </summary>
        private static void OnEditorUpdate()
        {
            // 主线程委托
            while (_mainThreadActions.Count > 0)
            {
                var action = _mainThreadActions.Dequeue();
                if (action != null) action();
            }
        }

        /// <summary>
        /// 异步线程回到主线程进行回调
        /// </summary>
        /// <param name="action"></param>
        public static void CallMainThread(Action action)
        {
            _mainThreadActions.Enqueue(action);
        }

        /// <summary>
        /// 清除Console log
        /// </summary>
        public static void ClearConsoleLog()
        {
            Assembly assembly = Assembly.GetAssembly(typeof (ActiveEditorTracker));
            Type type = assembly.GetType("UnityEditorInternal.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        /// <summary>
        /// 执行批处理命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="workingDirectory"></param>
        public static void ExecuteCommand(string command, string workingDirectory = null)
        {
            var fProgress = .1f;
            EditorUtility.DisplayProgressBar("KEditorUtils.ExecuteCommand", command, fProgress);

            try
            {
                string cmd;
                string preArg;
                var os = Environment.OSVersion;

                Debug.Log(String.Format("[ExecuteCommand]Command on OS: {0}", os.ToString()));
                if (os.ToString().Contains("Windows"))
                {
                    cmd = "cmd.exe";
                    preArg = "/C ";
                }
                else
                {
                    cmd = "sh";
                    preArg = "-c ";
                }
                Debug.Log("[ExecuteCommand]" + command);
                var allOutput = new StringBuilder();
                using (var process = new Process())
                {
                    if (workingDirectory != null)
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.FileName = cmd;
                    process.StartInfo.Arguments = preArg + "\"" + command + "\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    while (true)
                    {
                        var line = process.StandardOutput.ReadLine();
                        if (line == null)
                            break;
                        allOutput.AppendLine(line);
                        EditorUtility.DisplayProgressBar("[ExecuteCommand] " + command, line, fProgress);
                        fProgress += .001f;
                    }

                    var err = process.StandardError.ReadToEnd();
                    if (!String.IsNullOrEmpty(err))
                    {
                        Debug.LogError(String.Format("[ExecuteCommand] {0}", err));
                    }
                    process.WaitForExit();
                }
                Debug.Log("[ExecuteResult]" + allOutput);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public delegate void EachDirectoryDelegate(string fileFullPath, string fileRelativePath);

        /// <summary>
        /// 递归一个目录所有文件，callback
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="eachCallback"></param>
        public static void EachDirectoryFiles(string dirPath, EachDirectoryDelegate eachCallback)
        {
            foreach (var filePath in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                var fileRelativePath = filePath.Replace(dirPath, "");
                if (fileRelativePath.StartsWith("/") || fileRelativePath.StartsWith("\\"))
                    fileRelativePath = fileRelativePath.Substring(1, fileRelativePath.Length - 1);

                var cleanFilePath = filePath.Replace("\\", "/");
                fileRelativePath = fileRelativePath.Replace("\\", "/");
                eachCallback(cleanFilePath, fileRelativePath);
            }
        }

        /// <summary>
        /// 将丑陋的windows路径，替换掉\字符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetCleanPath(string path)
        {
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// 在指定目录中搜寻字符串并返回匹配}
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="searchWord"></param>
        /// <param name="fileFilter"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Match>> FindStrMatchesInFolderTexts(string sourceFolder, Regex searchWord,
            Func<string, bool> fileFilter = null)
        {
            var retMatches = new Dictionary<string, List<Match>>();
            var allFiles = new List<string>();
            AddFileNamesToList(sourceFolder, allFiles);
            foreach (string fileName in allFiles)
            {
                if (fileFilter != null && !fileFilter(fileName))
                    continue;

                retMatches[fileName] = new List<Match>();
                string contents = File.ReadAllText(fileName);
                var matches = searchWord.Matches(contents);
                if (matches.Count > 0)
                {
                    for (int i = 0; i < matches.Count; i++)
                    {
                        retMatches[fileName].Add(matches[i]);
                    }

                }
            }
            return retMatches;
        }

        private static void AddFileNamesToList(string sourceDir, List<string> allFiles)
        {

            string[] fileEntries = Directory.GetFiles(sourceDir);
            foreach (string fileName in fileEntries)
            {
                allFiles.Add(fileName);
            }

            //Recursion    
            string[] subdirectoryEntries = Directory.GetDirectories(sourceDir);
            foreach (string item in subdirectoryEntries)
            {
                // Avoid "reparse points"
                if ((File.GetAttributes(item) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    AddFileNamesToList(item, allFiles);
                }
            }

        }

        /// <summary>
        /// 从所有的程序集收集指定类型，public, 包括继承的
        /// </summary>
        /// <returns></returns>
        public static IList<Type> FindAllPublicTypes(Type findType)
        {
            var list = new List<Type>();
            Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int n = 0; n < Assemblies.Length; n++)
            {
                Assembly asm = Assemblies[n];
                foreach (var type in asm.GetExportedTypes())
                {
                    if (findType.IsAssignableFrom(type) || findType == type)
                    {
                        list.Add(type);
                    }
                }
            }
            return list;
        }

    }
}