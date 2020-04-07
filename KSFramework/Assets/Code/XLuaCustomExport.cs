using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
//using DG.Tweening;
using KEngine;
using KEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;
//using XUtils;

/// <summary>
/// xlua自定义导出
/// </summary>
public static class XLuaCustomExport
{
    /// <summary>
    /// dotween的扩展方法在lua中调用
    /// </summary>
	  /*
	[LuaCallCSharp]
    [ReflectionUse]
    public static List<Type> dotween_lua_call_cs_list = new List<Type>()
    {
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),

        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),

        typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),

        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
        typeof(DG.Tweening.ShortcutExtensions43),
        typeof(DG.Tweening.ShortcutExtensions46),
        typeof(DG.Tweening.ShortcutExtensions50),
       
        //在生成xlua的代码时以下会报错
        //typeof(DG.Tweening.DOTweenPath),
        //typeof(DG.Tweening.DOTweenVisualManager),
    }
	**/


    [LuaCallCSharp]
    public static List<Type> LuaCallCSharpUI
    {
        get
        {
            List<Type> list = new List<Type>();
            //            list.AddRange(AddNameSpaceClass<Lui.LButton>("Lui"));
            return list;
        }
    }


    public static List<Type> AddNameSpaceClass<T>(string namespaceName)
    {
        List<Type> typeList = new List<Type>();

        Assembly assembly = Assembly.GetAssembly(typeof(T));

        foreach (Type mType in assembly.GetExportedTypes())
        {
            if (mType != null && !string.IsNullOrEmpty(mType.Namespace) && mType.Namespace.Split('.')[0] == namespaceName)
            {
                typeList.Add(mType);
            }
        }
        return typeList;
    }

    [CSharpCallLua]
    public static List<Type> CSharpCallLuaList = new List<Type>()
    {
//        typeof(Action<EventX>),
        typeof(UnityAction<int, GameObject>),
        typeof(UnityAction<int, Transform>),
        typeof(AssetBundleLoader.CAssetBundleLoaderDelegate),
        typeof(Action<bool>),
        typeof(Action<int>),
        typeof(Action<float>),
        typeof(Action<bool, GameObject>),
        typeof(UnityEngine.Events.UnityEvent),
        typeof(UnityEngine.Events.UnityEvent<bool>),
        typeof(UnityEngine.Events.UnityEvent<int>),
        typeof(UnityEngine.Events.UnityEvent<float>),
        typeof(UnityEngine.Events.UnityAction<float>),
        typeof(UnityEngine.Events.UnityAction<int>),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityAction<bool>),
        typeof(Toggle.ToggleEvent),
        typeof(Dropdown.DropdownEvent),
        typeof(System.Action<GameObject>),
        typeof(UnityEngine.Events.UnityAction<Vector2>),
        typeof(KEngine.TextureLoader.CTextureLoaderDelegate),
        typeof(Action<PointerEventData>),
//        typeof(UnityEvent<Vector2>),
        //typeof(TweenCallback),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>),
        typeof(UnityEngine.Events.UnityAction<string>),

