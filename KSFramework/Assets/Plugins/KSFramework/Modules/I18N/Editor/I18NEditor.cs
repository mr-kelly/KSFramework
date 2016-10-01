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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KEngine;
using KEngine.Editor;
using KUnityEditorTools;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using UnityEditor;

namespace KSFramework.Editor
{
    public class I18NEditor
    {
        // default collector
        /// <summary>
        /// 搜索所有Collector，在所有程序集了
        /// </summary>
        /// <returns></returns>
        public static IList<I18NCollector> FindCollectors()
        {
            var list = new List<I18NCollector>();
            foreach (var type in KEditorUtils.FindAllPublicTypes(typeof(I18NCollector)))
            {
                if (type != typeof(I18NCollector))
                {
                    Debug.Log("Find I18NCollector : " + type.FullName);
                    list.Add((I18NCollector)Activator.CreateInstance(type));

                }
            }
            return list;
        }

        /// <summary>
        /// 设置有多少种语言
        /// </summary>
        private static string[] I18NLanguages
        {
            get
            {
                var langs = AppEngine.GetConfig("KSFramework.I18N", "I18NLanguages");
                return langs.Split(',');
            }
        }

        /// <summary>
        /// excel存放地址
        /// </summary>
        public static string SettingSourcePath
        {
            get
            {
                var settingSource = AppEngine.GetConfig("KEngine.Setting", "SettingSourcePath");
                return settingSource;
            }
        }

        [MenuItem("KEngine/I18N/Collect All")]
        public static void CollectAll()
        {
            // 如果没有，先确保创建新的
            foreach (var lang in I18NLanguages)
            {
                var xlsPath = string.Format("{0}/I18N/{1}.xlsx", SettingSourcePath, lang);
                var xlsDir = Path.GetDirectoryName(xlsPath);
                if (!Directory.Exists(xlsDir))
                    Directory.CreateDirectory(xlsDir);

                if (!File.Exists(xlsPath))
                {
                    // Id	Value	CommentSrcFile
                    CreateExcel(xlsPath);
                }
            }

            var refList = new I18NItems(); // key, 和 来源
            var collectors = FindCollectors();
            if (collectors.Count == 0)
                throw new Exception("No I18N Collectors found, write your custom `I18NInterface` class");
            foreach (var collector in collectors)
            {
                collector.Collect(ref refList);
            }

            WriteExcel(refList);
        }

        /// <summary>
        /// Create a new excel of I18N
        /// </summary>
        /// <param name="excelPath"></param>
        static void CreateExcel(string excelPath)
        {
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("Default");
            var newRow0 = sheet.CreateRow(0);
            var cell0 = newRow0.CreateCell(0);
            cell0.SetCellValue("Id");
            var cell1 = newRow0.CreateCell(1);
            cell1.SetCellValue("Value");
            var cell2 = newRow0.CreateCell(2);
            cell2.SetCellValue("CommentSrcFile");

            var newRow1 = sheet.CreateRow(1);
            var cell10 = newRow1.CreateCell(0);
            cell10.SetCellValue("string/int/pk");
            var cell11 = newRow1.CreateCell(1);
            cell11.SetCellValue("string");
            var cell12 = newRow1.CreateCell(2);
            cell12.SetCellValue("string");


            var newRow2 = sheet.CreateRow(2);
            var cell20 = newRow2.CreateCell(0);
            cell20.SetCellValue("原文");
            var cell21 = newRow2.CreateCell(1);
            cell21.SetCellValue("翻译");
            var cell22 = newRow2.CreateCell(2);
            cell22.SetCellValue("出处");

            using (FileStream stream = new FileStream(excelPath, FileMode.CreateNew))
            {
                workbook.Write(stream);
            }

            Debug.Log("New Excel: " + excelPath);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="theList"></param>
        /// <returns></returns>
        private static bool WriteExcel(I18NItems theList)
        {
            foreach (var lang in I18NLanguages)
            {
                var excelPath = string.Format("{0}/I18N/{1}.xlsx", SettingSourcePath, lang);

                var changedFile = false;
                ExcelFile excelFile;
                try
                {
                    excelFile = new ExcelFile(excelPath);
                }
                catch (IOException e)
                {
                    Debug.LogError("无法打开Excel，是否正在编辑： " + excelPath + " " + e.Message);
                    return false;

                }
                // 把表中的值缓存起来！ 方便查询是否已经存在过了！
                var id2RowMap = new Dictionary<string, int>();
                var excelRowCount = excelFile.GetRowsCount();
                for (int row = 3; row < excelRowCount; row++) // 跳过前3行
                {
                    var key = excelFile.GetString("Id", row);
                    id2RowMap[key] = row;
                }

                var rowCount = excelFile.GetRowsCount();

                // 存在的不动，不存在的添加
                HashSet<string> useList = new HashSet<string>(); // 用来缓存处理过的，后面把旧的标记出来方便后面删
                foreach (var kv in theList.Items)
                {
                    var newKey = kv.Key;
                    var srcsList = kv.Value; // 来源，几个字符串来源
                    int existRow;
                    if (!id2RowMap.TryGetValue(newKey, out existRow))
                    {
                        existRow = rowCount++;
                        excelFile.SetRow("Id", existRow, newKey);
                        changedFile = true;
                    }

                    // 来源列, 覆盖上去
                    //var sourcesStr = excelFile.GetString("CommentSrcFile", existRow);
                    //var existStrsList = CTool.Split<string>(sourcesStr, '|');
                    //var unionStrs = existStrsList.Union(srcsList);
                    var srcStr = string.Join("|", srcsList.ToArray());
                    var srcStrInExcel = excelFile.GetString("CommentSrcFile", existRow);
                    if (srcStr != srcStrInExcel)
                    {
                        excelFile.SetRow("CommentSrcFile", existRow, srcStr);
                        changedFile = true;
                    }

                    // 已存在，不处理，添加到列表，后面检测没用的
                    useList.Add(newKey);
                }

                // useList 和 theList做差集，找出没用的！
                if (id2RowMap.Count > useList.Count)
                {
                    var exceptList = id2RowMap.Keys.Except(useList);
                    //var exceptList2 = useList.Except(id2RowMap.Keys);
                    //var exCount = exceptList.Count();
                    foreach (var exceptStr in exceptList)
                    {
                        var exceptRow = id2RowMap[exceptStr];
                        Log.Info("多出来的: {0}, 旧的，所在行 {1}, 删除！", exceptStr, exceptRow);
                        //excelFile.SetRowGrey(exceptRow);
                        excelFile.ClearRow(exceptRow);
                        changedFile = true;
                    }

                }
                if (changedFile) // 有修改的才保存
                    excelFile.Save();
                Log.Info("Write Excel: {0}", excelPath);
            }
            return true;
        }

    }
}
