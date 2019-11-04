using System;
using UnityEngine;
using System.Collections;
using KEngine;
//using SLua;
using XLua;

public static class LuaBehaivourExtensions
{
    public static KSFramework.LuaBehaviour AddLuaComponent(this GameObject gameObj, string luaPath)
    {
        return KSFramework.LuaBehaviour.Create(gameObj, luaPath);
    }

    public static object CallFunc(this LuaTable luaTable, string funcName, params object[] args)
    {
        LuaFunction func = luaTable.Get<LuaFunction>(funcName);
        if (func == null)
        {
            return null;
        }
        return func.Call(args);
    }
}

namespace KSFramework
{
    /// <summary>
    /// Lua端的MonoBehaivour
    /// xlua版本请参考xlua官方的LuaBehaviour
    /// </summary>
    public class LuaBehaviour : MonoBehaviour
    {
        public string LuaPath = null;

        private LuaTable _cacheTable;

        public static LuaBehaviour Create(GameObject attach, string luaPath)
        {
            // only one same lua behaviour can attach
            foreach (var b in attach.GetComponents<LuaBehaviour>())
            {
                if (b.LuaPath == luaPath)
                    return b;
            }
            var behaviour = attach.AddComponent<LuaBehaviour>();
            behaviour.LuaPath = luaPath;
            behaviour.Awake();
            return behaviour;
        }

        protected virtual void Init()
        {
            Reload();
        }

        public void Reload()
        {
            _cacheTable = null;
            var ret = LuaModule.Instance.CallScript(LuaPath);
            Debuger.Assert(ret is LuaTable, "{0} Script Must Return Lua Table with functions!", LuaPath);
            _cacheTable = ret as LuaTable;
        }

        public object CallLuaFunction(string function, params object[] args)
        {
            if (string.IsNullOrEmpty(LuaPath))
            {
                return null;
            }
            if (!LuaModule.CacheMode)
                Reload();
            if (_cacheTable == null)
                throw new Exception(string.Format("{0}: cannot get table!", LuaPath));

            var retFunc = _cacheTable[function];
            if (retFunc != null)
            {
                if (!(retFunc is LuaFunction))
                {
                    throw new Exception(string.Format("{0}: {1} must be function!", LuaPath, function));
                }

                var func = retFunc as LuaFunction;
#if xLua
                return func.Call(args);
#else
                           return func.call(args);
#endif
            }

            return null;
        }

        protected virtual void Awake()
        {
            if (!string.IsNullOrEmpty(LuaPath))
            {
                Init();
                CallLuaFunction("Awake", _cacheTable, this);
            } // else Null Lua Path, pass Awake!
        }

        protected void Start()
        {
            CallLuaFunction("Start");
        }

        protected void Update()
        {
            CallLuaFunction("Update");
        }

        protected void LateUpdate()
        {
            CallLuaFunction("LateUpdate");
        }

        protected void OnDestroy()
        {
            CallLuaFunction("OnDestroy", _cacheTable, this);
        }
    }

}
