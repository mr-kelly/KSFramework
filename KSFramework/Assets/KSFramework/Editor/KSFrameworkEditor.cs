using UnityEngine;
using System.Diagnostics;
using System.IO;
using KEngine;
using KEngine.UI;
using KUnityEditorTools;
using UnityEditor;
using Debug = UnityEngine.Debug;
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
                //AppEngine.PreloadConfigs(true); 
                Debug.Log("TODO 运行时重载AppConfig");
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
#if xLua || SLUA
        
        [MenuItem("KEngine/UI(UGUI)/Auto Make UI Lua Scripts(Current Scene)")]
        public static void AutoMakeUILuaScripts()
        {
            var windowAssets = GameObject.FindObjectsOfType<UIWindowAsset>();
            if (windowAssets.Length > 0)
            {
                foreach (var windowAsset in windowAssets)
                {
                    var uiName = windowAsset.name;
                    var scriptPath = $"{KResourceModule.EditorProductFullPath}/{AppConfig.LuaPath}/UI/{uiName}/{uiName}.lua";
                    if (!File.Exists(scriptPath))
                    {
                        var scriptDir = Path.GetDirectoryName(scriptPath);
                        if (!string.IsNullOrEmpty(scriptDir) && Directory.Exists(scriptDir) == false)
                        {
                            Directory.CreateDirectory(scriptDir);
                        }
                        
                        File.WriteAllText(scriptPath, LuaUITempalteCode.Replace("$UI_NAME",  uiName));
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
        public static void ReloadAllUIScript()
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
            {
                ReloadUIScript(kv.Key);
            }
        }
        
        public static void ReloadUIScript(string uiName)
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }
            UILoadState state = null;
            if (UIModule.Instance.UIWindows.TryGetValue(uiName, out state))
            {
                var luaController = state.UIWindow as LuaUIController;
                if (luaController!=null)
                {
                    luaController.ClearLuaTableCache(true);
                    luaController.OnInit();
                    luaController.OnOpen(luaController.LastOnOpenArgs);
                }
            }
            else
            {
                Log.Info("UI:{0} 未打开过，无需处理",uiName);
            }
        }
        
        /// <summary>
        /// 找到所有的LuaUIController进行Reload
        /// 如果Reload时，UI正在打开，将对其进行关闭，并再次打开，来立刻看到效果
        /// </summary>
        [MenuItem("KEngine/UI(UGUI)/Reload Lua + ReOpen UI #%&r")]
        public static void ReloadAllUI()
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
            {
                ReloadUI(kv.Key);
            }
        }

        public static void ReloadUI(string uiName)
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }

            UILoadState state;
            if (UIModule.Instance.UIWindows.TryGetValue(uiName, out state))
            {
                ReloadUIAB(uiName);
                ReloadUIScript(uiName);
            }
            else
            {
                Log.Info("UI:{0} 未打开过，无需处理",uiName);
            }
        }
#endif     
        [MenuItem("KEngine/UI(UGUI)/Reload Lua + Reload UI AssetBundle")]
        public static void ReloadAllUIAB()
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }
            foreach (var kv in UIModule.Instance.UIWindows)
            {
                ReloadUIAB(kv.Key);
            }
            
        }

        public static void ReloadUIAB(string uiName)
        {
            if (!EditorApplication.isPlaying)
            {
                Log.LogError("Reload UI only when your editor is playing!");
                return;
            }

            UILoadState state = null;
            if (UIModule.Instance.UIWindows.TryGetValue(uiName, out state))
            {
                var inOpenState = UIModule.Instance.IsOpen(uiName);
                if (inOpenState)
                    UIModule.Instance.CloseWindow(uiName);
#if xLua || SLUA
                var luaController = state.UIWindow as LuaUIController;
                if (luaController != null)
                {
                    luaController.ClearLuaTableCache(true);
                }
                UIModule.Instance.ReloadWindow(uiName, (args, err) =>
                {
                    if (inOpenState)
                        UIModule.Instance.OpenWindow(uiName, luaController.LastOnOpenArgs);
                });
#elif ILRuntime
                UIModule.Instance.ReloadWindow(uiName, (args, err) =>
                {
                    if (inOpenState)
                        UIModule.Instance.OpenWindow(uiName, (state.UIWindow as ILRuntimeUIBase)?.LastOnOpenArgs);
                });
#endif

            }
            else
            {
                Log.Info("UI:{0} 未打开过，无需处理",uiName);
            }
        }
        
        /// <summary>
        /// 提供一个独立的gui工具编译excel
        /// </summary>
        [MenuItem("KEngine/Get Or Open Tableml GUI (gui compile excel)")]
        public static void GetTablemlGUI()
        {
            var path = Path.GetFullPath(Application.dataPath +"./../Product/Tableml_GUI/TableMLGUI.exe");
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                //下载tablemlgui,然后放在.\Product\目录下
                Application.OpenURL("https://github.com/zhaoqingqing/TableML/releases");
            }
        }
        
        /// <summary>
        /// unity editor引擎写入的日志
        /// </summary>
        [MenuItem("KEngine/Open Unity Editor Log")]
        public static void OpenEditorLog()
        {
            //windows10下editor log目录在：@"%USERPROFILE%\AppData\Local\Unity\Editor\"
            var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + @"\Unity\Editor";
            System.Diagnostics.Process.Start(dir);
            //if mac os see:https://answers.unity.com/questions/1484445/how-do-i-find-the-player-log-file-from-code.html
        }
        
        /// <summary>
        /// 自己写入的日志
        /// </summary>
        [MenuItem("KEngine/Open Custom Log")]
        public static void OpenCustomLog()
        {
            System.Diagnostics.Process.Start(Path.GetDirectoryName(LogFileManager.GetLogFilePath()));
        }
    }
}
