#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KSerializeMaterial.cs
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

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 序列化到AssetBundle的对象
/// 需要和打包工程保持一致的目录，
/// 当脚本所在目录改变，原打包的AssetBundle将失效，需要重新打包
/// </summary>
public class KSerializeMaterial : ScriptableObject
{
    public string MaterialName;
    public string ShaderName;
    public string ShaderPath;

    public List<KSerializeMaterialProperty> Props = new List<KSerializeMaterialProperty>();
}