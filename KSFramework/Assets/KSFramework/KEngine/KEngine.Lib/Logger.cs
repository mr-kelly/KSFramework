#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: Log.cs
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
using System.Diagnostics;
using System.IO;
using System.Text;
#if !KENGINE_DLL
using UnityEngine;
#endif
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
#endif

namespace KEngine
{
    public enum LogLevel
    {
        All = 0,
        Trace,
        Debug,
        Info, // Info, default
        Warning,
        Error,
    }

    [Obsolete("The name 'Logger' conflict with Unity 5 'Logger', use 'Log' instead, at 2016/04/08")]
    public class Logger : Log
    {}

    /// <summary>
    /// KEngine Logger, file write + console output
    /// </summary>
    public class Log
    {
        public delegate void LogCallback(string condition, string stackTrace, LogLevel type);
        public static LogLevel LogLevel = LogLevel.Info;

        private static event LogCallback LogCallbackEvent;
        private static bool _hasRegisterLogCallback = false;

        /// <summary>
        /// 第一次使用时注册，之所以不放到静态构造器，因为多线程问题
        /// </summary>
        /// <param name="callback"></param>
        public static void AddLogCallback(LogCallback callback)
        {
            if (!_hasRegisterLogCallback)
            {
#if !KENGINE_DLL
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                Application.logMessageReceivedThreaded += GetUnityLogCallback(OnLogCallback);
#else
                Application.RegisterLogCallbackThreaded(GetUnityLogCallback(OnLogCallback));
#endif
#endif
                _hasRegisterLogCallback = true;
            }
            LogCallbackEvent += callback;
        }

#if !KENGINE_DLL
        static Application.LogCallback GetUnityLogCallback(LogCallback callback)
        {
            Application.LogCallback unityCallback = (c, s, type) =>
            {
                LogLevel logLevel;
                if (type == LogType.Error) logLevel = LogLevel.Error;
                else if (type == LogType.Warning) logLevel = LogLevel.Warning;
                else logLevel = LogLevel.Info;

                OnLogCallback(c, s, logLevel);
            };
            return unityCallback;
        }
#endif
        public static void RemoveLogCallback(LogCallback callback)
        {
            if (!_hasRegisterLogCallback)
            {
#if !KENGINE_DLL
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                Application.logMessageReceivedThreaded += GetUnityLogCallback(callback);
#else
                Application.RegisterLogCallbackThreaded(GetUnityLogCallback(OnLogCallback));
#endif
#endif
                _hasRegisterLogCallback = true;
            }
            LogCallbackEvent -= callback;
        }


#if !KENGINE_DLL
        public static readonly bool IsUnityEditor = false;
#endif

        public static event Action<string> LogErrorEvent;

        static Log()
        {
#if !KENGINE_DLL
            // isDebugBuild先预存起来，因为它是一个get_属性, 在非Unity主线程里不能用，导致多线程网络打印log时报错
            try
            {
                //IsDebugBuild = Debug.isDebugBuild;
                IsUnityEditor = Application.isEditor;
            }
            catch (Exception e)
            {
                Log.LogConsole_MultiThread("Log Static Constructor Failed!");
                Log.LogConsole_MultiThread(e.Message + " , " + e.StackTrace);
            }
#endif

#if UNITY_EDITOR
            InitLogUtility();
#endif
        }

        /// <summary>
        /// 是否输出到日志文件,默认false，需要初始化手工设置
        /// </summary>
        private static bool _isLogFile = false;
        public static bool IsLogFile
        {
            get { return _isLogFile; }
            set
            {
                _isLogFile = value;
                if (_isLogFile)
                {
                    AddLogCallback(LogFileCallbackHandler);
                }
                else
                {
                    RemoveLogCallback(LogFileCallbackHandler);
                }

            }
        }

        public static void LogFileCallbackHandler(string condition, string stacktrace, LogLevel type)
        {
            try
            {
                if (type > LogLevel.Warning)
                    LogToFile(condition + "\n\n");
                else
                    LogToFile(condition + stacktrace + "\n\n");
            }
            catch (Exception e)
            {
                LogToFile(string.Format("LogFileError: {0}, {1}", condition, e.Message));
            }
        }

