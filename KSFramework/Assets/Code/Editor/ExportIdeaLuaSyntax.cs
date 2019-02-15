using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CSObjectWrapEditor;
using XLua;
using System.Reflection;
using System.IO;
using System.Linq;
using System;
//using ICSharpCode.SharpZipLib.Zip;

/// <summary>
/// intellij idea智能提示
/// form  w90ang@163.com(408880935)
/// 使用说明：https://www.cnblogs.com/zhaoqingqing/p/7719376.html
/// 1. 点击KEngine/Tools/生成Lua代码提示 2.在IDEA中设置添加UnityAPI目录的引用
/// </summary>
public class ExportIdeaLuaSyntax
{
    /// <summary>
    /// 导出方法类型
    /// </summary>
    enum GenMethodType
    {
        /// <summary>
        /// 静态方法
        /// </summary>
        Static,
        /// <summary>
        /// 实例方法
        /// </summary>
        Instance,
        /// <summary>
        /// 扩展方法
        /// </summary>
        Extension,
    }
    /// <summary>
    /// wrap文件生成文件夹
    /// </summary>
    public static string IntellijLuaWrapPath = Application.dataPath + "/../UnityAPI/";
    /// <summary>
    /// zip文件路径
    /// </summary>
    public static string IntellijLuaWrapZIPPath = Application.dataPath + "/../UnityAPI_xLua.zip";

    /// <summary>
    /// 从xlua copy的含有扩展方法类型列表
    /// </summary>
    static IEnumerable<Type> s_type_has_extension_methods = null;

