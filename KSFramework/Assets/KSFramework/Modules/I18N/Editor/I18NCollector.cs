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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSFramework.Editor
{
    /// <summary>
    /// 编辑环境，会编译所有程序集找出所有的Collector，并自动执行Collector的收集方法进行多语言收集
    /// </summary>
    public interface I18NCollector
    {
        /// <summary>
        /// 编辑环境，会编译所有程序集找出所有的Collector，并自动执行Collector的收集方法进行多语言收集
        /// </summary>
        void Collect(ref I18NItems i18List);
    }
}