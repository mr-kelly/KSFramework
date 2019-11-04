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
//#define SLUA
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KEngine;

#if SLUA
using SLua;
using LuaInterface;
#else
using XLua;
using XLua.LuaDLL;
#endif

namespace KSFramework
{
#if !SLUA
    [LuaCallCSharp]
#endif
    public class LuaModule : IModuleInitable
    {
#if SLUA
        private readonly LuaSvr _luaSvr;
#else
        private readonly LuaEnv _luaEnv;
        public LuaEnv ENV
        {
            get
            {
                return _luaEnv;
            }
        }
#endif


        public static LuaModule Instance = new LuaModule();

        public bool IsInited { get; private set; }

        private double _initProgress = 0;

        public double InitProgress { get { return _initProgress; } }

#if SLUA
        public LuaState State
        {
            get { return _luaSvr.luaState; }
        }
#endif

        /// <summary>
        /// 是否开启缓存模式，默认true，首次执行将把执行结果table存起来；在非缓存模式下，也可以通过编辑器的Reload来进行强制刷新缓存
        /// 对实时性重载要求高的，可以把开关设置成false，长期都进行Lua脚本重载，理论上会消耗额外的性能用于语法解析
        /// 
        /// 一般的脚本语言，如Python, NodeJS中，其import, require关键字都会对加载过的模块进行缓存(包括Lua原生的require)；如果不缓存，要注意状态的保存问题
        /// 该值调用频繁，就不放ini了
        /// </summary>
        public static bool CacheMode = false;

        /// <summary>
        /// Import result object caching
        /// </summary>
        Dictionary<string, object> _importCache = new Dictionary<string, object>();

        protected LuaModule()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log("Consturct LuaModule...");
#endif

#if SLUA
            _luaSvr = new LuaSvr();
            _luaSvr.init(progress => { _initProgress = progress; }, () => { });
#else
            _luaEnv = new LuaEnv();
#endif
        }

        /// <summary>
        /// Execute lua script directly!
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <param name="ret">return result</param>
        /// <returns></returns>
        public bool ExecuteScript(byte[] scriptCode, out object ret, string file = "code")
        {
#if SLUA
            return _luaSvr.luaState.doBuffer(scriptCode, Encoding.UTF8.GetString(scriptCode), out ret);
#else
            var results = _luaEnv.DoString(Encoding.UTF8.GetString(scriptCode), file);

            if (results != null && results.Length == 1)
            {
                ret = results[0];
            }
            else
            {
                ret = results;
            }
            return true;
#endif
        }

        /// <summary>
        /// Execute lua script directly!
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        public object ExecuteScript(byte[] scriptCode, string file = "code")
        {
            object ret;
            ExecuteScript(scriptCode, out ret, file);
            return ret;
        }

        /// <summary>
        /// Call script of script path (relative) specify
        /// 
        /// We don't recommend use this method, please use ImportScript which has Caching!
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        public object CallScript(string scriptRelativePath)
        {
            if (string.IsNullOrEmpty(scriptRelativePath))
            {
                return null;
            }
            Debuger.Assert(HasScript(scriptRelativePath), "Not exist Lua: " + scriptRelativePath);

            var scriptPath = GetScriptPath(scriptRelativePath);
            byte[] script;
            if (Log.IsUnityEditor)
            {
                script = File.ReadAllBytes(scriptPath);
            }
            else
            {
                //热更新从PersistentDataPath路径读取
                //script = KResourceModule.LoadSyncFromPersistentDataPath(scriptPath);
                script = KResourceModule.LoadSyncFromStreamingAssets(scriptPath);
            }
            var ret = ExecuteScript(script, scriptRelativePath);
            return ret;
        }

        /// <summary>
        /// Get script full path
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        static string GetScriptPath(string scriptRelativePath)
        {
            var luaPath = AppEngine.GetConfig("KSFramework.Lua", "LuaPath");
            var ext = AppEngine.GetConfig("KEngine", "AssetBundleExt");

            var relativePath = string.Format("{0}/{1}.lua", luaPath, scriptRelativePath);

            if (Log.IsUnityEditor)
            {
                var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath,relativePath);
                return editorLuaScriptPath;
            }
            else
            {
                relativePath += ext;
            }

