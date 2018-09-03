#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: Debugger.cs
// Date:     2016/03/08
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

#if UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_4 || UNITY_3
#define UNITY
#endif

using System;

namespace KEngine
{
    public class Debuger
    {

        /// <summary>
        /// Check if a object null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatStr"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool Check(object obj, string formatStr = null, params object[] args)
        {
            if (obj != null) return true;

            if (string.IsNullOrEmpty(formatStr))
                formatStr = "[Check Null] Failed!";

            Log.Error("[!!!]" + formatStr, args);
            return false;
        }

        public static void Assert(bool result)
        {
            Assert(result, null);
        }

        public static void Assert(bool result, string msg, params object[] args)
        {
            if (!result)
            {
                string formatMsg = "Assert Failed! ";
                if (!string.IsNullOrEmpty(msg))
                    formatMsg += string.Format(msg, args);

                Log.LogErrorWithStack(formatMsg, 2);

                throw new Exception(formatMsg); // 中断当前调用
            }
        }

        public static void Assert(int result)
        {
            Assert(result != 0);
        }

        public static void Assert(Int64 result)
        {
            Assert(result != 0);
        }

        public static void Assert(object obj)
        {
            Assert(obj != null);
        }

        #region Record Time

#if UNITY
        private static float[] RecordTime = new float[10];
        private static string[] RecordKey = new string[10];
        private static int RecordPos = 0;

        public static void BeginRecordTime(string key)
        {
            RecordTime[RecordPos] = UnityEngine.Time.realtimeSinceStartup;
            RecordKey[RecordPos] = key;
            RecordPos++;
        }

        public static string EndRecordTime(bool printLog = true)
        {
            RecordPos--;
            double s = (UnityEngine.Time.realtimeSinceStartup - RecordTime[RecordPos]);
            if (printLog)
            {
                Log.Info("[RecordTime] {0} use {1}s", RecordKey[RecordPos], s);
            }
            return string.Format("[RecordTime] {0} use {1}s.", RecordKey[RecordPos], s);
        }
#endif

        // 添加性能观察, 使用C#内置
        public static void WatchPerformance(Action del)
        {
            WatchPerformance("执行耗费时间: {0}s", del);
        }

        public static void WatchPerformance(string outputStr, Action del)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间

            if (del != null)
            {
                del();
            }

            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            //double seconds = timespan.TotalSeconds;  //  总秒数
            double millseconds = timespan.TotalMilliseconds;
            decimal seconds = (decimal)millseconds / 1000m;

            Log.Warning(outputStr, seconds.ToString("F7")); // 7位精度
        }

        #endregion
    }
}