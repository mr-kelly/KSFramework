using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;
using KEngine;

/// <summary>
/// 玩家时装自动导入
/// </summary>
public class HeroResourcesImporter
{
    string PathPattern = KEngineDef.CharacterPath + @"Hero/(fbx_\w+)/.*";
    string PathIgnore = "Others";
    string ABNameFormat = "characters/hero/{0}" + AppConfig.AssetBundleExt;

    public bool IsPathValid(string assetPath)
    {
        return Regex.IsMatch(assetPath, PathPattern) && !assetPath.Contains(PathIgnore);
    }

    public void DoImport(AssetImporter asset)
    {
        string abname = string.Format(ABNameFormat, Regex.Match(asset.assetPath, PathPattern).Groups[1]);
        
        if (asset.assetPath.Contains("/mesh"))
        {
            asset.assetBundleName = abname;
        }
        else if (asset.assetPath.Contains("/tex"))
        {
            asset.assetBundleName = abname;
        }
        else if (asset.assetPath.Contains("/mat"))
        {
            asset.assetBundleName = abname;
        }
        else if (asset.assetPath.Contains("/anim"))
        {
            asset.assetBundleName = abname;
        }
        else
        {
            asset.assetBundleName = string.Empty;
        }
    }
}