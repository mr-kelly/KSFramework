using System;
using System.Collections;
using System.Collections.Generic;
using KEngine;
using UnityEngine;

public class EditorLoadAssetTest : MonoBehaviour
{
    private AssetBundleLoader loader_prefab;

    public GameObject loadObj;
    [SerializeField] public int 执行次数 = 20;

    private WaitForSeconds _waitForSeconds;

    // Start is called before the first frame update
    void Start()
    {
        _waitForSeconds = new WaitForSeconds(1);
        StartCoroutine(TestLoadUI());
    }

    IEnumerator TestLoadUI()
    {
        if (loader_prefab != null)
        {
            Log.Info("alread exist !");
            yield break;
        }

        var path = "UI/UILogin";
        loader_prefab = AssetBundleLoader.Load(path, (isOk, ab) =>
        {
            Log.Info("load complete");
            if (isOk)
            {
                var request = ab.LoadAsset<GameObject>("UILogin");
                loadObj = GameObject.Instantiate(request);
                if (loadObj)
                    loadObj.transform.SetParent(null);
            }
        });
    }

    void UnLoadAB()
    {
        if (loader_prefab != null)
        {
            loader_prefab.Release();
            loader_prefab = null;
        }
        else
        {
            Log.Info("please load");
        }

        if (loadObj != null)
        {
            GameObject.Destroy(loadObj);
            loadObj = null;
        }
    }

    IEnumerator LoopTest()
    {
        for (int i = 0; i < 执行次数; i++)
        {
            StartCoroutine(TestLoadUI());
            yield return _waitForSeconds;
            UnLoadAB();
            yield return null;
        }
        
        Log.Info( $"执行完成{执行次数}次");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("加载Prefab", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            StartCoroutine(TestLoadUI());
        }

        if (GUILayout.Button("卸载Prefab", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            UnLoadAB();
        }

        if (GUILayout.Button($"执行{执行次数}次 加载->卸载", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            StartCoroutine(LoopTest());
        }
        
        if (GUILayout.Button("加载不存在的资源", GUILayout.MinWidth(100), GUILayout.MinHeight(60)))
        {
            AssetBundleLoader.Load("aa/bb");
        }
    }
}