using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2020/12/4 
/// Desc：app常量配置
/// </summary>
public class AppConfig
{
    /// <summary>
    /// 编辑器下默认不打印硬件设备的信息，真机写入到日志中
    /// </summary>
    public static bool IsLogDeviceInfo = false;
    /// <summary>
    /// 是否打印ab加载的日志
    /// </summary>
    public static bool IsLogAbInfo = false;
    /// <summary>
    /// 是否打印ab加载耗时
    /// </summary>
    public static bool IsLogAbLoadCost = false;
    /// <summary>
    /// 是否记录到文件中，包括：UI的ab加载耗时，UI函数执行耗时
    /// </summary>
    public static bool IsSaveCostToFile = false;
    /// <summary>
    /// 是否调试模式，可与Unity的Debug区分开
    /// </summary>
    public static bool IsDebugBuild = Debug.isDebugBuild;
    /// <summary>
    /// 是否创建AssetDebugger
    /// </summary>
    public static bool UseAssetDebugger = Application.isEditor;

    #region AppConfig.txt中的内容
    //TODO 制作一个edit工具，可在运行时修改值
    

    #endregion
}