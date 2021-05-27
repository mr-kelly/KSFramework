using DotLiquid.Tags;
using KSFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// Author：qingqing.zhao (569032731@qq.com)
    /// Date：2021/5/27 15:02
    /// Desc：扩展Unity的Text Inspector
    /// </summary>
    [CustomEditor(typeof(MyText), true)]
    [CanEditMultipleObjects]
    public class MyTextEditor : UnityEditor.UI.TextEditor
    {
        private string[] args;
        private SerializedProperty UseLangId, LangId, paramArr, m_Text;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("m_Text");

            UseLangId = serializedObject.FindProperty("UseLangId");
            LangId = serializedObject.FindProperty("LangId");
            paramArr = serializedObject.FindProperty("LangParams");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UseLangId.boolValue = EditorGUILayout.Toggle("使用语言包:", UseLangId.boolValue);

            if (UseLangId.boolValue)
            {
                LangId.stringValue = EditorGUILayout.TextField("语言包id:", LangId.stringValue);
                if (!string.IsNullOrEmpty(LangId.stringValue))
                {
                    paramArr.arraySize = EditorGUILayout.DelayedIntField("语言包参数个数:", paramArr.arraySize);
                    args = new string[paramArr.arraySize];
                    
                    ++EditorGUI.indentLevel;
                    for (int i = 0; i < paramArr.arraySize; i++)
                    {
                        paramArr.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(paramArr.GetArrayElementAtIndex(i).stringValue);
                        args[i] = paramArr.GetArrayElementAtIndex(i).stringValue;
                    }

                    --EditorGUI.indentLevel;

                    var str = I18N.Get(LangId.stringValue, args);
                    if (str != null && str.IndexOf("lang_id:") < 0)
                    {
                        m_Text.stringValue = str;
                    }
                    else
                    {
                        m_Text.stringValue = "";
                        EditorGUILayout.HelpBox($"语言包id:{LangId.stringValue}不存在!", MessageType.Error);
                    }
                }
                else
                {
                    m_Text.stringValue = "";
                    EditorGUILayout.HelpBox("请输入语言包id", MessageType.Error);
                }

                if (GUILayout.Button("刷新语言包"))
                {
                    I18N.ReLoad();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("请从语言包读取文本", MessageType.Warning);
            }


            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}