    static IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType)
    {
        if(s_type_has_extension_methods == null)
        {
            var gen_types = Generator.LuaCallCSharp;

            s_type_has_extension_methods = from type in gen_types
                                         where type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                .Any(method => Utils.IsSupportedMethod(method))
                                         select type;
        }
        return from type in s_type_has_extension_methods
               where type.IsSealed && !type.IsGenericType && !type.IsNested
               from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
               where Generator.isSupportedExtensionMethod(method, extendedType)
               select method;
    }
    
    static bool isSupportedGenericMethod(MethodInfo method)
    {
        if(!method.ContainsGenericParameters)
            return true;
        var methodParameters = method.GetParameters();
        var hasValidGenericParameter = false;
        for(var i = 0; i < methodParameters.Length; i++)
        {
            var parameterType = methodParameters[i].ParameterType;
            if(parameterType.IsGenericParameter)
            {
                var parameterConstraints = parameterType.GetGenericParameterConstraints();
                if(parameterConstraints.Length == 0 || !parameterConstraints[0].IsClass)
                    return false;
                hasValidGenericParameter = true;
            }
        }
        return hasValidGenericParameter;
    }

    static bool isObsolete(MemberInfo mb)
    {
//        if(mb == null)
//            return false;
//        return mb.IsDefined(typeof(System.ObsoleteAttribute), false);
        return false; // by qingqing.zhao 方法都生成出来
    }

    static bool isMemberInBlackList(MemberInfo mb)
    {
        if(mb.IsDefined(typeof(BlackListAttribute), false))
            return true;

        foreach(var exclude in Generator.BlackList)
        {
            if(mb.DeclaringType.FullName == exclude[0] && mb.Name == exclude[1])
            {
                return true;
            }
        }

        return false;
    }

    static bool isMethodInBlackList(MethodBase mb)
    {
        if(mb.IsDefined(typeof(BlackListAttribute), false))
            return true;

        foreach(var exclude in Generator.BlackList)
        {
            if(mb.DeclaringType.FullName == exclude[0] && mb.Name == exclude[1])
            {
                var parameters = mb.GetParameters();
                if(parameters.Length != exclude.Count - 2)
                {
                    continue;
                }
                bool paramsMatch = true;

                for(int i = 0; i < parameters.Length; i++)
                {
                    if(parameters[i].ParameterType.FullName != exclude[i + 2])
                    {
                        paramsMatch = false;
                        break;
                    }
                }
                if(paramsMatch)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取类型的文件名
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    static string GetTypeFileName(Type type)
    {
        return type.ToString().Replace("+", "").Replace(".", "").Replace("`", "").Replace("&", "").Replace("[", "").Replace("]", "").Replace(",", "");
    }

    /// <summary>
    /// 获取类型标签名
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    static string GetTypeTagName(Type type)
    {
        return type.ToString().Replace("+", "").Replace("`", "").Replace("&", "").Replace("[", "").Replace("]", "").Replace(",", "");
    }

    /// <summary>
    /// 生成代码提示
    /// </summary>
    [MenuItem("KEngine/Tools/为IntelliJ Idea生成代码提示", false, 10)]
    public static void GenIntellijLuaWrap()
    {
        s_type_has_extension_methods = null;
        Generator.GetGenConfig(Utils.GetAllTypes());
        if(!Directory.Exists(IntellijLuaWrapPath))
        {
            Directory.CreateDirectory(IntellijLuaWrapPath);
        }
        for(int i = 0, iMax = Generator.LuaCallCSharp.Count; i < iMax; i++)
        {
            var type = Generator.LuaCallCSharp[i];
            string wrapPath = GetTypeFileName(type) + "_Wrap.lua";
            EditorUtility.DisplayProgressBar("操作中", "生成" + wrapPath, (i + 1) / (float)iMax);
            if(!type.IsSubclassOf(typeof(Attribute)))
            {//属性不导出
                using(StreamWriter writer = new StreamWriter(IntellijLuaWrapPath + wrapPath, false, System.Text.Encoding.UTF8))
                {
                    writer.Write("---@class ");
                    writer.Write(GetTypeTagName(type));
                    if(type.IsEnum)
                    {
                        writer.Write("\r\n");
                        foreach(var item in Enum.GetValues(type))
                        {
                            writer.Write("---@field ");
                            writer.Write(item);
                            writer.Write(" @");
                            writer.WriteLine((int)item);
                        }
                        writer.WriteLine("local m = {};");
                    }
                    else
                    {
                        var baseType = type.BaseType;
                        if(baseType != null && baseType != typeof(object))
                        {
                            writer.Write(" : ");
                            writer.WriteLine(GetTypeTagName(baseType));
                        }
                        else
                        {
                            writer.WriteLine();
                        }
                        WriteFields(writer, type);
                        WriteProperties(writer, type);
                        writer.WriteLine("local m = {};");
                        WriteMethods(writer, type);
                    }
                    writer.Write("return m;");
                }
            }
        }
        //WriteZIP();
        s_type_has_extension_methods = null;
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("KEngine/Tools/清除为IntelliJ Idea生成的代码提示", false, 11)]
    public static void ClearIntellijLuaWrap()
    {
        if(Directory.Exists(IntellijLuaWrapPath))
        {
            Directory.Delete(IntellijLuaWrapPath, true);
        }
        if(File.Exists(IntellijLuaWrapZIPPath))
        {
            File.Delete(IntellijLuaWrapZIPPath);
        }
    }

    static void WriteFields(StreamWriter writer, Type type)
    {
        var staticItems = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static).Where((item) =>
        {
            return !isObsolete(item) && !isMemberInBlackList(item);
        });
        var instanceItems = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Where((item) =>
        {
            return !isObsolete(item) && !isMemberInBlackList(item);
        });
        WriteFields(writer, staticItems, true);
        WriteFields(writer, instanceItems, false);
    }

    static void WriteFields(StreamWriter writer, IEnumerable<FieldInfo> fields, bool isStatic)
    {
        bool writedHead = false;
        foreach(var item in fields)
        {
            if(!writedHead)
            {
                writer.WriteLine(isStatic ? "---static fields" : "---instance fields");
                writedHead = true;
            }
            writer.Write("---@field public ");
            writer.Write(item.Name);
            writer.Write(' ');
            writer.WriteLine(GetTypeTagName(item.FieldType));
        }
    }

    static void WriteProperties(StreamWriter writer, Type type)
    {
        var staticItems = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static).Where((item) =>
        {
            return !isObsolete(item) && !isMemberInBlackList(item);
        });
        var instanceItems = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Where((item) =>
        {
            return !isObsolete(item) && !isMemberInBlackList(item);
        });
        WriteProperties(writer, staticItems, true);
        WriteProperties(writer, instanceItems, false);
    }

    static void WriteProperties(StreamWriter writer, IEnumerable<PropertyInfo> properties, bool isStatic)
    {
        bool writedHead = false;
        foreach(var item in properties)
        {
            if(!writedHead)
            {
                writer.WriteLine(isStatic ? "---static properties" : "---instance properties");
                writedHead = true;
            }
            writer.Write("---@field public ");
            writer.Write(item.Name);
            writer.Write(' ');
            writer.WriteLine(GetTypeTagName(item.PropertyType));
        }
    }

    static void WriteMethods(StreamWriter writer, Type type)
    {
        var staticItems = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static).Where((item) =>
        {
            return !isObsolete(item) && !isMethodInBlackList(item);
        });
        var instanceItems = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance).Where((item) =>
        {
            return !isObsolete(item) && !isMethodInBlackList(item);
        });
        WriteMethods(writer, staticItems, GenMethodType.Static);
        writer.WriteLine();
        WriteMethods(writer, instanceItems, GenMethodType.Instance);
        var extensionItems = GetExtensionMethods(type);
