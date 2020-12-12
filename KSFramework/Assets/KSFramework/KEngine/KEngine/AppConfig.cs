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

    //TODO 制作一个edit工具，可在运行时修改值，
    /**
     * 1.如果需要根据打包平台进行定制，可替换cs文件中字符串的值
     * 2.路径都是相对于Product目录
     **/
    /// <summary>
    /// 打包ab文件的后缀
    /// </summary>
    public const string AssetBundleExt = ".ab";
    //
    /// <summary>
    /// Product目录的相对路径
    /// </summary>
    public const string ProductRelPath = "Product";

    public const string AssetBundleBuildRelPath = "Product/Bundles";
    public const string StreamingBundlesFolderName = "Bundles";

    /// <summary>
    /// lua源代码文件目录，相对于Product目录
    /// </summary>
    public const string LuaPath = "Lua";

    public static bool IsLoadAssetBundle = true;

    //whether use assetdata.loadassetatpath insead of load asset bundle, editor only
    public static bool IsEditorLoadAsset = false;

    /// <summary>
    /// 配置表编译出文件的后缀名, 可修改
    /// </summary>
    public const string SettingExt = ".tsv";

    //; config use lua  or c# + tsv
    public static bool IsUseLuaConfig = false;
    public const string SettingSourcePath = "Product/SettingSource";
    public const string ExportLuaPath = "Lua/configs";
    public const string ExportTsvPath = "Product/Setting";
    public const string ExportCSharpPath = "Assets/Code/AppSettings/";


    //;Folder in Resources
    public const string SettingResourcesPath = "Setting";

    //; Ignore genereate code for these excel.
    public const string SettingCodeIgnorePattern = "(I18N/.*)|(StringsTable.*)";

    /// <summary>
    /// 支持C#/lua来编写UI代码，Lua中基类是LuaUIControl，C#是UIControl
    /// </summary>
    public const string UIModuleBridge = "LuaBridge";

    /// <summary>
    /// UI设计的分辨率
    /// </summary>
    public static Vector2 UIResolution = new Vector2(1280, 720);

    /// <summary>
    /// 游戏共有多少种语言
    /// </summary>
    public static string[] I18NLanguages = new string[] {"zh_CN", "en_US"};

    /// <summary>
    /// 当前的语言
    /// </summary>
    public const string I18N = "en_US";

    #endregion
}