using System;
using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using KEngine.UI;
using SLua;
using UnityEngine.UI;

namespace KSFramework
{
    /// <summary>
    /// Auto Add Lua
    /// </summary>
    public class SLuaUIController : KEngine.UI.UIController
    {
        private LuaTable _luaTable;
        public override void OnInit()
        {
            base.OnInit();

            CheckInitScript();
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            CheckInitScript();

            var onOpenFuncObj = _luaTable["OnOpen"];
            if (onOpenFuncObj == null)
            {
                KLogger.LogError("Not Exists `OnOpen` in lua: {0}", UITemplateName);
                return;
            }

            var newArgs = new object[args.Length + 1];
            newArgs[0] = _luaTable;
            for (var i = 0; i < args.Length; i++)
            {
                newArgs[i + 1] = args[i];
            }

            (onOpenFuncObj as LuaFunction).call(newArgs);
        }

        /// <summary>
        /// Try to load script and init.
        /// Script will be cached,
        /// But in development, script cache can be clear, which will be load and init in the next time
        /// </summary>
        void CheckInitScript()
        {
            // if cacheing? ignore!
            if (_luaTable != null)
                return;

            var scriptResult = Game.Instance.SLuaModule.CallScript(string.Format("UI/UI{0}", UITemplateName));
            Debuger.Assert(scriptResult  is LuaTable, "{0} Script Must Return Lua Table with functions!", UITemplateName);

            _luaTable = scriptResult  as LuaTable;

            var newFuncObj = _luaTable["New"]; // if a New function exist, new a table!
            if (newFuncObj != null)
            {
                var newTableObj = (newFuncObj as LuaFunction).call(this);
                _luaTable = newTableObj as LuaTable;
            }

            var luaInitObj = _luaTable["OnInit"];
            Debuger.Assert(luaInitObj is LuaFunction, "Must have OnInit function - {0}", UIName);

            (luaInitObj as LuaFunction).call(_luaTable, this);
            
        }

        public UnityEngine.Object GetControl(string typeName, string uri, Transform findTrans)
        {
            return GetControl(typeName, uri, findTrans);
        } 

        public UnityEngine.Object GetControl(string typeName, string uri)
        {
            return GetControl(typeName, uri, null, true);
        }
        public UnityEngine.Object GetControl(string typeName, string uri, Transform findTrans, bool isLog)
        {
            if (findTrans == null)
                findTrans = transform;

            Transform trans = findTrans.Find(uri);
            if (trans == null)
            {
                if (isLog)
                    KLogger.LogError("Get UI<{0}> Control Error: " + uri, this);
                return null;
            }

            if (typeName == "GameObject")
                return trans.gameObject;

            return trans.GetComponent(typeName);
        }

    }
}
