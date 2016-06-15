using UnityEngine;
using System.Collections;
using KEngine;
using KEngine.UI;
using UnityEngine.EventSystems;

namespace KSFramework
{
    public class UGUISLuaBridge : IUIBridge
    {
        public EventSystem EventSystem;

        public void InitBridge()
        {
            EventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            EventSystem.gameObject.AddComponent<StandaloneInputModule>();
#if !UNITY_5
            EventSystem.gameObject.AddComponent<TouchInputModule>();
#else
            EventSystem.gameObject.GetComponent<StandaloneInputModule>().forceModuleActive = true;
#endif
        }

        public UIController CreateUIController(GameObject uiObj, string uiTemplateName)
        {
            UIController uiBase = uiObj.AddComponent<LuaUIController>();
            
            KEngine.Debuger.Assert(uiBase);
            return uiBase;
        }

        public void UIObjectFilter(UIController controller, GameObject uiObject)
        {
        }

        public IEnumerator LoadUIAsset(CUILoadState loadState, UILoadRequest request)
        {
            string path = string.Format("ui/{0}.prefab{1}", loadState.TemplateName, KEngine.AppEngine.GetConfig("KEngine", "AssetBundleExt"));
            var assetLoader = KStaticAssetLoader.Load(path);
            loadState.UIResourceLoader = assetLoader; // 基本不用手工释放的
            while (!assetLoader.IsCompleted)
                yield return null;

            request.Asset = assetLoader.TheAsset;
        }
    }

}
