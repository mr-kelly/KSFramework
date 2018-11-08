#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: IUIBridge.cs
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

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace KEngine.UI
{

    public interface IUIBridge
    {
        // Init the UI Bridge, necessary
        void InitBridge();

        /// <summary>
        /// CreateUIController
        /// </summary>
        /// <returns></returns>
        UIController CreateUIController(GameObject uiObj, string uiTemplateName);

        void UIObjectFilter(UIController controller, GameObject uiObject);

        IEnumerator LoadUIAsset(UILoadState loadState, UILoadRequest request);

    }
}