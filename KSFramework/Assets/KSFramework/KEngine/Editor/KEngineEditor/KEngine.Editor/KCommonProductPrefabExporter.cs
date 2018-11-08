#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KCommonProductPrefabExporter.cs
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

using System.IO;
using KEngine;
using KEngine.Editor;
using UnityEditor;

/// <summary>
/// Example of KBuild_Base
/// </summary>
public class KCommonProductPrefabExporter : KBuild_Base
{
    public override string GetDirectory()
    {
        return "";
    }

    public override string GetExtention()
    {
        return "dir";
    }

    public override void Export(string path)
    {
        path = path.Replace('\\', '/');

        string[] fileArray = Directory.GetFiles(path, "*.prefab");
        foreach (string file in fileArray)
        {
            string filePath = file.Replace('\\', '/');
            Log.Info("Build Func To: " + filePath);
        }
    }

    [MenuItem("KEngine/Build Product Folder Prefabs")]
    private static void BuildProductFolderPrefabs()
    {
        KResourceBuilder.ProductExport(new KCommonProductPrefabExporter());
    }
}