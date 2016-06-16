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
using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
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
            _luaSvr.init(progress => { }, () => { });
        }

        /// <summary>
        /// Execute lua script directly!
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        object _DoScript(byte[] scriptCode)
        {
            string script = Encoding.UTF8.GetString(scriptCode);
            return _luaSvr.luaState.doString(script);
        }

        /// <summary>
        /// Call script of script path (relative) specify
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        public object CallScript(string scriptRelativePath)
        {
            Debuger.Assert(HasScript(scriptRelativePath), "Not exist Lua: " + scriptRelativePath);

            var scriptPath = GetScriptPath(scriptRelativePath);
            byte[] script;
            if (Log.IsUnityEditor)
                script = File.ReadAllBytes(scriptPath);
            else
                script = KResourceModule.LoadSyncFromStreamingAssets(scriptPath);
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
            var luaPath = AppEngine.GetConfig("KSFramework.Lua", "LuaPath");
            var ext = AppEngine.GetConfig("KEngine", "AssetBundleExt");

            var relativePath = string.Format("{0}/{1}.lua", luaPath, scriptRelativePath);

            if (Log.IsUnityEditor)
            {
                var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath,
                    relativePath);

                return editorLuaScriptPath;
            }

            relativePath += ext;
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

        public IEnumerator Init()
        {
            int frameCount = 0;
            while (!_luaSvr.inited)
            {
                if (frameCount % 30 == 0)
                    Log.LogWarning("SLua Initing...");
                yield return null;
                frameCount++;
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
            var obj = KSGame.Instance.LuaModule.CallScript(fileName);

            LuaObject.pushValue(L, obj);
            LuaObject.pushValue(L, true);
            return 2;
        }

    }

}
