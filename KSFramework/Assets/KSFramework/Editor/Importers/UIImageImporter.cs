using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class UIImageImporter : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        UnityEditor.TextureImporter importer = this.assetImporter as UnityEditor.TextureImporter;
        if (importer.assetPath.Contains("/BundleEditing/UI/"))
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
        }
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }

}

