#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AppEngineInspector.cs
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

using UnityEditor;
using UnityEngine;

namespace KEngine.Editor
{
    [InitializeOnLoad]
    public class AppEngineInitializeOnLoad
    {
        static AppEngineInitializeOnLoad()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        }

        private static void HierarchyItemCB(int instanceid, Rect selectionrect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if (obj != null)
            {
                if (obj.GetComponent<AppEngine>() != null)
                {
                    Rect r = new Rect(selectionrect);
                    r.x = r.width - 80;
                    r.width = 80;
                    var style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    style.hover.textColor = Color.cyan;
                    GUI.Label(r, "[KEngine]", style);
                }
            }
        }
    }

    [CustomEditor(typeof (AppEngine))]
    public class AppEngineInspector : UnityEditor.Editor
    {
        private bool _showModules = false;

        public override void OnInspectorGUI()
        {
            var engine = target as AppEngine;
            //Log.InfoLevel
            Log.LogLevel = (LogLevel) EditorGUILayout.EnumPopup("Log Level", Log.LogLevel);
            EditorGUILayout.LabelField("Modules Count: ", engine.GameModules.Count.ToString());

            _showModules = EditorGUILayout.Foldout(_showModules, "Modules");

            if (_showModules)
            {
                var modCount = engine.GameModules.Count;
                for (var m = 0; m < modCount; m++)
                {
                    var module = engine.GameModules[m];
                    EditorGUILayout.LabelField("- " + module.ToString());
                }
            }

            base.OnInspectorGUI();
        }
    }
}