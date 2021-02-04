using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KEngine;
using ILRuntime;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntimeAdapter;
using KEngine.UI;
using IType = ILRuntime.CLR.TypeSystem.ILType;

namespace KSFramework
{
    /// <summary>
    /// ILRuntime中UI的父类
    /// </summary>
    public class ILRuntimeUIBase : KEngine.UI.UIController
    {
        /// <summary>
        /// 一般编辑器模式下用于reload时用，记录上一次OnOpen的参数
        /// </summary>
        public object[] LastOnOpenArgs { get; private set; }


        private ILRuntimeUIBase instance;
        
        public override void OnInit()
        {
            if (string.IsNullOrEmpty(UITemplateName))
            {
                return;
            }
            base.OnInit();
            instance = ILRuntimeModule.Instance.appdomain.Instantiate<ILRuntimeUIBase>($"UI{UITemplateName}");//NOTE 如果有Namespace需要加上
            Debuger.Assert(instance != null,$"{UIName} is null");
            instance.gameObject = gameObject;
            instance.transform = gameObject.transform;
            instance.OnInit();
        }

        public override void BeforeOpen(object[] onOpenArgs, Action doOpen)
        {
            base.BeforeOpen(onOpenArgs, doOpen);
            Debuger.Assert(instance != null,$"{UIName} is null");
            instance.BeforeOpen(onOpenArgs,doOpen);//TODO 重复调用
        }
        
        public override void OnOpen(params object[] args)
        {
            LastOnOpenArgs = args;

            base.OnOpen(args);
            Debuger.Assert(instance != null,$"{UIName} is null");
            instance.OnOpen(args);
        }

        public override void OnClose()
        {
            base.OnClose();
            Debuger.Assert(instance != null,$"{UIName} is null");
            instance.OnClose();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Debuger.Assert(instance != null,$"{UIName} is null");
            instance.OnDestroy();
            instance = null;
        }

        public override void DisPlay(bool visiable)
        {
            if (visiable)
            {
                UIModule.Instance.OpenWindow(gameObject.name);
            }
            else
            {
                UIModule.Instance.CloseWindow(gameObject.name);
            }
        }
    }
}