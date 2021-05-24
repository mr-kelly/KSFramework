using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KEngine
{
    public enum LogState
    {
        LoadAB = 0,
        LoadAsset,
        OnInit,
        OnOpen
    }
    
    /// <summary>
    /// 写入日志到文件中
    /// </summary>
    public class LogFileRecorder
    {
        private StreamWriter writer;
        /// <summary>
        /// 初始化记录器，在游戏退出时调用Close
        /// </summary>
        /// <param name="filePath">文件名中不能包含特殊字符比如:</param>
        /// <param name="mode"></param>
        public LogFileRecorder(string filePath, FileMode mode = FileMode.Create)
        {
            int index = 0;
            try
            {
                var fs = new FileStream(filePath, mode);
                writer = new StreamWriter(fs);
            }
            catch (IOException e)
            {
                filePath = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + "_" + (index++) + Path.GetExtension(filePath);
				Debug.LogError(e.Message+" \n"+e.StackTrace);
                var fs = new FileStream(filePath, mode);
                writer = new StreamWriter(fs);
            }
        }

        public void WriteLine(string line)
        {
            writer.WriteLine(line);
            writer.Flush();
        }

        public void Close()
        {
            writer.Flush();
            writer.Close();
        }
        
    }
    
    /// <summary>
    /// 监听Unity的日志事件
    ///     用于在调试阶段写入Unity的所有日志到文件中，Log.LogToFile只会记录手动调用的，而它会记录所有的日志。
    /// </summary>
    public static class LogFileManager
    {
        static LogFileRecorder logWritter;
        
        public static void Start()
        {
            Application.logMessageReceivedThreaded += OnLogCallback;
        }
        
        public static void Destory()
        {
            Application.logMessageReceivedThreaded -= OnLogCallback;
            if (logWritter != null) logWritter.Close();
        }

        #region Unity的日志回调

        public static string GetLogFilePath()
        {
            string filePath = "";
            var logName = "/log_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".log";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    filePath = string.Format("{0}/{1}", Application.persistentDataPath, logName);
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    filePath = string.Format("{0}/../logs/{1}", Application.dataPath, logName);
                    break;
                default:
                    filePath = string.Format("{0}/{1}", Application.persistentDataPath, logName);
                    break;
            }

            return filePath;
        }
        
        //把Unity所有的日志都保存起来
        private static void OnLogCallback(string condition, string stackTrace, LogType type)
        {
            if (logWritter == null)
            {
                string filePath = GetLogFilePath();
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(filePath);
                logWritter = new LogFileRecorder(filePath, FileMode.Append);
            }
            
            //NOTE System.Environment.StackTrace是非常完整的堆栈包括Unity底层调用栈，而stackTrace只有exception才有堆栈，对于Log/LogWarning/LogError都是没有堆栈，可以通过StackTrace加上堆栈。 by qingqing.zhao test in unity2019.3.7
            logWritter.WriteLine(string.Format("{0}\n{1}",  condition,  stackTrace));
        }
        
        #endregion
        
        #region 记录某种行为的耗时到csv用于分析

        private static Dictionary<string, LogFileRecorder> loggers = new Dictionary<string, LogFileRecorder>();

        public static void CloseStream()
        {
            foreach (var kv in loggers)
            {
                kv.Value.Close();
            }

            loggers.Clear();
        }
        
        /// <summary>
        /// 记录UI的一些数据到csv中，方便统计，格式如下：
        ///     UI名字	操作(函数)	耗时(ms)
        ///     Billboard	LoadAB	0.127
        ///     Login	LoadAB	0.516
        ///     Login	OnInit	0.004
        ///     Navbar	LoadAB	0.109
        /// </summary>
        public static void WriteUILog(string uiName,LogState state, float time)
        {
            LogFileRecorder logger;
            var logType = "ui";
            if (!loggers.TryGetValue(logType, out logger))
            {
                logger = new LogFileRecorder(Application.persistentDataPath + $"/profiler_ui_{DateTime.Now.ToString("yyyy-M-d HH.mm.ss")}.csv");
                loggers.Add(logType, logger);
                logger.WriteLine("UI名字,操作(函数),耗时(ms)");
            }

            logger.WriteLine(string.Format("{0},{1},{2:0.###}",uiName,state,time));
        }
        
        public static void WriteLoadAbLog(string abName,float time)
        {
            LogFileRecorder logger;
            var logType = "loadab";
            if (!loggers.TryGetValue(logType, out logger))
            {
                logger = new LogFileRecorder(Application.persistentDataPath + $"/profiler_loadab_{DateTime.Now.ToString("yyyy-M-d HH.mm.ss")}.csv");
                loggers.Add(logType, logger);
                logger.WriteLine("AB资源,耗时");
            }

            logger.WriteLine(string.Format("{0},{1:0.###}",abName,time));
        }

        public static void WriteProfileLog(string logType, string line)
        {
            LogFileRecorder logger;
            if (!loggers.TryGetValue(logType, out logger))
            {
                logger = new LogFileRecorder(Application.persistentDataPath + $"/profiler_{logType}_{DateTime.Now.ToString("yyyy-M-d HH.mm.ss")}.csv");
                loggers.Add(logType, logger);
            }

            logger.WriteLine(line);
        }

        #endregion
    }
}