using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 作者：赵青青 (569032731@qq.com)
/// 时间：2019/11/4 20:53
/// 说明：对于Unity的Find扩充，可查找active=false的节点
/// </summary>
public static class FindEx
{
    #region 扩展Unity的FindChild

    private static Queue<Transform> s_findchild_stack = new Queue<Transform>();

    /// <summary>
    /// 按名字查找
    /// 出于性能考虑应避免节点太深，可优先查找他们共用的父节点
    /// 经过多次测试和项目中使用此查找方法性能比递归调用耗时和gc都更少
    /// 比如下面的层级，dst_name会忽略层级查找到节点
    ///     ."dst_name" 单个名字
    ///     ."name1/name2/.../dst_name" 多个名字组合的路径，越详细性能越好
    /// </summary>
    /// <param name="go">父节点</param>
    /// <param name="id">要查找的节点名字</param>
    /// <param name="check_visible">是否只查找gameobject.active=true的节点</param>
    /// <param name="raise_error">当查找失败时是否打印Error</param>
    public static GameObject FindChild(this GameObject go, string id, bool check_visible = false, bool raise_error = true)
    {
        if (!go)
        {
            if (raise_error)
            {
                Debug.LogError("FindChild faild. go is null!");
            }

            return null;
        }

        var t = FindChild(go.transform, id, check_visible, raise_error);
        if (t) return t.gameObject;
        return null;
    }

    public static T FindChild<T>(this GameObject go, string id, bool check_visible = false, bool raise_error = true) where T : Component
    {
        var t = FindChild(go.transform, id, check_visible, raise_error);
        if (t)
        {
            var com = t.GetComponent<T>();
            if (com == null && raise_error)
            {
                Debug.LogError(string.Format("FindChild<T> faild. {0} Not Have Component:{1}", t, typeof(T)));
            }

            return com;
        }

//        return null;
        return default(T);
    }

    public static Transform FindChild(Transform findTrans, string id, bool check_visible, bool raise_error)
    {
        if (!findTrans)
        {
            if (raise_error)
            {
                Debug.LogError("FindChild faild. findTrans is null!");
            }

            return null;
        }

        Transform transform = findTrans;
        if (string.IsNullOrEmpty(id))
            return null;
        if (check_visible && !findTrans.IsActive())
            return null;
        if (id == ".")
            return findTrans;
        if (id.IndexOf('/') >= 0)
        {
            string str = id;
            char[] chArray = new char[1] {'/'};
            foreach (string id1 in str.Split(chArray))
            {
                findTrans = FindChildDirect(findTrans, id1, check_visible);
                if (findTrans == null)
                {
                    if (raise_error)
                    {
                        Debug.LogError(string.Format("FindChild failed, id:{0} ,parent={1}", id, transform.name));
                        break;
                    }

                    break;
                }
            }

            return findTrans;
        }

        findTrans = FindChildDirect(findTrans, id, check_visible);
        if (findTrans == null && raise_error)
            Debug.LogError(string.Format("FindChild failed, id:{0},parent={1}", id, transform));
        return findTrans;
    }

    public static Transform FindChildX(this Transform t, string id, bool check_visible = false, bool raise_error = true)
    {
        return FindChild(t, id, check_visible, raise_error);
    }

    private static Transform FindChildDirect(Transform trans, string id, bool check_visible)
    {
        Profiler.BeginSample("FindChildDirect");
        Queue<Transform> findchildStack = s_findchild_stack;
        findchildStack.Enqueue(trans);
        while (findchildStack.Count > 0)
        {
            trans = findchildStack.Dequeue();
            Transform t1 = trans.Find(id);
            if (t1 != null && (!check_visible || t1.IsActive()))
            {
                findchildStack.Clear();
                Profiler.EndSample();
                return t1;
            }

            int num = trans.childCount;
            for (int i = 0; i < num; i++)
            {
                t1 = trans.GetChild(i);
                if (!check_visible || t1.IsActive())
                    findchildStack.Enqueue(t1);
            }
        }

        Profiler.EndSample();
        return null;
    }

    /// <summary>
    /// 搜索第一个匹配的
    /// </summary>
    public static T FindInChild<T>(this GameObject go, string name = "") where T : Component
    {
        if (!go)
            return null;

        T comp = null;
        if (!string.IsNullOrEmpty(name) && !go.name.Contains(name))
        {
            comp = null;
        }
        else
        {
            comp = go.GetComponent<T>();
        }

        if (!comp)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                comp = FindInChild<T>(go.transform.GetChild(i).gameObject, name);
                if (comp)
                    return comp;
            }
        }

        return comp;
    }

    #endregion
}