#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AppEngine.cs
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

#define USE_UGUI_FPS

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KEngine.Table;
using UnityEngine;
using UnityEngine.UI;

namespace KEngine
{
    /// <summary>
    /// Entry
    /// </summary>
    public interface IAppEntry
    {
        IEnumerator OnBeforeInit();
        IEnumerator OnGameStart();
    }

    /// <summary>
    /// KEngine - Unity3D Game Develop Framework
    /// </summary>
    public class AppEngine : MonoBehaviour
    {
#if !DEBUG_DISABLE
        public bool ShowFps = true; //show fps
#else
        public bool ShowFps = false;
#endif
        public bool UseDevFunc = true;
        /// <summary>
        /// To Display FPS in the Debug Mode (AppConfig.IsDebugBuild is true)
        /// </summary>
        public static FpsWatcher RenderWatcher { get; set; } // 帧数监听器

        /// <summary>
        /// In Init func has a check if the user has the write privillige
        /// </summary>
        public static bool IsRootUser; // 是否越狱iOS

        public static AppEngine EngineInstance { get; private set; }

        //private static AppVersion _appVersion = null;

        /// <summary>
        /// Get App Version from KEngineConfig.txt
        /// </summary>
        //public static AppVersion AppVersion
        //{
        //    get
        //    {
        //        if (_appVersion == null)
        //        {
        //            var appVersionStr = GetConfig(KEngineDefaultConfigs.AppVersion);
        //            if (string.IsNullOrEmpty(appVersionStr))
        //            {
        //                Log.Error("Cannot find AppVersion in KEngineConfig.txt, use 1.0.0.0 as default");
        //                appVersionStr = "1.0.0.0.alpha.default";
        //            }
        //            _appVersion = new AppVersion(appVersionStr);
        //        }
        //        return _appVersion;
        //    }
        //}
        
        /// <summary>
        /// Modules passed from the AppEngine.New function. All your custom game logic modules
        /// </summary>
        public IList<IModuleInitable> GameModules { get; private set; }

        /// <summary>
        /// 是否初始化完成
        /// </summary>
        public bool IsInited { get; private set; }

        public bool IsBeforeInit { get; private set; }

        public bool IsOnInit { get; private set; }

        public bool IsStartGame { get; private set; }

        /// <summary>
        /// AppEngine must be new by static function New(xxx)!
        /// This is a flag to identity whether AddComponent from Unity
        /// </summary>
        private bool _isNewByStatic = false;

        public IAppEntry AppEntry { get; private set; }
        public static bool IsApplicationQuit = false;
        public static bool IsApplicationFocus = true;
        public static bool IsAppPlaying = false;
        public static Action UpdateEvent;
        public static Action UpdatePer300msEvent;
        public static Action UpdatePer1sEvent;
        
        /// <summary>
        /// Engine entry.... all begins from here
        /// </summary>
        public static AppEngine New(GameObject gameObjectToAttach, IAppEntry entry, IList<IModuleInitable> modules)
        {
            Debuger.Assert(gameObjectToAttach != null && modules != null);
            AppConfig.Init();
            AppEngine appEngine = gameObjectToAttach.AddComponent<AppEngine>();
            appEngine._isNewByStatic = true;
            appEngine.GameModules = modules;
            appEngine.AppEntry = entry;

            return appEngine;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            if (EngineInstance != null)
            {
                Log.Error("Duplicated Instance Engine!!!");
            }

            EngineInstance = this;

            Init();
        }

        void Start()
        {
            IsAppPlaying = true;
            if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
                LogFileManager.Start();
            Debuger.Assert(_isNewByStatic);
        }

        private void Init()
        {
            if (AppConfig.IsLogDeviceInfo)
            {
                IsRootUser = KTool.HasWriteAccessToFolder(Application.dataPath); // Root User运行时，能穿越沙盒写DataPath, 以此为依据
                Log.Info("====================================================================================");
                Log.Info("Application.platform = {0}", Application.platform);
                Log.Info("Application.dataPath = {0} , WritePermission: {1}", Application.dataPath, IsRootUser);
                Log.Info("Application.streamingAssetsPath = {0} , WritePermission: {1}",
                    Application.streamingAssetsPath, KTool.HasWriteAccessToFolder(Application.streamingAssetsPath));
                Log.Info("Application.persistentDataPath = {0} , WritePermission: {1}", Application.persistentDataPath,
                    KTool.HasWriteAccessToFolder(Application.persistentDataPath));
                Log.Info("Application.temporaryCachePath = {0} , WritePermission: {1}", Application.temporaryCachePath,
                    KTool.HasWriteAccessToFolder(Application.temporaryCachePath));
                Log.Info("Application.unityVersion = {0}", Application.unityVersion);
                Log.Info("SystemInfo.deviceModel = {0}", SystemInfo.deviceModel);
                Log.Info("SystemInfo.deviceUniqueIdentifier = {0}", SystemInfo.deviceUniqueIdentifier);
                Log.Info("SystemInfo.graphicsDeviceVersion = {0}", SystemInfo.graphicsDeviceVersion);
                Log.Info("====================================================================================");
            }
            StartCoroutine(DoInit());
        }

        /// <summary>
        /// Use Coroutine to initialize the two base modules: Resource & UI
        /// </summary>
        private IEnumerator DoInit()
        {
            yield return null;
            IsBeforeInit = true;
            if (AppEntry != null)
            {
                yield return StartCoroutine(AppEntry.OnBeforeInit());
            }


            IsOnInit = true;
            yield return StartCoroutine(DoInitModules(GameModules));

            IsStartGame = true;
            if (AppEntry != null)
            {
                yield return StartCoroutine(AppEntry.OnGameStart());

            }
            UnityThreadDetect.Start();
            IsInited = true;
        }

