using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KEngine;

public class KTextureImporter : AssetPostprocessor
{
    //把UI图片设置为只读，减少内存占用
    public void OnPreprocessTexture()
    {
        UnityEditor.TextureImporter importer = this.assetImporter as UnityEditor.TextureImporter;
        if (importer == null)
            return;
        var andSettings = new TextureImporterPlatformSettings();
        var iosSettings = new TextureImporterPlatformSettings();
        andSettings.name = "Android";
        iosSettings.name = "iPhone";
        andSettings.overridden = true;
        iosSettings.overridden = true;
        andSettings.compressionQuality = iosSettings.compressionQuality = 50;

        andSettings.format = TextureImporterFormat.ETC2_RGBA8;
        andSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32Bit;
        iosSettings.format = TextureImporterFormat.ASTC_6x6;
        andSettings.maxTextureSize = iosSettings.maxTextureSize = 1024;//NOTE 如果有特殊需求可通过配置修改

        importer.SetPlatformTextureSettings(andSettings);
        importer.SetPlatformTextureSettings(iosSettings);

        var rootDir = importer.assetPath.Split('/')[1];
        if (rootDir == KEngineDef.ResourcesEditDir)
        {
            if (importer.isReadable)
                importer.isReadable = false;

            if (importer.assetPath.StartsWith(KEngineDef.UIPath))
            {
                if (assetPath.Contains("Fonts"))
                {
                    importer.textureType = TextureImporterType.Default;
                }
                else
                {
                    importer.textureType = TextureImporterType.Sprite;
                }

                importer.mipmapEnabled = false;
                //NOTE 根据项目的实际情况，是把同一种类所有icon打到一个ab中,还是每个icon打成一个ab
            }
            else if (importer.assetPath.Contains("Character"))
            {
                if (!(importer.assetPath.Contains("Character/Hero") || importer.assetPath.Contains("Character/Hair")))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.mipmapEnabled = false;
                }
            }
            else if (importer.assetPath.Contains(".exr"))
            {
                //Debug.Log("lightmap settings: " + importer.assetPath);
                importer.textureType = TextureImporterType.Lightmap;
#if UNITY_2018_3_OR_NEWER
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                var ftm = importer.GetDefaultPlatformTextureSettings();
                ftm.format = TextureImporterFormat.RGBA32;
                importer.SetPlatformTextureSettings(ftm);
#else
                importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
#endif
                importer.wrapMode = TextureWrapMode.Clamp;
                //importer.mipmapEnabled = false;
            }
        }
        else
        {
            if (importer.assetPath.Contains("Editor/GameTools/XSceneTool/MapImages"))
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                }
            }
        }
    }
}