        private static void OnLogCallback(string condition, string stacktrace, LogLevel type)
        {
            if (LogCallbackEvent != null)
            {
                lock (LogCallbackEvent)
                {
                    LogCallbackEvent(condition, stacktrace, type);
                }
            }
        }

        /// <summary>
        /// Check if a object null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatStr"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete("Use Debugger.Check instead")]
        public static bool Check(object obj, string formatStr = null, params object[] args)
        {
            if (obj != null) return true;

            if (string.IsNullOrEmpty(formatStr))
                formatStr = "[Check Null] Failed!";

            LogError("[!!!]" + formatStr, args);
            return false;
        }

        [Obsolete("Use Debugger.Assert instead")]
        public static void Assert(bool result)
        {
            if (result)
                return;

            LogErrorWithStack("Assertion Failed!", 2);

            throw new Exception("Assert"); // 中断当前调用
        }

        [Obsolete("Use Debugger.Assert instead")]
        public static void Assert(int result)
        {
            Assert(result != 0);
        }

        [Obsolete("Use Debugger.Assert instead")]
        public static void Assert(Int64 result)
        {
            Assert(result != 0);
        }

        [Obsolete("Use Debugger.Assert instead")]
        public static void Assert(object obj)
        {
            Assert(obj != null);
        }

        // 这个使用系统的log，这个很特别，它可以再多线程里用，其它都不能再多线程内用！！！
        public static void LogConsole_MultiThread(string log, params object[] args)
        {
#if !KENGINE_DLL
            if (IsUnityEditor)
                DoLog(log, args, LogLevel.Info);
            else
#endif
                Console.WriteLine(log, args);
        }

        public static void Trace(string log, params object[] args)
        {
            DoLog(log, args, LogLevel.Trace);
        }

        public static void Debug(string log, params object[] args)
        {
            DoLog(log, args, LogLevel.Debug);
        }

        //[Obsolete]
        //public static void Trace(string log, params object[] args)
        //{
        //    DoLog(string.Format(log, args), LogLevel.Debug);
        //}
        public static void Info(string log, params object[] args)
        {
            DoLog(log, args, LogLevel.Info);
        }

