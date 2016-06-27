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

    public class KSFrameworkEditor : EditorWindow
    {
        private const string LastScenePrefKey = "KSFramework.LastSceneOpen";

        private static KSFrameworkEditor Instance;

        private static string HelpText = string.Format(@"
KSFramework {0} <https://github.com/mr-kelly/KSFramework>

Shorcuts:
    Ctrl+Alt+B - Quick Build All Assets to AssetBundles in BundleResources folder
    Ctrl+Alt+R - Reload current cached lua scripts


",KSFrameworkInfo.Version);

        [MenuItem("KSFramework/Options and Help")]
        private static void Open()
        {
            // Get existing open window or if none, make a new one:

            if (Instance == null)
            {
                Instance = GetWindow<KSFrameworkEditor>(true, "KSFramework Options");
            }
            Instance.Show();
        }

        void OnGUI()
        {
            GUILayout.Label(HelpText);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reload AppConfigs.txt");
            if (GUILayout.Button("Reload"))
            {
                AppEngine.PreloadConfigs(true);
                Debug.Log("Reload AppConfigs.txt!");
            }
            EditorGUILayout.EndHorizontal();
        }


        [MenuItem("KSFramework/Open Last Scene(before main) %&o")]
        public static void OpenLastScene()
        {
            var lastScene = EditorPrefs.GetString(LastScenePrefKey);
            Log.Info("Open Last Game Scene!");
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
                Log.LogWarning("Not found last scene!");
            }
        }

        [MenuItem("KSFramework/Open Main Scene %&i")]
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

            Log.Info("Open Main Game Scene!");
#if UNITY_5
            EditorSceneManager.OpenScene(mainScene);
#else
            EditorApplication.OpenScene(mainScene);
#endif
        }

        [MenuItem("KSFramework/UI/Reload UI Lua %&r")]
        public static void ReloadLuaCache()
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
            {
                var luaController = kv.Value.UIWindow as LuaUIController;
                if (luaController) // 只处理LuaUIController
                {
                    luaController.ReloadLua();
                    Log.LogWarning("Reload Lua - {0}", kv.Key);
                }
            }
        }
        /// <summary>
        /// 找到所有的LuaUIController被进行Reload
        /// 如果Reload时，UI正在打开，将对其进行关闭，并再次打开，来立刻看到效果
        /// </summary>
        [MenuItem("KSFramework/UI/Reload Lua + ReOpen UI #%&r")]
        public static void ReloadUI()
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
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
                    Log.LogWarning("Reload Lua - {0}", kv.Key);

                    if (inOpenState)
                        UIModule.Instance.OpenWindow(kv.Key, luaController.LastOnOpenArgs);
                }
            }
        }
    }
}
