using System;
using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using KEngine.UI;
//using SLua;
using XLua;
using UnityEngine.UI;

namespace KSFramework
{
    /// <summary>
    /// 自动根据UIName和UITemplateName，寻找适合的Lua脚本执行
    /// </summary>
    public class LuaUIController : KEngine.UI.UIController
    {
        /// <summary>
        /// 一般编辑器模式下用于reload时用，记录上一次OnOpen的参数
        /// </summary>
        public object[] LastOnOpenArgs { get; private set; }

        LuaTable _luaTable;

        /// <summary>
        /// Lua Script for this UI 's path
        /// </summary>
        public string UILuaPath
        {
            get
            {
                var relPath = string.Format("UI/{0}/{0}", UITemplateName);
                return relPath;
            }
        }

        /// <summary>
        /// Whether or not cache mode
        /// </summary>
        protected virtual bool IsCachedLuaTable
        {
            get
            {
                return LuaModule.CacheMode;
            }
        }

        public override void OnInit()
        {
            base.OnInit();

            if (!CheckInitScript(true))
                return;
        }

        /// <summary>
        /// 调用Lua:OnOpen函数
        /// </summary>
        /// <param name="args"></param>
        public override void OnOpen(params object[] args)
        {
            // 编辑器模式下，记录
            LastOnOpenArgs = args;

            base.OnOpen(args);
            if (!CheckInitScript())
                return;

            var onOpenFuncObj = _luaTable.Get<LuaFunction>("OnOpen");
            if (onOpenFuncObj == null)
            {
                Log.LogError("Not Exists `OnOpen` in lua: {0}", UITemplateName);
                return;
            }

            var newArgs = new object[args.Length + 1];
            newArgs[0] = _luaTable;
            for (var i = 0; i < args.Length; i++)
            {
                newArgs[i + 1] = args[i];
            }

            (onOpenFuncObj as LuaFunction).Call(newArgs);
        }

        public override void OnClose()
        {
            base.OnClose();
            if (!CheckInitScript())
                return;
            var closeFunc = _luaTable["OnClose"];
            if (closeFunc != null)
            {
                (closeFunc as LuaFunction).Call(_luaTable);
            }
        }

        /// <summary>
        /// Try to load script and init.
        /// Script will be cached,
        /// But in development, script cache can be clear, which will be load and init in the next time
        /// 
        /// 开发阶段经常要使用Lua热重载，热重载过后，要确保OnInit重新执行
        /// </summary>
        bool CheckInitScript(bool showWarn = false)
        {
            if (!IsCachedLuaTable)
            {
                ClearLuaTableCache();
            }

            var relPath = UILuaPath;

            var luaModule = KSGame.Instance.LuaModule;
            object scriptResult;
            if (!luaModule.TryImport(relPath, out scriptResult))
            {
                if (showWarn)
                    Log.LogWarning("Import UI Lua Script failed: {0}", relPath);
                return false;
            }

            scriptResult = KSGame.Instance.LuaModule.CallScript(relPath);
            Debuger.Assert(scriptResult is LuaTable, "{0} Script Must Return Lua Table with functions!", UITemplateName);

            _luaTable = scriptResult as LuaTable;

            var newFuncObj = _luaTable.Get<LuaFunction>("new"); // if a New function exist, new a table!
            if (newFuncObj != null)
            {
#if SLUA
				var newTableObj = (newFuncObj as LuaFunction).call(this);
				_luaTable = newTableObj as LuaTable;
#else
                var newTableObj = (newFuncObj as LuaFunction).Call(this);
                _luaTable = newTableObj[0] as LuaTable;
#endif
            }

            SetOutlet(_luaTable);
            var luaInitObj = _luaTable.Get<LuaFunction>("OnInit");
            Debuger.Assert(luaInitObj is LuaFunction, "Must have OnInit function - {0}", UIName);

            // set table variable `Controller` to this
            _luaTable["Controller"] = this;

#if SLUA
                (luaInitObj as LuaFunction).call(_luaTable, this);
#else
            (luaInitObj as LuaFunction).Call(_luaTable, this);
#endif

            return true;
        }

