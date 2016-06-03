using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KSFramework.Editor
{

    public class I18NItems
    {
        /// <summary>
        /// 保存出处指
        /// </summary>
        public Dictionary<string, HashSet<string>> Items = new Dictionary<string, HashSet<string>>();

        public void Add(string theString, string fromDesc)
        {
            HashSet<string> sources;
            if (!Items.TryGetValue(theString, out sources))
            {
                sources = Items[theString] = new HashSet<string>();
            }
            sources.Add(fromDesc);  // 添加来源
        }

        public void Clear()
        {
            Items.Clear();
        }
    }

}
