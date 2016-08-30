using System;
using UnityEngine;
using System.Collections;
using KEngine;
using SLua;

public static class LuaBehaivourExtensions
{
    public static KSFramework.LuaBehaviour AddLuaComponent(this GameObject gameObj, string luaPath)
    {
        return KSFramework.LuaBehaviour.Create(gameObj, luaPath);
    }
}

namespace KSFramework
{
    /// <summary>
    /// Lua端的MonoBehaivour
    /// </summary>
    public class LuaBehaviour : MonoBehaviour
    {
        /// <summary>
        /// 是否开启缓存模式，默认true，首次执行将把执行结果table存起来；在非缓存模式下，也可以通过编辑器的Reload来进行强制刷新缓存
        /// 对实时性重载要求高的，可以把开关设置成false，长期都进行Lua脚本重载，理论上会消耗额外的性能用于语法解析
        /// 该值调用频繁，就不放ini了
        /// 跟LuaUIController的CacheMode分别两个值控制
        /// </summary>
        public static bool CacheMode = true;

        /// <summary>
        /// 是否通过工厂函数创建，不允许直接AddComponent
        /// </summary>
        private bool _safeCreate = false;

        public string LuaPath;

        private LuaTable CacheTable;

        public static LuaBehaviour Create(GameObject attach, string luaPath)
        {
            var behaviour = attach.AddComponent<LuaBehaviour>();
            behaviour._safeCreate = true;
            behaviour.Init(luaPath);
            return behaviour;
        }

        protected virtual void Init(string luaPath)
        {
            LuaPath = luaPath;
            Reload();
        }

        public void Reload()
        {
            CacheTable = null;
            var ret = LuaModule.Instance.CallScript(LuaPath);
            Debuger.Assert(ret is LuaTable, "{0} Script Must Return Lua Table with functions!", LuaPath);
            CacheTable = ret as LuaTable;
        }

        public object CallLuaFunction(string function, params object[] args)
        {
            if (!CacheMode)
                Reload();
            if (CacheTable == null)
                throw new Exception(string.Format("{0}: cannot get table!", LuaPath));

            var retFunc = CacheTable[function];
            if (retFunc != null)
            {
                if (!(retFunc is LuaFunction))
                {
                    throw new Exception(string.Format("{0}: {1} must be function!", LuaPath, function));
                }

                var func = retFunc as LuaFunction;

                return func.call(args);
            }

            return null;
        }
        protected virtual void Awake()
        {
            if (!string.IsNullOrEmpty(LuaPath)) _safeCreate = true;
            CallLuaFunction("Awake");
        }

        protected void Start()
        {
            if (!_safeCreate) throw new Exception("LuaBehaivour error! would you AddComponent<LuaBehaivour> ? use LuaBehaivour.Create() instead!");

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
    }

}
