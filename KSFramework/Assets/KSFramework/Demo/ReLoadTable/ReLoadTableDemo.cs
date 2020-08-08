using AppSettings;
using KEngine;
using UnityEngine;

/// <summary>
/// 1、修改Excel配置文件，保存
/// 2、回到Unity，提示重新编译，点击OK
/// 3、重新启用脚本，再次读取到的值就是新值
/// </summary>
public class ReLoadTableDemo : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        LoadTable();
    }

    void OnEnable()
    {
        LoadTable();
    }

    void LoadTable()
    {
        //运行时热重载配置表数据
        var val1 = BillboardSettings.Get("Billboard1");
        Log.Info(val1.Title);
    }
}
