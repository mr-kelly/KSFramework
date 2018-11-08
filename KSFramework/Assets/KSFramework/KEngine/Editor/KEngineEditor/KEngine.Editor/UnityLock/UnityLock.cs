using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UnityLock : EditorWindow
{
    const string lockMenuItem = "GameObject/UnityLock/Lock GameObject";
    const string lockRecursivelyMenuItem = "GameObject/UnityLock/Lock GameObject and Children %#l";
    const string unlockMenuItem = "GameObject/UnityLock/Unlock GameObject";
    const string unlockRecursivelyMenuItem = "GameObject/UnityLock/Unlock GameObject and Children %#u";

    const string showIconPrefKey = "UnityLock_ShowIcon";
    const string addUndoRedoPrefKey = "UnityLock_EnableUndoRedo";
    const string disableSelectionPrefKey = "UnityLock_DisableSelection";

    static UnityLock()
    {
        if (!EditorPrefs.HasKey(showIconPrefKey))
        {
            EditorPrefs.SetBool(showIconPrefKey, true);
        }
        if (!EditorPrefs.HasKey(addUndoRedoPrefKey))
        {
            EditorPrefs.SetBool(addUndoRedoPrefKey, true);
        }
        if (!EditorPrefs.HasKey(disableSelectionPrefKey))
        {
            EditorPrefs.SetBool(disableSelectionPrefKey, false);
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        HierarchyIcon.drawingIcon = ShowPrefsBoolOption(showIconPrefKey, "Show lock icon in hierarchy");
        EditorGUILayout.HelpBox(
            "When enabled a small lock icon will appear in the hierarchy view for all locked objects.",
            MessageType.None);

        EditorGUILayout.Space();

        bool wasSelectionDisabled = EditorPrefs.GetBool(disableSelectionPrefKey);
        bool isSelectionDisabled = ShowPrefsBoolOption(disableSelectionPrefKey, "Disable selecting locked objects");
        EditorGUILayout.HelpBox(
            "When enabled locked objects will not be selectable in the scene view with a left click. Some objects can still be selected by using a selection rectangle; it doesn't appear to be possible to prevent this.\n\nObjects represented only with gizmos will not be drawn as gizmos aren't rendered when selection is disabled.",
            MessageType.None);

        if (wasSelectionDisabled != isSelectionDisabled)
        {
            ToggleSelectionOfLockedObjects(isSelectionDisabled);
        }

        EditorGUILayout.Space();

        ShowPrefsBoolOption(addUndoRedoPrefKey, "Support undo/redo for lock and unlock");
        EditorGUILayout.HelpBox(
            "When enabled the lock and unlock operations will be properly inserted into the undo stack just like any other action.\n\nIf this is disabled the Undo button will never lock or unlock an object. This can cause other operations to silently fail, such as trying to undo a translation on a locked object.",
            MessageType.None);

        EditorGUILayout.EndVertical();
    }

    bool ShowPrefsBoolOption(string key, string name)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(name, GUILayout.ExpandWidth(true));
        bool oldValue = EditorPrefs.GetBool(key);
        bool newValue = EditorGUILayout.Toggle(oldValue, GUILayout.Width(20));
        if (newValue != oldValue)
        {
            EditorPrefs.SetBool(key, newValue);
        }

        EditorGUILayout.EndHorizontal();

        return newValue;
    }

    [MenuItem("Window/UnityLock Preferences")]
    static void ShowPreferences()
    {
        var window = GetWindow<UnityLock>();
        window.minSize = new Vector2(275, 300);
        window.Show();
    }

    private static void ToggleSelectionOfLockedObjects(bool disableSelection)
    {
        foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if ((go.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable)
            {
                foreach (Component comp in go.GetComponents(typeof(Component)))
                {
                    if (!(comp is Transform))
                    {
                        if (disableSelection)
                        {
                            comp.hideFlags |= HideFlags.NotEditable;
                            comp.hideFlags |= HideFlags.HideInHierarchy;
                        }
                        else
                        {
                            comp.hideFlags &= ~HideFlags.NotEditable;
                            comp.hideFlags &= ~HideFlags.HideInHierarchy;
                        }
                    }
                }

                EditorUtility.SetDirty(go);
            }
        }
    }

    private static void LockObject(GameObject go)
    {
        if (EditorPrefs.GetBool(addUndoRedoPrefKey))
        {
            Undo.RecordObjects(new Object[] {go}, "Lock Object");
        }
        go.hideFlags |= HideFlags.NotEditable;
        foreach (Component comp in go.GetComponents(typeof(Component)))
        {
            if (!(comp is Transform))
            {
                if (EditorPrefs.GetBool(disableSelectionPrefKey))
                {
                    comp.hideFlags |= HideFlags.NotEditable;
                    comp.hideFlags |= HideFlags.HideInHierarchy;
                }
            }
        }
        EditorUtility.SetDirty(go);
    }

    private static void UnlockObject(GameObject go)
    {
        if (EditorPrefs.GetBool(addUndoRedoPrefKey))
        {
            Undo.RecordObjects(new Object[]{go}, "Unlock Object");
        }
        go.hideFlags &= ~HideFlags.NotEditable;
        foreach (Component comp in go.GetComponents(typeof(Component)))
        {
            if (!(comp is Transform))
            {
                // Don't check pref key; no harm in removing flags that aren't there
                comp.hideFlags &= ~HideFlags.NotEditable;
                comp.hideFlags &= ~HideFlags.HideInHierarchy;
            }
        }
        EditorUtility.SetDirty(go);
    }

    [MenuItem(lockMenuItem)]
    static void Lock()
    {
        foreach (var go in Selection.gameObjects)
        {
            LockObject(go);
        }
    }

    [MenuItem(lockMenuItem, true)]
    static bool CanLock()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(lockRecursivelyMenuItem)]
    static void LockRecursively()
    {
        Stack<GameObject> objectsToLock = new Stack<GameObject>(Selection.gameObjects);

        while (objectsToLock.Count > 0)
        {
            var go = objectsToLock.Pop();
            LockObject(go);

            foreach (Transform childTransform in go.transform)
            {
                objectsToLock.Push(childTransform.gameObject);
            }
        }
    }

    [MenuItem(lockRecursivelyMenuItem, true)]
    static bool CanLockRecursively()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(unlockMenuItem)]
    static void Unlock()
    {
        foreach (var go in Selection.gameObjects)
        {
            UnlockObject(go);
        }
    }

    [MenuItem(unlockMenuItem, true)]
    static bool CanUnlock()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(unlockRecursivelyMenuItem)]
    static void UnlockRecursively()
    {
        Stack<GameObject> objectsToUnlock = new Stack<GameObject>(Selection.gameObjects);

        while (objectsToUnlock.Count > 0)
        {
            var go = objectsToUnlock.Pop();
            UnlockObject(go);

            foreach (Transform childTransform in go.transform)
            {
                objectsToUnlock.Push(childTransform.gameObject);
            }
        }
    }

    [MenuItem(unlockRecursivelyMenuItem, true)]
    static bool CanUnlockRecursively()
    {
        return Selection.gameObjects.Length > 0;
    }

    [InitializeOnLoad]
    private static class HierarchyIcon
    {
        private const float _iconSize = 16f;
        private static Texture2D _icon;
        private static bool _drawingIcon;

        private static string iconFile
        {
            get
            {
                return ""; // TODO: 图标
                //return "Assets" + Directory.GetFiles(Application.dataPath, "UnityLockHierarchyIcon.png", SearchOption.AllDirectories)[0].Substring(Application.dataPath.Length).Replace('\\', '/');
            }
        }

        public static bool drawingIcon
        {
            get { return _drawingIcon; }
            set
            {
                if (_drawingIcon != value)
                {
                    _drawingIcon = value;

                    if (_drawingIcon)
                    {
                        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
                    }
                    else
                    {
                        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
                    }

                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        static HierarchyIcon()
        {
            drawingIcon = EditorPrefs.GetBool(UnityLock.showIconPrefKey);
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as UnityEngine.Object;
            if (obj && (obj.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable)
            {
                if (!_icon)
                {
                    _icon = AssetDatabase.LoadAssetAtPath(iconFile, typeof(Texture2D)) as Texture2D;
                }

                GUI.Box(new Rect(selectionRect.xMax - _iconSize, selectionRect.center.y - (_iconSize / 2f), _iconSize, _iconSize), _icon, GUIStyle.none);
            }
        }
    }
}
