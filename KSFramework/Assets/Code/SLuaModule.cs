using UnityEngine;
using System.Collections;
using KEngine;
using SLua;

namespace KSFramework
{
    public class SLuaModule : IModule
    {
        private LuaSvr _luaSvr;

        public SLuaModule()
        {
            _luaSvr = new LuaSvr();
        }

        /// <summary>
        /// Execute lua script directly!
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        public object CallScript(string scriptCode)
        {
            return _luaSvr.luaState.doString(scriptCode);
        }

        public IEnumerator Init()
        {
            _luaSvr.init(progress => { }, () => { });


            var startTime = Time.time;
            while (!_luaSvr.inited)
            {
                if ((Time.time - startTime) > 10)
                {
                    if (Time.frameCount % 10 == 0)
                        KLogger.LogError("SLua Init too long time!!!!");
                }
                yield return null;
            }
        }
    }

}
