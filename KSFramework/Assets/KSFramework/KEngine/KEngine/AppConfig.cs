using System;
using System.Collections.Generic;
using KEngine;
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
    /// 是否打印函数执行耗时
    /// </summary>
    public static bool IsLogFuncCost = false;
    /// <summary>
    /// 是否记录到文件中，包括：UI的ab加载耗时，UI函数执行耗时
    /// </summary>
    public static bool IsSaveCostToFile = Application.isEditor;

    /// <summary>
    /// 是否调试模式，可与Unity的Debug区分开
    /// </summary>
    public static bool IsDebugBuild = Debug.isDebugBuild;
    /// <summary>
    /// 是否Editor模式，可在某些情况下与Unity的isEditor区分，比如详细/网络日志调试模式下在真机输出
    /// </summary>
    public static bool isEditor = Application.isEditor;

    /// <summary>
    /// 是否创建AssetDebugger
    /// </summary>
    public static bool UseAssetDebugger = Application.isEditor;
    /// <summary>
    /// 仅对Editor有效，Editor下加载资源默认从磁盘的相对目录读取，如果需要从Aplication.streamingAssets则设置为true
    /// </summary>
    public static bool ReadStreamFromEditor;
    /// <summary>
    /// cdn资源地址，正式项目通过服务器下发
    /// </summary>
     public static string resUrl = "http://127.0.0.1:8080/cdn/";
    //public static string resUrl = "http://192.168.190.112:8080/cdn/";
    /// <summary>
    /// 是否开启下载更新的功能，doc:https://mr-kelly.github.io/KSFramework/advanced/autoupdate/
    /// </summary>
    public static bool IsDownloadRes = false;

    public static string VersionTextPath
    {
        get { return KResourceModule.AppBasePath + "/" + VersionTxtName; }
    }
    public static string VersionTxtName
    {
        get { return KResourceModule.GetBuildPlatformName() + "-version.txt"; }
    }
    /// <summary>
    /// filelist.txt的相对路径
    /// </summary>
    public static string FilelistName
    {
        get { return $"Bundles\\{KResourceModule.GetBuildPlatformName()}\\filelist.txt"; }
    }
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
    public const string ExportLuaPath = "Product/Lua/configs/";
    public const string ExportTsvPath = "Product/Setting";
    public const string ExportCSharpPath = "Assets/Code/AppSettings/";


    //;Folder in Resources
    public const string SettingResourcesPath = "Setting";

    //; Ignore genereate code for these excel.
    public const string SettingCodeIgnorePattern = "(I18N/.*)|(StringsTable.*)";
    
    /// <summary>
    /// UI设计的分辨率
    /// </summary>
    public static Vector2 UIResolution = new Vector2(1280, 720);

    /// <summary>
    /// 游戏共有多少种语言
    /// </summary>
    public static string[] I18NLanguages = new string[] {"cn", "en"};
    /// <summary>
    /// 当前的语言ID
    /// </summary>
    public static string LangId = "";
    
    /// <summary>
    /// 开发中的语言，此语言下所有的文件不需要加后缀，否则文件名后加: --语种
    /// </summary>
    public const string dev_lang = "cn";
    private static string langFileFlag;
    /// <summary>
    /// 多语言加载文件的标识，比如不同地区读的语言包、图片，会在资源名后面加上此标识(eg: atlas.ab -> atlas--en.ab)
    /// </summary>
    public static string LangFileFlag
    {
        get
        {
            if (langFileFlag == null)
            {
                langFileFlag = (LangId == dev_lang || string.IsNullOrEmpty(LangId)) ? "" : "--" + LangId;
            }
            return langFileFlag;
        }
    }

    #endregion

    public static void  Init()
    {
        IsLogDeviceInfo = !Application.isEditor;
    }
}