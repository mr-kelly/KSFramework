#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KAsync.cs
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
using System.Threading;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// 链式操作，结合协程和DOTween, 并且支持真线程（用于密集运算，无法调用Unity大部分函数）
    /// 适合做动画、结合协程、回调一堆的情况
    /// </summary>
    /// <example>
    /// KAsync.Start(doSomething)
    ///         .WaitForSeconds(1f)
    ///         .Coroutine(CGame.Instance.StartCoroutine(enumtor))
    ///         .Then(logSomething)
    ///         .Then((next)=>{
    ///             DOTween.DO(tween).OnComplete(next);
    ///         });
    ///         .When(()=> booleanVarTrue)
    ///         .Then(()=>{
    ///             // Over!
    ///         });
    /// </example>
    public class KAsync
    {
        #region 核心调度

        private Queue<AsyncWaitNextDelegate> _cacheCallbacks;
        private bool _canNext;

        private KAsync()
        {
            _canNext = true;
        }

        public bool IsFinished { get; private set; }

        public UnityEngine.Coroutine WaitFinish()
        {
            return KAsyncManager.Instance.StartCoroutine(EmWaitFinish());
        }

        private IEnumerator EmWaitFinish()
        {
            while (!IsFinished)
                yield return null;
        }

        private delegate void AsyncWaitNextDelegate(Action nextFunc);

        private void WaitNext(AsyncWaitNextDelegate callback)
        {
            if (!_canNext)
            {
                if (_cacheCallbacks == null)
                    _cacheCallbacks = new Queue<AsyncWaitNextDelegate>();
                _cacheCallbacks.Enqueue(callback);
            }
            else
            {
                _canNext = false;
                callback(Next);
            }
        }

        private void Next()
        {
            _canNext = true;
            if (_cacheCallbacks != null && _cacheCallbacks.Count > 0)
                WaitNext(_cacheCallbacks.Dequeue());
            else
                IsFinished = true;
        }

        #endregion

        /// <summary>
        /// 在子线程执行一个函数，让其回到主线程再执行的
        /// </summary>
        /// <param name="call"></param>
        public static void AddMainThreadCall(Action call)
        {
            KAsyncManager.Instance._mainThreadCallbacks.Add(call);
        }

        public static KAsync Start()
        {
            var async = new KAsync();
            return async;
        }

        public static KAsync Start(Action callback)
        {
            var async = new KAsync();

            return async.Then(callback);
        }

        public static KAsync Start(AsyncThenDelegateEasy callback)
        {
            var async = new KAsync();
            return async.Then(callback);
        }

        public KAsync Then(Action callback)
        {
            WaitNext((next) =>
            {
                callback();
                next();
            });
            return this;
        }

        public delegate void AsyncThreadDelegateFull(object param, Action next);

        public delegate void AsyncThreadDelegate(Action next);

        public delegate void AsyncThenDelegateEasy(Action next);

        public delegate void AsyncThenDelegate(Action next, Action kill);

        public KAsync Then(AsyncThenDelegate thenFunc)
        {
            WaitNext((next) => { thenFunc(next, () => { Debug.LogError("TODO: kill!"); }); });
            return this;
        }

        public KAsync Until(Func<bool> retBool, float timeout = 20)
        {
            return When(retBool, timeout);
        }

        /// <summary>
        /// 等待条件成立
        /// </summary>
        /// <param name="retBool"></param>
        /// <returns></returns>
        public KAsync When(Func<bool> retBool, float timeout = 20)
        {
            WaitNext((next) => { KAsyncManager.Instance.StartCoroutine(_CoWhen(retBool, timeout, next)); });
            return this;
        }

        private IEnumerator _CoWhen(Func<bool> retBool, float timeout, Action next)
        {
            var time = 0f;
            while (!(retBool()))
            {
                time += Time.deltaTime;
                if (time > timeout)
                {
                    Log.Error("[KAsync:When]A WHEN Timeout!!!");
                    break;
                }
                yield return null;
            }

            next();
        }

        public KAsync Then(AsyncThenDelegateEasy thenFunc)
        {
            WaitNext((next) => { thenFunc(next); });
            return this;
        }

        /// <summary>
        /// 线程。注意大部分Unity函数不能使用！ 借用协程配合~
        /// </summary>
        /// <param name="threadCalAction"></param>
        /// <returns></returns>
        public KAsync Thread(AsyncThreadDelegate threadCalAction)
        {
            return Coroutine(_Thread((thread, next) => { threadCalAction(next); }));
        }

        public KAsync Thread(AsyncThreadDelegateFull threadCalAction, object param)
        {
            return Coroutine(_Thread(threadCalAction));
        }

        public KAsync Thread(Action threadCalAction)
        {
            return Coroutine(_Thread((thread, next) =>
            {
                threadCalAction();
                next();
            }));
        }

        public IEnumerator _Thread(AsyncThreadDelegateFull threadCalAction, object param = null)
        {
            bool waitThreadFinish = false;

            var thread = new Thread(() =>
            {
                Action customNext = () => { waitThreadFinish = true; };
                threadCalAction(param, customNext);
            });

            thread.Start();

            KAsyncManager.Instance._threads.Add(thread);
            while (!waitThreadFinish)
                yield return null;
            KAsyncManager.Instance._threads.Remove(thread);
        }

        /// <summary>
        /// 开启并等待一个协程
        /// </summary>
        /// <param name="enumtor"></param>
        /// <returns></returns>
        public KAsync Coroutine(IEnumerator enumtor)
        {
            WaitNext((next) => { KAsyncManager.Instance.StartCoroutine(_StartCoroutine(enumtor, next)); });
            return this;
        }

        private IEnumerator _StartCoroutine(IEnumerator enumtor, Action next)
        {
            yield return KAsyncManager.Instance.StartCoroutine(enumtor);
            next();
        }

        /// <summary>
        /// 等待一个已经被其它MonoBehaviour开启的协程
        /// </summary>
        /// <param name="co"></param>
        /// <returns></returns>
        public KAsync Coroutine(Coroutine co)
        {
            WaitNext((next) => { KAsyncManager.Instance.StartCoroutine(_Coroutine(co, next)); });
            return this;
        }

        private IEnumerator _Coroutine(Coroutine co, Action next)
        {
            yield return co;
            next();
        }

        /// <summary>
        /// 等待一定帧数
        /// </summary>
        /// <param name="frameCount"></param>
        /// <returns></returns>
        public KAsync WaitForFrames(int frameCount)
        {
            WaitNext((next) => { KAsyncManager.Instance.StartCoroutine(_WaitForFrames(frameCount, next)); });
            return this;
        }

        private IEnumerator _WaitForFrames(int frameCount, Action next)
        {
            for (var i = 0; i < frameCount; i++)
                yield return null;
            next();
        }

        /// <summary>
        /// 等待秒数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public KAsync WaitForSeconds(float time)
        {
            WaitNext((next) => { KAsyncManager.Instance.StartCoroutine(_CoWaitForSeconds(time, next)); });
            return this;
        }

        private IEnumerator _CoWaitForSeconds(float time, Action next)
        {
            yield return new WaitForSeconds(time);
            next();
        }

        /// <summary>
        /// 等到本帧结束
        /// </summary>
        /// <returns></returns>
        public KAsync WaitForEndOfFrame()
        {
            WaitNext((next) => { KAsyncManager.Instance.StartCoroutine(_WaitForEndOfFrame(next)); });
            return this;
        }

        private IEnumerator _WaitForEndOfFrame(Action next)
        {
            yield return new WaitForEndOfFrame();
            next();
        }
    }

    public class CCoroutineState
    {
        public bool IsOk = true;
        public object Param;
    }

    #region 管理器~用于开启协程，执行主线程回调等

    internal class KAsyncManager : KBehaviour
    {
        private static KAsyncManager _instance;

        public static KAsyncManager Instance
        {
            get
            {
                if (!Application.isPlaying || IsApplicationQuited)
                    return null;

                if (_instance != null) return _instance;

                const string name = "[AsyncManager]";
                var findObj = new GameObject(name);
                GameObject.DontDestroyOnLoad(findObj);
                _instance = findObj.GetComponent<KAsyncManager>() ?? findObj.AddComponent<KAsyncManager>();

                return _instance;
            }
        }

        public readonly List<Action> _mainThreadCallbacks = new List<Action>(); // 主線程調用Unity類，如StartCoroutine
        public readonly HashSet<Thread> _threads = new HashSet<Thread>(); // 主線程調用Unity類，如StartCoroutine

        private void Update()
        {
            foreach (var i in _mainThreadCallbacks)
            {
                i();
            }
            _mainThreadCallbacks.Clear();
        }

        private void StopAllThreads()
        {
            foreach (var thread in _threads)
            {
                if (thread != null)
                {
                    thread.Abort();
                }
            }

            _threads.Clear();
        }

        private void OnApplicationQuit()
        {
            StopAllThreads();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllThreads();
        }
    }

    #endregion

    // 用于协程内部返回信息传递
}