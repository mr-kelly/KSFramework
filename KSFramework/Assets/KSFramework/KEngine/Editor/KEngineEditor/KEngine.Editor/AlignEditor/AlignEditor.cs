#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AlignEditor.cs
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
using UnityEditor;
using UnityEngine;

/// <summary>
/// AlignEditor用于UI编辑的批量对齐。  第一个项目的工具， by chenpeilin
/// </summary>
public class AlignEditor : EditorWindow
{
    public string alignX = "0";
    public string alignY = "0";
    public string alignZ = "0";

    [MenuItem("Window/AlignEditor")]
    public static void Init()
    {
        // Init Editor Window
        var alignEditor = EditorWindow.GetWindow(typeof (AlignEditor));
        alignEditor.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Distance Align Options");
        alignX = EditorGUILayout.TextField("X", alignX);
        alignY = EditorGUILayout.TextField("Y", alignY);
        alignZ = EditorGUILayout.TextField("Z", alignZ);
        /* ����ѡ�ж���ť */
        if (GUILayout.Button("Align jelection"))
        {
            GameObject[] gameObjects = this.getSortedGameObjects();

            /* ���ݵ�һ����������λ�ã������������ */
            Vector3 firstObjectVec = Vector3.zero; /* ��ʼ�� */
            for (int i = 0; i < gameObjects.Length; i++)
            {
                /* ѭ����һ�����󣬸����� */
                if (i == 0)
                {
                    firstObjectVec = gameObjects[i].transform.localPosition;
                    continue;
                }

                /*ѭ����������*/
                gameObjects[i].transform.localPosition = new Vector3(
                    firstObjectVec.x + Convert.ToSingle(alignX)*i,
                    firstObjectVec.y + -Convert.ToSingle(alignY)*i, /* �����Ӿ�������Ϊy����������Ϊx������ ����x */
                    firstObjectVec.z + Convert.ToSingle(alignZ)*i);
            }
        }

        GUILayout.Label("Other Align");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Left/Right Align"))
        {
            this.PositionSelectionObjects(AlignType.LeftAlign);
        }

        /* ������, ���ж����y���ڵ�һ������ */
        if (GUILayout.Button("Top/Bottom Align"))
        {
            this.PositionSelectionObjects(AlignType.TopAlign);
        }
        GUILayout.EndHorizontal();
    }


    private enum AlignType
    {
        TopAlign,
        LeftAlign,
        RightAlign,
        BottomAlign
    }


    /*
    /// <summary>
    /// �Ƚ���Ϸ��������ί�з���
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    */

    private int CompareGameObjectsByName(GameObject a, GameObject b)
    {
        /* ʹ��ϵͳ���ַ����Ƚ� */

        return a.name.CompareTo(b.name);

        ///* �Ƚ���Ϸ�����������һλ */
        //char aLast = a[a.Length-1];
        //char bLast = b[b.Length - 1];
        //if (a == b)
        //{
        //    return 0;  // equal
        //}
        //else
        //{

        //}
    }

    /*
    /// <summary>
    ///  ��ȡ����������������� ��ѡ��Ϸ����
    /// </summary>
    /// <returns></returns>
    */

    private GameObject[] getSortedGameObjects()
    {
        List<GameObject> gameObjects = new List<GameObject>(Selection.gameObjects);

        gameObjects.Sort(this.CompareGameObjectsByName); /* ���� ί��*/

        return gameObjects.ToArray();
    }

    /*
    /// <summary>
    /// ͳһ��λ����ѡ�еĶ���
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    */

    private void PositionSelectionObjects(AlignType alignType)
    {
        GameObject[] gameObjects = this.getSortedGameObjects();


        /* ���뿪ʼ */
        if (gameObjects.Length > 0)
        {
            /* ��ȡ��һ��Ԫ�أ�����Ԫ�ظ�������λ */
            if (alignType == AlignType.TopAlign)
            {
                float firstY = gameObjects[0].transform.localPosition.y; /*ͳһy, ������ */

                foreach (GameObject obj in gameObjects)
                {
                    float selfX = obj.transform.localPosition.x;
                    float selfZ = obj.transform.localPosition.z;

                    obj.transform.localPosition = new Vector3(selfX, firstY, selfZ);
                }
            }
            else if (alignType == AlignType.LeftAlign) /*�����*/
            {
                float fisrtX = gameObjects[0].transform.localPosition.x;

                foreach (GameObject obj in gameObjects)
                {
                    float selfY = obj.transform.localPosition.y;
                    float selfZ = obj.transform.localPosition.z;

                    obj.transform.localPosition = new Vector3(fisrtX, selfY, selfZ);
                }
            }
        }
    }
}