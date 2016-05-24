using UnityEngine;
using System.Collections;
using KEngine;
using KEngine.UI;
using UnityEditor;

namespace KSFramework.Editor
{

    public class KSFrameworkEditor
    {
        /// <summary>
        /// 找到所有的LuaUIController被进行Reload
        /// 如果Reload时，UI正在打开，将对其进行关闭，并再次打开，来立刻看到效果
        /// </summary>
        [MenuItem("KEngine/UI(UGUI)/Reload UI Lua %&r")]
        public static void ReloadUI()
        {
            if (!EditorApplication.isPlaying)
            {
                KLogger.Log("Reload UI only when your editor is playing!");
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
