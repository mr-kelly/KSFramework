using UnityEngine;
using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;

public class IEnumerableAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IEnumerable);
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

    internal class Adaptor : IEnumerable, CrossBindingAdaptorType
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


        IMethod mGetEnumeratorMethod;
        bool mGetEnumeratorMethodGot;
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!mGetEnumeratorMethodGot)
            {
                mGetEnumeratorMethod = instance.Type.GetMethod("System.Collections.IEnumerable.GetEnumerator", 0);
                mGetEnumeratorMethodGot = true;
            }

            if (mGetEnumeratorMethod != null)
            {
                return appdomain.Invoke(mGetEnumeratorMethod, instance, null) as IEnumerator;
            }
            return null;
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

public class IEnumerableAdapter<T> : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IEnumerable<T>);
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

    internal class Adaptor : IEnumerable<T>, CrossBindingAdaptorType
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


        IMethod mGetEnumeratorMethod;
        bool mGetEnumeratorMethodGot;
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!mGetEnumeratorMethodGot)
            {
                mGetEnumeratorMethod = instance.Type.GetMethod("System.Collections.IEnumerable.GetEnumerator", 0);
                mGetEnumeratorMethodGot = true;
            }

            if (mGetEnumeratorMethod != null)
            {
                return appdomain.Invoke(mGetEnumeratorMethod, instance, null) as IEnumerator;
            }
            return null;
        }

        IMethod mGetEnumeratorTMethod;
        bool mGetEnumeratorTMethodGot;
        public IEnumerator<T> GetEnumerator()
        {
            if (!mGetEnumeratorTMethodGot)
            {
                mGetEnumeratorTMethod = instance.Type.GetMethod("System.Collections.IEnumerable`1.GetEnumerator", 0);
                mGetEnumeratorTMethodGot = true;
            }

            if (mGetEnumeratorTMethod != null)
            {
                return appdomain.Invoke(mGetEnumeratorTMethod, instance, null) as IEnumerator<T>;
            }
            return null;
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