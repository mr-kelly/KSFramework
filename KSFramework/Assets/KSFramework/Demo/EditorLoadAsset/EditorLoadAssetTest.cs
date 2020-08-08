using System;
using System.Collections;
using System.Collections.Generic;
using KEngine;
using UnityEngine;

public class EditorLoadAssetTest : MonoBehaviour
{
    private InstanceAssetLoader instanceLoader;
    private StaticAssetLoader staticLoader;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TestInstanceLoad());
    }

    IEnumerator TestInstanceLoad()
    {
        if (instanceLoader != null)
        {
            Log.Debug("alread exist !");
            yield break;
        }

        var path = "UI/Login.prefab";
        instanceLoader = InstanceAssetLoader.Load(path);
        while (!instanceLoader.IsCompleted)
        {
            yield return null;
        }

        Log.Debug("load complete");
//        assetLoader.InstanceAsset.transform.SetParent(this.transform);
    }

    IEnumerator TestStaticLoad()
    {
        if (staticLoader != null)
        {
            Log.Debug("alread exist !");
            yield break;
        }

        var path = "UI/Login.prefab";
        staticLoader = StaticAssetLoader.Load(path);
        while (!staticLoader.IsCompleted)
        {
            yield return null;
        }

        Log.Debug("load complete");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Instance Load", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            StartCoroutine(TestInstanceLoad());
        }

        if (GUILayout.Button("Instance Release", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            if (instanceLoader != null)
            {
                instanceLoader.Release();
                instanceLoader = null;
            }
            else
            {
                Log.Debug("please load");
            }
        }


        if (GUILayout.Button("Static Load", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            StartCoroutine(TestStaticLoad());
        }

        if (GUILayout.Button("Static Release", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            if (staticLoader != null)
            {
                staticLoader.Release();
                staticLoader = null;
            }
            else
            {
                Log.Debug("please load");
            }
        }
    }
}