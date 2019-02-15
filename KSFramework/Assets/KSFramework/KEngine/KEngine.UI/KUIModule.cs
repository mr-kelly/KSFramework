#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KUIModule.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KEngine;
using UnityEngine;

namespace KEngine.UI
{
    public class UILoadRequest
    {
        public UnityEngine.Object Asset;
    }

    [Obsolete("Use UIModule instead of KUIModule")]
    public class KUIModule : UIModule
    {

    }

    /// <summary>
    /// UI Module
    /// </summary>
    public class UIModule
    {
        private class _InstanceClass
        {
            public static UIModule _Instance = new UIModule();
        }

        public static UIModule Instance
        {
            get { return _InstanceClass._Instance; }
        }

        /// <summary>
        /// 正在加载的UI统计
        /// </summary>
        private int _loadingUICount = 0;

        public int LoadingUICount
        {
            get { return _loadingUICount; }
            set
            {
                _loadingUICount = value;
                if (_loadingUICount < 0) Log.Error("Error ---- LoadingUICount < 0");
            }
        }


        /// <summary>
        /// A bridge for different UI System, for instance, you can use NGUI or EZGUI or etc.. UI Plugin through UIBridge
        /// </summary>
        public IUIBridge UiBridge;

        public Dictionary<string, UILoadState> UIWindows = new Dictionary<string, UILoadState>();

        public static Action<UIController> OnInitEvent;

        public static Action<UIController> OnOpenEvent;
        public static Action<UIController> OnCloseEvent;

        public UIModule()
        {
            var configUiBridge = AppEngine.GetConfig("KEngine.UI", "UIModuleBridge");

            if (!string.IsNullOrEmpty(configUiBridge))
            {
                var uiBridgeTypeName = string.Format("{0}", configUiBridge);
                var uiBridgeType = KTool.FindType(uiBridgeTypeName);
                if (uiBridgeType != null)
                {
                    UiBridge = Activator.CreateInstance(uiBridgeType) as IUIBridge;
                    Log.Debug("Use UI Bridge: {0}", uiBridgeType);
                }
                else
                {
                    Log.Error("Cannot find UIBridge Type: {0}", uiBridgeTypeName);
                }
            }

            if (UiBridge == null)
            {
                UiBridge = new UGUIBridge();
            }

            UiBridge.InitBridge();
        }

//        [Obsolete("Use string ui name instead for more flexible!")]
//        public UILoadState OpenWindow(Type type, params object[] args)
//        {
//            string uiName = type.Name.Remove(0, 3); // 去掉"CUI"
//            return OpenWindow(uiName, args);
//        }
//
//        [Obsolete("Use string ui name instead for more flexible!")]
//        public UILoadState OpenWindow<T>(params object[] args) where T : UIController
//        {
//            return OpenWindow(typeof(T), args);
//        }
        public UILoadState PreLoadUIWindow(string uiTemplateName, bool isOnInit = false,params object[] args)
        {
            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiTemplateName, out uiState))
            {
                if (isOnInit)
                {
                    LoadWindow(uiTemplateName, false, args);
                }
                else
                {
                    uiState = new UILoadState(uiTemplateName, uiTemplateName);
                    uiState.IsStaticUI = true;
                    uiState.OpenArgs = args;

                    uiState.OpenWhenFinish = false;
                    UIWindows.Add(uiTemplateName, uiState);
                    LoadingUICount++;
                    KResourceModule.Instance.StartCoroutine(PreLoadUIAssetBundle(uiTemplateName, uiState)); 
                }
            }

