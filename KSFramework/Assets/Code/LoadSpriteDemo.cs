using KEngine;
using UnityEngine;
using UnityEngine.UI;

public class LoadSpriteDemo : MonoBehaviour
{
    public Image targetImage;
    public Dropdown dropDown;

    public string[] iconNames = {"button_green", "button_red", "button_yellow" };
    // Use this for initialization
    void Start()
    {
        dropDown.options.Clear();
        foreach (string iconName in iconNames)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData(iconName);
            dropDown.options.Add(optionData);
        }

        dropDown.onValueChanged.AddListener((index) =>
        {
            var selectName = iconNames[index];
            Log.Info("切换{0},{1}",selectName,index);
            SpriteLoader.Load(string.Format("uiatlas/button/{0}.png", selectName), (isOk, loadSprite) =>
         {
             if (isOk)
             {
                 targetImage.sprite = loadSprite;
                 targetImage.SetNativeSize();
                 Log.Info("图片加载完成:{0}", selectName);
             }
         });
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
