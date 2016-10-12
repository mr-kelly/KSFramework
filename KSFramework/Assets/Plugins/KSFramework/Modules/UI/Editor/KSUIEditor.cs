#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KAssetBundleLoader.cs
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

using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using UnityEditor;

namespace KSFramework.Editor
{
    public class KSUIEditor
    {
        /// <summary>
        /// 自动设置所有UI图集，使用Unity的图集机制 SpritePacker
        /// 所有UIAtlas目录下的图片，都会根据其所在目录被设置成图集
        /// </summary>
        [MenuItem("KEngine/UI(UGUI)/Make All Atlas (SpritePacker)")]
        public static void MakeAllAtlasSpritePacker()
        {
            var spriteDir = "Assets/" + KEngineDef.ResourcesEditDir + "/UIAtlas";

            if (!Directory.Exists(spriteDir))
            {
                Log.LogError("Not found dir : {0}", spriteDir);
                return;
            }

            foreach (var atlasDir in Directory.GetDirectories(spriteDir))
            {
                var dirName = Path.GetFileName(atlasDir);
                foreach (var imagePath in Directory.GetFiles(atlasDir, "*.*", SearchOption.AllDirectories))
                {
                    var texImpoter = TextureImporter.GetAtPath(imagePath) as TextureImporter;
                    if (texImpoter != null)
                    {
                        texImpoter.spriteImportMode = SpriteImportMode.Single;
                        texImpoter.textureType = TextureImporterType.Sprite;
                        texImpoter.spritePackingTag = dirName;
                        texImpoter.mipmapEnabled = false;

                        // 原图非真彩，安卓进行压缩分离
                        //if (texImpoter.textureFormat != TextureImporterFormat.AutomaticTruecolor &&
                        //    !texImpoter.textureFormat.ToString().Contains("32"))
                        //{
                        //    texImpoter.SetPlatformTextureSettings("Android", texImpoter.maxTextureSize, texImpoter.textureFormat, true);
                        //}
                    }

                }
                Log.Info("Set Atlas `{0}` success!", dirName);
            }

            AssetDatabase.Refresh();
        }

    }


}