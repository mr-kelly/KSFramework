#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: UIWindowAssetEditor.cs
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

using KEngine.UI;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal class KUIPanelAssetEditorInitializer
{
    static KUIPanelAssetEditorInitializer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    private static void HierarchyItemCB(int instanceid, Rect selectionrect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
        if (obj != null)
        {
            if (obj.GetComponent<UIWindowAsset>() != null)
            {
                Rect r = new Rect(selectionrect);
                r.x = r.width - 30;
                r.width = 30;
                var style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.hover.textColor = Color.cyan;
                GUI.Label(r, string.Format("[UI]", obj.name), style);
            }
        }
    }
}

[CustomEditor(typeof (UIWindowAsset))]
[CanEditMultipleObjects]
public class UIWindowAssetEditor : Editor
{
	public static System.Action<UIWindowAsset> CustomInspectorGUIAfter;
	public static System.Action<UIWindowAsset> CustomInspectorGUIBefore;
    
    private SerializedProperty IsShowTabBar, TabBarId, Atals_arr, m_Text;
    
    protected void OnEnable()
    {
        IsShowTabBar = serializedObject.FindProperty("IsShowTabBar");
        TabBarId = serializedObject.FindProperty("TabBarId");
        Atals_arr = serializedObject.FindProperty("Atals_arr");
       
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
		if (CustomInspectorGUIBefore != null) CustomInspectorGUIBefore(target as UIWindowAsset);
        EditorGUILayout.HelpBox("A UI Will be build for name: " + target.name, MessageType.Info);
        
        //add custom editor 
        IsShowTabBar.boolValue = EditorGUILayout.Toggle("是否有侧边栏:", IsShowTabBar.boolValue);
        if (IsShowTabBar.boolValue)
        {
            TabBarId.intValue = EditorGUILayout.IntField("侧边栏id:", TabBarId.intValue);
        }
        Atals_arr.stringValue = EditorGUILayout.TextField("包含的图集(打包会自动赋值):", Atals_arr.stringValue);
        //TODO 在分开两个工程之后，添加宏定义在Client工程不可修改IsUIEditor的属性值
        
        
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
		if (CustomInspectorGUIAfter != null) CustomInspectorGUIAfter(target as UIWindowAsset);
    }
}