#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - AssetBundle framework for Unity3D
// ===================================
// 
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
using System.IO;
using System.Text;

namespace KEngine.Table
{
    /// <summary>
    /// Write the TabFile!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TabFileWriter : IDisposable
    {
        public readonly TableFile TabFile;

        public TabFileWriter()
        {
            TabFile = new TableFile();
        }

        public TabFileWriter(TableFile tabFile)
        {
            TabFile = tabFile;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
             
            foreach (var header in TabFile.Headers.Values)
                sb.Append(string.Format("{0}\t", header.HeaderName));
            sb.Append("\r\n");

            foreach (var header in TabFile.Headers.Values)
                sb.Append(string.Format("{0}\t", header.HeaderMeta));
            sb.Append("\r\n");

            // 获取所有值
            foreach (var kv in TabFile.Rows)
            {
                var rowT = kv.Value;
                var rowItemCount = rowT.Values.Length;
                for (var i = 0; i < rowItemCount; i++)
                {
                    sb.Append(rowT.Values[i]);
                    if (i != (rowItemCount - 1))
                        sb.Append('\t'); 
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        // 将当前保存成文件
        public bool Save(string fileName)
        {
            lock (this)
            {
                bool result = false;
                try
                {
                    //using (FileStream fs = )
                    {
                        using (StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Create), System.Text.Encoding.UTF8))
                        {
                            sw.Write(ToString());

                            result = true;
                        }
                    }
                }
                catch (IOException e)
                {
                    result = false;
                    throw new Exception("可能文件正在被Excel打开?" + e.Message);
                }

                return result;
            }
        }

        public TableRow NewRow()
        {
            int rowId = TabFile.Rows.Count + 1;
            var newRow = new TableRow(rowId, TabFile.Headers);

            TabFile.Rows.Add(rowId, newRow);

            return newRow;
        }

        public bool RemoveRow(int row)
        {
            return TabFile.Rows.Remove(row);
        }

        public TableRow GetRow(int row)
        {
            TableRow rowT;
            if (TabFile.Rows.TryGetValue(row, out rowT))
            {
                return rowT;
            }

            return null;
        }

        public int NewColumn(string colName)
        {
            return NewColumn(colName, "");
        }
        public int NewColumn(string colName, string defineStr)
        {
            if (string.IsNullOrEmpty(colName))
                throw new Exception("Null Col Name : " + colName);

            var newHeader = new HeaderInfo
            {
                ColumnIndex = TabFile.Headers.Count,
                HeaderName = colName,
                HeaderMeta = defineStr,
            };

            TabFile.Headers.Add(colName, newHeader);
            TabFile._colCount++;

            return TabFile._colCount;
        }

        public void Dispose()
        {
            TabFile.Dispose();
        }
    }
}
