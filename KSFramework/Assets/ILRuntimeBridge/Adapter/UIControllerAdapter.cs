using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeAdapter
{   
    public class UIControllerAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mOnInit_0 = new CrossBindingMethodInfo("OnInit");
        static CrossBindingMethodInfo<System.Object[], System.Action> mBeforeOpen_1 = new CrossBindingMethodInfo<System.Object[], System.Action>("BeforeOpen");
        static CrossBindingMethodInfo<System.Object[]> mOnOpen_2 = new CrossBindingMethodInfo<System.Object[]>("OnOpen");
        static CrossBindingMethodInfo mOnClose_3 = new CrossBindingMethodInfo("OnClose");
        static CrossBindingMethodInfo mOnDestroy_4 = new CrossBindingMethodInfo("OnDestroy");
        static CrossBindingMethodInfo<System.Boolean> mDisPlay_5 = new CrossBindingMethodInfo<System.Boolean>("DisPlay");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(KEngine.UI.UIController);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : KEngine.UI.UIController, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public override void OnInit()
            {
                if (mOnInit_0.CheckShouldInvokeBase(this.instance))
                    base.OnInit();
                else
                    mOnInit_0.Invoke(this.instance);
            }

            public override void BeforeOpen(System.Object[] onOpenArgs, System.Action doOpen)
            {
                if (mBeforeOpen_1.CheckShouldInvokeBase(this.instance))
                    base.BeforeOpen(onOpenArgs, doOpen);
                else
                    mBeforeOpen_1.Invoke(this.instance, onOpenArgs, doOpen);
            }

            public override void OnOpen(System.Object[] args)
            {
                if (mOnOpen_2.CheckShouldInvokeBase(this.instance))
                    base.OnOpen(args);
                else
                    mOnOpen_2.Invoke(this.instance, args);
            }

            public override void OnClose()
            {
                if (mOnClose_3.CheckShouldInvokeBase(this.instance))
                    base.OnClose();
                else
                    mOnClose_3.Invoke(this.instance);
            }

            public override void OnDestroy()
            {
                if (mOnDestroy_4.CheckShouldInvokeBase(this.instance))
                    base.OnDestroy();
                else
                    mOnDestroy_4.Invoke(this.instance);
            }

            public override void DisPlay(System.Boolean visiable)
            {
                if (mDisPlay_5.CheckShouldInvokeBase(this.instance))
                    base.DisPlay(visiable);
                else
                    mDisPlay_5.Invoke(this.instance, visiable);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

