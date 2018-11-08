using UnityEngine;
using System.Collections;
using System.Threading;

namespace KUnityEditorTools
{
    public class DirectoryWatcher
    {
        public DirectoryWatcher()
        {
            ThreadPool.QueueUserWorkItem(ThreadStart);
        }

        void ThreadStart(object arg)
        {
            while (true)
            {
                
                Thread.Sleep(1000);
            }
        }
    }
}