        typeof(Action),
        typeof(Func<double, double, double>),
        typeof(Action<string>),
        typeof(Action<string,int>),
        typeof(Action<int>),
        typeof(Action<double>),
        typeof(Action<Sprite>),
        typeof(AbstractResourceLoader.LoaderDelgate),
        typeof(KEngine.AssetFileLoader.AssetFileBridgeDelegate),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityAction<bool>),
        typeof(UnityEngine.Events.UnityAction<Vector2>),
        typeof(UnityEngine.Events.UnityAction<string>),
        typeof(System.Collections.IEnumerator),
        typeof(System.Action<Transform,int>),
        typeof(KEngine.SpriteLoader.CSpriteLoaderDelegate),
        typeof(System.Action<KEngine.UI.UIController, object[]>),
        typeof(UnityAction<string,string>),

    };

    [ReflectionUse]
    public static List<Type> CSharpCallLuaByReflec = new List<Type>()
    {
        typeof(System.Action<GameObject>),
        typeof(System.Action<string, string, PointerEventData>),
    };
    
    [ReflectionUse]
    public static List<Type> LuaCallCSharpByRelfect = new List<Type>()
    {
        typeof(KEngine.UI.UIModule),
        typeof(KEngineExtensions),

    };

    [LuaCallCSharp]
    public static List<Type> LuaCallCSharpList = new List<Type>()
    {
        typeof(KBehaviour),
        typeof(KEngine.Log),
        typeof(KEngine.KTool),
        typeof(KEngine.KAsync),
        typeof(KEngine.CCoroutineState),
        typeof(KBehaviour),
        typeof(System.Action<GameObject>),
        typeof(KSFramework.LuaUIController),

        //typeof(LuaBehaviour),
        typeof(KEngine.AssetFileLoader),
        typeof(KEngine.StaticAssetLoader),
        typeof(KEngine.SpriteLoader), 
        typeof(InstanceAssetLoader),
        typeof(AssetBundleLoader),
        typeof(SceneLoader),
        typeof(AssetBundle),
        typeof(KSFramework.Cookie),
        typeof(KEngine.UI.UIModule),
        typeof(UnityEngine.Physics),
#if UNITY_2018_1_OR_NEWER
        typeof(UnityEngine.Profiling.Profiler),
#else
        typeof(UnityEngine.Profiler),
#endif 
    };

    [LuaCallCSharp]
    public static List<Type> LuaCallCSharpListUnity = new List<Type>()
    {
        /*************** Unity Structs&Enums ***************/
        typeof(Vector2),
        typeof(Vector3),
        typeof(Quaternion),
        typeof(Color),
        typeof(LayerMask),
        typeof(Rect),
        typeof(KeyCode),
        typeof(Debug),
        typeof(RuntimePlatform),
        typeof(FogMode),
        typeof(LightmapsMode),
        typeof(EventSystem),
        typeof(RectTransformUtility),
        typeof(Graphic),
        typeof(Component),
        typeof(AnimatorStateInfo),
        typeof(QualitySettings),

        /***************Unity Commom***************/
        typeof(UnityEngine.Object),
        typeof(Application),
        typeof(GameObject),
        typeof(Transform),
        typeof(RectTransform),
        typeof(Time),
        typeof(WWW),
        typeof(Rigidbody),
        typeof(CharacterController),
        typeof(PlayerPrefs),
#if !UNITY_2019_1_OR_NEWER
        typeof(GUIText),
#endif
        typeof(Input),
        typeof(Renderer),
        typeof(Camera),
        typeof(Screen),
        typeof(AnimationClip),
        typeof(AnimatorCullingMode),
        typeof(RuntimeAnimatorController),
        typeof(Animator),
#if UNITY_2018_1_OR_NEWER
        typeof(UnityEngine.AI.NavMeshAgent),
        typeof(UnityEngine.AI.NavMeshPath),
        typeof(UnityEngine.AI.NavMesh),
#else
        typeof(NavMeshAgent),
        typeof(NavMeshPath),
        typeof(NavMesh),
#endif
        typeof(RaycastHit),
        typeof(Physics),
        typeof(Resources),
        typeof(Mesh),
        typeof(SkinnedMeshRenderer),
        typeof(RenderTexture),
        typeof(RenderTextureFormat),
        typeof(RenderTextureReadWrite),
        typeof(Shader),
        typeof(Collider),
        typeof(SphereCollider),
        typeof(ParticleSystem),
        typeof(RenderSettings),
        typeof(MeshFilter),
        typeof(Material),
        typeof(SpriteRenderer),
        typeof(SystemInfo),
        typeof(UnityEngine.SceneManagement.SceneManager),
        typeof(UnityEngine.SceneManagement.Scene),
        typeof(UnityEngine.Events.UnityEventBase),
        typeof(Mathf),
        typeof(StaticBatchingUtility),
        typeof(LightmapSettings),
        typeof(AudioSource),
        typeof(Color),

        /******************** UnityEngine.UI ***********************/
        typeof(Text),
        typeof(CanvasGroup),
        typeof(Canvas),
        typeof(Button),
        typeof(Toggle),
        typeof(Sprite),
        typeof(Slider),
        typeof(Image),
        typeof(RawImage),
        typeof(Dropdown),
        typeof(LayoutUtility),
        typeof(LayoutElement),
        typeof(LayoutRebuilder),
        typeof(VerticalLayoutGroup),
        typeof(GUIUtility),
        typeof( CollisionFlags ),
        typeof( MaskableGraphic ),
    };

    [GCOptimize]
    public static List<Type> LuaCallCSharpStruct = new List<Type>()
    {
        typeof(Ray),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Color),
        // typeof(AnimatorStateInfo),
        // 
    };

    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"UnityEngine.WWW", "movie"},
                new List<string>(){"UnityEngine.UI.Graphic", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.Input", "IsJoystickPreconfigured", "System.String"},
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
#if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
#endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
#if UNITY_2018_1_OR_NEWER
                       new List<string>(){ "UnityEngine.QualitySettings", "streamingMipmapsRenderersPerFrame"},
#endif
    };

    //[MenuItem("XLua/获取所有的LuaCallCSharp")]
    public static List<Type> GetLuaCallCSharpList()
    {
        //TODO 获取所有使用了LuaCallCSharp这个Attribute的类

        return null;
    }
}
