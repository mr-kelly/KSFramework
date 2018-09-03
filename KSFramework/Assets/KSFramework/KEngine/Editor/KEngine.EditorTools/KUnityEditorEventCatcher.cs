#region Copyright(c) Kingsoft Xishanju

// Company: Kingsoft Xishanju
// Filename: KUnityEditorEventCatcher.cs
// Date:     2015/11/07
// Author:   Kelly / chenpeilin1
// Email: chenpeilin1@kingsoft.com / 23110388@qq.com

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace KUnityEditorTools
{
    /// <summary>
    /// 用于捕捉编辑器变更事件，不做具体逻辑处理，只暴露各种封装事件
    /// 
    /// 注意不要加具体逻辑在这里，namespace KAdminTools 没有具体逻辑依赖具体逻辑塞到KGameAdminEditor里
    /// 
    /// 具体逻辑使用时，注意加上[InitializeOnLoad]
    /// </summary>
    [InitializeOnLoad]
    public class KUnityEditorEventCatcher
    {
        /// <summary>
        /// Editor update事件
        /// </summary>
        private static System.Action _OnEditorUpdateEvent;
        public static System.Action OnEditorUpdateEvent
        {
            get
            {
                return _OnEditorUpdateEvent;
            }
            set
            {
                _OnEditorUpdateEvent = value;
                if (IsInited && _OnEditorUpdateEvent != null)
                    _OnEditorUpdateEvent();
            }
        }
        /// <summary>
        /// 将要播放游戏事件
        /// </summary>
        private static System.Action _OnWillPlayEvent;

        public static System.Action OnWillPlayEvent
        {
            get { return _OnWillPlayEvent; }
            set
            {
                _OnWillPlayEvent = value;
                //if (IsInited && _OnWillPlayEvent != null)
                //    _OnWillPlayEvent();
            }
        }

        /// <summary>
        /// 进入播放时刻事件
        /// </summary>
        private static System.Action _OnBeginPlayEvent;

        public static System.Action OnBeginPlayEvent
        {
            get { return _OnBeginPlayEvent; }
            set
            {
                _OnBeginPlayEvent = value;
                //if (IsInited && _OnBeginPlayEvent != null)
                //    _OnBeginPlayEvent();
            }
        }

        /// <summary>
        /// 将要停止游戏 (不包括暂停哦)
        /// </summary>
        private static System.Action _OnWillStopEvent;
        public static System.Action OnWillStopEvent
        {
            get { return _OnWillStopEvent; }
            set
            {
                _OnWillStopEvent = value;
                //if (IsInited && _OnWillStopEvent != null)
                //    _OnWillStopEvent();
            }
        }

        /// <summary>
        /// 程序集锁定事件，事件中可以进行DLL的注入修改
        /// </summary>
        private static System.Action _OnLockingAssembly;
        public static System.Action OnLockingAssembly
        {
            get { return _OnLockingAssembly; }
            set
            {
                _OnLockingAssembly = value;
                if (IsInited && _OnLockingAssembly != null)
                    _OnLockingAssembly();
            }
        }


        /// <summary>
        /// 编译前事件，比较特殊的处理，配合了PostBuildProcess和PostBuildScene
        /// </summary>
        public static Action OnBeforeBuildPlayerEvent;


        /// <summary>
        /// 编译完成后事件
        /// </summary>
        private static System.Action<BuildTarget, string> _OnPostBuildPlayerEvent;
        public static System.Action<BuildTarget, string> OnPostBuildPlayerEvent
        {
            get { return _OnPostBuildPlayerEvent;}
            set
            {
                _OnPostBuildPlayerEvent = value;
            }
        }
        /// <summary>
        /// Save Scene事件
        /// </summary>
        internal static System.Action _onSaveSceneEvent;
        public static System.Action OnSaveSceneEvent
        {
            get
            {
                return _onSaveSceneEvent;
            }
            set
            {
                _onSaveSceneEvent= value;
            }
        }

        /// <summary>
        /// 是否静态构造完成
        /// </summary>
        public static bool IsInited { get; private set; }
        static KUnityEditorEventCatcher()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            SceneView.onSceneGUIDelegate -= OnSceneViewGUI;
            SceneView.onSceneGUIDelegate += OnSceneViewGUI;

            EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;



            if (OnLockingAssembly != null)
            {
                EditorApplication.LockReloadAssemblies();
                OnLockingAssembly();
                EditorApplication.UnlockReloadAssemblies();
            }


            IsInited = true;
        }

        /// <summary>
        /// For BeforeBuildEvent, Because in Unity:   PostProcessScene -> PostProcessScene ->.... PostProcessScene -> PostProcessBuild
        /// When true, waiting PostProcessBuild to revert to false
        /// </summary>
        private static bool _beforeBuildFlag = false;

        [PostProcessScene]
        private static void OnProcessScene()
        {
            if (!_beforeBuildFlag && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _beforeBuildFlag = true;

                if (OnBeforeBuildPlayerEvent != null)
                    OnBeforeBuildPlayerEvent();
            }
        }
        /// <summary>
        /// Unity标准Build后处理函数
        /// </summary>
        [PostProcessBuild()]
        private static void OnPostBuildPlayer(BuildTarget target, string pathToBuiltProject)
        {
            if (OnPostBuildPlayerEvent != null)
            {
                OnPostBuildPlayerEvent(target, pathToBuiltProject);
            }

            UnityEngine.Debug.Log(string.Format("Success build ({0}) : {1}", target, pathToBuiltProject));
        }

        /// <summary>
        /// 播放状态改变，进行一些编译性的东西, 比如点击播放，编译文件、编译脚本、编译配置等
        /// </summary>
        private static void OnPlayModeStateChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!EditorApplication.isPlaying) // means Will Change to Playmode
                {
                    if (OnWillPlayEvent != null)
                    {
                        OnWillPlayEvent();
                    }
                }
                else
                {
                    if (OnBeginPlayEvent != null)
                    {
                        OnBeginPlayEvent();
                    }
                }
            }
            else
            {
                if (EditorApplication.isPlaying)
                {
                    if (OnWillStopEvent != null)
                    {
                        OnWillStopEvent();
                    }
                }
            }
        }

        /// <summary>
        /// 捕捉编译过程中、同时播放游戏的状态，强制暂停，避免运行出错
        /// </summary>
        /// <param name="view"></param>
        //static void OnSceneViewGUI(SceneView view)
        static void OnEditorUpdate()
        {
            CheckComplie();
            if (OnEditorUpdateEvent != null)
            {
                OnEditorUpdateEvent();
            }
        }

        private static void OnSceneViewGUI(SceneView sceneview)
        {
            CheckComplie();
        }

        static void CheckComplie()
        {
            // 检查编译中，立刻暂停游戏！
            if (EditorApplication.isCompiling)
            {
                if (EditorApplication.isPlaying)
                {
                    UnityEngine.Debug.Log("Force Stop Play, because of Compiling.");
                    EditorApplication.isPlaying = false;
                }
            }
        }
    }

    internal class SaveSceneAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.Contains(".unity"))
                {
                    //scenePath = Path.GetDirectoryName(path);
                    //sceneName = Path.GetFileNameWithoutExtension(path);
                    KUnityEditorEventCatcher._onSaveSceneEvent();
                }
            }

            return paths;
        }
    }
}