        public static void Logs(params object[] logs)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < logs.Length; ++i)
            {
                sb.Append(logs[i].ToString());
                sb.Append(", ");
            }
            DoLog(sb.ToString(), null, LogLevel.Info);
        }

        public static void LogException(Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Exception: {0}", e.Message);
            if (e.InnerException != null)
                sb.AppendFormat(" InnerException: {0}", e.InnerException.Message);

            LogErrorWithStack(sb.ToString() + " , " + e.StackTrace);
        }

        public static void LogErrorWithStack(string err = "", int stack = 2)
        {
            StackFrame sf = GetTopStack(stack);
            string log = string.Format("[ERROR]{0}\n\n{1}:{2}\t{3}", err, sf.GetFileName(), sf.GetFileLineNumber(),
                sf.GetMethod());
            Console.Write(log);
            DoLog(log, null, LogLevel.Error);

            if (LogErrorEvent != null)
                LogErrorEvent(err);
        }


        public static StackFrame GetTopStack(int stack = 2)
        {
            StackFrame[] stackFrames = new StackTrace(true).GetFrames();
            ;
            StackFrame sf = stackFrames[Math.Min(stack, stackFrames.Length - 1)];
            return sf;
        }

        public static void Error(string err, params object[] args)
        {
            LogErrorWithStack(string.Format(err, args), 2);
        }

        public static void LogError(string err, params object[] args)
        {
            LogErrorWithStack(string.Format(err, args), 2);
        }

        public static void Warning(string err, params object[] args)
        {
            DoLog(err, args, LogLevel.Warning);
        }

        public static void LogWarning(string err, params object[] args)
        {
            DoLog(err, args, LogLevel.Warning);
        }

        private static void DoLog(string szMsg, object[] args, LogLevel emLevel)
        {
            if (LogLevel > emLevel)
                return;
            if (args != null)
                szMsg = string.Format(szMsg, args);
            szMsg = string.Format("[{0}]{1}\n\n=================================================================\n\n",
                DateTime.Now.ToString("HH:mm:ss.ffff"), szMsg);
#if UNITY_EDITOR
            StackTrace stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(2);
            s_LogStackFrameList.Add(stackFrame);
#endif
            switch (emLevel)
            {
                case LogLevel.Warning:
                case LogLevel.Trace:
#if !KENGINE_DLL
                    UnityEngine.Debug.LogWarning(szMsg);
#else
                    Console.WriteLine(szMsg);
#endif
                    break;
                case LogLevel.Error:
#if !KENGINE_DLL
                    UnityEngine.Debug.LogError(szMsg);
#else
                    Console.WriteLine(szMsg);
#endif
                    break;
                default:
#if !KENGINE_DLL
                    UnityEngine.Debug.Log(szMsg);
#else
                    Console.WriteLine(szMsg);
#endif
                    break;
            }
        }

        public static void LogToFile(string szMsg)
        {
            LogToFile(szMsg, true); // 默认追加模式
        }

        // 是否写过log file
        public static bool HasLogFile()
        {
            string fullPath = GetLogPath();
            return File.Exists(fullPath);
        }

        // 写log文件
        public static void LogToFile(string szMsg, bool append)
        {
            string fullPath = GetLogPath();
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (
                FileStream fileStream = new FileStream(fullPath, append ? FileMode.Append : FileMode.CreateNew,
                    FileAccess.Write, FileShare.ReadWrite)) // 不会锁死, 允许其它程序打开
            {
                lock (fileStream)
                {
                    StreamWriter writer = new StreamWriter(fileStream); // Append
                    writer.Write(szMsg);
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        // 用于写日志的可写目录
        public static string GetLogPath()
        {
            string logPath;

#if !KENGINE_DLL
            if (IsUnityEditor)
#endif

                logPath = "logs/";
#if !KENGINE_DLL
            else
                logPath = Path.Combine(Application.persistentDataPath, "logs/");
#endif

            var now = DateTime.Now;
            var logName = string.Format("game_{0}_{1}_{2}.log", now.Year, now.Month, now.Day);

            return logPath + logName;
        }

        #region 控制台双击日志跳到指定行
#if UNITY_EDITOR
        private static string s_logFilePath = "Assets/KSFramework/KEngine/KEngine.Lib/Logger.cs";
        private static int s_InstanceID;
        private static int[] s_Lines = { 354, 361, 368 };
        private static List<StackFrame> s_LogStackFrameList = new List<StackFrame>();
        private static object s_ConsoleWindow;
        private static object s_LogListView;
        private static FieldInfo s_LogListViewTotalRows;
        private static FieldInfo s_LogListViewCurrentRow;
        private static MethodInfo s_LogEntriesGetEntry;
        private static object s_LogEntry;
        private static FieldInfo s_LogEntryCondition;
        static void InitLogUtility()
        {
            s_InstanceID = AssetDatabase.LoadAssetAtPath<MonoScript>(s_logFilePath).GetInstanceID();
            s_LogStackFrameList.Clear();

            GetConsoleWindowListView();
        }

        private static void GetConsoleWindowListView()
        {
            if (s_LogListView == null && !UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                Type consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");
                FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
                s_ConsoleWindow = fieldInfo.GetValue(null);
                FieldInfo listViewFieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
                if (listViewFieldInfo != null && s_ConsoleWindow != null)
                {
                    s_LogListView = listViewFieldInfo.GetValue(s_ConsoleWindow);
                    s_LogListViewTotalRows =
                        listViewFieldInfo.FieldType.GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
                    s_LogListViewCurrentRow =
                        listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
                }

                //LogEntries  
#if UNITY_2017_1_OR_NEWER
                Type logEntriesType = unityEditorAssembly.GetType("UnityEditor.LogEntries");
                s_LogEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                Type logEntryType = unityEditorAssembly.GetType("UnityEditor.LogEntry");
#else
                //Unity5.3适用
                Type logEntriesType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntries");                
                s_LogEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                Type logEntryType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntry");
#endif
                s_LogEntry = Activator.CreateInstance(logEntryType);
                //s_LogEntryInstanceId = logEntryType.GetField("instanceID", BindingFlags.Instance | BindingFlags.Public);
                //s_LogEntryLine = logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.Public);
                s_LogEntryCondition = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        private static StackFrame GetListViewRowCount()
        {
            GetConsoleWindowListView();
            if (s_LogListView == null)
                return null;
            else
            {
                int totalRows = (int)s_LogListViewTotalRows.GetValue(s_LogListView);
                int row = (int)s_LogListViewCurrentRow.GetValue(s_LogListView);
                int logByThisClassCount = 0;
                for (int i = totalRows - 1; i >= row; i--)
                {
                    s_LogEntriesGetEntry.Invoke(null, new object[] { i, s_LogEntry });
                    string condition = s_LogEntryCondition.GetValue(s_LogEntry) as string;
                    //判断是否是由LoggerUtility打印的日志  
                    if (condition.Contains("==================="))
                        logByThisClassCount++;
                    if (condition.Contains("]LUA:") || condition.Contains("][ERROR][string") || condition.Contains("][ERROR]c# excep"))
                        _luaLog = condition;
                    else _luaLog = null;


                }

                //同步日志列表，ConsoleWindow 点击Clear 会清理  
                while (s_LogStackFrameList.Count > totalRows)
                    s_LogStackFrameList.RemoveAt(0);
                if (s_LogStackFrameList.Count >= logByThisClassCount)
                    return s_LogStackFrameList[s_LogStackFrameList.Count - logByThisClassCount];
                return null;
            }
        }
        private static string _luaLog;
        private static bool ArrayContains<T>(T[] t, T v)
        {
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i].Equals(v)) return true;
            }
            return false;
        }
        [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
        static bool OnOpenAsset(int instanceID, int line)
        {

            if (instanceID == s_InstanceID && ArrayContains(s_Lines, line))
            {
                var stackFrame = GetListViewRowCount();
                if (stackFrame != null)
                {
                    if (_luaLog != null)
                    {
                        Regex reg = new Regex(@".*\[string ""(.*)""\]:(\d+):.*");
                        string luaFile = reg.Match(_luaLog).Groups[1].Value;
                        int luaLine = int.Parse(reg.Match(_luaLog).Groups[2].Value);

                        string fullPath = Application.dataPath.Replace("Assets", "") + "../Product/Lua/" + luaFile + ".lua";
                        return openFileInSublime(fullPath, luaLine);
                    }
                    string fileName = stackFrame.GetFileName();
                    string fileAssetPath = fileName.Substring(fileName.IndexOf("Assets"));
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(fileAssetPath), stackFrame.GetFileLineNumber());
                    return true;
                }
            }

            return false;
        }

        static bool openFileInVS(string filePath, int line)
        {
            System.Diagnostics.Process scriptProc = new System.Diagnostics.Process();

            scriptProc.StartInfo.FileName = Application.dataPath.Replace("Assets", "") + "runvs.vbs";
            scriptProc.StartInfo.Arguments = filePath + " " + line + " 1";
            scriptProc.Start();
            scriptProc.WaitForExit();
            scriptProc.Close();
            return true;
        }

        static bool openFileInSublime(string filePath, int line)
        {
            string idePath = EditorPrefs.GetString("sublimePath", "");
            filePath = Path.GetFullPath(filePath);
            if (!string.IsNullOrEmpty(idePath) && File.Exists(idePath))
            {
                string fileArgs = null;
                if (idePath.Contains("idea"))
                {
                    //TODO idea跳到指定行
                    //                    fileArgs = string.Format("{0}?{1}:{2}",filePath,line,1);
                    //                    UnityEngine.Debug.Log("idea==>"+fileArgs);
                    //                    fileArgs = string.Format("{0}:{1}", filePath, line);
                }
                else
                {
                    fileArgs = string.Format("{0}:{1}", filePath, line);
                }
                System.Diagnostics.Process.Start(idePath, fileArgs);
            }
            else
            {
                openFileInVS(filePath, line);
            }
            return true;
        }
#endif
        #endregion
    }
}
