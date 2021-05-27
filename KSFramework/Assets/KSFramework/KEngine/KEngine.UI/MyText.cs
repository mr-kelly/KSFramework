using System;
using KEngine;
using KSFramework;
using UnityEngine;

namespace UnityEngine.UI
{
    /// <summary>
    /// Author：qingqing.zhao (569032731@qq.com)
    /// Date：2021/5/27 15:02
    /// Desc：扩展Text，支持多语言
    /// </summary>
    [Serializable]
    public class MyText : Text
    {
        [SerializeField] public bool UseLangId = true;
        [SerializeField] public string LangId;
        [SerializeField] public string[] LangParams;

        void Start()
        {
            if (UseLangId && string.IsNullOrEmpty(text))
            {
                if (!string.IsNullOrEmpty(LangId))
                {
                    text = I18N.Get(LangId, LangParams);
                }
                else
                {
                    Log.LogError($"{KTool.GetRootPathName(this.transform)},lang id is null");
                }
            }
        }
    }
}