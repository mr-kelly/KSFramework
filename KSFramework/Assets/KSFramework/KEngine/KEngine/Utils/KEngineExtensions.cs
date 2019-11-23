#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KEngineExtensions.cs
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
using System.Text;
using KEngine;
using UnityEngine;

/// <summary>
/// Extension Unity's function, to be more convinient
/// </summary>
public static class KEngineExtensions
{
    public static void SetWidth(this RectTransform rectTrans, float width)
    {
        var size = rectTrans.sizeDelta;
        size.x = width;
        rectTrans.sizeDelta = size;
    }

    public static void SetHeight(this RectTransform rectTrans, float height)
    {
        var size = rectTrans.sizeDelta;
        size.y = height;
        rectTrans.sizeDelta = size;
    }

    public static void SetPositionX(this Transform t, float newX)
    {
        t.position = new Vector3(newX, t.position.y, t.position.z);
    }

    public static void SetPositionY(this Transform t, float newY)
    {
        t.position = new Vector3(t.position.x, newY, t.position.z);
    }

    public static void SetLocalPositionX(this Transform t, float newX)
    {
        t.localPosition = new Vector3(newX, t.localPosition.y, t.localPosition.z);
    }

    public static void SetLocalPositionY(this Transform t, float newY)
    {
        t.localPosition = new Vector3(t.localPosition.x, newY, t.localPosition.z);
    }

    public static void SetPositionZ(this Transform t, float newZ)
    {
        t.position = new Vector3(t.position.x, t.position.y, newZ);
    }

    public static void SetLocalPositionZ(this Transform t, float newZ)
    {
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, newZ);
    }

    public static void SetLocalScale(this Transform t, Vector3 newScale)
    {
        t.localScale = newScale;
    }

    public static void SetLocalScaleZero(this Transform t)
    {
        t.localScale = Vector3.zero;
    }

    public static float GetPositionX(this Transform t)
    {
        return t.position.x;
    }

    public static float GetPositionY(this Transform t)
    {
        return t.position.y;
    }

    public static float GetPositionZ(this Transform t)
    {
        return t.position.z;
    }

    public static float GetLocalPositionX(this Transform t)
    {
        return t.localPosition.x;
    }

    public static float GetLocalPositionY(this Transform t)
    {
        return t.localPosition.y;
    }

    public static float GetLocalPositionZ(this Transform t)
    {
        return t.localPosition.z;
    }
    
    public static bool IsActive(this Transform t)
    {
        if(t&&t.gameObject)
            return t.gameObject.activeInHierarchy;
        return false;
    }
    
    public static RectTransform rectTransform(this Transform t)
    {
        if(t&&t.gameObject)
            return t.gameObject.GetComponent<RectTransform>();
        return null;
    }
    
    public static bool HasRigidbody(this GameObject gobj)
    {
        return (gobj.GetComponent<Rigidbody>() != null);
    }

    public static bool HasAnimation(this GameObject gobj)
    {
        return (gobj.GetComponent<Animation>() != null);
    }

    public static void SetSpeed(this Animation anim, float newSpeed)
    {
        anim[anim.clip.name].speed = newSpeed;
    }

    public static Vector2 ToVector2(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    public static byte ToByte(this string val)
    {
        byte ret = 0;
        try
        {
            if (!String.IsNullOrEmpty(val))
            {
                ret = Convert.ToByte(val);
    }
        }
        catch (Exception)
    {
        }
        return ret;
    }

    public static long ToInt64(this string val)
    {
        long ret = 0;
        try
        {
            if (!String.IsNullOrEmpty(val))
            {
                ret = Convert.ToInt64(val);
            }
        }
        catch (Exception)
        {
        }
        return ret;
    }

    public static float ToFloat(this string val)
    {
        float ret = 0;
        try
        {
            if (!String.IsNullOrEmpty(val))
            {
                ret = Convert.ToSingle(val);
            }
        }
        catch (Exception)
        {
        }
        return ret;
    }

    static public Int32 ToInt32(this string str)
    {
        Int32 ret = 0;
        try
        {
            if (!String.IsNullOrEmpty(str))
            {
                ret = Convert.ToInt32(str);
            }
        }
        catch (Exception)
        {
        }
        return ret;
    }
    public static Int32 ToInt32(this object obj)
    {
        Int32 ret = 0;
        try
        {
            if (obj != null)
            {
                ret = Convert.ToInt32(obj);
            }
        }
        catch (Exception)
        {
        }
        return ret;
    }
    /// <summary>
    /// Get from object Array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="openArgs"></param>
    /// <param name="offset"></param>
    /// <param name="isLog"></param>
    /// <returns></returns>
    public static T Get<T>(this object[] openArgs, int offset, bool isLog = true)
    {
        T ret;
        if ((openArgs.Length - 1) >= offset)
        {
            var arrElement = openArgs[offset];
            if (arrElement == null)
                ret = default(T);
            else
            {
                try
                {
                    ret = (T)Convert.ChangeType(arrElement, typeof(T));
                }
                catch (Exception)
                {
                    if (arrElement is string && string.IsNullOrEmpty(arrElement as string))
                        ret = default(T);
                    else
                    {
                        Log.Error("[Error get from object[],  '{0}' change to type {1}", arrElement, typeof(T));
                        ret = default(T);
                    }
                }
            }
        }
        else
        {
            ret = default(T);
            if (isLog)
                Log.Error("[GetArg] {0} args - offset: {1}", openArgs, offset);
        }

        return ret;
    }
}

