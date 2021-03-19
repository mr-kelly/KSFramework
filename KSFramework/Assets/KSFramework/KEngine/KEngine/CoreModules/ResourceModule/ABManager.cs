using System;
using System.Collections.Generic;
using KEngine;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2020/12/9 
/// Desc：ab加载管理器
///	关于图集(SpriteAtlas)的建议:
///		1.物品，称号，技能图标等不放在图集中，使用单独的图片 
///		2.每个UIPrefab界面(系统)一个图集
/// </summary>
public class ABManager
{
    /// <summary>
    /// type -> <url,loader>
    /// </summary>
    private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> _loadersPool = new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();
    /// <summary>
    /// todo 等待加载中的ab
    /// </summary>
    private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> waiting = new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();
    public static int MAX_LOAD_NUM = 10;


    #region 垃圾回收 Garbage Collect

    /// <summary>
    /// Loader延迟Dispose
    /// </summary>
    private const float LoaderDisposeTime = 0;

    /// <summary>
    /// 间隔多少秒做一次GC(在AutoNew时)
    /// </summary>
    public static float GcIntervalTime
    {
        get
        {
            /*if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor)
                return 1f;

            return AppConfig.IsDebugBuild ? 5f : 10f;*/
            return 0.1f;
        }
    }

    /// <summary>
    /// 上次做GC的时间
    /// </summary>
    private static float _lastGcTime = -1;

    /// <summary>
    /// 缓存起来要删掉的，供DoGarbageCollect函数用, 避免重复的new List
    /// </summary>
    private static readonly List<AbstractResourceLoader> CacheLoaderToRemoveFromUnUsed = new List<AbstractResourceLoader>();

    /// <summary>
    /// 进行垃圾回收
    /// </summary>
    internal static readonly Dictionary<AbstractResourceLoader, float> UnUsesLoaders =  new Dictionary<AbstractResourceLoader, float>();

    #endregion

    public static int GetCount<T>()
    {
        return GetTypeDict(typeof(T)).Count;
    }

    public static Dictionary<string, AbstractResourceLoader> GetTypeDict(Type type)
    {
        Dictionary<string, AbstractResourceLoader> typesDict;
        if (!_loadersPool.TryGetValue(type, out typesDict))
        {
            typesDict = _loadersPool[type] = new Dictionary<string, AbstractResourceLoader>();
        }

        return typesDict;
    }

    public static int GetRefCount<T>(string url)
    {
        var dict = GetTypeDict(typeof(T));
        AbstractResourceLoader loader;
        if (dict.TryGetValue(url, out loader))
        {
            return loader.RefCount;
        }

        return 0;
    }
    
    /// <summary>
    /// 是否进行垃圾收集
    /// </summary>
    public static void CheckGcCollect()
    {
        if (_lastGcTime.Equals(-1) || (Time.time - _lastGcTime) >= GcIntervalTime)
        {
            DoGarbageCollect();
            _lastGcTime = Time.time;
        }
    }

    /// <summary>
    /// 进行垃圾回收
    /// </summary>
    internal static void DoGarbageCollect()
    {
        foreach (var kv in UnUsesLoaders)
        {
            var loader = kv.Key;
            var time = kv.Value;
            if ((Time.time - time) >= LoaderDisposeTime)
            {
                CacheLoaderToRemoveFromUnUsed.Add(loader);
            }
        }

        for (var i = CacheLoaderToRemoveFromUnUsed.Count - 1; i >= 0; i--)
        {
            try
            {
                var loader = CacheLoaderToRemoveFromUnUsed[i];
                UnUsesLoaders.Remove(loader);
                CacheLoaderToRemoveFromUnUsed.RemoveAt(i);
                loader.Dispose();
            }
            catch (Exception e)
            {
                Log.LogException(e);
            }
        }

        if (CacheLoaderToRemoveFromUnUsed.Count > 0)
        {
            Log.Error("[DoGarbageCollect]CacheLoaderToRemoveFromUnUsed not empty!!");
        }
    }

    #region 对外接口

    #endregion
    
    public static Dictionary<string,SpriteAtlas> SpriteAtlases = new Dictionary<string, SpriteAtlas>();

    public static void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {
        SpriteAtlas atlas = null;
        if (SpriteAtlases.TryGetValue(tag, out atlas))
        {
            if (atlas != null)
            {
                if (Application.isEditor) Log.Info($"Request spriteAtlas {tag}");
                callback(atlas);
            }
            else
            {
                SpriteAtlases.Remove(tag);
                Log.LogError($"not load spriteAtlas {tag}");
            }
        }
        else
        {
            Log.LogError($"not load spriteAtlas {tag}");
        }
    }
}