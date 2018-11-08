

using System;

namespace SLua
{

    /// <summary>
    /// A bridge between UnityEngine.Debug.LogXXX and standalone.LogXXX
    /// </summary>
    internal class Logger
    {
        public static void Log(string msg)
        {
#if !SLUA_STANDALONE
            UnityEngine.Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif 
        }
        public static void LogError(string msg)
        {
#if !SLUA_STANDALONE
            UnityEngine.Debug.LogError(msg);
#else
            Console.WriteLine(msg);
#endif
        }

		public static void LogWarning(string msg)
		{
#if !SLUA_STANDALONE
			UnityEngine.Debug.LogWarning(msg);
#else
            Console.WriteLine(msg);
#endif
		}
    }


}