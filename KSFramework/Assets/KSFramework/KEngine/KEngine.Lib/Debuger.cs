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

using System;

namespace KEngine
{
    /// <summary>
    /// 对UnityEngine.Debug.Assert的扩展
    /// </summary>
    public class Debuger
    {
        /// <summary>
        /// Check if a object null，条件不满足打印Error，不会中断当前调用
        /// </summary>
        public static bool Check(object obj, string formatStr = null, params object[] args)
        {
            if (obj != null) return true;

            if (string.IsNullOrEmpty(formatStr))
                formatStr = "[Check Null] Failed!";

            Log.Error("[!!!]" + formatStr, args);
            return false;
        }
        
        /// <summary>
        /// 条件不满足打印Error，不会中断当前调用
        /// </summary>
        public static bool Check(bool result, string formatStr = null, params object[] args)
        {
            if (result) return true;

            if (string.IsNullOrEmpty(formatStr))
                formatStr = "Check Failed!";

            Log.Error("[!!!]" + formatStr, args);
            return false;
        }
        
        /// <summary>
        /// 条件不满足会中断当前调用
        /// </summary>
        public static void Assert(bool result)
        {
            Assert(result, null);
        }
        
        /// <summary>
        /// 条件不满足会中断当前调用
        /// </summary>
        /// <param name="msg">出错时的error日志</param>
        public static void Assert(bool result, string msg, params object[] args)
        {
            if (!result)
            {
                string formatMsg = $"[Error]{DateTime.Now.ToString("HH:mm:ss.fff")} Assert Failed! ";
                if (!string.IsNullOrEmpty(msg))
                {
                    if (args != null && args.Length > 0)
                        msg = string.Format(msg, args);
                    formatMsg += msg;
                }

                //Log.LogErrorWithStack(formatMsg, 2); //Exception会打印error，这里不再打印error
                throw new Exception(formatMsg); // 中断当前调用
            }
        }
        
        /// <summary>
        /// 当前值是否!=0
        /// </summary>
        public static void Assert(int result)
        {
            Assert(result != 0);
        }

        public static void Assert(Int64 result)
        {
            Assert(result != 0);
        }
        
        /// <summary>
        /// 检查参数是否为null，条件不满足会中断当前调用
        /// </summary>
        public static void Assert(object obj)
        {
            Assert(obj != null);
        }

       
    }
}