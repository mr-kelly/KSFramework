using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;


public class IComparableAdapter<T> : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IComparable<T>);
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

    internal class Adaptor : IComparable<T>, CrossBindingAdaptorType
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

        IMethod mCompareTo;
        bool mCompareToGot;
        public int CompareTo(T other)
        {
            if (!mCompareToGot)
            {
                mCompareTo = instance.Type.GetMethod("CompareTo", 1);
                mCompareToGot = true;
            }

            if (mCompareTo != null)
            {
                return (int)appdomain.Invoke(mCompareTo, instance, null);
            }
            return 0;
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

