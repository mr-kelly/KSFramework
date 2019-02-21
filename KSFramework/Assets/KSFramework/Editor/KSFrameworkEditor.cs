using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using KEngine.UI;
using UnityEditor;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
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

        [MenuItem("KEngine/KSFramework Options and Help")]
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


        [MenuItem("KEngine/Open Last Scene(before main) %&o")]
        public static void OpenLastScene()
        {
            var lastScene = EditorPrefs.GetString(LastScenePrefKey);
            Log.Info("Open Last Game Scene!");
            if (!string.IsNullOrEmpty(lastScene))
            {

#if UNITY_5 || UNITY_2017_1_OR_NEWER
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

        [MenuItem("KEngine/Open Main Scene %&i")]
        public static void OpenMainScene()
        {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            var currentScene = EditorSceneManager.GetActiveScene().path;
#else
            var currentScene = EditorApplication.currentScene;
#endif
            var mainScene = "Assets/Game.unity";
            if (mainScene != currentScene)
                EditorPrefs.SetString(LastScenePrefKey, currentScene);

            Log.Info("Open Main Game Scene!");
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            EditorSceneManager.OpenScene(mainScene);
#else
            EditorApplication.OpenScene(mainScene);
#endif
        }

        [MenuItem("KEngine/UI(UGUI)/Auto Make UI Lua Scripts(Current Scene)")]
        public static void AutoMakeUILuaScripts()
        {
            var luaPath = AppEngine.GetConfig("KSFramework.Lua", "LuaPath");
            Debug.Log("Find UI from current scenes, LuaScriptPath: " + luaPath);

            var windowAssets = GameObject.FindObjectsOfType<UIWindowAsset>();
            if (windowAssets.Length > 0)
            {
                foreach (var windowAsset in windowAssets)
                {
                    var uiName = windowAsset.name;
                    var scriptPath = string.Format("{0}/{1}/UI/{2}/{3}.lua", KResourceModule.EditorProductFullPath,
                        luaPath, uiName,uiName);
                    if (!File.Exists(scriptPath))
                    {
                        var scriptDir = Path.GetDirectoryName(scriptPath);
                        if (!string.IsNullOrEmpty(scriptDir) && Directory.Exists(scriptDir) == false)
                        {
                            Directory.CreateDirectory(scriptDir);
                        }

                        File.WriteAllText(scriptPath, LuaUITempalteCode.Replace("$UI_NAME", "UI" + uiName));
                        Debug.Log("New Lua Script: " + scriptPath);
                    }
                    else
                    {
                        Debug.Log("Exists Lua Script, ignore: " + scriptPath);
                    }
                }
                
            }
            else
            {
                Debug.LogError("Not found any `UIWindowAsset` Component");
            }
        }
#if SLUA
 
        /// <summary>
        /// UI Lua Scripts Tempalte Code
        /// </summary>
        private static string LuaUITempalteCode = @"
local UIBase = import('KSFramework/UIBase')
---@type $UI_NAME
local $UI_NAME = {}
extends($UI_NAME, UIBase)

-- create a ui instance
function $UI_NAME.New(controller)
    local newUI = new($UI_NAME)
    newUI.Controller = controller
    return newUI
end

function $UI_NAME:OnInit(controller)
    Log.Info('$UI_NAME OnInit, do controls binding')
end

function $UI_NAME:OnOpen()
    Log.Info('$UI_NAME OnOpen, do your logic')
end

return $UI_NAME
";
#else
        /// <summary>
        /// UI Lua Scripts Tempalte Code
        /// </summary>
        private static string LuaUITempalteCode = @"
local UIBase = import('UI/UIBase')
---@type $UI_NAME
local $UI_NAME = {}
extends($UI_NAME, UIBase)

-- create a ui instance
function $UI_NAME.New(controller)
    local newUI = new($UI_NAME)
    newUI.Controller = controller
    return newUI
end

function $UI_NAME:OnInit(controller)
    Log.Info('$UI_NAME OnInit, do controls binding')
end

function $UI_NAME:OnOpen()
    Log.Info('$UI_NAME OnOpen, do your logic')
end

return $UI_NAME
";
#endif
        [MenuItem("KEngine/UI(UGUI)/Reload UI Lua %&r")]
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
                    luaController.ClearLuaTableCache();
                    luaController.OnOpen(luaController.LastOnOpenArgs);
                    Log.LogWarning("Reload Lua - {0}", kv.Key);
                }
            }
        }

        [MenuItem("KEngine/UI(UGUI)/Reload Lua + Reload UI AssetBundle")]
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

                    luaController.ClearLuaTableCache();
                    Log.LogWarning("Reload Lua - {0}", kv.Key);

                    UIModule.Instance.ReloadWindow(kv.Key, (args, err) =>
                    {
                        if (inOpenState)
                            UIModule.Instance.OpenWindow(kv.Key, luaController.LastOnOpenArgs);
                    });

                }
            }
            
        }
        /// <summary>
        /// 找到所有的LuaUIController被进行Reload
        /// 如果Reload时，UI正在打开，将对其进行关闭，并再次打开，来立刻看到效果
        /// </summary>
        [MenuItem("KEngine/UI(UGUI)/Reload Lua + ReOpen UI #%&r")]
        public static void ReloadUILua()
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

                    luaController.ClearLuaTableCache();
                    Log.LogWarning("Reload Lua - {0}", kv.Key);

                    if (inOpenState)
                        UIModule.Instance.OpenWindow(kv.Key, luaController.LastOnOpenArgs);
                }
            }
        }
    }
}
