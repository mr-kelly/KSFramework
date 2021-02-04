using ILRuntime.Runtime.Enviorment;
using System;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;

public class IDisposableAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IDisposable);
        }
    }

    public override Type AdaptorType
    {
        get
        {
            return typeof(Adaptor);
        }
    }

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    internal class Adaptor : IDisposable, CrossBindingAdaptorType
    {
        ILTypeInstance instance;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        public Adaptor()
        {

        }

        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            this.appdomain = appdomain;
            this.instance = instance;
        }

        public ILTypeInstance ILInstance { get { return instance; } }

        IMethod mDisposeMethod;
        bool mDisposeMethodGot;
        public void Dispose()
        {
            if (!mDisposeMethodGot)
            {
                mDisposeMethod = instance.Type.GetMethod("Dispose", 0);
                mDisposeMethodGot = true;
            }

            if (mDisposeMethod != null)
            {
                appdomain.Invoke(mDisposeMethod, instance, null);
            }
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