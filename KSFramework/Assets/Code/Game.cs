using UnityEngine;
using System.Collections;
using KEngine;
using KEngine.UI;
using SLua;

namespace KSFramework
{
    /// <summary>
    /// Game Main Entry
    /// </summary>
    public class Game : MonoBehaviour, IAppEntry
    {
        public static Game Instance { get; private set; }
        //private AppEngine _engine;

        /// <summary>
        /// Module/Manager of Slua
        /// </summary>
        public LuaModule LuaModule { get; private set; }

        /// <summary>
        /// Unity `Awake`
        /// </summary>
        void Awake()
        {
            LuaModule = new LuaModule();
            AppEngine.New(gameObject, this, new IModule[]
            {
                UIModule.Instance,
                LuaModule,
            });

            Instance = this;
        }


        /// <summary>
        /// Before KEngine init modules
        /// </summary>
        /// <returns></returns>
        public IEnumerator OnBeforeInitModules()
        {
            yield break;
        }

        /// <summary>
        /// After KEngine inited all module, make the game start!
        /// </summary>
        /// <returns></returns>
        public IEnumerator OnFinishInitModules()
        {
            UIModule.Instance.OpenWindow("Login", 888);
            yield break;
        }
    }
}
