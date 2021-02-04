using UnityEngine;
using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;

namespace ILRuntimeAdapter
{
    public class MonoBehaviourAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get { return typeof(MonoBehaviour); }
        }

        public override Type AdaptorType
        {
            get { return typeof(Adaptor); }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
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

            public ILTypeInstance ILInstance
            {
                get { return instance; }
                set { instance = value; }
            }

            public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
            {
                get { return appdomain; }
                set { appdomain = value; }
            }

            IMethod mAwakeMethod;
            bool mAwakeMethodGot;

            public void Awake()
            {
                if (instance != null)
                {
                    if (!mAwakeMethodGot)
                    {
                        mAwakeMethod = instance.Type.GetMethod("Awake", 0);
                        mAwakeMethodGot = true;
                    }

                    if (mAwakeMethod != null)
                    {
                        appdomain.Invoke(mAwakeMethod, instance, null);
                    }
                }
            }

            IMethod mStartMethod;
            bool mStartMethodGot;

            void Start()
            {
                if (!mStartMethodGot)
                {
                    mStartMethod = instance.Type.GetMethod("Start", 0);
                    mStartMethodGot = true;
                }

                if (mStartMethod != null)
                {
                    appdomain.Invoke(mStartMethod, instance, null);
                }
            }

            IMethod mOnDestroyMethod;
            bool mOnDestroyMethodGot;

            void OnDestroy()
            {
                if (!mOnDestroyMethodGot)
                {
                    mOnDestroyMethod = instance.Type.GetMethod("OnDestroy", 0);
                    mOnDestroyMethodGot = true;
                }

                if (mOnDestroyMethod != null)
                {
                    appdomain.Invoke(mOnDestroyMethod, instance, null);
                }
            }

            IMethod mOnEnableMethod;
            bool mOnEnableMethodGot;

            public void OnEnable()
            {
                if (instance != null)
                {
                    if (!mOnEnableMethodGot)
                    {
                        mOnEnableMethod = instance.Type.GetMethod("OnEnable", 0);
                        mOnEnableMethodGot = true;
                    }

                    if (mOnEnableMethod != null)
                    {
                        appdomain.Invoke(mOnEnableMethod, instance, null);
                    }
                }
            }

            IMethod mOnDisableMethod;
            bool mOnDisableMethodGot;

            void OnDisable()
            {
                if (!mOnDisableMethodGot)
                {
                    mOnDisableMethod = instance.Type.GetMethod("OnDisable", 0);
                    mOnDisableMethodGot = true;
                }

                if (mOnDisableMethod != null)
                {
                    appdomain.Invoke(mOnDisableMethod, instance, null);
                }
            }

            IMethod mUpdateMethod;
            bool mUpdateMethodGot;

            void Update()
            {
                if (!mUpdateMethodGot)
                {
                    mUpdateMethod = instance.Type.GetMethod("Update", 0);
                    mUpdateMethodGot = true;
                }

                if (mUpdateMethod != null)
                {
                    appdomain.Invoke(mUpdateMethod, instance, null);
                }
            }

            IMethod mFixedUpdateMethod;
            bool mFixedUpdateMethodGot;

            void FixedUpdate()
            {
                if (!mFixedUpdateMethodGot)
                {
                    mFixedUpdateMethod = instance.Type.GetMethod("FixedUpdate", 0);
                    mFixedUpdateMethodGot = true;
                }

                if (mFixedUpdateMethod != null)
                {
                    appdomain.Invoke(mFixedUpdateMethod, instance, null);
                }
            }

            IMethod mLateUpdateMethod;
            bool mLateUpdateMethodGot;

            void LateUpdate()
            {
                if (!mLateUpdateMethodGot)
                {
                    mLateUpdateMethod = instance.Type.GetMethod("LateUpdate", 0);
                    mLateUpdateMethodGot = true;
                }

                if (mLateUpdateMethod != null)
                {
                    appdomain.Invoke(mLateUpdateMethod, instance, null);
                }
            }

#if UNITY_EDITOR
            IMethod mOnGUIMethod;
            bool mOnGUIMethodGot;

            void OnGUI()
            {
                if (!mOnGUIMethodGot)
                {
                    mOnGUIMethod = instance.Type.GetMethod("OnGUI", 0);
                    mOnGUIMethodGot = true;
                }

                if (mOnGUIMethod != null)
                {
                    appdomain.Invoke(mOnGUIMethod, instance, null);
                }
            }

            IMethod mOnDrawGizmosMethod;
            bool mOnDrawGizmosMethodGot;
            void OnDrawGizmos()
            {
                if (!mOnDrawGizmosMethodGot)
                {
                    mOnDrawGizmosMethod = instance.Type.GetMethod("OnDrawGizmos", 0);
                    mOnDrawGizmosMethodGot = true;
                }

                if (mOnDrawGizmosMethod != null)
                {
                    appdomain.Invoke(mOnDrawGizmosMethod, instance, null);
                }
            }
#endif

            object[] param = new object[1];
            IMethod mOnApplicationFocusMethod;
            bool mOnApplicationFocusMethodGot;

            void OnApplicationFocus(bool b)
            {
                if (!mOnApplicationFocusMethodGot)
                {
                    mOnApplicationFocusMethod = instance.Type.GetMethod("OnApplicationFocus", 1);
                    mOnApplicationFocusMethodGot = true;
                }

                if (mOnApplicationFocusMethod != null)
                {
                    param[0] = b;
                    appdomain.Invoke(mOnApplicationFocusMethod, instance, param);
                }
            }

            IMethod mOnApplicationPauseMethod;
            bool mOnApplicationPauseMethodGot;

            void OnApplicationPause(bool b)
            {
                if (!mOnApplicationPauseMethodGot)
                {
                    mOnApplicationPauseMethod = instance.Type.GetMethod("OnApplicationPause", 1);
                    mOnApplicationPauseMethodGot = true;
                }

                if (mOnApplicationPauseMethod != null)
                {
                    param[0] = b;
                    appdomain.Invoke(mOnApplicationPauseMethod, instance, param);
                }
            }

            IMethod mOnApplicationQuitMethod;
            bool mOnApplicationQuitMethodGot;

            void OnApplicationQuit()
            {
                if (!mOnApplicationQuitMethodGot)
                {
                    mOnApplicationQuitMethod = instance.Type.GetMethod("OnApplicationQuit", 0);
                    mOnApplicationQuitMethodGot = true;
                }

                if (mOnApplicationQuitMethod != null)
                {
                    appdomain.Invoke(mOnApplicationQuitMethod, instance, null);
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
}