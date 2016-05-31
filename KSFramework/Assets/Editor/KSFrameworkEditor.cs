using UnityEngine;
using System.Collections;
using KEngine;
using KEngine.UI;
using UnityEditor;
#if UNITY_5
using UnityEditor.SceneManagement;
#endif

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
            {

#if UNITY_5
                EditorSceneManager.OpenScene(lastScene);
#else
                EditorApplication.OpenScene(lastScene);
#endif

            }
            else
            {
                KLogger.LogWarning("Not found last scene!");
            }
        }

        [MenuItem("KEngine/KSFramework/Open Main Scene %&i")]
        public static void OpenMainScene()
        {
#if UNITY_5
            var currentScene = EditorSceneManager.GetActiveScene().path;
#else
            var currentScene = EditorApplication.currentScene;
#endif
            var mainScene = "Assets/Game.unity";
            if (mainScene != currentScene)
                EditorPrefs.SetString(LastScenePrefKey, currentScene);

            KLogger.Log("Open Main Game Scene!");
#if UNITY_5
            EditorSceneManager.OpenScene(mainScene);
#else
            EditorApplication.OpenScene(mainScene);
#endif
        }

        [MenuItem("KEngine/UI(UGUI)/Reload UI Lua %&r")]
        public static void ReloadLuaCache()
        {
            if (!EditorApplication.isPlaying)
            {
                KLogger.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
            {
                var luaController = kv.Value.UIWindow as LuaUIController;
                if (luaController) // 只处理LuaUIController
                {
                    luaController.ReloadLua();
                    KLogger.LogWarning("Reload Lua - {0}", kv.Key);
                }
            }
        }
        /// <summary>
        /// 找到所有的LuaUIController被进行Reload
        /// 如果Reload时，UI正在打开，将对其进行关闭，并再次打开，来立刻看到效果
        /// </summary>
        [MenuItem("KEngine/UI(UGUI)/Reload Lua + ReOpen UI #%&r")]
        public static void ReloadUI()
        {
            if (!EditorApplication.isPlaying)
            {
                KLogger.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
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
