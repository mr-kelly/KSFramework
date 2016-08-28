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
	/// 自动根据UIName和UITemplateName，寻找适合的Lua脚本执行
	/// </summary>
	public class LuaUIController : KEngine.UI.UIController
	{
		/// <summary>
		/// 是否开启缓存模式，默认true，首次执行将把执行结果table存起来；在非缓存模式下，也可以通过编辑器的Reload来进行强制刷新缓存
		/// 对实时性重载要求高的，可以把开关设置成false，长期都进行Lua脚本重载，理论上会消耗额外的性能用于语法解析
		/// 该值调用频繁，就不放ini了
		/// </summary>
		public static bool CacheMode = true;

		/// <summary>
		/// 缓存的LuaTable
		/// </summary>
		private LuaTable _luaTable;

		/// <summary>
		/// 一般编辑器模式下用于reload时用，记录上一次OnOpen的参数
		/// </summary>
		public object[] LastOnOpenArgs { get; private set; }

		public override void OnInit ()
		{
			base.OnInit ();

			if (!CheckInitScript (true))
				return;
		}

		/// <summary>
		/// 调用Lua:OnOpen函数
		/// </summary>
		/// <param name="args"></param>
		public override void OnOpen (params object[] args)
		{
			// 编辑器模式下，记录
			LastOnOpenArgs = args;

			base.OnOpen (args);
			if (!CheckInitScript ())
				return;

			var onOpenFuncObj = _luaTable ["OnOpen"];
			if (onOpenFuncObj == null) {
				Log.LogError ("Not Exists `OnOpen` in lua: {0}", UITemplateName);
				return;
			}

			var newArgs = new object[args.Length + 1];
			newArgs [0] = _luaTable;
			for (var i = 0; i < args.Length; i++) {
				newArgs [i + 1] = args [i];
			}

			(onOpenFuncObj as LuaFunction).call (newArgs);
		}

		/// <summary>
		/// Try to load script and init.
		/// Script will be cached,
		/// But in development, script cache can be clear, which will be load and init in the next time
		/// 
		/// 开发阶段经常要使用Lua热重载，热重载过后，要确保OnInit重新执行
		/// </summary>
		bool CheckInitScript (bool showWarn = false)
		{
			// 有缓存的lua，根据开关判定是否重新加载
			if (_luaTable != null && CacheMode)
				return true;

			var relPath = string.Format ("UI/{0}/{0}", UITemplateName);

			if (!KSGame.Instance.LuaModule.HasScript (relPath)) {
				if (showWarn)
					Log.LogWarning ("Not found UI Lua Script: {0}", relPath);
				return false;
			}

			var scriptResult = KSGame.Instance.LuaModule.CallScript (relPath);
			Debuger.Assert (scriptResult is LuaTable, "{0} Script Must Return Lua Table with functions!", UITemplateName);

			_luaTable = scriptResult as LuaTable;

			var newFuncObj = _luaTable ["New"]; // if a New function exist, new a table!
			if (newFuncObj != null) {
				var newTableObj = (newFuncObj as LuaFunction).call (this);
				_luaTable = newTableObj as LuaTable;
			}

			var outlet = this.GetComponent<UILuaOutlet> ();
			if (outlet != null) {
				for (var i = 0; i < outlet.OutletInfos.Count; i++) {
					var outletInfo = outlet.OutletInfos [i];

					var gameObj = outletInfo.Object as GameObject;

					if (gameObj != null)
						_luaTable [outletInfo.Name] = gameObj.GetComponent (outletInfo.ComponentType);
					else
						_luaTable [outletInfo.Name] = outletInfo.Object;
				}

			}


			var luaInitObj = _luaTable ["OnInit"];
			Debuger.Assert (luaInitObj is LuaFunction, "Must have OnInit function - {0}", UIName);

			(luaInitObj as LuaFunction).call (_luaTable, this);

			return true;
		}

		public UnityEngine.Object GetControl (string typeName, string uri, Transform findTrans)
		{
			return GetControl (typeName, uri, findTrans);
		}

		public UnityEngine.Object GetControl (string typeName, string uri)
		{
			return GetControl (typeName, uri, null, true);
		}

		public UnityEngine.Object GetControl (string typeName, string uri, Transform findTrans, bool isLog)
		{
			if (findTrans == null)
				findTrans = transform;

			Transform trans = findTrans.Find (uri);
			if (trans == null) {
				if (isLog)
					Log.LogError ("Get UI<{0}> Control Error: " + uri, this);
				return null;
			}

			if (typeName == "GameObject")
				return trans.gameObject;

			return trans.GetComponent (typeName);
		}

		/// <summary>
		/// 清理Lua脚本缓存，下次执行时将重新加载Lua
		/// </summary>
		public void ReloadLua ()
		{
			if (_luaTable != null) {
				_luaTable.Dispose ();
				_luaTable = null;
			}
		}
	}
}