            return relativePath;
        }

        /// <summary>
        /// whether the script file exists?
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        public bool HasScript(string scriptRelativePath)
        {
            var scriptPath = GetScriptPath(scriptRelativePath);
            if (Log.IsUnityEditor)
                return File.Exists(scriptPath);
            else
                return KResourceModule.IsStreamingAssetsExists(scriptPath);
        }

        /// <summary>
        /// Import script, with caching
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public object Import(string fileName)
        {
            //			if (!HasScript (fileName))
            //                throw new FileNotFoundException(string.Format("Not found UI Lua Script: {0}", fileName));

            return DoImportScript(fileName);
        }

        /// <summary>
        /// Try import script, if 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryImport(string fileName, out object result)
        {
            //            result = null;

            //            if (!HasScript(fileName))
            //                return false;

            result = DoImportScript(fileName);
            return true;
        }

        object DoImportScript(string fileName)
        {
            object obj;
            if (!_importCache.TryGetValue(fileName, out obj))
            {
                obj = this.CallScript(fileName);
                if (CacheMode)
                    _importCache[fileName] = obj;
            }

            return obj;
        }


        /// <summary>
        /// Clear all imported cache
        /// </summary>
        public void ClearAllCache()
        {
            _importCache.Clear();
        }

        /// <summary>
        /// Clear dest lua script cache
        /// </summary>
        /// <param name="uiLuaPath"></param>
        /// <returns></returns>
        public bool ClearCache(string uiLuaPath)
        {
            return _importCache.Remove(uiLuaPath);
        }

        public IEnumerator Init()
        {
#if SLUA
            int frameCount = 0;
            while (!_luaSvr.inited)
            {
                if (frameCount % 30 == 0)
                    Log.LogWarning("SLua Initing...");
                yield return null;
                frameCount++;
            }
            var L = _luaSvr.luaState.L;
            LuaDLL.lua_pushcfunction(L, LuaImport);
            LuaDLL.lua_setglobal(L, "import");
            LuaDLL.lua_pushcfunction(L, LuaUsing);
            LuaDLL.lua_setglobal(L, "using"); // same as SLua's import, using namespace
            LuaDLL.lua_pushcfunction(L, ImportCSharpType);
            LuaDLL.lua_setglobal(L, "import_type"); // same as SLua's SLua.GetClass(), import C# type
#else

            var L = _luaEnv.L;
            Lua.lua_pushstdcallcfunction(L, LuaImport);
            Lua.xlua_setglobal(L, "require");
            //TODO 如果有新加入的库，加入进去
//            _luaEnv.AddBuildin("",);
            yield return null;
#endif

            CallScript("Init");

            IsInited = true;
        }


#if SLUA
		[LuaInterface.MonoPInvokeCallback(typeof(LuaCSFunction))]
		static public int ImportCSharpType(IntPtr l)
		{
			try
			{
				string cls;
				Helper.checkType(l, 1, out cls);
				Type t = LuaObject.FindType(cls);
				if (t == null)
				{
					return Helper.error(l, "Can't find {0} to create", cls);
				}

				LuaClassObject co = new LuaClassObject(t);
				LuaObject.pushObject(l,co);
				Helper.pushValue(l, true);
				return 2;
			}
			catch (Exception e)
			{
				return Helper.error(l, e);
			}
		}
        /// <summary>
        /// same as SLua default import
        /// </summary>
        /// <param name="luastate"></param>
        /// <returns></returns>
        [LuaInterface.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private int LuaUsing(IntPtr l)
        {
            try
            {
                LuaDLL.luaL_checktype(l, 1, LuaTypes.LUA_TSTRING);
                string str = LuaDLL.lua_tostring(l, 1);

                string[] ns = str.Split('.');

                LuaDLL.lua_pushglobaltable(l);

                for (int n = 0; n < ns.Length; n++)
                {
                    LuaDLL.lua_getfield(l, -1, ns[n]);
                    if (!LuaDLL.lua_istable(l, -1))
                    {
                        return LuaObject.error(l, "expect {0} is type table", ns);
                    }
                    LuaDLL.lua_remove(l, -2);
                }

                LuaDLL.lua_pushnil(l);
                while (LuaDLL.lua_next(l, -2) != 0)
                {
                    string key = LuaDLL.lua_tostring(l, -2);
                    LuaDLL.lua_getglobal(l, key);
                    if (!LuaDLL.lua_isnil(l, -1))
                    {
                        LuaDLL.lua_pop(l, 1);
                        return LuaObject.error(l, "{0} had existed, import can't overload it.", key);
                    }
                    LuaDLL.lua_pop(l, 1);
                    LuaDLL.lua_setglobal(l, key);
                }

                LuaDLL.lua_pop(l, 1);

                LuaObject.pushValue(l, true);
                return 1;
            }
            catch (Exception e)
            {
                return LuaObject.error(l, e);
            }
        }
        
        /// <summary>
        /// This will override SLua default `import`
        /// 
        /// TODO: cache the result!
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        [LuaInterface.MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LuaImport(IntPtr L)
        {
            LuaModule luaModule = Instance;

            string fileName = LuaDLL.lua_tostring(L, 1);
            var obj = luaModule.Import(fileName);


            LuaObject.pushValue(L, obj);
            LuaObject.pushValue(L, true);
            return 2;

        }
#else
        [MonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int LuaImport(IntPtr L)
        {
            LuaModule luaModule = Instance;

            string fileName = Lua.lua_tostring(L, 1);
            var obj = luaModule.Import(fileName);

            ObjectTranslator ot = ObjectTranslatorPool.Instance.Find(L);
            ot.PushAny(L, obj);
            ot.PushAny(L, true);

            return 2;

        }
#endif

    }

}
