using UnityEngine;
using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;

public class IEnumeratorAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IEnumerator);
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

    internal class Adaptor : IEnumerator, CrossBindingAdaptorType
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

        IMethod mCurrentMethod;
        public object Current
        {
            get
            {
                if (mCurrentMethod == null)
                {
                    mCurrentMethod = instance.Type.GetMethod("get_Current", 0);
                }

                if (mCurrentMethod != null)
                {
                    return appdomain.Invoke(mCurrentMethod, instance, null);
                }
                else
                {
                    return null;
                }
            }
        }

        IMethod mMoveNextMethod;
        public bool MoveNext()
        {
            if (mMoveNextMethod == null)
            {
                mMoveNextMethod = instance.Type.GetMethod("MoveNext", 0);
            }

            if (mMoveNextMethod != null)
            {
                return (bool)appdomain.Invoke(mMoveNextMethod, instance);
            }
            else
            {
                return false;
            }
        }

        IMethod mResetMethod;
        public void Reset()
        {
            if (mResetMethod == null)
            {
                mResetMethod = instance.Type.GetMethod("Reset", 0);
            }

            if (mResetMethod != null)
            {
                appdomain.Invoke(mResetMethod, instance);
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

public class IEnumeratorAdapter<T> : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IEnumerator<T>);
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

    internal class Adaptor : IEnumerator<T>, CrossBindingAdaptorType
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

        IMethod mCurrentMethod;
        public object Current
        {
            get
            {
                if (mCurrentMethod == null)
                {
                    mCurrentMethod = instance.Type.GetMethod("get_Current", 0);
                }

                if (mCurrentMethod != null)
                {
                    return appdomain.Invoke(mCurrentMethod, instance);
                }
                else
                {
                    return null;
                }
            }
        }

        IMethod mCurrentMethod2;
        T IEnumerator<T>.Current
        {
            get
            {
                if (mCurrentMethod2 == null)
                {
                    mCurrentMethod2 = instance.Type.GetMethod("System.Collections.IEnumerator.get_Current", 0);
                }

                if (mCurrentMethod2 != null)
                {
                    return (T)appdomain.Invoke(mCurrentMethod2, instance);
                }
                else
                {
                    return default(T);
                }
            }
        }

        IMethod mMoveNextMethod;
        public bool MoveNext()
        {
            if (mMoveNextMethod == null)
            {
                mMoveNextMethod = instance.Type.GetMethod("MoveNext", 0);
            }

            if (mMoveNextMethod != null)
            {
                return (bool)appdomain.Invoke(mMoveNextMethod, instance);
            }
            else
            {
                return false;
            }
        }

        IMethod mResetMethod;
        public void Reset()
        {
            if (mResetMethod == null)
            {
                mResetMethod = instance.Type.GetMethod("Reset", 0);
            }

            if (mResetMethod != null)
            {
                appdomain.Invoke(mResetMethod, instance);
            }
        }

        IMethod mDispose;
        public void Dispose()
        {
            if (mDispose == null)
            {
                mDispose = instance.Type.GetMethod("Dispose", 0);
            }

            if (mDispose != null)
            {
                appdomain.Invoke(mDispose, instance);
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
