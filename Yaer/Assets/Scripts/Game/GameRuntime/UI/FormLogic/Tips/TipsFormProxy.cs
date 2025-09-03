using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Path;
using SingularityGroup.HotReload;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.Tips
{
    public class TipsFormProxy : BaseFormProxy
    {
        private SpriteAtlas atlas;
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;
        public override void OnInit()
        {
            base.OnInit();
            
            // 加载资源
            //GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(SpriteAtlasPath.GetPath("Tips_Char"), spriteAtlas => atlas = spriteAtlas);

            var path = "Assets/GameRes/Atlas/TipsPanel/tipsInfo.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
            });
            path = "Assets/GameRes/Atlas/TipsPanel/tipsInfo_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
            });
            path = "Assets/GameRes/Atlas/TipsPanel/tipsInfo_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_jp != null) { return; }
                spriteAtlas_jp = atlas;
            });
        }

        public Sprite GetTipsSprite(string key)
        {
            if (spriteAtlas == null || spriteAtlas_en == null || spriteAtlas_jp == null)
            {
                Log.Error("atlas数据没有初始化完成哦");
                return null;
            }
            //if (atlas == null)
            //{
            //    Log.Error("atlas数据没有初始化完成哦");
            //    return null;
            //}
            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, spriteAtlas }, {  LanguageEnumType.English, spriteAtlas_en },
                {  LanguageEnumType.Japanese, spriteAtlas_jp },
            };

            var curLaunageType = GameManager.Instance.language;
            SpriteAtlas mySpriteAtlas;
            if (!spriteAtlasData.ContainsKey(curLaunageType))
            {
                // 不存在的语言一律使用英文
                mySpriteAtlas = spriteAtlas_en;
            }
            else
            {
                mySpriteAtlas = spriteAtlasData[curLaunageType];
            }
            //var s = atlas.GetSprite(key);
            var s = mySpriteAtlas.GetSprite(key);
            if (s is null)
            {
                Debug.LogError("未找到Tips图片：" + key);
                return null;
            }

            return s;
        }
    }
}