// C# 扩展, 扩充C#类的功能
public static class KEngineToolExtensions
{
    // 扩展List/  
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T KFirstOrDefault<T>(this IEnumerable<T> source)
    {
        if (source != null)
        {
            foreach (T item in source)
            {
                return item;
            }
        }

        return default(T);
    }

    public static List<T> KFirst<T>(this IEnumerable<T> source, int num)
    {
        var count = 0;
        var items = new List<T>();
        if (source != null)
        {
            foreach (T item in source)
            {
                if (++count > num)
                {
                    break;
                }
                items.Add(item);
            }
        }

        return items;
    }

    public delegate bool KFilterAction<T>(T t);

    public static List<T> KFilter<T>(this IEnumerable<T> source, KFilterAction<T> testAction)
    {
        var items = new List<T>();
        if (source != null)
        {
            foreach (T item in source)
            {
                if (testAction(item))
                {
                    items.Add(item);
                }
            }
        }

        return items;
    }

    public delegate bool KFilterAction<T, K>(T t, K k);

    public static Dictionary<T, K> KFilter<T, K>(this IEnumerable<KeyValuePair<T, K>> source,
        KFilterAction<T, K> testAction)
    {
        var items = new Dictionary<T, K>();
        if (source != null)
        {
            foreach (KeyValuePair<T, K> pair in source)
            {
                if (testAction(pair.Key, pair.Value))
                {
                    items.Add(pair.Key, pair.Value);
                }
            }
        }

        return items;
    }

    public static T KLastOrDefault<T>(this IEnumerable<T> source)
    {
        var result = default(T);
        foreach (T item in source)
        {
            result = item;
        }
        return result;
    }

    /// <summary>
    /// == Linq Last
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="num"></param>
    /// <returns></returns>
    public static List<T> KLast<T>(this IEnumerable<T> source, int num)
    {
        // 开始读取的位置
        var startIndex = Math.Max(0, source.KToList().Count - num);
        var index = 0;
        var items = new List<T>();
        if (source != null)
        {
            foreach (T item in source)
            {
                if (index < startIndex)
                {
                    continue;
                }
                items.Add(item);
            }
        }

        return items;
    }

    /// <summary>
    /// HashSet AddRange
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static bool AddRange<T>(this HashSet<T> @this, IEnumerable<T> items)
    {
        bool allAdded = true;
        foreach (T item in items)
        {
            allAdded &= @this.Add(item);
        }
        return allAdded;
    }

    public static T[] KToArray<T>(this IEnumerable<T> source)
    {
        var list = new List<T>();
        foreach (T item in source)
        {
            list.Add(item);
        }
        return list.ToArray();
    }

    public static List<T> KToList<T>(this IEnumerable<T> source)
    {
        var list = new List<T>();
        foreach (T item in source)
        {
            list.Add(item);
        }
        return list;
    }

    public static List<T> KUnion<T>(this List<T> first, List<T> second, IEqualityComparer<T> comparer)
    {
        var results = new List<T>();
        var list = first.KToList();
        list.AddRange(second);
        foreach (T item in list)
        {
            var include = false;
            foreach (T result in results)
            {
                if (comparer.Equals(result, item))
                {
                    include = true;
                    break;
                }
            }
            if (!include)
            {
                results.Add(item);
            }
        }
        return results;
    }

    public static string KJoin<T>(this IEnumerable<T> source, string sp)
    {
        var result = new StringBuilder();
        foreach (T item in source)
        {
            if (result.Length == 0)
            {
                result.Append(item);
            }
            else
            {
                result.Append(sp).Append(item);
            }
        }
        return result.ToString();
    }

    public static bool KContains<TSource>(this IEnumerable<TSource> source, TSource value)
    {
        foreach (TSource item in source)
        {
            if (Equals(item, value))
            {
                return true;
            }
        }
        return false;
    }

    // by KK, 获取自动判断JSONObject的str，n
    //public static object Value(this JSONObject jsonObj)
    //{
    //    switch (jsonObj.type)
    //    {
    //        case JSONObject.Type.NUMBER:  // 暂时返回整形！不管浮点了, lua目前少用浮点
    //            return (int)jsonObj.n;
    //        case JSONObject.Type.STRING:
    //            return jsonObj.str;
    //        case JSONObject.Type.NULL:
    //            return null;
    //        case JSONObject.Type.ARRAY:
    //        case JSONObject.Type.OBJECT:
    //            return jsonObj;
    //        case JSONObject.Type.BOOL:
    //            return jsonObj.b;
    //    }

    //    return null;
    //}
}