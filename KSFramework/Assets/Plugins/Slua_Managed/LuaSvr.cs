// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// uncomment this will use static binder(class BindCustom/BindUnity), 
// init will not use reflection to speed up the speed
//#define USE_STATIC_BINDER  

namespace SLua
{
	using System;
	using System.Threading;
	using System.Collections;
	using System.Collections.Generic;
	using LuaInterface;
	using System.Reflection;
#if !SLUA_STANDALONE
	using UnityEngine;
	using Debug = UnityEngine.Debug;
#endif

	public enum LuaSvrFlag {
		LSF_BASIC = 0,
		LSF_DEBUG = 1,
		LSF_EXTLIB = 2,
		LSF_3RDDLL = 4
	};

	public class LuaSvr 
	{
		public LuaState luaState;
#if !SLUA_STANDALONE
		static LuaSvrGameObject lgo;
#endif
		int errorReported = 0;
		public bool inited = false;

		public LuaSvr()
		{
#if !SLUA_STANDALONE
			GameObject go = new GameObject("LuaSvrProxy");
			lgo = go.AddComponent<LuaSvrGameObject>();
			GameObject.DontDestroyOnLoad(go);
#endif
		}

		private volatile int bindProgress = 0;
		private void doBind(object state)
		{
			IntPtr L = (IntPtr)state;

            List<Action<IntPtr>> list = new List<Action<IntPtr>>();
            
#if !SLUA_STANDALONE
#if USE_STATIC_BINDER
			Assembly[] ams = AppDomain.CurrentDomain.GetAssemblies();
			
			bindProgress = 0;

			List<Type> bindlist = new List<Type>();
			for (int n = 0; n < ams.Length;n++ )
			{
				Assembly a = ams[n];
				Type[] ts = null;
				try
				{
					ts = a.GetExportedTypes();
				}
				catch
				{
					continue;
				}
				for (int k = 0; k < ts.Length; k++)
				{
					Type t = ts[k];
					if (t.IsDefined(typeof(LuaBinderAttribute), false))
					{
						bindlist.Add(t);
					}
				}
			}
			
			bindProgress = 1;
			
			bindlist.Sort(new System.Comparison<Type>((Type a, Type b) => {
				LuaBinderAttribute la = System.Attribute.GetCustomAttribute( a, typeof(LuaBinderAttribute) ) as LuaBinderAttribute;
				LuaBinderAttribute lb = System.Attribute.GetCustomAttribute( b, typeof(LuaBinderAttribute) ) as LuaBinderAttribute;
				
				return la.order.CompareTo(lb.order);
			}));
			
			for (int n = 0; n < bindlist.Count; n++)
			{
				Type t = bindlist[n];
				var sublist = (Action<IntPtr>[])t.GetMethod("GetBindList").Invoke(null, null);
				list.AddRange(sublist);
			}
#else
		    var assemblyName = "Assembly-CSharp";
            Assembly assembly = Assembly.Load(assemblyName);
			list.AddRange(getBindList(assembly,"SLua.BindUnity"));
			list.AddRange(getBindList(assembly,"SLua.BindUnityUI"));
			list.AddRange(getBindList(assembly,"SLua.BindDll"));
			list.AddRange(getBindList(assembly,"SLua.BindCustom"));
#endif
#endif
			
			bindProgress = 2;
			
			int count = list.Count;
			for (int n = 0; n < count; n++)
			{
				Action<IntPtr> action = list[n];
				action(L);
				bindProgress = (int)(((float)n / count) * 98.0) + 2;
			}
			
			bindProgress = 100;
		}

		Action<IntPtr>[] getBindList(Assembly assembly,string ns) {
			Type t=assembly.GetType(ns);
			if(t!=null)
				return (Action<IntPtr>[]) t.GetMethod("GetBindList").Invoke(null, null);
			return new Action<IntPtr>[0];
		}

		
		public IEnumerator waitForBind(Action<int> tick, Action complete)
		{
			int lastProgress = 0;
			do {
				if (tick != null)
					tick (bindProgress);
				// too many yield return will increase binding time
				// so check progress and skip odd progress
				if (lastProgress != bindProgress && bindProgress % 2 == 0)
				{
					lastProgress = bindProgress;
					yield return null;
				}
			} while (bindProgress != 100);
			
			if (tick != null)
				tick (bindProgress);
			
			complete();
		}

		void doinit(IntPtr L,LuaSvrFlag flag)
		{
#if !SLUA_STANDALONE
			LuaTimer.reg(L);
			LuaCoroutine.reg(L, lgo);
#endif
			Helper.reg(L);
			LuaValueType.reg(L);
			
			if((flag&LuaSvrFlag.LSF_EXTLIB)!=0)
				LuaDLL.luaS_openextlibs(L);
			if((flag&LuaSvrFlag.LSF_3RDDLL)!=0)
				Lua3rdDLL.open(L);

#if !SLUA_STANDALONE
			lgo.state = luaState;
			lgo.onUpdate = this.tick;
			lgo.init();
#endif
			
			inited = true;
		}

		void checkTop(IntPtr L)
		{
			if (LuaDLL.lua_gettop(luaState.L) != errorReported)
			{
				Logger.LogError("Some function not remove temp value from lua stack. You should fix it.");
				errorReported = LuaDLL.lua_gettop(luaState.L);
			}
		}

		public void init(Action<int> tick,Action complete,LuaSvrFlag flag=LuaSvrFlag.LSF_BASIC)
        {
			LuaState luaState = new LuaState();

			IntPtr L = luaState.L;
			LuaObject.init(L);

#if SLUA_STANDALONE
            doBind(L);
		    this.luaState = luaState;
            doinit(L, flag);
		    complete();
            checkTop(L);
#else
			

			// be caurefull here, doBind Run in another thread
			// any code access unity interface will cause deadlock.
			// if you want to debug bind code using unity interface, need call doBind directly, like:
			// doBind(L);
			ThreadPool.QueueUserWorkItem(doBind, L);

			lgo.StartCoroutine(waitForBind(tick, () =>
			{
				this.luaState = luaState;
				doinit(L,flag);
				complete();
				checkTop(L);
			}));
#endif
        }

		public object start(string main)
		{
			if (main != null)
			{
				luaState.doFile(main);
				LuaFunction func = (LuaFunction)luaState["main"];
				if(func!=null)
					return func.call();
			}
			return null;
		}

#if !SLUA_STANDALONE
		void tick()
		{
			if (!inited)
				return;

			if (LuaDLL.lua_gettop(luaState.L) != errorReported)
			{
				errorReported = LuaDLL.lua_gettop(luaState.L);
				Logger.LogError(string.Format("Some function not remove temp value({0}) from lua stack. You should fix it.",LuaDLL.luaL_typename(luaState.L,errorReported)));
			}

			luaState.checkRef();
			LuaTimer.tick(Time.deltaTime);
		}
#endif
	}
}
