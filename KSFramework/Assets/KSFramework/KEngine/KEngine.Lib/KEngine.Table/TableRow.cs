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
using System.Collections.Generic;

namespace KEngine.Table
{
    public partial class TableRowParser
    {
        public string Get_String(string value, string defaultValue)
        {
            return Get_string(value, defaultValue);
        }

        public string Get_string(string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return value;
        }

        public int Get_Int32(string value, string defaultValue)
        {
            return Get_int(value, defaultValue);
        }
        public bool Get_bool(string value, string defaultValue)
        {
            return Get_Boolean(value, defaultValue);
        }

        public bool Get_Boolean(string value, string defaultValue)
        {
            var str = Get_string(value, defaultValue);
            bool result;
            if (bool.TryParse(str, out result))
            {
                return result;
            }
            return Get_int(value, defaultValue) != 0;
        }
        public int Get_int(string value, string defaultValue)
        {
            var str = Get_string(value, defaultValue);
            return string.IsNullOrEmpty(str) ? default(int) : int.Parse(str);
        }

        public double Get_double(string value, string defaultValue)
        {
            var str = Get_string(value, defaultValue);
            return string.IsNullOrEmpty(str) ? default(double) : double.Parse(str);
        }

        public float Get_float(string value, string defaultValue)
        {
            var str = Get_string(value, defaultValue);
            return string.IsNullOrEmpty(str) ? default(float) : float.Parse(str);
        }
        public uint Get_uint(string value, string defaultValue)
        {
            var str = Get_string(value, defaultValue);
            return string.IsNullOrEmpty(str) ? default(int) : uint.Parse(str);
        }

        public string[] Get_string_array(string value, string defaultValue)
        {
            var str = Get_string(value, defaultValue);
            return str.Split(',');
        }

        public Dictionary<string, int> Get_Dictionary_string_int(string value, string defaultValue)
        {
            return GetDictionary<string, int>(value, defaultValue);
        }

        protected Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string value, string defaultValue)
        {
            var dict = new Dictionary<TKey, TValue>();
            var str = Get_String(value, defaultValue);
            var arr = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in arr)
            {
                var kv = item.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var itemKey = ConvertString<TKey>(kv[0]);
                var itemValue = ConvertString<TValue>(kv[1]);
                dict[itemKey] = itemValue;
            }
            return dict;
        }

        protected T ConvertString<T>(string value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

    }

    public partial class TableRow : TableRowParser
    {
        /// <summary>
        /// TableRow's row number of table
        /// </summary>
        public int RowNumber { get; internal set; }

        /// <summary>
        /// 是否自动使用反射解析，不自动，则使用Parse方法
        /// </summary>
        public virtual bool IsAutoParse
        {
            get
            {
                return false;
            }
        }

        public TableRow(int rowNumber, Dictionary<string, HeaderInfo> headerInfos)
        {
            Ctor(rowNumber, headerInfos);
        }

        private void Ctor(int rowNumber, Dictionary<string, HeaderInfo> headerInfos)
        {
            RowNumber = rowNumber;
            HeaderInfos = headerInfos;
            Values = new string[headerInfos.Count];
        }

        /// <summary>
        /// When true, will use reflection to map the Tab File
        /// </summary>
        //public virtual bool IsAutoParse
        //{
        //    get { return false; }
        //}

        /// <summary>
        /// Table Header, name and type definition
        /// </summary>
        public Dictionary<string, HeaderInfo> HeaderInfos { get; internal set; }

        /// <summary>
        /// Store values of this row
        /// </summary>
        public string[] Values { get; internal set; }

        /// <summary>
        /// Cache save the row values
        /// </summary>
        /// <param name="cellStrs"></param>
        public virtual void Parse(string[] cellStrs)
        {
        }

        /// <summary>
        /// Use first object of array as primary key!
        /// </summary>
        public virtual string PrimaryKey
        {
            get { return this[0]; }
        }

        public string Get(int index)
        {
            return this[index];
        }

        public string Get(string headerName)
        {
            return this[headerName];
        }

        /// <summary>
        /// Get Value by Indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index]
        {
            get
            {
                if (index > Values.Length || index < 0)
                {
                    throw new Exception(string.Format("Overflow index `{0}`", index));
                }

                return Values[index];
            }
            set { Values[index] = value; }
        }

        /// <summary>
        /// Get or set Value by Indexer, be careful the `newline` character!
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public string this[string headerName]
        {
            get
            {
                HeaderInfo headerInfo;
                if (!HeaderInfos.TryGetValue(headerName, out headerInfo))
                {
                    throw new Exception("not found header: " + headerName);
                }

                return this[headerInfo.ColumnIndex];
            }
            set
            {
                HeaderInfo headerInfo;
                if (!HeaderInfos.TryGetValue(headerName, out headerInfo))
                {
                    throw new Exception("not found header: " + headerName);
                }

                this[headerInfo.ColumnIndex] = value;
            }
        }
    }
}
