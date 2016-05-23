using System;
using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using KEngine.UI;
using SLua;

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
            var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath,
                string.Format("Lua/UI/UI{0}.lua", UITemplateName));
            Debuger.Assert(File.Exists(editorLuaScriptPath), "Not exist Lua: " + editorLuaScriptPath);
            var script = File.ReadAllText(editorLuaScriptPath);
            var ret = Game.Instance.SLuaModule.CallScript(script);

            Debuger.Assert(ret is LuaTable, "{0} Script Must Return Lua Table with functions!", editorLuaScriptPath);

            _luaTable = ret as LuaTable;


            var luaInitObj = _luaTable["OnInit"];

            Debuger.Assert(luaInitObj is LuaFunction, "Must have OnInit function - {0}", UIName);

            (luaInitObj as LuaFunction).call(_luaTable, this);
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
