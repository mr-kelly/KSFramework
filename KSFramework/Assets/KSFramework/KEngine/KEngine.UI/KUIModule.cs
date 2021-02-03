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
using UnityEngine.UI;

namespace KEngine.UI
{
    public class UILoadRequest
    {
        public UnityEngine.Object Asset;
    }
    
    /// <summary>
    /// UI Module
    /// TODO UI可自定义资源名和lua脚本路径，需要先创建脚本对象，再根据脚本中的值进行加载资源
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

        public List<string> CommonAtlases = new List<string>(){"atlas_common"};
        
        private int _loadingUICount = 0;
        /// <summary>
        /// 正在加载的UI统计
        /// </summary>
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
        /// A bridge for different UI System, for instance, you can use NGUI or EZGUI or UGUI etc.. UI Plugin through UIBridge
        /// </summary>
        public GUIBridge UiBridge;

        public Dictionary<string, UILoadState> UIWindows = new Dictionary<string, UILoadState>();

        public static Action<UIController> OnInitEvent;

        public static Action<UIController> OnOpenEvent;
        public static Action<UIController> OnCloseEvent;
        /// <summary>
        /// 每个界面打开都会+1，新打开的界面始终在最顶层
        /// </summary>
        public static int sortOrder = 0;

        
        public UIModule()
        {
            UiBridge = new GUIBridge();
            UiBridge.InitBridge();
            CreateRoot();
        }
        
        #region UI根节点初始化

        public GameObject UIRoot { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Camera UICamera { get; private set; }
        public GameObject MainUIRoot { get; private set; }
        public GameObject NormalUIRoot { get; private set; }
        public GameObject HeadInfoUIRoot { get; private set; }

        private void CreateRoot()
        {
            UIRoot = new GameObject("UIRoot");
            MainUIRoot = new GameObject("MainUIRoot");
            NormalUIRoot = new GameObject("NormalUIRoot");
            HeadInfoUIRoot = new GameObject("HeadInfoUIRoot");
            MainUIRoot.transform.SetParent(UIRoot.transform,true);
            NormalUIRoot.transform.SetParent(UIRoot.transform,true);
            HeadInfoUIRoot.transform.SetParent(UIRoot.transform,true);
           
            //create camera
            UICamera = new GameObject("UICamera").AddComponent<Camera>();
            UICamera.transform.SetParent(UIRoot.transform,true);
            UICamera.cullingMask =  (1<< (int)UnityLayerDef.UI);
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.nearClipPlane = UIDefs.Camera_Near;
            UICamera.farClipPlane = UIDefs.Camera_Far;
            UICamera.orthographic = true;
            UICamera.orthographicSize = UIDefs.Camera_Size;
            UICamera.depth = UIDefs.Camera_Depth;
            GameObject.DontDestroyOnLoad(UIRoot);
        }

        private void InitUIAsset(GameObject uiObj)
        {
            if (!uiObj)
            {
                Log.LogError("uiObj is null !");
                return;
            }
            var windowAsset = uiObj.GetComponent<UIWindowAsset>();
            var canvas = uiObj.GetComponent<Canvas>();
            switch (windowAsset.PanelType)
            {
                case PanelType.MainUI:
                    uiObj.transform.SetParent(MainUIRoot.transform);
                    break;
                case PanelType.NormalUI:
                    uiObj.transform.SetParent(NormalUIRoot.transform);
                    break;
                case PanelType.HeadInfoUI:
                    uiObj.transform.SetParent(HeadInfoUIRoot.transform);
                    break;
                default:
                    Log.LogError("not define PanelType",windowAsset.PanelType);
                    uiObj.transform.SetParent(UIRoot.transform);
                    break;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UICamera;
        }
        
        #endregion

        #region 预加载
        
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
                if(AppConfig.IsLogAbInfo) Log.Info("Release UI ResourceLoader: {0}", uiState.UIResourceLoader.Url);
                uiState.UIResourceLoader = null;
            }

            var request = new UILoadRequest();
            yield return KResourceModule.Instance.StartCoroutine(UiBridge.LoadUIAsset(uiState, request));

            GameObject uiObj = (GameObject)request.Asset;
 
            if (uiObj != null)
            {
                InitUIAsset(uiObj);
                
                uiObj.transform.localRotation = Quaternion.identity;
                uiObj.transform.localScale = Vector3.one;
                
                var canvas = uiObj.GetComponent<Canvas>();
                if (canvas)
                    canvas.enabled = false;
                else 
                    uiObj.SetActiveX(false);
                uiObj.name = uiState.TemplateName;

                var uiBase = UiBridge.CreateUIController(uiObj, uiState.TemplateName);
                if (uiState.UIWindow != null)
                {
                    Log.Info("Destroy exist UI Window, maybe for reload");
                    GameObject.Destroy(uiState.UIWindow.gameObject);
                    uiState.UIWindow = null;
                }

                uiState.UIWindow = uiBase;
                uiState.WindowAsset = uiObj.GetComponent<UIWindowAsset>();

                uiState.IsLoading = false; // Load完
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
        
        #endregion

        #region 公共接口

        /// <summary>
        /// 打开窗口（非复制）
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public UILoadState OpenWindow(string uiName, params object[] args)
        {
            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiName, out uiState))
            {
                uiState = LoadWindow(uiName, true, args);
                return uiState;
            }

            if (!uiState.isOnInit)
            {
                uiState.isOnInit = true;
                if (uiState.UIWindow != null)
                {
                    uiState.UIWindow.OnInit();
                }
            }
            OnOpen(uiState, args);
            return uiState;
        }
        
