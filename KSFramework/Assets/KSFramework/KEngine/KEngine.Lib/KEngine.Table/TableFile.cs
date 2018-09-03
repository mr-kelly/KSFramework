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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace KEngine.Table
{
    public enum TableFileExceptionType
    {
        DuplicatedKey,

        HeadLineNull,
        MetaLineNull, // 第二行
        NotFoundHeader,
        NotFoundGetMethod,
        NotFoundPrimaryKey,
        NotFoundRow,
    }

    /// <summary>
    /// 表头信息
    /// </summary>
    public class HeaderInfo
    {
        public int ColumnIndex;
        public string HeaderName;
        public string HeaderMeta;
    }

    public delegate void TableFileExceptionDelegate(TableFileExceptionType exceptionType, string[] args);

    public class TableFileStaticConfig
    {
        public static TableFileExceptionDelegate GlobalExceptionEvent;
    }
    public class TableFileConfig
    {
        public string Content;
        public char[] Separators = new char[] { '\t' };
        public TableFileExceptionDelegate OnExceptionEvent;
    }

    /// <summary>
    /// Simple way of default table file
    /// </summary>
    public class TableFile : TableFile<TableRow>
    {
        public TableFile(string content) : base(content) { }
        public TableFile() : base() { }
        public TableFile(string fileFullPath, Encoding encoding) : base(fileFullPath, encoding) { }

        public new static TableFile LoadFromString(string content)
        {
            TableFile tabFile = new TableFile(content);
            return tabFile;
        }

        public new static TableFile LoadFromFile(string fileFullPath, Encoding encoding = null)
        {
            return new TableFile(fileFullPath, encoding);

        }
    }

    /// <summary>
    /// Read TSV
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class TableFile<T> : IDisposable, IEnumerable<TableRow> where T : TableRow
    {
        private readonly TableFileConfig _config;

        public TableFile(string content)
            : this(new TableFileConfig()
                {
                    Content = content
                })
        {
        }

        public TableFile()
            : this(new TableFileConfig())
        {
        }

        public TableFile(TableFileConfig config)
        {
            _config = config;
            if (!string.IsNullOrEmpty(_config.Content))
                ParseString(_config.Content);
        }

        public TableFile(string fileFullPath, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8; // default encoding
            }

            // 不会锁死, 允许其它程序打开
            using (FileStream fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader oReader = new StreamReader(fileStream, encoding);
                //return new TableFile<T>(oReader.ReadToEnd());
                var config = new TableFileConfig()
                {
                    Content = oReader.ReadToEnd()
                };
                _config = config;
                if (!string.IsNullOrEmpty(_config.Content))
                    ParseString(_config.Content);
            }
        }

        protected internal int _colCount;  // 列数

        public readonly Dictionary<string, HeaderInfo> Headers = new Dictionary<string, HeaderInfo>();
        protected internal Dictionary<int, string[]> TabInfo = new Dictionary<int, string[]>();

        /// <summary>
        /// Row Id to Rows , start from 1
        /// </summary>
        protected internal Dictionary<int, TableRow> Rows = new Dictionary<int, TableRow>();  // iOS不支持 Dict<int, T>

        /// <summary>
        /// Store the Primary Key to Rows
        /// </summary>
        protected Dictionary<object, TableRow> PrimaryKey2Row = new Dictionary<object, TableRow>();

        public Dictionary<string, HeaderInfo>.KeyCollection HeaderNames
        {
            get { return Headers.Keys; }
        }

        // 直接从字符串分析
        public static TableFile<T> LoadFromString(string content)
        {
            TableFile<T> tabFile = new TableFile<T>(content);

            return tabFile;
        }

        // 直接从文件, 传入完整目录，跟通过资源管理器自动生成完整目录不一样，给art库用的
        public static TableFile<T> LoadFromFile(string fileFullPath, Encoding encoding = null)
        {
            return new TableFile<T>(fileFullPath, encoding);
        }

        protected bool ParseReader(TextReader oReader)
        {
            // 首行
            var headLine = oReader.ReadLine();
            if (headLine == null)
            {
                OnException(TableFileExceptionType.HeadLineNull);
                return false;
            }

            var metaLine = oReader.ReadLine(); // 声明行
            if (metaLine == null)
            {
                OnException(TableFileExceptionType.MetaLineNull);
                return false;
            }


            string[] firstLineSplitString = headLine.Split(_config.Separators, StringSplitOptions.None);  // don't remove RemoveEmptyEntries!
            string[] firstLineDef = new string[firstLineSplitString.Length];

            var metaLineArr = metaLine.Split(_config.Separators, StringSplitOptions.None);
            Array.Copy(metaLineArr, 0, firstLineDef, 0, metaLineArr.Length);  // 拷贝，确保不会超出表头的

            for (int i = 0; i < firstLineSplitString.Length; i++)
            {
                var headerString = firstLineSplitString[i];

                var headerInfo = new HeaderInfo
                {
                    ColumnIndex = i,
                    HeaderName = headerString,
                    HeaderMeta = firstLineDef[i],
                };

                Headers[headerInfo.HeaderName] = headerInfo;
            }
            _colCount = firstLineSplitString.Length;  // 標題

            // 读取行内容

            string sLine = "";
            int rowIndex = 1; // 从第1行开始
            while (sLine != null)
            {
                sLine = oReader.ReadLine();
                if (sLine != null)
                {
                    string[] splitString1 = sLine.Split(_config.Separators, StringSplitOptions.None);

                    TabInfo[rowIndex] = splitString1;

                    T newT = (T)Activator.CreateInstance(typeof(T), rowIndex, Headers);  // the New Object may not be used this time, so cache it!

                    newT.Values = splitString1;

                    if (!newT.IsAutoParse)
                        newT.Parse(splitString1);
                    else
                        AutoParse(newT, splitString1);

                    if (newT.PrimaryKey != null)
                    {
                        TableRow oldT;
                        if (!PrimaryKey2Row.TryGetValue(newT.PrimaryKey, out oldT))  // 原本不存在，使用new的，释放cacheNew，下次直接new
                        {
                            PrimaryKey2Row[newT.PrimaryKey] = newT;
                        }
                        else  // 原本存在，使用old的， cachedNewObj(newT)因此残留, 留待下回合使用
                        {
                            TableRow toT = oldT;
                            // Check Duplicated Primary Key, 使用原来的，不使用新new出来的, 下回合直接用_cachedNewObj
                            OnException(TableFileExceptionType.DuplicatedKey, toT.PrimaryKey.ToString());
                            newT = (T)toT;
                        }
                    }

                    Rows[rowIndex] = newT;
                    rowIndex++;
                }
            }

            return true;
        }

        /// <summary>
        /// Auto get fields from class definition, use reflection (poor performance warning!)
        /// </summary>
        internal FieldInfo[] AutoTabFields
        {
            get
            {
                return typeof(TableRow).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        //internal PropertyInfo[] TabProperties
        //{
        //    get
        //    {
        //        List<PropertyInfo> props = new List<PropertyInfo>();
        //        foreach (var fieldInfo in typeof(T).GetProperties())
        //        {
        //            if (fieldInfo.GetCustomAttributes(typeof(TabColumnAttribute), true).Length > 0)
        //            {
        //                props.Add(fieldInfo);
        //            }
        //        }
        //        return props.ToArray();
        //    }
        //}

        /// <summary>
        /// Auto parser with class's definition fields (poor performance warning)
        /// </summary>
        /// <param name="tableRow"></param>
        /// <param name="cellStrs"></param>
        protected void AutoParse(TableRow tableRow, string[] cellStrs)
        {
            var type = tableRow.GetType();
            var okFields = new List<FieldInfo>();

            foreach (FieldInfo field in AutoTabFields)
            {
                if (!HasColumn(field.Name))
                {
                    OnException(TableFileExceptionType.NotFoundHeader, type.Name, field.Name);
                    continue;
                }
                okFields.Add(field);
            }

            foreach (var field in okFields)
            {
                var fieldName = field.Name;
                var fieldType = field.FieldType;
                var methodName = string.Format("Get_{0}", fieldType.Name);
                var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    // 找寻FieldName所在索引
                    int index = Headers[fieldName].ColumnIndex;
                    // default value
                    //string szType = "string";
                    string defaultValue = "";
                    var headerDef = Headers[fieldName].HeaderMeta;
                    if (!string.IsNullOrEmpty(headerDef))
                    {
                        var defs = headerDef.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        //if (defs.Length >= 1) szType = defs[0];
                        if (defs.Length >= 2) defaultValue = defs[1];
                    }

                    field.SetValue(tableRow, method.Invoke(tableRow, new object[]
                    {
                       cellStrs[index] , defaultValue
                    }));
                }
                else
                {
                    OnException(TableFileExceptionType.NotFoundGetMethod, methodName);
                }
            }

        }
        protected bool ParseString(string content)
        {
            using (var oReader = new StringReader(content))
            {
                ParseReader(oReader);
            }

            return true;
        }

        public bool HasColumn(string colName)
        {
            return Headers.ContainsKey(colName);
        }

        protected internal void OnException(TableFileExceptionType message, params string[] args)
        {
            if (TableFileStaticConfig.GlobalExceptionEvent != null)
            {
                TableFileStaticConfig.GlobalExceptionEvent(message, args);
            }

            if (_config.OnExceptionEvent != null)
            {
                _config.OnExceptionEvent(message, args);
            }

            if (TableFileStaticConfig.GlobalExceptionEvent == null && _config.OnExceptionEvent == null)
            {
                string[] argsStrs = new string[args.Length];
                for (var i = 0; i < argsStrs.Length; i++)
                {
                    var arg = args[i];
                    if (arg == null) continue;
                    argsStrs[i] = arg.ToString();
                }
                throw new Exception(string.Format("{0} - {1}", message, string.Join("|", argsStrs)));
            }
        }

        [Obsolete("Use GetRowCount() instead")]
        public int GetHeight()
        {
            return Rows.Count;
        }

        /// <summary>
        /// Get rows count
        /// </summary>
        /// <returns></returns>
        public int GetRowCount()
        {
            return Rows.Count;
        }

        /// <summary>
        /// Get table columns count
        /// </summary>
        /// <returns></returns>
        public int GetColumnCount()
        {
            return _colCount;
        }

        public int GetWidth()
        {
            return _colCount;
        }

        public T GetRow(int row)
        {
            TableRow rowT;
            if (!Rows.TryGetValue(row, out rowT))
            {
                OnException(TableFileExceptionType.NotFoundRow, row.ToString());
                return null;
            }

            return (T)rowT;
        }

        public void Dispose()
        {
            Headers.Clear();
            TabInfo.Clear();
            Rows.Clear();
            PrimaryKey2Row.Clear();
        }

        public void Close()
        {
            Dispose();
        }

        public bool HasPrimaryKey(object primaryKey)
        {
            return PrimaryKey2Row.ContainsKey(primaryKey);
        }

        /// <summary>
        /// Get object of the primary key (pk in meta header)
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="throwError">Whether or not throw exception</param>
        /// <returns></returns>
        public T FindByPrimaryKey(object primaryKey, bool throwError = true)
        {
            TableRow ret;

            if (PrimaryKey2Row.TryGetValue(primaryKey, out ret))
                return (T)ret;
            else
            {
                if (throwError)
                    OnException(TableFileExceptionType.NotFoundPrimaryKey, primaryKey.ToString());
                return null;
            }
        }

        /// <summary>
        /// Get object of the primary key (pk in meta header)
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public T GetByPrimaryKey(object primaryKey)
        {
            return FindByPrimaryKey(primaryKey);
        }

        /// <summary>
        /// Return a IEnumerable class, for each this
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TableRow> GetAll()
        {
            return Rows.Values;
        }

        public IEnumerator<TableRow> GetEnumerator()
        {
            return Rows.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.Values.GetEnumerator();
        }
    }
}
