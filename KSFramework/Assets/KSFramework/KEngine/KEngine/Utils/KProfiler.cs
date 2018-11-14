#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KProfiler.cs
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
using UnityEngine;

namespace KEngine
{
    public class KProfiler
    {
        public static bool bEnable = false;

        /// <summary>
        /// 缓存起来的监测器Wachter~
        /// </summary>
        private static Dictionary<string, System.Diagnostics.Stopwatch> m_WachterDictionary = null;

        /// <summary>
        /// Watcher内存埋点
        /// </summary>
        private static Dictionary<System.Diagnostics.Stopwatch, long> m_WachterMems = null;

        /// <summary>
        /// 是否可以Watch监测，为了后续方便修改监测条件
        /// 当前设置成DebugBuild才进行监测和输出
        /// </summary>
        public static bool CanWatch
        {
            get { return UnityEngine.Debug.isDebugBuild; }
        }

        /// <summary>
        /// BeginWatch(string)的任意枚举版
        /// </summary>
        /// <param name="emKey"></param>
        public static void BeginWatch(Enum emKey)
        {
            if (!CanWatch)
                return;
            BeginWatch(emKey.ToString());
        }

        /// <summary>
        /// EndWatch的任意枚举版
        /// </summary>
        /// <param name="emKey"></param>
        public static void EndWatch(Enum emKey)
        {
            if (!CanWatch)
                return;
            EndWatch(emKey.ToString());
        }

        /// <summary>
        /// 使用Stopwatch， debug模式下无行为
        /// </summary>
        /// <param name="key"></param>
        /// <param name="del"></param>
        public static void BeginWatch(string key)
        {
            if (!CanWatch)
                return;

            if (m_WachterDictionary == null)
                m_WachterDictionary = new Dictionary<string, Stopwatch>();
            if (m_WachterMems == null)
                m_WachterMems = new Dictionary<Stopwatch, long>();

            System.Diagnostics.Stopwatch stopwatch;
            if (!m_WachterDictionary.TryGetValue(key, out stopwatch))
            {
                stopwatch = m_WachterDictionary[key] = new System.Diagnostics.Stopwatch();
            }

            m_WachterMems[stopwatch] = GC.GetTotalMemory(false);

            if (stopwatch.IsRunning)
            {
                Log.Error("Running stopwatch need reset: {0}", key);
            }

            stopwatch.Reset();
            stopwatch.Start(); //  开始监视代码运行时间
        }

        /// <summary>
        /// 结束性能监测，输出监测的时间消耗
        /// </summary>
        /// <param name="key"></param>
        public static void EndWatch(string key, string name = null)
        {
            if (!CanWatch)
                return;

            if (m_WachterDictionary == null)
                m_WachterDictionary = new Dictionary<string, Stopwatch>();
            if (m_WachterMems == null)
                m_WachterMems = new Dictionary<Stopwatch, long>();

            System.Diagnostics.Stopwatch stopwatch;
            if (!m_WachterDictionary.TryGetValue(key, out stopwatch))
            {
                Log.Error("Not exist Stopwatch: {0}", key);
                return;
            }
            long lastMem = 0;
            m_WachterMems.TryGetValue(stopwatch, out lastMem);

            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            //double seconds = timespan.TotalSeconds;  //  总秒数
            double millseconds = timespan.TotalMilliseconds;
            decimal seconds = (decimal) millseconds/1000m;

            string format = "[Watcher] {0}, Time: {1}s, MemDiff: {2}KB";
            var memDiff = GC.GetTotalMemory(false) - lastMem; // byte
            Log.Error(string.Format(format,
                string.IsNullOrEmpty(name) ? key : name, seconds.ToString("F7"),
                memDiff/1000f)); // 7位精度
        }

        public static void BeginSample(string strName)
        {
            if (!CanWatch) return;

            UnityEngine.Profiling.Profiler.BeginSample(strName);
        }

        public static void EndSample()
        {
            if (!CanWatch) return;

            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}