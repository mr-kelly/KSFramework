#if UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEngine.U2D;

namespace KEngine
{
    // 加载spriteAtlas
    public class KSpriteAtlasLoader : AbstractResourceLoader
    {
        public SpriteAtlas Asset
        {
            get { return ResultObject as SpriteAtlas; }
        }

        public delegate void CKSpriteAtlasLoaderDelegate(bool isOk, Sprite tex);

        private AssetFileLoader AssetFileBridge;

        public override float Progress
        {
            get { return AssetFileBridge.Progress; }
        }

        public string Path { get; private set; }

        public static KSpriteAtlasLoader Load(string path, string spriteName, CKSpriteAtlasLoaderDelegate callback = null)
        {
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) =>
                {
                    SpriteAtlas spriteAtlas = obj as SpriteAtlas;
                    if (spriteAtlas != null)
                    {
                        var sp = spriteAtlas.GetSprite(spriteName);
                        callback(isOk, sp);
                    }
                    else
                    {
                        callback(isOk, null);
                    }
                };
            }
            return AutoNew<KSpriteAtlasLoader>(path, newCallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);
            Path = url;
            AssetFileBridge = AssetFileLoader.Load(Path, OnAssetLoaded);
        }

        private void OnAssetLoaded(bool isOk, UnityEngine.Object obj)
        {
            OnFinish(obj);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            AssetFileBridge.Release(); // all, Texture is singleton!
        }
    }
}
#endif