        public void ToggleWindow(string uiName, params object[] args)
        {
            if (IsOpen(uiName))
            {
                CloseWindow(uiName);
            }
            else
            {
                OpenWindow(uiName, args);
            }
        }

        /// <summary>
        /// // Dynamic动态窗口，复制基准面板
        /// </summary>
        public UILoadState OpenDynamicWindow(string uiName, string instanceName, params object[] args)
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
                uiInstanceState = new UILoadState(uiName, instanceName);
                uiInstanceState.IsStaticUI = false;
                uiInstanceState.IsLoading = true;
                uiInstanceState.UIWindow = null;
                uiInstanceState.WindowAsset = null;
                uiInstanceState.OpenWhenFinish = true;
				uiInstanceState.OpenArgs = args;
                UIWindows[instanceName] = uiInstanceState;
            }

            CallUI(uiName, (_ui, _args) =>
            {
                // _args useless

					UILoadState newUiInstanceState = _GetUIState(instanceName);
					UILoadState templateState = _GetUIState(uiName);

                // 组合template和name的参数 和args外部参数
					object[] totalArgs = new object[newUiInstanceState.OpenArgs.Length + 2];
                	totalArgs[0] = uiName;
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

            UILoadState instanceUIState = UIWindows[name];
            instanceUIState.IsLoading = false;

            UIController uiBase = UiBridge.CreateUIController(uiObj,template);
            instanceUIState.UIWindow = uiBase;
            instanceUIState.WindowAsset = uiObj.GetComponent<UIWindowAsset>();

            object[] originArgs = new object[_args.Length - 2]; // 去除前2个参数
            for (int i = 2; i < _args.Length; i++)
                originArgs[i - 2] = _args[i];
            InitWindow(instanceUIState, uiBase, instanceUIState.OpenWhenFinish, originArgs);
        }
        

        public void CloseWindow(string uiName)
        {
            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiName, out uiState))
            {
                if (Debug.isDebugBuild)
                    Log.Warning("[CloseWindow]没有加载的UIWindow: {0}", uiName);
                return; // 未开始Load
            }

            if (uiState.IsLoading) // Loading中
            {
                if (Debug.isDebugBuild)
                    Log.Info("[CloseWindow]IsLoading的{0}", uiName);
                uiState.OpenWhenFinish = false;
                return;
            }

            if (uiState.UIWindow.Canvas != null)
            {
                uiState.UIWindow.Canvas.enabled = false;
            }
            else
            {
                uiState.UIWindow.gameObject.SetActiveX(false);
            }
            uiState.UIWindow.OnClose();

            if (OnCloseEvent != null)
                OnCloseEvent(uiState.UIWindow);

            if (!uiState.IsStaticUI)
            {
                DestroyWindow(uiName);
            }
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
        
        /// <summary>
        /// 关闭全部界面
        /// </summary>
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
        
        #endregion
        
        #region 泛形接口

        //[Obsolete("Use string ui name instead for more flexible!")]
        public UILoadState OpenWindow(Type type, params object[] args)
        {
            string uiName = type.Name.Remove(0, 2); // 去掉"UI",ILRuntime传过来的type为：ILRuntimeAdapter.UIControllerAdapter+Adapter
            return OpenWindow(uiName, args);
        }
        
        //[Obsolete("Use string ui name instead for more flexible!")]
        public UILoadState OpenWindow<T>(params object[] args) where T : UIController
        {
            return OpenWindow(typeof(T), args);
        }
        
        //隐藏时打开，打开时隐藏
        public void ToggleWindow<T>(params object[] args)
        {
            string uiName = typeof(T).Name.Remove(0, 3); // 去掉"KUI"
            ToggleWindow(uiName, args);
        }
        
        public void CloseWindow(Type t)
        {
            CloseWindow(t.Name.Remove(0, 3)); // KUI remove
        }

        public void CloseWindow<T>()
        {
            CloseWindow(typeof(T));
        }
        #endregion
        
        private UILoadState _GetUIState(string name)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(name, out uiState);
            if (uiState != null)
                return uiState;

            return null;
        }

        private UIController GetUIBase(string uiName)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(uiName, out uiState);
            if (uiState != null && uiState.UIWindow != null)
                return uiState.UIWindow;

            return null;
        }

        public bool IsOpen<T>() where T : UIController
        {
            string uiName = typeof(T).Name.Remove(0, 3); // 去掉"KUI"
            return IsOpen(uiName);
        }

        public bool IsOpen(string uiName)
        {
            UIController uiBase = GetUIBase(uiName);
            if (uiBase != null)
            {
                if (uiBase.Canvas != null) 
                    return uiBase.Canvas.enabled;
                var gameObject = uiBase.gameObject;
                return gameObject && gameObject.activeSelf;
            }

            return false;
        }

        public UIController GetOpenedWindow(string name)
        {
            UIController uiBase = GetUIBase(name);
            if (uiBase != null)
            {
                if (uiBase.Canvas != null && uiBase.Canvas.enabled)
                    return uiBase;
                if (uiBase.gameObject && uiBase.gameObject.activeSelf)
                    return uiBase;
            }
            return null;
        }
		
        public bool IsLoad(string uiName)
        {
            return UIWindows.ContainsKey(uiName);
        }

        public UILoadState LoadWindow(string uiName, bool openWhenFinish, params object[] args)
        {
            if (UIWindows.ContainsKey(uiName))
            {
                Log.Error("[LoadWindow]多次重复LoadWindow: {0}", uiName);
            }
            Debuger.Assert(!UIWindows.ContainsKey(uiName));

            UILoadState openState = new UILoadState(uiName, uiName);
            openState.IsStaticUI = true;
            openState.OpenArgs = args;
            openState.OpenWhenFinish = openWhenFinish;

			UIWindows.Add(uiName, openState);
            KResourceModule.Instance.StartCoroutine(LoadUIAssetBundle(uiName, openState));

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

            GameObject uiObj = request.Asset as GameObject;
         
            if (uiObj != null)
            {
                InitUIAsset(uiObj);
                // 具体加载逻辑结束

                var canvas = uiObj.GetComponent<Canvas>(); //设置Canvas的enable，减少SetActive的消耗
                if (canvas) canvas.enabled = false;
                uiObj.name = openState.TemplateName;

                var uiBase = UiBridge.CreateUIController(uiObj, openState.TemplateName);
                if (openState.UIWindow != null)
                {
                    Log.Info("Destroy exist UI Window, maybe for reload");
                    GameObject.Destroy(openState.UIWindow.gameObject);
                    openState.UIWindow = null;
                }

                openState.UIWindow = uiBase;
                openState.WindowAsset = uiObj.GetComponent<UIWindowAsset>();

                openState.IsLoading = false; // Load完
                openState.isOnInit = true;
                InitWindow(openState, uiBase, openState.OpenWhenFinish, openState.OpenArgs);
            }
            else
            {
                Log.LogError("load ui {0} result.Asset not a gameobject",name);
            }

            LoadingUICount--;

            if (callback != null)
                callback(null);
        }

        /// <summary>
        /// Hot reload a ui asset bundle
        /// </summary>
        /// <param name="uiTemplateName"></param>
        public UnityEngine.Coroutine ReloadWindow(string uiTemplateName, KCallback callback)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(uiTemplateName, out uiState);
            if (uiState == null)
            {
                Log.Info("{0} has been destroyed", uiTemplateName);
                return null;
            }
            return KResourceModule.Instance.StartCoroutine(LoadUIAssetBundle(uiTemplateName, uiState));
        }

        public void DestroyWindow(string uiName, bool destroyImmediate=false)
        {
            UILoadState uiState;
            UIWindows.TryGetValue(uiName, out uiState);
            if (uiState == null || uiState.UIWindow == null)
            {
                Log.Info("{0} has been destroyed", uiName);
                return;
            }

            //if (uiState.WindowAsset != null && !string.IsNullOrEmpty(uiState.WindowAsset.atals_arr))
            {
                //NOTE 按照约定SpriteAtlas和UI在同一个ab中，无需处理
            }
            uiState.UIWindow.OnDestroy();
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
            uiState.WindowAsset = null;

            UIWindows.Remove(uiName);
        }

        /// <summary>
        /// 等待并获取UI实例，执行callback
        /// 源起Loadindg UI， 在加载过程中，进度条设置方法会失效
        /// 如果是DynamicWindow,，使用前务必先要Open!
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        public void CallUI(string uiName, Action<UIController, object[]> callback, params object[] args)
        {
            Debuger.Assert(callback);
            UILoadState uiState;
            if (!UIWindows.TryGetValue(uiName, out uiState))
            {
                uiState = LoadWindow(uiName, false); // 加载，这样就有UIState了, 但注意因为没参数，不要随意执行OnOpen
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
            string uiName = typeof(T).Name.Remove(0, 3); // 去掉 "KUI"

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
            if (uiBase.Canvas != null && uiBase.Canvas.enabled)
            {
                //已经打开无需再次打开
				return;
            }

            uiBase.BeforeOpen(args, () =>
            {
                uiBase.gameObject.SetActiveX(true);
                if (uiBase.Canvas)
                {
                    uiBase.Canvas.enabled = true;
                    if (sortOrder >= int.MaxValue)
                    {
                        sortOrder = 0;
                    }

                    uiBase.Canvas.sortingOrder = sortOrder++;
                }

                KProfiler.BeginWatch("UI.OnOpen");
                uiBase.OnOpen(args);
                KWatchResult profilerData = null;
                if (AppConfig.IsLogFuncCost) profilerData = KProfiler.EndWatch("UI.OnOpen", string.Concat(uiBase.UIName, ".OnOpen"));
                if (AppConfig.IsSaveCostToFile)
                {
                    if (profilerData == null) profilerData = KProfiler.EndWatch("UI.OnOpen", string.Concat(uiBase.UIName, ".OnOpen"));
                    LogFileRecorder.WriteUILog(uiBase.UIName, LogFileRecorder.UIState.OnOpen, profilerData.costTime);
                }

                if (OnOpenEvent != null)
                    OnOpenEvent(uiBase);
            });
        }


        private void InitWindow(UILoadState uiState, UIController uiBase, bool open, params object[] args)
        {
           KProfiler.BeginWatch("UI.Init");
            uiBase.OnInit();
            KWatchResult profilerData = null;
            if(AppConfig.IsLogFuncCost) profilerData = KProfiler.EndWatch("UI.Init",string.Concat(uiState.InstanceName,".OnInit"));
            if (AppConfig.IsSaveCostToFile)
            {
                if(profilerData == null) profilerData = KProfiler.EndWatch("UI.Init",string.Concat(uiState.InstanceName,".OnInit"));
                LogFileRecorder.WriteUILog(uiState.InstanceName,LogFileRecorder.UIState.OnInit,profilerData.costTime);
            }
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
                    if(uiBase.Canvas!=null)
                        uiBase.Canvas.enabled = false;
                    else
                    {
                        uiBase.gameObject.SetActiveX(false);
                    }
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
        public UIWindowAsset WindowAsset;
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