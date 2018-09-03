#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
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
using System.Linq;
using System.Text;
using KEngine;
using UnityEngine;
using TableML;

namespace KSFramework.Editor
{
    /// <summary>
    /// default collector, 收集StringsTable.xlsx编译后的表StringsTable.bytes的多语言字符串
    /// </summary>
    public class StringsTableI18NCollector : I18NCollector
    {
        public void Collect(ref I18NItems i18List)
        {
            CollectStringsTable(ref i18List);
        }

        /// <summary>
        /// 收集StringsTalbe.bytes的字符串
        /// </summary>
        /// <param name="refItems"></param>
        static void CollectStringsTable(ref I18NItems refItems)
        {
            var compilePath = AppEngine.GetConfig("KEngine.Setting", "SettingCompiledPath");
            var ext = AppEngine.GetConfig("KEngine", "AssetBundleExt");
            var stringsTablePath = string.Format("{0}/StringsTable{1}", compilePath, ext);
            if (!File.Exists(stringsTablePath))
                return;

            string stringsTableContent;
            using (var stream = new FileStream(stringsTablePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    stringsTableContent = reader.ReadToEnd();
                }
            }

            var tableFile = new TableFile(stringsTableContent);
            foreach (var row in tableFile)
            {
                var srcStr = row["Id"];
                refItems.Add(srcStr, stringsTablePath);
            }
            Debug.Log("[CollectStringsTable]Success!");
        }
    }
}