using System;
using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using LuaInterface;
using SLua;

namespace KSFramework
{
    public class LuaModule : IModuleInitable
    {
        private LuaSvr _luaSvr;

        public LuaModule()
        {
            _luaSvr = new LuaSvr();
        }

        /// <summary>
        /// Execute lua script directly!
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        object _DoScript(string scriptCode)
        {
            return _luaSvr.luaState.doString(scriptCode);
        }

        /// <summary>
        /// Call script of script path (relative) specify
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        public object CallScript(string scriptRelativePath)
        {
            var scriptPath = GetScriptPath(scriptRelativePath);
            Debuger.Assert(HasScript(scriptRelativePath), "Not exist Lua: " + scriptRelativePath);
            var script = File.ReadAllText(scriptPath);
            var ret = _DoScript(script);
            return ret;
        }

        /// <summary>
        /// Get script full path
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        string GetScriptPath(string scriptRelativePath)
        {
            var relativePath = string.Format("Lua/{0}.lua", scriptRelativePath);

            var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath,
                relativePath);

            return editorLuaScriptPath;

        }

        /// <summary>
        /// whether the script file exists?
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        public bool HasScript(string scriptRelativePath)
        {
            var scriptPath = GetScriptPath(scriptRelativePath);
            return File.Exists(scriptPath);
        }

        public IEnumerator Init()
        {
            _luaSvr.init(progress => { }, () => { });


            var startTime = Time.time;
            while (!_luaSvr.inited)
            {
                if ((Time.time - startTime) > 10)
                {
                    if (Time.frameCount % 10 == 0)
                        Log.LogError("SLua Init too long time!!!!");
                }
                yield return null;
            }

            var L = _luaSvr.luaState.L;
            LuaDLL.lua_pushcfunction(L, import);
            LuaDLL.lua_setglobal(L, "import");
            CallScript("Init");
        }

        /// <summary>
        /// This will override SLua default `import`
        /// 
        /// TODO: cache the result!
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        internal static int import(IntPtr L)
        {
            string fileName = LuaDLL.lua_tostring(L, 1);
            var obj = Game.Instance.LuaModule.CallScript(fileName);

            LuaObject.pushValue(L, obj);
            LuaObject.pushValue(L, true);
            return 2;
        }

    }

}
