#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KObjectDebuggerEditor.cs
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

using System.Reflection;
using KEngine;
using UnityEditor;

/// <summary>
/// KObjectDebugger的Inspector具体信息输出
/// </summary>
[CustomEditor(typeof (KObjectDebugger))]
public class KObjectDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var dTarget = (KObjectDebugger) target;
        if (dTarget.WatchObject != null)
        {
            foreach (
                var field in
                    dTarget.WatchObject.GetType()
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(dTarget.WatchObject);
                EditorGUILayout.LabelField(field.Name, value != null ? value.ToString() : "[NULL]");
            }
            foreach (
                var prop in
                    dTarget.WatchObject.GetType()
                        .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var getMethod = prop.GetGetMethod();
                if (getMethod != null)
                {
                    var ret = getMethod.Invoke(dTarget.WatchObject, new object[] {});

                    EditorGUILayout.LabelField(prop.Name, ret != null ? ret.ToString() : "[NULL]");
                }
            }
        }
    }
}