#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: SettingModule.cs
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
using TableML;
//using KEngine.Table;
namespace KEngine.Modules
{
    /// <summary>
    /// For all class `XXXSettings`
    /// </summary>
    public interface IReloadableSettings
    {
        void ReloadAll();
        int Count { get; }
    }
    /// <summary>
    /// 带有惰式加载的数据表加载器基类，
    /// 不带具体的加载文件的方法，需要自定义。
    /// 主要为了解耦，移除对UnityEngine命名空间的依赖使之可以进行其它.net平台的兼容
    /// 使用时进行继承，并注册LoadSettingMethod自定义的
    /// </summary>
    public abstract class SettingModuleBase
    {
        /// <summary>
        /// table缓存
        /// </summary>
        private readonly Dictionary<string, object> _tableFilesCache = new Dictionary<string, object>();

        /// <summary>
        /// You can custom method to load file.
        /// </summary>
        protected abstract string LoadSetting(string path);

        /// <summary>
        /// 通过SettingModule拥有缓存与惰式加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="useCache">是否缓存起来？还是单独创建新的</param>
        /// <returns></returns>
        public TableFile GetTableFile(string path, bool useCache = false) 
        {
            object tableFile;
            if (!useCache || !_tableFilesCache.TryGetValue(path, out tableFile))
            {
                var fileContent = LoadSetting(path);
                var tab = TableFile.LoadFromString(fileContent);
                _tableFilesCache[path] = tableFile = tab;
                return tab;
            }

            return tableFile as TableFile;
        }
    }
}