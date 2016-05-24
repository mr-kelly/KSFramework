using UnityEngine;
using System.Collections;
using KEngine;
using KEngine.UI;
using UnityEditor;

namespace KSFramework.Editor
{

    public class KSFrameworkEditor
    {
        private const string LastScenePrefKey = "KSFramework.LastSceneOpen";

        [MenuItem("KEngine/KSFramework/Open Last Scene(before main) %&o")]
        public static void OpenLastScene()
        {
            var lastScene = EditorPrefs.GetString(LastScenePrefKey);
            KLogger.Log("Open Last Game Scene!");
            if (!string.IsNullOrEmpty(lastScene))
                EditorApplication.OpenScene(lastScene);
            else
            {
                KLogger.LogWarning("Not found last scene!");
            }
        }

        [MenuItem("KEngine/KSFramework/Open Main Scene %&i")]
        public static void OpenMainScene()
        {
            var mainScene = "Assets/Game.unity";
            if (mainScene != EditorApplication.currentScene)
                EditorPrefs.SetString(LastScenePrefKey, EditorApplication.currentScene);

            KLogger.Log("Open Main Game Scene!");
            EditorApplication.OpenScene(mainScene);
        }

        /// <summary>
        /// 找到所有的LuaUIController被进行Reload
        /// 如果Reload时，UI正在打开，将对其进行关闭，并再次打开，来立刻看到效果
        /// </summary>
        [MenuItem("KEngine/UI(UGUI)/Reload UI Lua %&r")]
        public static void ReloadUI()
        {
            if (!EditorApplication.isPlaying)
            {
                KLogger.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
            {
                {
                    var luaController = kv.Value.UIWindow as LuaUIController;
                    if (luaController) // 只处理LuaUIController
                    {
                        var inOpenState = UIModule.Instance.IsOpen(kv.Key);
                        if (inOpenState)
                            UIModule.Instance.CloseWindow(kv.Key);

                        luaController.ReloadLua();
                        KLogger.LogWarning("Reload Lua - {0}", kv.Key);

                        if (inOpenState)
                            UIModule.Instance.OpenWindow(kv.Key, luaController.LastOnOpenArgs);
                    }
                }
            }
        }
    }
}
