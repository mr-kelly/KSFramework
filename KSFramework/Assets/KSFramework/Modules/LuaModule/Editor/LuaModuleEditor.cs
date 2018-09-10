#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
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
using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace KSFramework.Editor
{
    /// <summary>
    /// LuaModule Editor code, process lua compile
    /// </summary>
    public class LuaModuleEditor
    {

        /// <summary>
        /// 标记是否重复执行BeforeBuildApp
        /// </summary>
        private static bool _hasBeforeBuildApp = false;

        /// <summary>
        /// 复制文件事件, 可以进行加密行为
        /// </summary>
        public static Action<string> OnCopyFile;
        /// <summary>
        /// 这里可以进行DLL篡改, 这里PostProcessScene时，DLL已经被生成了
        /// </summary>
        [PostProcessScene]
        private static void OnPostProcessScene()
        {
            if (!_hasBeforeBuildApp && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _hasBeforeBuildApp = true;
                // 这里是编译前, 对Lua进行编译处理
                Debug.Log("[LuaModuleEditor]Start compile lua script...");
                var luaPath = AppEngine.GetConfig("KSFramework.Lua", "LuaPath");
                var ext = AppEngine.GetConfig("KEngine", "AssetBundleExt");

                var luaCount = 0;
                var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath, luaPath);
                editorLuaScriptPath = editorLuaScriptPath.Replace("\\", "/");
                if (!Directory.Exists(editorLuaScriptPath))
                {
                    Debug.LogError("[LuaModuleEditor]lua script path not exist!");
                    return;
                }
                var toDir = "Assets/StreamingAssets/" + luaPath;

                // 所有的Lua脚本拷贝到StreamingAssets
                foreach (var path in Directory.GetFiles(editorLuaScriptPath, "*", SearchOption.AllDirectories))
                {
                    var cleanPath = path.Replace("\\", "/");
                    
                    var relativePath = cleanPath.Replace(editorLuaScriptPath+"/", "");
                    var toPath = Path.Combine(toDir, relativePath) + ext;

                    if (!Directory.Exists(Path.GetDirectoryName(toPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(toPath));

                    File.Copy(cleanPath, toPath, true);
                    if (OnCopyFile != null)
                        OnCopyFile(toPath);
                    luaCount++;
                }
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log(string.Format("[LuaModuleEditor]compile lua script count: {0}", luaCount));
            }
        }

        /// <summary>
        /// 构建完成,恢复标记
        /// </summary>
        [PostProcessBuild()]
        static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            Debug.Log("[LuaModuleEditor]Build Player Finished");
            _hasBeforeBuildApp = false;
        }
    }

}
