#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AppEngine.cs
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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KEngine;
using KEngine.Modules;

namespace KSFramework
{
    /// <summary>
    /// 多语言模块，从语言包文件中读取，语言包格式：key=value
    /// </summary>
    public class I18N
    {
        private static  Dictionary<string, string> dict = new Dictionary<string, string>();
        
        /// <summary>
        /// 是否已经初始化完成
        /// </summary>
        private static bool _isInited = false;

        /// <summary>
        /// 惰式初始化
        /// </summary>
        public static void Init()
        {
            if (_isInited)
                return;
            dict.Clear();
            //读取语言包
            var lang_file = $"I18N/lang{AppConfig.LangFileFlag}.txt";
            var bytes = KResourceModule.LoadAssetsSync(SettingModule.GetSettingFilePath(lang_file));
            var fileContent = Encoding.UTF8.GetString(bytes);
            TextParser.ParseText(fileContent,dict);

#if UNITY_EDITOR
            // 开发热重载
            if (SettingModule.IsFileSystemMode)
            {
                SettingModule.WatchSetting(lang_file, (_) =>
                {
                    _isInited = false;
                });
            }
#endif
            _isInited = true;
        }
        
 
        /// <summary>
        /// 从语言包中读取字符串
        /// </summary>
        /// <param name="str"></param>
        public static string Get(string str, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            if (!_isInited) Init();

            string value;
            if (dict.TryGetValue(str, out value))
            {
                if (args != null && args.Length > 0 && !string.IsNullOrEmpty(value)) 
                    return String.Format(value, args);
                return value;
            }

            //Log.LogError($"not find lang_id:{str}");
            return $"lang_id:{str}";
        }

        /// <summary>
        /// clear cache ,reload
        /// </summary>
        public static void ReLoad()
        {
            if (!Application.isEditor) return;
            _isInited = false;
            Init();
        }
    }

}
