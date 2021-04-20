using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 动画导入预处理  by qingqing-zhao
/// doc:https://docs.unity3d.com/ScriptReference/AssetPostprocessor.OnPreprocessAnimation.html
/// </summary>
public class AnimationImporter : AssetPostprocessor
{
    void OnPreprocessAnimation()
    {
        var importer = assetImporter as ModelImporter;
        if (importer != null)
        {
            //这些名字的动画设置为循环的
            List<string> loopList = new List<string>() {"run", "stand", "riderun", "ridestand", "idle", "ridestand", "ready", "walk"};
            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) 
                clips = importer.defaultClipAnimations;
            if (clips != null)
            {
                foreach (ModelImporterClipAnimation clipAnimation in clips)
                {
                    if (loopList.Contains(clipAnimation.name.ToLower()))
                    {
                        clipAnimation.loopTime = true;
                    }
                }
            }

            importer.clipAnimations = clips;

            //NOTE 如果需要其它的设置，在这儿扩充
        }
    }
}