using System.Collections.Generic;
using KEngine;
using UnityEngine;
using UnityEngine.UI;

public class LoadSpriteDemo : MonoBehaviour
{
    public Image targetImage;
    public Dropdown dropDown;
    public Dropdown dropDownAtlas;
    public Button btnRelease, btnReleaseAtlas;
    private KSpriteAtlasLoader atlasLoader;

    private Dictionary<string, SpriteLoader> spriteLoaders = new Dictionary<string, SpriteLoader>();
    public string[] iconNames = { "button_green", "button_red", "button_yellow" };
   
    // Use this for initialization
    void Start()
    {
        dropDown.options.Clear();
        dropDownAtlas.options.Clear();
        foreach (string iconName in iconNames)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData(iconName);
            dropDown.options.Add(optionData);
            dropDownAtlas.options.Add(optionData);
        }

        dropDown.onValueChanged.AddListener((index) =>
        {
            var selectName = iconNames[index];
            Log.Info("切换{0},{1}", selectName, index);
            var spriteLoader = SpriteLoader.Load(string.Format("uiatlas/button/{0}.png", selectName), (isOk, loadSprite) =>
         {
             if (isOk)
             {
                 targetImage.sprite = loadSprite;
                 targetImage.SetNativeSize();
                 Log.Info("图片加载完成:{0}", selectName);
             }
         });
            if (!spriteLoaders.ContainsKey(spriteLoader.Path))
            {
                spriteLoaders.Add(spriteLoader.Path, spriteLoader);
            }
        });

        //从spriteAtlas加载
        dropDownAtlas.onValueChanged.AddListener((index) =>
        {
            var selectName = iconNames[index];
            Log.Info("切换{0},{1}", selectName, index);
            var atlasPath = "uiatlas/buttonatlas.spriteatlas";
            atlasLoader = KSpriteAtlasLoader.Load(atlasPath, selectName, (isOk, loadSprite) =>
            {
                if (isOk)
                {
                    targetImage.sprite = loadSprite;
                    targetImage.SetNativeSize();
                    Log.Info("图片加载完成:{0}", selectName);

                }
            });
        });
        btnReleaseAtlas.onClick.AddListener(RelaseAtlasLoader);
        btnRelease.onClick.AddListener(RelaseSpriteLoader);
    }

    public void RelaseSpriteLoader()
    {
        KAsync.Start().WaitForSeconds(5).Then(() =>
        {
            foreach (KeyValuePair<string, SpriteLoader> keyValuePair in spriteLoaders)
            {
                keyValuePair.Value.Release(true);
            }

            Destroy(targetImage);
            KResourceModule.Collect();
        });
    }

    public void RelaseAtlasLoader()
    {
        KAsync.Start().WaitForSeconds(5).Then(() =>
        {
            atlasLoader.Release(true);
            Destroy(targetImage);
            KResourceModule.Collect();
        });
    }
}
