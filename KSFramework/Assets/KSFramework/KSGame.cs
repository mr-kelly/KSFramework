using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AppSettings;
using KEngine;
using KEngine.UI;
using SLua;

namespace KSFramework
{
    /// <summary>
    /// KSFramework base game entry,  create your class extends from this
    /// </summary>
    public abstract class KSGame : MonoBehaviour, IAppEntry
    {
        public static KSGame Instance { get; private set; }

        /// <summary>
        /// Module/Manager of Slua
        /// </summary>
        public LuaModule LuaModule { get; private set; }

        /// <summary>
        /// Create Module, with new some class inside
        /// </summary>
        /// <returns></returns>
        protected virtual IList<IModuleInitable> CreateModules()
        {
            return new List<IModuleInitable>
            {
                UIModule.Instance,
                LuaModule,
            };
        }

        /// <summary>
        /// Unity `Awake`
        /// </summary>
        protected virtual void Awake()
        {
            Instance = this;
            LuaModule = new LuaModule();
            AppEngine.New(gameObject, this, CreateModules());
        }



        /// <summary>
        /// Before KEngine init modules
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator OnBeforeInitModules();

        /// <summary>
        /// After KEngine inited all module, make the game start!
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator OnFinishInitModules();
    }
}