        public void SetOutlet(LuaTable _luaTable)
        {
            if (_luaTable != null)
            {
                Action<UILuaOutlet> fun = delegate (UILuaOutlet outlet)
                {
                    for (var i = 0; i < outlet.OutletInfos.Count; i++)
                    {
                        var outletInfo = outlet.OutletInfos[i];

                        var gameObj = outletInfo.Object as GameObject;
                        if (gameObj == null || outletInfo.ComponentType == typeof(UnityEngine.GameObject).FullName)
                        {
                            _luaTable.Set<string, UnityEngine.Object>(outletInfo.Name, outletInfo.Object);
                            continue;
                        }

                        if (outletInfo.ComponentType == typeof(UnityEngine.Transform).FullName)
                        {
                            _luaTable.Set<string, Component>(outletInfo.Name, gameObj.transform);
                        }
                        else
                        {
                            var comp = gameObj.GetComponent(outletInfo.ComponentType);
                            //UnityEngine.xxx，非UnityEngine.UI.xxx，只能通过typof获取。
                            if (comp == null && outletInfo.ComponentType.StartsWith("UnityEngine"))
                            {
                                //UnityEngine.xxx下的使用typeof获取
                                var comNames= outletInfo.ComponentType.Split('.');
                                if (!comNames[1].StartsWith("UI"))
                                {
                                    var components = gameObj.GetComponents<Component>();
                                    for (var c = 0; c < components.Length; c++)
                                    {
                                        var typeName = components[c].GetType().FullName;
                                        if (typeName == outletInfo.ComponentType)
                                        {
                                            comp = components[c];
                                            break;
                                        }
                                    }
                                }
                            }
                     
                            if (comp == null)
                            {
                                var fmt = "Missing Component `{0}` at object `{1}` which named `{2}`";
                                Debug.LogError(string.Format(fmt, outletInfo.ComponentType, gameObj, outletInfo.Name));
                            }
                            else
                            {
                                _luaTable.Set<string, Component>(outletInfo.Name, comp);
                            }
                        }
                    }
                };


                UILuaOutletCollection outletCollection = this.GetComponent<UILuaOutletCollection>();
                if (outletCollection)
                {
                    if (outletCollection.UILuaOutlets != null && outletCollection.UILuaOutlets.Length > 0)
                    {
                        for (int i = 0; i < outletCollection.UILuaOutlets.Length; i++)
                        {
                            UILuaOutlet item = outletCollection.UILuaOutlets[i];
                            if (item != null)
                            {
                                fun(item);
                            }
                        }
                    }
                }
                else
                {
                    var outlet = this.GetComponent<UILuaOutlet>();
                    if (outlet != null)
                    {
                        fun(outlet);
                    }
                }

            }
        }

        //slua版本
        public void SetOutlet_Slua(LuaTable _luaTable)
        {
            var outlet = this.GetComponent<UILuaOutlet>();
            if (outlet != null)
            {
                for (var i = 0; i < outlet.OutletInfos.Count; i++)
                {
                    var outletInfo = outlet.OutletInfos[i];

                    var gameObj = outletInfo.Object as GameObject;
                    if (gameObj == null || outletInfo.ComponentType == typeof(UnityEngine.GameObject).FullName)
                    {
                        _luaTable[outletInfo.Name] = outletInfo.Object;
                        continue;
                    }

                    if (outletInfo.ComponentType == typeof(UnityEngine.Transform).FullName)
                    {
                        _luaTable[outletInfo.Name] = gameObj.transform;
                    }
                    else if (outletInfo.ComponentType == typeof(UnityEngine.RectTransform).FullName)
                    {
                        _luaTable[outletInfo.Name] = gameObj.GetComponent(typeof(UnityEngine.RectTransform));
                    }
                    else if (outletInfo.ComponentType == typeof(UnityEngine.Canvas).FullName)
                    {
                        _luaTable[outletInfo.Name] = gameObj.GetComponent(typeof(UnityEngine.Canvas));
                    }
                    else
                    {
                        var comp = gameObj.GetComponent(outletInfo.ComponentType);
                        if (comp == null)
                        {
                            var fmt = "Missing Component `{0}` at object `{1}` which named `{2}`";
                            Debug.LogError(string.Format(fmt, outletInfo.ComponentType, gameObj, outletInfo.Name));
                        }
                        else
                        {
                            _luaTable[outletInfo.Name] = comp;
                        }
                    }
                }
            }
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
                    Log.LogError("Get UI<{0}> Control Error: " + uri, this);
                return null;
            }

            if (typeName == "GameObject")
                return trans.gameObject;

            return trans.GetComponent(typeName);
        }

        /// <summary>
        /// 清理Lua脚本缓存，下次执行时将重新加载Lua
        /// </summary>
        public void ClearLuaTableCache()
        {
            _luaTable = null;

            var luaModule = KSGame.Instance.LuaModule;
            luaModule.ClearCache(UILuaPath);
            Log.Warning("Reload Lua: {0}", UILuaPath);
        }

    }
}
