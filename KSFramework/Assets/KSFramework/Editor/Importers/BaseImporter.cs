using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using UnityEditor;


public class BaseImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //deletedAssets movedFromAssetPaths 删除资源
        //importedAssets movedAssets  新增和移动资源

        OnPostprocessAssets(importedAssets);
        OnPostprocessAssets(movedAssets);
    }

    static void OnPostprocessAssets(string[] assets)
    {
        if (assets == null || assets.Length <= 0)
            return;
        foreach (var path in assets)
        {
            if (path.EndsWith(".meta"))
                continue;
            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
                continue;
            if (path.Contains(KEngineDef.ShaderPath)) //全部的shader打包到一个ab中
            {
                if (importer.assetBundleName != KEngineDef.ShaderabName)
                {
                    importer.assetBundleName = KEngineDef.ShaderabName;
                }
            }
        }
    }

    void OnPreprocessAudio()
    {
        var importer = assetImporter as AudioImporter;
        if (importer == null)
            return;
        importer.loadInBackground = true;
        AudioImporterSampleSettings setting = new AudioImporterSampleSettings();
        setting.compressionFormat = AudioCompressionFormat.Vorbis;
        setting.quality = 0.5f;
        setting.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        importer.preloadAudioData = false;
        var tmpPath = "Assets/" + KEngineDef.ResourcesEditDir + "/";
        var path = importer.assetPath.Replace(tmpPath, "");
        if (importer.assetPath.Contains("/Sounds/BGM")) //单个背景音乐，单独打包AB
        {
            setting.loadType = AudioClipLoadType.Streaming;
            importer.assetBundleName = path.ToLower() + AppConfig.AssetBundleExt;
        }
        else
        {
            setting.loadType = AudioClipLoadType.DecompressOnLoad;
            var dirName = Path.GetDirectoryName(path).ToLower();
            //特殊处理放在sounds根目录的音频把包到sounds/目录下，如果刚好有个子目录也叫sounds则需要特殊处理
            if (dirName == "sounds")
                dirName = "sounds/sounds";
            importer.assetBundleName = dirName + AppConfig.AssetBundleExt;
        }

        importer.defaultSampleSettings = setting;
    }

    public void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (importer == null)
            return;
        //TODO 根据情况是否导入动画
        // importer.importAnimation = false;
        // importer.importBlendShapes = false;
        //非effect目录，则不勾选Read/Write
        if (importer.assetPath.Contains(KEngineDef.EffectPath))
            return;
        if (importer.isReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }
    }
}