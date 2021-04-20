using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using KEngine;

public class EffectImporter : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        if (!assetImporter.assetPath.Contains(KEngineDef.EffectPath))
            return;
        var importer = assetImporter as UnityEditor.TextureImporter;
        if (importer == null)
            return;
        importer.mipmapEnabled = false;
        if (importer.assetPath.Contains("common"))
        {
            importer.assetBundleName = "effect/effect_shared_tex" + AppConfig.AssetBundleExt;
        }
        else
        {
            importer.assetBundleName = null;
        }

        if (importer.assetPath.Contains("UIEffect"))
        {
            importer.textureType = TextureImporterType.Sprite;
        }
    }


    public void OnPreprocessModel()
    {
        if (!assetImporter.assetPath.Contains(KEngineDef.EffectPath))
            return;
        var importer = assetImporter as ModelImporter;
        if (importer == null)
            return;
        if (importer.assetPath.Contains("common"))
        {
            importer.assetBundleName = "effect/effect_shared_model" + AppConfig.AssetBundleExt;
        }
        else
        {
            importer.assetBundleName = null;
        }

        importer.meshCompression = ModelImporterMeshCompression.High;
#if UNITY_2018_3_OR_NEWER
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.optimizeMeshPolygons = true;
        importer.optimizeMeshVertices = true;
#else
        importer.importMaterials = false;
        importer.optimizeMesh = true;
#endif

        importer.importTangents = ModelImporterTangents.None;
        importer.isReadable = false;
    }
}