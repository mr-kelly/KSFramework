using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using KSFramework;

[InitializeOnLoad]
[CustomEditor(typeof(UILuaOutlet))]
public class UILuaOutletEditor : Editor
{
    /// <summary>
    /// mark all Game objects that has Outlet, for GUIhierarchyWindow mark
    /// </summary>
    static Dictionary<GameObject, string> _outletObjects = new Dictionary<GameObject, string>();

    static UILuaOutletEditor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        UIWindowAssetEditor.CustomInspectorGUIAfter += (KEngine.UI.UIWindowAsset target) => {
            if (target.gameObject.GetComponent<UILuaOutlet>() == null)
            {
                if (GUILayout.Button("Add UILuaOutlet"))
                {
                    target.gameObject.AddComponent<UILuaOutlet>();

                }
            }
        };
    }

    private static void HierarchyItemCB(int instanceid, Rect selectionrect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
        if (obj != null)
        {
            if (_outletObjects.ContainsKey(obj))
            {
                Rect r = new Rect(selectionrect);
                r.x = r.width - 80;
                r.width = 80;
                var style = new GUIStyle();
                style.normal.textColor = Color.green;
                style.hover.textColor = Color.cyan;
                GUI.Label(r, string.Format("=>'{0}'", _outletObjects[obj]), style);
            }
        }
    }

    GUIStyle GreenFont;
    GUIStyle RedFont;

    private HashSet<string> _cachedPropertyNames = new HashSet<string>();

    void OnEnable()
    {
        GreenFont = new GUIStyle();
        GreenFont.fontStyle = FontStyle.Bold;
        GreenFont.fontSize = 11;
        GreenFont.normal.textColor = Color.green;
        RedFont = new GUIStyle();
        RedFont.fontStyle = FontStyle.Bold;
        RedFont.fontSize = 11;
        RedFont.normal.textColor = Color.red;
    }

    public override void OnInspectorGUI()
    {
        _cachedPropertyNames.Clear();

        EditorGUI.BeginChangeCheck();

        var outlet = target as UILuaOutlet;
        //根据选中Object的名字自动填充Name，不会影响已设置的Name
        outlet.FillByObjectName = EditorGUILayout.Toggle("自动填充Name", outlet.FillByObjectName);
        if (outlet.OutletInfos == null || outlet.OutletInfos.Count == 0)
        {
            if (GUILayout.Button("Add New Outlet"))
            {
                if (outlet.OutletInfos == null)
                    outlet.OutletInfos = new List<UILuaOutlet.OutletInfo>();

                Undo.RecordObject(target, "Add OutletInfo");
                outlet.OutletInfos.Add(new UILuaOutlet.OutletInfo());
            }

        }
        else
        {


            // outlet ui edit

            for (var j = outlet.OutletInfos.Count - 1; j >= 0; j--)
            {
                var currentTypeIndex = -1;
                var outletInfo = outlet.OutletInfos[j];
                string[] typesOptions = new string[0];

                var isValid = outletInfo.Object != null && !_cachedPropertyNames.Contains(outletInfo.Name);
                // check duplicate property name
                _cachedPropertyNames.Add(outletInfo.Name);

                if (outletInfo.Object != null)
                {
                    if (outletInfo.Object is GameObject)
                    {


                        currentTypeIndex = 0;// give it default
                        var gameObj = outletInfo.Object as GameObject;
                        Component[] components = gameObj.GetComponents<Component>();
                        //                        //TODO 把Transform组件增加进去
                        //                        var count = components.Length + 1;
                        //                        components[count - 1] = gameObj.transform;


                        _outletObjects[gameObj] = outletInfo.Name;

                        typesOptions = new string[components.Length];
                        for (var i = 0; i < components.Length; i++)
                        {
                            var com = components[i];
                            if (com == null) continue;
                            var typeName = typesOptions[i] = com.GetType().FullName;
                            if (typeName == outletInfo.ComponentType)
                            {
                                currentTypeIndex = i;
                            }
                        }
                    }
                }


                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Lua Property: '{0}'", outletInfo.Name), isValid ? GreenFont : RedFont);
                EditorGUILayout.Space();
                if (GUILayout.Button("+"))
                {
                    Undo.RecordObject(target, "Insert OutletInfo");
                    outlet.OutletInfos.Insert(j, new UILuaOutlet.OutletInfo());
                }
                if (GUILayout.Button("-"))
                {

                    Undo.RecordObject(target, "Remove OutletInfo");
                    outlet.OutletInfos.RemoveAt(j);
                }
                EditorGUILayout.EndHorizontal();

                outletInfo.Name = EditorGUILayout.TextField("Name:", outletInfo.Name);
                outletInfo.Object = EditorGUILayout.ObjectField("Object:", outletInfo.Object, typeof(UnityEngine.Object), true);
                //在开发期可以考虑自动填充Name,不会修改原已赋值的name
                if (outlet.FillByObjectName && outletInfo.Object != null && string.IsNullOrEmpty(outletInfo.Name))
                {
                    outletInfo.Name = outletInfo.Object.name;
                }
                if (currentTypeIndex >= 0)
                {
                    var typeIndex = EditorGUILayout.Popup("Component:", currentTypeIndex, typesOptions);
                    outletInfo.ComponentType = typesOptions[typeIndex].ToString();

                }
            }
        }
        //base.OnInspectorGUI ();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "GUI Change Check");
        }
    }


}
