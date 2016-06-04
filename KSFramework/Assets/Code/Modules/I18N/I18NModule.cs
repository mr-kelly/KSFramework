using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CosmosTable;
using KEngine;
using KEngine.Modules;

namespace KSFramework
{

    /// <summary>
    /// I18NModule的Alias别名
    /// </summary>
    public class I18N : I18NModule { }

    /// <summary>
    /// 多语言模块
    /// </summary>
    public class I18NModule
    {
        //class _InstanceClass { public static KI18N _Instance = new KI18N();}
        //public static KI18N Instance { get { return _InstanceClass._Instance; } }
        protected I18NModule() { }

        public string CurLang { get; private set; }
        private static readonly Dictionary<string, string> Strs = new Dictionary<string, string>(); // 翻译字符串集合

        private static string _lang = null;

        /// <summary>
        /// 如果没配置语言，使用第一个第一
        /// </summary>
        private static string Lang
        {
            get
            {
                if (_lang == null)
                {
                    var readLang = AppEngine.GetConfig("KSFramework.I18N", "I18N", false);
                    if (!string.IsNullOrEmpty(readLang))
                        _lang = readLang;
                    else
                        return AppEngine.GetConfig("KSFramework.I18N", "I18NLanguages").Split(',')[0];
                }
                return _lang;
            }
        }

        /// <summary>
        /// 是否已经初始化完成
        /// </summary>
        private static bool _isInited = false;

        /// <summary>
        /// 惰式初始化
        /// </summary>
        public static void Init()
        {
            if (_isInited)
                return;

            var settingPath = string.Format("I18N/{0}{1}", Lang,
                AppEngine.GetConfig(KEngineDefaultConfigs.AssetBundleExt));
            var settingReader = SettingModule.Get(settingPath, false);
            foreach (var row in settingReader)
            {
                Strs[row["Id"]] = row["Value"];
            }

#if UNITY_EDITOR
            // 开发热重载
            if (SettingModule.IsFileSystemMode)
            {

                SettingModule.WatchSetting(settingPath, (_) =>
                {
                    _isInited = false;
                });
            }
#endif
            _isInited = true;
        }

        /// <summary>
        /// 配置使用的语言
        /// </summary>
        /// <param name="lang"></param>
        public static void SetLang(string lang)
        {
            _lang = lang;
        }

        /// <summary>
        /// 翻译字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Str(string str)
        {
            if (str == null) return null;

            if (!_isInited) Init();

            if (str != null)
            {
                string value;
                if (Strs.TryGetValue(str, out value) && !string.IsNullOrEmpty(value))
                    return value;
            }

            return str;
        }


    }

}
