using System.Collections;
using System.Collections.Generic;
using KEngine;
using UnityEngine;
using UnityEngine.UI;

public class FindDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestFind();
    }

    void TestFind()
    {
        var btn_1 = gameObject.FindChild<Button>("btn_1");
        if (btn_1)
        {
            btn_1.onClick.RemoveAllListeners();
            btn_1.onClick.AddListener(() =>
            {
                TestFind();
            });
        }
        Log.Info("find btn_1:{0}",btn_1 );
        
        var btn_xxx = gameObject.FindChild<Button>("btn_xxx");
        Log.Info("find btn_xxx:{0}",btn_xxx);
        
        var text = gameObject.FindChild<Text>("Text");
        Log.Info("find Text:{0} ,Text.text={1}",text,text?.text);
        
        var text_name = gameObject.FindChild<Text>("text_name");
        Log.Info("find text_name:{0} ,Text.text={1}",text_name,text_name?.text);
        
        var text_test = gameObject.FindChild<Button>("text_name");
        
    }
}