        private IEnumerator DoInitModules(IList<IModuleInitable> modules)
        {
            var startInitTime = 0f;
            var startMem = 0f;
            foreach (IModuleInitable initModule in modules)
            {
                if (AppConfig.IsDebugBuild)
                {
                    startInitTime = Time.time;
                    startMem = GC.GetTotalMemory(false);
                }
                yield return StartCoroutine(initModule.Init());
                if (AppConfig.IsDebugBuild)
                {
                    var nowMem = GC.GetTotalMemory(false);
                    Log.Info("Init Module: #{0}# Time:{1}, DiffMem:{2}, NowMem:{3}", initModule.GetType().FullName,
                        Time.time - startInitTime, nowMem - startMem, nowMem);
                }
            }
        }

        private float time_update_per1s,time_update_per300ms;
#if USE_UGUI_FPS
        protected virtual void Update()
#else
        protected virtual void OnGUI()
#endif
        {
            if (ShowFps)
            {
                if (RenderWatcher == null)
                    RenderWatcher = new FpsWatcher(0.95f);
                RenderWatcher.OnUIUpdate();
            }

            UpdateEvent?.Invoke();
            float time = Time.time;
            if (time > time_update_per1s)
            {
                time_update_per1s = time + 1.0f;
                UpdatePer1sEvent?.Invoke();
            }
            if (time > time_update_per300ms)
            {
                time_update_per300ms = time + 0.3f;
                UpdatePer300msEvent?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            Log.TotalFrame ++;
        }

        void OnApplicationQuit()
        {
            IsApplicationQuit = true;
            IsAppPlaying = false;
            if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
             LogFileManager.Destory();
            LogFileManager.CloseStream();
            
            
        }
        
        void OnApplicationFocus(bool focus)
        {
            IsApplicationFocus = focus;
        }
        
        /// <summary>
        /// 清除数据，比如切换帐号/低内存等清空缓存数据
        /// </summary>
        public void ClearModuleData()
        {
            var modules = GameModules;
            foreach (IModuleInitable initModule in modules)
            {
                initModule.ClearData();
            }
        }
    }


    public class FpsWatcher
    {
        private float Value;
        private float Sensitivity;

        private string _cacheMemoryStr;
        private string _cacheFPSStr;

#if USE_UGUI_FPS
        private Text CacheText;
#endif
        private float _LastInterval;  
        //统计周期
        private float _UpdateInterval = 0.1f;  
        private int _Frames = 0;  
        private float _FPS;  
        
        public FpsWatcher(float sensitivity)
        {
            Value = 0f;
            Sensitivity = sensitivity;
#if USE_UGUI_FPS
            var canvasGameObj = new GameObject("__FPSCanvas__").AddComponent<Canvas>();
            canvasGameObj.renderMode = RenderMode.ScreenSpaceOverlay;
            var fpsTextObj = new GameObject("FPSText").AddComponent<Text>();
            fpsTextObj.transform.SetParent(canvasGameObj.transform);
            var rectTransform = fpsTextObj.rectTransform;
            //位置固定在左下角
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchoredPosition = new Vector3(2,-2,0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(300, 20);
            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            fpsTextObj.font = font;
            CacheText = fpsTextObj;
            GameObject.DontDestroyOnLoad(canvasGameObj);
            _Frames = 0;
            _LastInterval = Time.realtimeSinceStartup;
#endif
        }

        public void OnUIUpdate()
        {
            _Frames++;  
            if(Time.realtimeSinceStartup > _LastInterval + _UpdateInterval)  
            {  
                _FPS = _Frames / (Time.realtimeSinceStartup - _LastInterval);  
                _Frames = 0;  
                _LastInterval = Time.realtimeSinceStartup;  
                
                _cacheFPSStr = string.Format("FPS: {0}", (int)_FPS);
            }  

            if (Time.frameCount % 30 == 0 || _cacheMemoryStr == null || _cacheFPSStr == null)
            {
                _cacheMemoryStr = string.Format("(mem:{0:F1}MB)", Log.GetMonoUseMemory());
#if USE_UGUI_FPS
                if (CacheText == null) return;
                CacheText.SetText(_cacheFPSStr  + " "+ _cacheMemoryStr);
#endif
            }

#if !USE_UGUI_FPS
            // Must run in OnGUI
            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Label(_cacheMemoryStr);
            GUILayout.Label(_cacheFPSStr);
            GUILayout.EndVertical();
#endif
        }

        public string Watch(string format, float value)
        {
            Value = Value * Sensitivity + value * (1f - Sensitivity);
            return string.Format(format, Value);
        }
    }


    /// <summary>
    /// Engine Config, Wrapper of the TableRow
    /// </summary>
    public class KEngineInfo
    {
        private static KEngineInfo _instance;

        public static KEngineInfo Wrap(TableRow row)
        {
            if (_instance == null)
                _instance = new KEngineInfo();

            _instance._row = row;
            return _instance;
        }

        private TableRow _row;

        private KEngineInfo()
        {
        }

        public string Key
        {
            get { return _row["Key"]; }
        }

        public string Value
        {
            get { return _row["Value"]; }
            set { _row["Value"] = value; }
        }


    }
}