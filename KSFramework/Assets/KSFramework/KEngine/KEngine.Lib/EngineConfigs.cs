#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AppEngine.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using System;
using IniParser.Model;
using IniParser.Model.Formatting;
using IniParser.Parser;

namespace KEngine
{
    public class EngineConfigs
    {
        public static string DefaultEngineConfigs = @"

[KEngine]
ProductRelPath	= Product	

AssetBundleBuildRelPath	= Product/Bundles	
StreamingBundlesFolderName = Bundles	
AssetBundleExt = .bytes	
IsLoadAssetBundle = 1	

;whether use assetdata.loadassetatpath insead of load asset bundle, editor only
IsEditorLoadAsset = 0

[KEngine.Setting]
SettingSourcePath = Product/SettingSource	

;settings where compile to?	
SettingCompiledPath = Product/Setting

;Folder in Resources
SettingResourcesPath = Setting

; Ignore genereate code for these excel.
SettingCodeIgnorePattern = (I18N/.*)|(StringsTable.*)

[KEngine.UI]
UIModuleBridge = KEngine.UI.UGUIBridge	

";
        private readonly IniData _iniData;
        public EngineConfigs(string customConfigs)
        {
            var parser = new IniDataParser();
            _iniData = parser.Parse(DefaultEngineConfigs);

            
            if (!string.IsNullOrEmpty(customConfigs))
            {
                if (customConfigs.Trim() != "")
                {
                    var userIniData = parser.Parse(customConfigs);
                    _iniData.Merge(userIniData);
                }
            }
        }

        /// <summary>
        /// GetConfig from section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="throwError">whether or not throw error when get no config</param>
        /// <returns></returns>
        public string GetConfig(string section, string key, bool throwError = true)
        {
            var sectionData = _iniData[section];
            if (sectionData == null)
            {
                if (throwError)
                    throw new Exception("Not found section from ini config: " + section);
                return null;
            }
            var value = sectionData[key];
            if (value == null)
            {
                if (throwError)
                    throw new Exception(string.Format("Not found section:`{0}`, key:`{1}` config", section, key));
            }
            return value;
        }

        /// <summary>
        /// Get ini 's all sections
        /// </summary>
        /// <returns></returns>
        public SectionDataCollection GetSections()
        {
            return _iniData.Sections;
        }
        /// <summary>
        /// To Ini string for save operation
        /// </summary>
        /// <returns></returns>
        public string ToIniString()
        {
            return _iniData.ToString(new DefaultIniDataFormatter());
        }

    }

}
