using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntimeAdapter;
using KEngine;
using KSFramework;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;

/// <summary>
/// ILRuntime模块
///     实际项目中可以在进到选角/选服后再加载热更的dll，提高启动速度
/// </summary>
public class ILRuntimeModule : IModuleInitable
{
    public static ILRuntimeModule Instance = new ILRuntimeModule();

    /// <summary>
    /// AppDomain是ILRuntime的入口，整个游戏全局唯一
    /// </summary>
    public AppDomain appdomain;

    System.IO.MemoryStream fs;
    System.IO.MemoryStream p;

    protected ILRuntimeModule()
    {
    }

    public IEnumerator Init()
    {
        LoadHotFixAssembly();
        yield return null;
    }

    public double InitProgress { get; }

    void LoadHotFixAssembly()
    {
        //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        byte[] dll = null, pdb = null;
        //if (AppConfig.UseMobilePath)
        {
            dll = KResourceModule.LoadAssetsSync(KResourceModule.BundlesPathRelative + "/HotFix_Project.dll");
            //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
            pdb = KResourceModule.LoadAssetsSync(KResourceModule.BundlesPathRelative + "/HotFix_Project.pdb");
        }

        fs = new MemoryStream(dll);
        p = new MemoryStream(pdb);
        try
        {
            appdomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch (Exception ex)
        {
            Log.LogError($"加载热更DLL失败，Message:{ex.Message}");
        }

        InitializeILRuntime(appdomain);
        OnHotFixLoaded();
    }

    public void InitializeILRuntime(AppDomain appdomain)
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        SetupCLRRedirection();
        
        InitBinders(appdomain);
		//初始化CLR绑定请放在初始化的最后一步
		ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
#if UNITY_EDITOR //打开调试端口
        appdomain.DebugService.StartDebugService(56000);
#endif
    }

    void OnHotFixLoaded()
    {
        //调用hotfix的入口
        appdomain.Invoke("LogicApp", "Start", null, null);
    }

    public void OnDestroy()
    {
        if (fs != null)
            fs.Close();
        if (p != null)
            p.Close();
        fs = null;
        p = null;
    }

    public void InitBinders(AppDomain app)
    {
        //TODO 这里做一些ILRuntime的注册
        //注册Adapter
        Assembly assembly = typeof(KSGame).Assembly;
        var types = assembly.GetTypes();
        var baseType = typeof(CrossBindingAdaptor);
        foreach (Type type in types)
        {
            // if (type.Namespace != null && type.Namespace == "ILRuntimeAdapter")
            if (type.BaseType == baseType && !type.IsGenericType) //过滤掉泛型
            {
                var instance = Activator.CreateInstance(type);
                var adaptor = instance as CrossBindingAdaptor;
                if (adaptor != null)
                {
                    app.RegisterCrossBindingAdaptor(adaptor);
                }
            }
        }

        app.RegisterCrossBindingAdaptor(new IEnumerableAdapter<byte>());
        app.RegisterCrossBindingAdaptor(new IEnumerableAdapter<int>());
        app.RegisterCrossBindingAdaptor(new IEnumerableAdapter<ILTypeInstance>());
        app.RegisterCrossBindingAdaptor(new IEnumeratorAdapter<ILTypeInstance>());
        //app.RegisterCrossBindingAdaptor(new IEnumeratorAdapter<System.Object>());
        app.RegisterCrossBindingAdaptor(new IComparableAdapter<ILTypeInstance>());
        //注册值类型Binder
        app.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        app.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        app.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());

        //注册delegate
        app.DelegateManager.RegisterMethodDelegate<byte[]>();
        app.DelegateManager.RegisterMethodDelegate<bool>();
        app.DelegateManager.RegisterMethodDelegate<int>();
        app.DelegateManager.RegisterMethodDelegate<object>();
        app.DelegateManager.RegisterMethodDelegate<object[]>();
        app.DelegateManager.RegisterMethodDelegate<GameObject>();
        app.DelegateManager.RegisterMethodDelegate<MemoryStream>();
        //带返回值的委托的话需要用RegisterFunctionDelegate，返回类型为最后一个
        app.DelegateManager.RegisterFunctionDelegate<int, string>();
        //Action<string> 的参数为一个string
        app.DelegateManager.RegisterMethodDelegate<string>();

        app.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) => { return new UnityEngine.Events.UnityAction(() => { ((Action) act)(); }); });
        app.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<float>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<float>((a) => { ((Action<float>) act)(a); });
        });

        app.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<Vector2>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<Vector2>((obj) => { ((Action<Vector2>) act)(obj); });
        });
        app.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<Vector3>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<Vector3>((obj) => { ((Action<Vector3>) act)(obj); });
        });
        app.DelegateManager.RegisterDelegateConvertor<Action>((action) => { return new Action(() => { ((Action) action)(); }); });
        app.DelegateManager.RegisterDelegateConvertor<Action<object>>((action) => { return new Action<object>((args) => { ((Action<object>) action)(args); }); });
        app.DelegateManager.RegisterDelegateConvertor<Action<object[]>>((action) => { return new Action<object[]>((args) => { ((Action<object[]>) action)(args); }); });
        app.DelegateManager.RegisterDelegateConvertor<Action<GameObject>>((action) => { return new Action<GameObject>((go) => { ((Action<GameObject>) action)(go); }); });
        app.DelegateManager.RegisterDelegateConvertor<Action<MonoBehaviour>>((act) =>
        {
            return new Action<MonoBehaviour>((behaviour) => { ((Action<MonoBehaviour>) act)(behaviour); });
        });
    }

    #region GetCompoent重定向

    unsafe void SetupCLRRedirection()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
            {
                appdomain.RegisterCLRMethodRedirection(i, GetComponent);
            }
            else if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                appdomain.RegisterCLRMethodRedirection(i, AddComponent);
            }
        }
    }

    unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                var ilInstance = new ILTypeInstance(type as ILType, false); //手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                //接下来创建Adapter实例
                var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance

                clrInstance.Awake(); //因为Unity调用这个方法时还没准备好所以这里补调一次
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                for (int i = 0; i < clrInstances.Length; i++)
                {
                    var clrInstance = clrInstances[i];
                    if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    #endregion
}