using UnityEngine;
using System.Collections;
using CosmosTable;

/// <summary>
/// Extension for TableRow, new method Get_I18NString, mark in excel column type "I18NString"
/// 无侵入式的，对Table解析器，加入对I18NString的解析支持
/// </summary>
public static class I18NTableRowExtensions
{
    public static I18NString Get_I18NString(this TableRow row, string value, string defaultValue)
    {
        var str = row.Get_string(value, defaultValue);
        return new I18NString(str);
    }
}

/// <summary>
/// A string auto translated by I18N Module
/// 多语言翻译的字符串, 没有放命名空间，因为全局基础调用
/// </summary>
public class I18NString
{
    private readonly string _str;

    public I18NString(string str)
    {
        _str = str;
    }

    public static implicit operator string(I18NString str)
    {
        return str._str;
    }
}