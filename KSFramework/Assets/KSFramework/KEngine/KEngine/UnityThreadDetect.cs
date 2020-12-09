using System;
using System.Threading;
using UnityEngine;

namespace KEngine
{
    /// <summary>
    /// 开另外一个线程检测unity是否被卡死
    /// </summary>
    public static class UnityThreadDetect
    {
        public static Thread _MainThread = System.Threading.Thread.CurrentThread;//获取unity线程
        private static int check_interval = 3000;//检测间隔

        public static void Start()
        {
            new Thread(CheckMainThread).Start();
        }
        
        static void CheckMainThread()
        {
            long frame = 0;
            while(!AppEngine.IsApplicationQuit)
            {
                frame = Log.TotalFrame;
                Thread.Sleep(check_interval);
                if (frame == Log.TotalFrame && AppEngine.IsAppPlaying)
                {
                    Log.LogToFile("unity thread dead,ThreadState:{0}",_MainThread.ThreadState);
                    if (AppEngine.IsApplicationFocus)
                    {
                        //todo report error
                    }
                }
            }
        }

        //测试代码让unity卡死
        public static void TesBadCode()
        {
            if (!Application.isEditor) 
                return;

            byte i = 0;  
            while (true)
            {
                i++;
            }
        }
    }
}