//        var extensionItems = Generator.GetExtensionMethods(type);
        WriteMethods(writer, extensionItems, GenMethodType.Extension);
    }

    static void WriteMethods(StreamWriter writer, IEnumerable<MethodInfo> methods, GenMethodType methodType)
    {
        bool wroteHead = false;
        foreach(var item in methods)
        {
            var methodName = item.Name;
            if(methodName.StartsWith("get_") || methodName.StartsWith("set_"))
            {
                continue;
            }
            if(item.IsGenericMethod && !isSupportedGenericMethod(item))
            {
                continue;
            }
            if(methodType == GenMethodType.Extension)
            {
                if(!wroteHead)
                {
                    writer.WriteLine();
                    writer.WriteLine("---extension methods");
                    wroteHead = true;
                }
            }
            var paramArray = item.GetParameters();
            for(int i = 0, iMax = paramArray.Length; i < iMax; i++)
            {
                if(i == 0 && methodType == GenMethodType.Extension)
                {
                    continue;
                }
                var param = paramArray[i];
                writer.Write("---@param ");
                writer.Write(param.Name);
                writer.Write(' ');
                writer.Write(GetTypeTagName(param.ParameterType));
                if((param.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
                {
                    writer.Write(" @default_value:");
                    writer.WriteLine(param.DefaultValue);
                }
                else if(param.IsOut)
                {
                    writer.WriteLine(" @out");
                }
                else if(param.IsIn)
                {
                    writer.WriteLine(" @in");
                }
                else
                {
                    writer.WriteLine();
                }
            }
            if(item.ReturnType != typeof(void))
            {
                writer.Write("---@return ");
                writer.WriteLine(GetTypeTagName(item.ReturnType));
            }
            writer.Write("function m");
            writer.Write(methodType == GenMethodType.Static ? "." : ":");
            writer.Write(item.Name);
            writer.Write("(");
            for(int i = 0, iMax = paramArray.Length; i < iMax; i++)
            {
                if(i == 0 && methodType == GenMethodType.Extension)
                {
                    continue;
                }
                writer.Write(paramArray[i].Name);
                if(i < iMax - 1)
                {
                    writer.Write(", ");
                }
            }
            writer.WriteLine(") end");
        }
    }

    /*static void WriteZIP()
    {
        //不用压缩成zip也可以
        DirectoryInfo wrapDir = new DirectoryInfo(IntellijLuaWrapPath);
        if(wrapDir.Exists)
        {
            using(ZipFile zipFile = ZipFile.Create(IntellijLuaWrapZIPPath))
            {
                zipFile.BeginUpdate();
                var fileInfos = wrapDir.GetFiles("*", SearchOption.AllDirectories);
                var dirName = wrapDir.FullName;
                foreach(var fileInfo in fileInfos)
                {
                    string fileFullName = fileInfo.FullName;
                    string entryName;
                    int index = fileFullName.IndexOf(dirName, StringComparison.OrdinalIgnoreCase);
                    if(index > -1)
                        entryName = fileFullName.Substring(index + dirName.Length);
                    else
                        entryName = fileFullName;
                    zipFile.Add(fileInfo.FullName, entryName);
                }
                zipFile.CommitUpdate();
            }
        }
    }*/
}