            return uiState;
        }

        private IEnumerator PreLoadUIAssetBundle(string windowTemplateName, UILoadState uiState)
        {
            if (uiState.UIResourceLoader != null)
            {
                uiState.UIResourceLoader.Release(true);// now!
                Log.Info("Release UI ResourceLoader: {0}", uiState.UIResourceLoader.Url);
                uiState.UIResourceLoader = null;
            }

            var request = new UILoadRequest();
            yield return KResourceModule.Instance.StartCoroutine(UiBridge.LoadUIAsset(uiState, request));

            GameObject uiObj = (GameObject)request.Asset;
            GameObject uiRoot = GameObject.Find("UIRoot");
            if (uiRoot == null)
            {
                uiRoot = new GameObject("UIRoot");
                if(!SceneLoader.isLoadSceneAdditive) GameObject.DontDestroyOnLoad(uiRoot);
            }
            if (uiObj != null)
            {
                uiObj.transform.SetParent(uiRoot.transform);
                uiObj.transform.localRotation = Quaternion.identity;
                uiObj.transform.localScale = Vector3.one;
                // 具体加载逻辑结束...这段应该放到Bridge里

                uiObj.SetActive(false);
                uiObj.name = uiState.TemplateName;

                var uiBase = UiBridge.CreateUIController(uiObj, uiState.TemplateName);

                if (uiState.UIWindow != null)
                {
                    Log.Info("Destroy exist UI Window, maybe for reload");
                    GameObject.Destroy(uiState.UIWindow.CachedGameObject);
                    uiState.UIWindow = null;
                }

                uiState.UIWindow = uiBase;

                uiBase.UIName = uiBase.UITemplateName = uiState.TemplateName;

                UiBridge.UIObjectFilter(uiBase, uiObj);

                uiState.IsLoading = false; // Load完

                uiBase.gameObject.SetActive(false);
                uiState.OnUIWindowLoadedCallbacks(uiState, uiBase);

                if (uiState.OpenWhenFinish)
                {
                    InitWindow(uiState, uiBase, true, uiState.OpenArgs);
                }
                else
                {
                    if (OnInitEvent != null)
                        OnInitEvent(uiBase);
                }
            }

            LoadingUICount--;
        }

        // 打开窗口（非复制）
        public UILoadState OpenWindow(string uiTemplateName, params object[] args)
        {
            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiTemplateName, out uiState))
            {
                uiState = LoadWindow(uiTemplateName, true, args);
                return uiState;
            }

            if (!uiState.isOnInit)
            {
                uiState.isOnInit = true;
                if (uiState.UIWindow != null) uiState.UIWindow.OnInit();
            }
            OnOpen(uiState, args);
            return uiState;
        }

        // 隐藏时打开，打开时隐藏
        public void ToggleWindow<T>(params object[] args)
        {
            string uiName = typeof(T).Name.Remove(0, 3); // 去掉"CUI"
            ToggleWindow(uiName, args);
        }

        public void ToggleWindow(string name, params object[] args)
        {
            if (IsOpen(name))
            {
                CloseWindow(name);
            }
            else
            {
                OpenWindow(name, args);
            }
        }

        /// <summary>
        /// // Dynamic动态窗口，复制基准面板
        /// </summary>
        public UILoadState OpenDynamicWindow(string template, string instanceName, params object[] args)
        {
            UILoadState uiState = _GetUIState(instanceName);
            if (uiState != null)
            {
                OnOpen(uiState, args);
                return uiState;
            }

            UILoadState uiInstanceState;
            if (!UIWindows.TryGetValue(instanceName, out uiInstanceState)) // 实例创建
            {
                uiInstanceState = new UILoadState(template, instanceName);
                uiInstanceState.IsStaticUI = false;
                uiInstanceState.IsLoading = true;
                uiInstanceState.UIWindow = null;
                uiInstanceState.OpenWhenFinish = true;
				uiInstanceState.OpenArgs = args;
                UIWindows[instanceName] = uiInstanceState;
            }

            CallUI(template, (_ui, _args) =>
            {
                // _args useless

					UILoadState newUiInstanceState = _GetUIState(instanceName);
					UILoadState templateState = _GetUIState(template);

                // 组合template和name的参数 和args外部参数
					object[] totalArgs = new object[newUiInstanceState.OpenArgs.Length + 2];
                	totalArgs[0] = template;
	                totalArgs[1] = instanceName;
					newUiInstanceState.OpenArgs.CopyTo(totalArgs, 2);

					OnDynamicWindowCallback(templateState.UIWindow, totalArgs);
            });

            return uiInstanceState;
        }

        private void OnDynamicWindowCallback(UIController _ui, object[] _args)
        {
            string template = (string)_args[0];
            string name = (string)_args[1];

            GameObject uiObj = (GameObject)UnityEngine.Object.Instantiate(_ui.gameObject);

            uiObj.name = name;

            UiBridge.UIObjectFilter(_ui, uiObj);

            UILoadState instanceUIState = UIWindows[name];
            instanceUIState.IsLoading = false;

            UIController uiBase = uiObj.GetComponent<UIController>();
            uiBase.UITemplateName = template;
            uiBase.UIName = name;

            instanceUIState.UIWindow = uiBase;

            object[] originArgs = new object[_args.Length - 2]; // 去除前2个参数
            for (int i = 2; i < _args.Length; i++)
                originArgs[i - 2] = _args[i];
            InitWindow(instanceUIState, uiBase, instanceUIState.OpenWhenFinish, originArgs);
        }

        public void CloseWindow(Type t)
        {
            CloseWindow(t.Name.Remove(0, 3)); // XUI remove
        }

        public void CloseWindow<T>()
        {
            CloseWindow(typeof(T));
        }

        public void CloseWindow(string name)
        {
            UILoadState uiState;
            if (!UIWindows.TryGetValue(name, out uiState))
            {
                if (Debug.isDebugBuild)
                    Log.Warning("[CloseWindow]没有加载的UIWindow: {0}", name);
                return; // 未开始Load
            }

            if (uiState.IsLoading) // Loading中
            {
                if (Debug.isDebugBuild)
                    Log.Info("[CloseWindow]IsLoading的{0}", name);
                uiState.OpenWhenFinish = false;
                return;
            }

            Action doCloseAction = () =>
            {
                uiState.UIWindow.gameObject.SetActive(false);

                uiState.UIWindow.OnClose();

                if (OnCloseEvent != null)
                    OnCloseEvent(uiState.UIWindow);

                if (!uiState.IsStaticUI)
                {
                    DestroyWindow(name);
                }
            };

            doCloseAction();
        }

        /// <summary>
        /// Destroy all windows that has LoadState.
        /// Be careful to use.
        /// </summary>
        public void DestroyAllWindows()
        {
            List<string> LoadList = new List<string>();

            foreach (KeyValuePair<string, UILoadState> uiWindow in UIWindows)
            {
                if (IsLoad(uiWindow.Key))
                {
                    LoadList.Add(uiWindow.Key);
                }
            }

            foreach (string item in LoadList)
                DestroyWindow(item, true);
        }

        [Obsolete("Deprecated: Please don't use this")]
        public void CloseAllWindows()
        {
            List<string> toCloses = new List<string>();

            foreach (KeyValuePair<string, UILoadState> uiWindow in UIWindows)
            {
                if (IsOpen(uiWindow.Key))
                {
                    toCloses.Add(uiWindow.Key);
                }
            }

            for (int i = toCloses.Count - 1; i >= 0; i--)
            {
                CloseWindow(toCloses[i]);
            }
        }

        private UILoadState _GetUIState(string name)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(name, out uiState);
            if (uiState != null)
                return uiState;

            return null;
        }

        private UIController GetUIBase(string name)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(name, out uiState);
            if (uiState != null && uiState.UIWindow != null)
                return uiState.UIWindow;

            return null;
        }

        public bool IsOpen<T>() where T : UIController
        {
            string uiName = typeof(T).Name.Remove(0, 3); // 去掉"CUI"
            return IsOpen(uiName);
        }

        public bool IsOpen(string name)
        {
            UIController uiBase = GetUIBase(name);
            return uiBase == null ? false : uiBase.gameObject.activeSelf;
        }

        public UIController GetOpenedWindow(string name)
        {
            UIController uiBase = GetUIBase(name);
            if (uiBase != null && uiBase.gameObject.activeSelf) { return uiBase; }
            return null;
        }
        public bool IsLoad(string name)
        {
            if (UIWindows.ContainsKey(name))
                return true;
            return false;
        }

        public UILoadState LoadWindow(string windowTemplateName, bool openWhenFinish, params object[] args)
        {
            if (UIWindows.ContainsKey(windowTemplateName))
            {
                Log.Error("[LoadWindow]多次重复LoadWindow: {0}", windowTemplateName);
            }
            Debuger.Assert(!UIWindows.ContainsKey(windowTemplateName));

            UILoadState openState = new UILoadState(windowTemplateName, windowTemplateName);
            openState.IsStaticUI = true;
            openState.OpenArgs = args;

            //if (openState.IsLoading)
            openState.OpenWhenFinish = openWhenFinish;

			UIWindows.Add(windowTemplateName, openState);
            KResourceModule.Instance.StartCoroutine(LoadUIAssetBundle(windowTemplateName, openState));

            return openState;
        }

        private IEnumerator LoadUIAssetBundle(string name, UILoadState openState, KCallback callback = null)
        {
            if (openState.UIResourceLoader != null)
            {
                openState.UIResourceLoader.Release(true);// now!
                Log.Info("Release UI ResourceLoader: {0}", openState.UIResourceLoader.Url);
                openState.UIResourceLoader = null;
            }

            LoadingUICount++;

            var request = new UILoadRequest();
            yield return KResourceModule.Instance.StartCoroutine(UiBridge.LoadUIAsset(openState, request));

            GameObject uiObj = (GameObject)request.Asset;
            GameObject uiRoot = GameObject.Find("UIRoot");
            if (uiRoot == null)
            {
                uiRoot = new GameObject("UIRoot");
                if (!SceneLoader.isLoadSceneAdditive) GameObject.DontDestroyOnLoad(uiRoot);
            }
            if (uiObj != null)
            {
                uiObj.transform.SetParent(uiRoot.transform);
                // 具体加载逻辑结束...这段应该放到Bridge里

                uiObj.SetActive(false);
                uiObj.name = openState.TemplateName;

                var uiBase = UiBridge.CreateUIController(uiObj, openState.TemplateName);

                if (openState.UIWindow != null)
                {
                    Log.Info("Destroy exist UI Window, maybe for reload");
                    GameObject.Destroy(openState.UIWindow.CachedGameObject);
                    openState.UIWindow = null;
                }

                openState.UIWindow = uiBase;

                uiBase.UIName = uiBase.UITemplateName = openState.TemplateName;

                UiBridge.UIObjectFilter(uiBase, uiObj);

                openState.IsLoading = false; // Load完
                openState.isOnInit = true;
                InitWindow(openState, uiBase, openState.OpenWhenFinish, openState.OpenArgs);
            }

            LoadingUICount--;

            if (callback != null)
                callback(null);
        }

        /// <summary>
        /// Hot reload a ui asset bundle
        /// </summary>
        /// <param name="uiTemplateName"></param>
        public UnityEngine.Coroutine ReloadWindow(string windowTemplateName, KCallback callback)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(windowTemplateName, out uiState);
            if (uiState == null || uiState.UIWindow == null)
            {
                Log.Info("{0} has been destroyed", windowTemplateName);
                return null;
            }
            return KResourceModule.Instance.StartCoroutine(LoadUIAssetBundle(windowTemplateName, uiState));
        }

        public void DestroyWindow(string uiTemplateName, bool destroyImmediate=false)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(uiTemplateName, out uiState);
            if (uiState == null || uiState.UIWindow == null)
            {
                Log.Info("{0} has been destroyed", uiTemplateName);
                return;
            }
            if (destroyImmediate)
            {
                UnityEngine.Object.DestroyImmediate(uiState.UIWindow.gameObject);
            }
            else
            {
            UnityEngine.Object.Destroy(uiState.UIWindow.gameObject);
            }

            // Instance UI State has no Resources loader, so fix here
            if (uiState.UIResourceLoader != null)
                uiState.UIResourceLoader.Release();
            uiState.UIWindow = null;

            UIWindows.Remove(uiTemplateName);
        }

        /// <summary>
        /// 等待并获取UI实例，执行callback
        /// 源起Loadindg UI， 在加载过程中，进度条设置方法会失效
        /// 如果是DynamicWindow,，使用前务必先要Open!
        /// </summary>
        /// <param name="uiTemplateName"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        public void CallUI(string uiTemplateName, Action<UIController, object[]> callback, params object[] args)
        {
            Debuger.Assert(callback);

            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiTemplateName, out uiState))
            {
                uiState = LoadWindow(uiTemplateName, false); // 加载，这样就有UIState了, 但注意因为没参数，不要随意执行OnOpen
            }

            uiState.DoCallback(callback, args);
        }

        /// <summary>
        /// DynamicWindow专用, 不会自动加载，会提示报错
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        public void CallDynamicUI(string uiName, Action<UIController, object[]> callback, params object[] args)
        {
            Debuger.Assert(callback);

            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiName, out uiState))
            {
                Log.Error("找不到UIState: {0}", uiName);
                return;
            }

            UILoadState openState = UIWindows[uiName];
            openState.DoCallback(callback, args);
        }

        [Obsolete("Use string ui name instead for more flexible!")]
        public void CallUI<T>(Action<T> callback) where T : UIController
        {
            CallUI<T>((_ui, _args) => callback(_ui));
        }

        // 使用泛型方式
        [Obsolete("Use string ui name instead for more flexible!")]
        public void CallUI<T>(Action<T, object[]> callback, params object[] args) where T : UIController
        {
            string uiName = typeof(T).Name.Remove(0, 3); // 去掉 "XUI"

            CallUI(uiName, (_uibase, _args) => { callback(_uibase as T, _args); }, args);
        }

        private void OnOpen(UILoadState uiState, params object[] args)
        {
            if (uiState.IsLoading)
            {
                uiState.OpenWhenFinish = true;
                uiState.OpenArgs = args;
                return;
            }

            UIController uiBase = uiState.UIWindow;

            //Action doOpenAction = () =>
            {
                if (uiBase.gameObject.activeSelf)
                {
					uiBase.OnClose();

					if (OnCloseEvent != null)
						OnCloseEvent(uiBase);
                }

                uiBase.BeforeOpen(args, () =>
                {
                    uiBase.gameObject.SetActive(true);
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    uiBase.OnOpen(args);
                    stopwatch.Stop();
                    Log.Debug("OnOpen UI {0}, cost {1}", uiBase.name, stopwatch.Elapsed.TotalMilliseconds*0.001f);

                    if (OnOpenEvent != null)
                        OnOpenEvent(uiBase);
                });
            };

            //            doOpenAction();
        }


        private void InitWindow(UILoadState uiState, UIController uiBase, bool open, params object[] args)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            uiBase.OnInit();
            stopwatch.Stop();
            Log.Debug("OnInit UI {0}, cost {1}", uiBase.name, stopwatch.Elapsed.TotalMilliseconds * 0.001f);
            if (OnInitEvent != null)
                OnInitEvent(uiBase);
            if (open)
            {
                OnOpen(uiState, args);
            }

            if (!open)
            {
                if (!uiState.IsStaticUI)
                {
                    CloseWindow(uiBase.UIName); // Destroy
                    return;
                }
                else
                {
                    uiBase.gameObject.SetActive(false);
                }
            }

            uiState.OnUIWindowLoadedCallbacks(uiState, uiBase);
        }
    }

    /// <summary>
    /// UI Async Load State class
    /// </summary>
    public class UILoadState
    {
        public string TemplateName;
        public string InstanceName;
        public UIController UIWindow;
        public Type UIType;
        public bool IsLoading;
        public bool IsStaticUI; // 非复制出来的, 静态UI

        public bool OpenWhenFinish;
        public object[] OpenArgs;

        internal Queue<Action<UIController, object[]>> CallbacksWhenFinish;
        internal Queue<object[]> CallbacksArgsWhenFinish;
        public AbstractResourceLoader UIResourceLoader; // 加载器，用于手动释放资源
        public bool isOnInit = false;//是否初始化

        public UILoadState(string uiTemplateName, string uiInstanceName, Type uiControllerType = default(Type))
        {
            if (uiControllerType == default(Type)) uiControllerType = typeof(UIController);

            TemplateName = uiTemplateName;
            InstanceName = uiInstanceName;
            UIWindow = null;
            UIType = uiControllerType;

            IsLoading = true;
            OpenWhenFinish = false;
            OpenArgs = null;

            CallbacksWhenFinish = new Queue<Action<UIController, object[]>>();
            CallbacksArgsWhenFinish = new Queue<object[]>();
        }


        /// <summary>
        /// 确保加载完成后的回调
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        public void DoCallback(Action<UIController, object[]> callback, object[] args = null)
        {
            if (args == null)
                args = new object[0];

            if (IsLoading) // Loading
            {
                CallbacksWhenFinish.Enqueue(callback);
                CallbacksArgsWhenFinish.Enqueue(args);
                return;
            }

            // 立即执行即可
            callback(UIWindow, args);
        }

        internal void OnUIWindowLoadedCallbacks(UILoadState uiState, UIController uiObject)
        {
            //if (openState.OpenWhenFinish)  // 加载完打开 模式下，打开时执行回调
            {
                while (uiState.CallbacksWhenFinish.Count > 0)
                {
                    Action<UIController, object[]> callback = uiState.CallbacksWhenFinish.Dequeue();
                    object[] _args = uiState.CallbacksArgsWhenFinish.Dequeue();
                    //callback(uiBase, _args);

                    DoCallback(callback, _args);
                }
            }
        }
    }

}