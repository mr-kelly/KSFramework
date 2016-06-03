using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CosmosTable;
using KEngine;
using UnityEngine;

namespace KSFramework.Editor
{
    /// <summary>
    /// default collector, 收集StringsTable.xlsx编译后的表StringsTable.bytes的多语言字符串
    /// </summary>
    public class StringsTableI18NCollector : I18NCollector
    {
        public void Collect(ref I18NItems i18List)
        {
            CollectStringsTable(ref i18List);
        }

        /// <summary>
        /// 收集StringsTalbe.bytes的字符串
        /// </summary>
        /// <param name="refItems"></param>
        static void CollectStringsTable(ref I18NItems refItems)
        {
            var compilePath = AppEngine.GetConfig("SettingCompiledPath");
            var ext = AppEngine.GetConfig("AssetBundleExt");
            var stringsTablePath = string.Format("{0}/StringsTable{1}", compilePath, ext);
            if (!File.Exists(stringsTablePath))
                return;

            string stringsTableContent;
            using (var stream = new FileStream(stringsTablePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    stringsTableContent = reader.ReadToEnd();
                }
            }

            var tableFile = new TableFile(stringsTableContent);
            foreach (var row in tableFile)
            {
                var srcStr = row["Id"];
                refItems.Add(srcStr, stringsTablePath);
            }
            Debug.Log("[CollectStringsTable]Success!");
        }
    }
}