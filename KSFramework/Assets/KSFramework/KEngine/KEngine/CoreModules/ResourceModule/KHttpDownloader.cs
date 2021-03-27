#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KHttpDownloader.cs
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
using System.IO;
using System.Threading;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// 多线程+断点续传 http下载器, 注意用完后要Dispose
    /// 
    /// TODO: 线程的回调Callback有点难看，以后弄个KHttpDownloader2（本类稳定就不改本类）
    /// </summary>
    public class KHttpDownloader :  IDisposable
    {
        private string _saveFullPath;

        public string SaveFullPath
        {
            get { return _saveFullPath; }
            private set { _saveFullPath = value; }
        }

        public string Url { get; private set; }
        private float TIME_OUT_DEF;

        private bool FinishedFlag = false;

        public bool IsFinished
        {
            get { return ErrorFlag || FinishedFlag; }
        }

        private bool ErrorFlag = false;

        public bool IsError
        {
            get { return ErrorFlag; }
        }

        private bool _isLog; // 是否显示下载过程中的日志
        private bool _useContinue; // 是否断点续传
        private bool UseCache;
        private int ExpireDays = 1; // 过期时间, 默认1天

        public float Progress = 0; // 進度
        public int TotalSize = int.MaxValue; // 下载的整个大小，在获取到Response后，会设置这个值

        private KHttpDownloader()
        {
        }


        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="fullUrl">网络上的url</param>
        /// <param name="saveFullPath">完整的保存路径！</param>
        /// <param name="useContinue">是否断点续传</param>
        /// <param name="useCache">如果存在则不下载了！</param>
        /// <param name="expireDays"></param>
        /// <param name="timeout"></param>
        public static KHttpDownloader Load(string fullUrl, string saveFullPath, bool useContinue = false,
            bool useCache = false, int expireDays = 1, int timeout = 5,bool isLog = false)
        {
            var downloader = new KHttpDownloader();
            downloader.Init(fullUrl, saveFullPath, useContinue, useCache, expireDays, timeout,isLog);

            return downloader;
        }
        
        private void Init(string fullUrl, string saveFullPath, bool useContinue, bool useCache = false,
            int expireDays = 1, int timeout = 10,bool isLog = false)
        {
            Url = fullUrl;
            SaveFullPath = saveFullPath;
            UseCache = useCache;
            _useContinue = useContinue;
            ExpireDays = expireDays;
            TIME_OUT_DEF = timeout; // 默认10秒延遲
            _isLog = isLog;
            Game.Instance.StartCoroutine(StartDownload(fullUrl));
        }

        public static KHttpDownloader Load(string fullUrl, string saveFullPath, int expireDays, int timeout = 5)
        {
            return Load(fullUrl, saveFullPath, expireDays != 0, true, expireDays, timeout);
        }

        private IEnumerator StartDownload(string fullUrl)
        {
            if (UseCache && File.Exists(SaveFullPath))
            {
                var lastWriteTime = File.GetLastWriteTimeUtc(SaveFullPath);
                Debug.Log(string.Format("缓存文件: {0}, 最后修改时间: {1}", SaveFullPath, lastWriteTime));
                var deltaDays = (DateTime.Now - lastWriteTime).TotalDays;
                // 文件未过期
                if (deltaDays < ExpireDays)
                {
                    Debug.Log(string.Format("缓存文件未过期 {0}，跳过下载", SaveFullPath));
                    FinishedFlag = true;
                    ErrorFlag = false;
                    yield break;
                }
            }

            string dir = Path.GetDirectoryName(SaveFullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var totalSize = int.MaxValue;
            var downloadSize = 0;
            var isThreadError = false;
            var isThreadFinish = false;
            var isThreadStart = false;

            ThreadPool.QueueUserWorkItem((_) =>
            {
                ThreadableResumeDownload(fullUrl, (totalSizeNow, downSizeNow) =>
                {
                    totalSize = totalSizeNow;
                    downloadSize = downSizeNow;
                    isThreadStart = true;
                }, () =>
                {
                    isThreadError = true;
                    isThreadFinish = true;
                    isThreadStart = true;
                }, () => { isThreadFinish = true; });
            });
            //NOTE 先下载一段数据再保存，这样会导致进度条不平滑所以要做个插值
            var MaxTime = Time.time + TIME_OUT_DEF;
            while (!isThreadFinish && !isThreadError)
            {
                if (Time.time > MaxTime && !isThreadStart)
                {
                    //#if !UNITY_IPHONE  // TODO: 新的异步机制去暂停，Iphone 64不支持
                    //                downloadThread.Abort();
                    //#endif
                    Debug.LogError(string.Format("[KHttpDownloader]下载线程超时{0}s！: {1}",MaxTime, fullUrl));
                    isThreadError = true;
                    break;
                }
                Progress = (downloadSize/(float) totalSize);
                //if(_isLog) Log.Info($"Progress2222222 Progress:{Progress} ,downloadSize:{downloadSize} ,totalSize:{totalSize}");
                yield return null;
            }

            if (isThreadError)
            {
                Debug.LogError(string.Format("Download WWW Error: {0}", fullUrl));
                ErrorFlag = true;

                // TODO:
                //try
                //{
                //    if (File.Exists(TmpDownloadPath))
                //        File.Delete(TmpDownloadPath); // delete temporary file
                //}
                //catch (Exception e)
                //{
                //    CDebug.LogError(e.Message);
                //}

                OnFinish();
                yield break;
            }
            OnFinish();
        }

        private void OnFinish()
        {
            FinishedFlag = true;
        }

        public byte[] GetDatas()
        {
            //CDebug.Assert(IsFinished);
            //CDebug.Assert(!IsError);
            if (!IsFinished || IsError)
                throw new Exception("GetDatas: Error");
            return System.IO.File.ReadAllBytes(SaveFullPath);
        }

        public string TmpDownloadPath
        {
            get { return SaveFullPath + ".download"; }
        }

        private void ThreadableResumeDownload(string url, Action<int, int> stepCallback, Action errorCallback,Action successCallback)
        {
            System.IO.FileStream downloadFileStream;
            //打开上次下载的文件或新建文件 
            long lStartPos = 0;

            if (_useContinue && System.IO.File.Exists(TmpDownloadPath))
            {
                downloadFileStream = System.IO.File.OpenWrite(TmpDownloadPath);
                lStartPos = downloadFileStream.Length;
                downloadFileStream.Seek(lStartPos, System.IO.SeekOrigin.Current); //移动文件流中的当前指针 

                if(_isLog) Log.LogConsole_MultiThread("Resume.... from {0}", lStartPos);
            }
            else
            {
                downloadFileStream = new System.IO.FileStream(TmpDownloadPath, System.IO.FileMode.OpenOrCreate);
                lStartPos = 0;
            }
            System.Net.HttpWebRequest request = null;
            //打开网络连接 
            try
            {
                request = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(url);
                if (lStartPos > 0)
                    request.AddRange((int) lStartPos); //设置Range值
                
                if(_isLog) Log.LogConsole_MultiThread("Getting Response : {0}", url);

                //向服务器请求，获得服务器回应数据流 
                using (var response = request.GetResponse()) // TODO: Async Timeout
                {
                    TotalSize = (int) response.ContentLength;
                    if(_isLog) Log.LogConsole_MultiThread("Getted Response : {0}", url);
                    if (IsFinished)
                    {
                        throw new Exception(string.Format("Get Response ok, but is finished , maybe timeout! : {0}", url));
                    }
                    else
                    {
                        var totalSize = TotalSize;
                        using (var ns = response.GetResponseStream())
                        {
                            if(_isLog) Log.LogConsole_MultiThread("Start Stream: {0}", url);

                            int downSize = (int) lStartPos;
                            int chunkSize = 10240;
                            byte[] nbytes = new byte[chunkSize];
                            int nReadSize = (int) lStartPos;
                            while ((nReadSize = ns.Read(nbytes, 0, chunkSize)) > 0)
                            {
                                if (IsFinished)
                                    throw new Exception("When Reading Web stream but Downloder Finished!");
                                downloadFileStream.Write(nbytes, 0, nReadSize);
                                downSize += nReadSize;
                                //if(_isLog) Log.Info($"Progress11111111 nReadSize:{nReadSize} ,downloadSize:{downSize} ,totalSize:{totalSize}");
                                stepCallback(totalSize, downSize);
                            }
                            stepCallback(totalSize, totalSize);

                            request.Abort();
                            downloadFileStream.Close();
                        }
                    }
                }

                if(_isLog) Log.LogConsole_MultiThread("下载完成: {0}", url);

                if (File.Exists(SaveFullPath))
                {
                    File.Delete(SaveFullPath);
                }
                File.Move(TmpDownloadPath, SaveFullPath);
            }
            catch (Exception ex)
            {
                if(_isLog) Log.LogConsole_MultiThread("下载过程中出现错误:" + ex.ToString());

                downloadFileStream.Close();

                if (request != null)
                    request.Abort();

                try
                {
                    if (File.Exists(TmpDownloadPath))
                        File.Delete(TmpDownloadPath); // delete temporary file
                }
                catch (Exception e)
                {
                    if(_isLog) Log.LogConsole_MultiThread("删除临时下载文件出错:"+e.Message);
                }

                errorCallback();
            }
            successCallback();
        }
        
        public void Dispose()
        {
            if (!FinishedFlag)
            {
                FinishedFlag = true;
                ErrorFlag = true;
                Log.LogError(string.Format("[HttpDownloader]Not finish but Dispose: {0}", Url));
            }
        }
    }
}