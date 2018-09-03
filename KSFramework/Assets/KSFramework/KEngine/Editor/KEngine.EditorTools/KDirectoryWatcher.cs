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

using System.Collections.Generic;
using System.IO;

namespace KUnityEditorTools
{
    /// <summary>
    /// 一个目录监视器
    /// </summary>
    public class KDirectoryWatcher
    {
        static readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>(); 

        /// <summary>
        /// 监视一个目录，如果有修改则触发事件函数, 包含其子目录！
        /// <para>使用更大的buffer size确保及时触发事件</para>
        /// <para>不用includesubdirect参数，使用自己的子目录扫描，更稳健</para>
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public KDirectoryWatcher(string dirPath, FileSystemEventHandler handler)
        {
            CreateWatch(dirPath, handler);
        }

        void CreateWatch(string dirPath, FileSystemEventHandler handler)
        {
            if (_watchers.ContainsKey(dirPath))
            {
                _watchers[dirPath].Dispose();
                _watchers[dirPath] = null;
            }

            if (!Directory.Exists(dirPath)) return;

            var watcher = new FileSystemWatcher();
            watcher.IncludeSubdirectories = false;//includeSubdirectories;
            watcher.Path = dirPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*";
            watcher.Changed += handler;
            watcher.EnableRaisingEvents = true;
            watcher.InternalBufferSize = 10240;
            //return watcher;
            _watchers[dirPath] = watcher;


            foreach (var childDirPath in Directory.GetDirectories(dirPath))
            {
                CreateWatch(childDirPath, handler);
            }
        }
    }
}