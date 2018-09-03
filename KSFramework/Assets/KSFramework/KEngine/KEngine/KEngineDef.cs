#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KEngineDef.cs
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

namespace KEngine
{
    public class KEngineDef
    {
        // 美术库用到
        public const string ResourcesBuildDir = "BundleResources";

        /// <summary>
        /// 编辑状态的资源目录存放, Unity 5中采用自动对BundleResources整体打包，把编辑器部分移出
        /// </summary>
        public const string ResourcesEditDir = "BundleEditing";

        // 打包缓存，一些不同步的资源，在打包时拷到这个目录，并进行打包
        public const string ResourcesBuildCacheDir = "_ResourcesCache_";

        public const string ResourcesBuildInfosDir = "ResourcesBuildInfos";

        public const string RedundaciesDir = "_Redundancies_";
    }
}
