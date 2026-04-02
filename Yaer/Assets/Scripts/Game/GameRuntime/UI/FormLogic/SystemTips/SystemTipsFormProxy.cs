using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Manager.Res;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.SystemTips.Args;
using Game.Static.Path;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.SystemTips
{
    public class SystemTipsFormProxy : BaseFormProxy
    {
        private SpriteAtlas avatarAtlas;
        //private SpriteAtlas charAtlas;
        private ResComponentGM resComponentGM;
        private Action delayAction;

        public Action<UpdatedSystemTipsArgs> onUpdateTips;
        public Action onSureEvent;
        public Action onCancelEvent;

        SpriteAtlas tipsCharAtlas;
        SpriteAtlas tipsCharAtlas_en;
        SpriteAtlas tipsCharAtlas_jp;
        int curLoadAtlasCount = 0;
        int targetLoadAtlasCount = 4;
        public override void OnInit()
        {
            base.OnInit();
            
            resComponentGM = GameManager.GetGMComponent<ResComponentGM>();
            // 先加载图集数据
            //resComponentGM.LoadAsset<SpriteAtlas>(SpriteAtlasPath.GetPath("SystemTips_Char.spriteatlas"), atlas =>
            //{
            //    charAtlas = atlas;
            //    if (avatarAtlas)
            //    {
            //        delayAction?.Invoke();
            //    }
            //});
            resComponentGM.LoadAsset<SpriteAtlas>(SpriteAtlasPath.GetPath("SystemTips_Avatar.spriteatlas"), atlas =>
            {
                if (atlas == null) { return; }
                if (avatarAtlas != null) { return; }
                avatarAtlas = atlas;
                curLoadAtlasCount++;
                if (curLoadAtlasCount >= targetLoadAtlasCount)
                {
                    delayAction?.Invoke();
                }
            });
            var path = "Assets/GameRes/Atlas/SystemTips/tipsChar.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (tipsCharAtlas != null) { return; }
                tipsCharAtlas = atlas;
                curLoadAtlasCount++;
                if (curLoadAtlasCount >= targetLoadAtlasCount)
                {
                    delayAction?.Invoke();
                }
            });
            path = "Assets/GameRes/Atlas/SystemTips/tipsChar_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (tipsCharAtlas_en != null) { return; }
                tipsCharAtlas_en = atlas;
                curLoadAtlasCount++;
                if (curLoadAtlasCount >= targetLoadAtlasCount)
                {
                    delayAction?.Invoke();
                }
            });
            path = "Assets/GameRes/Atlas/SystemTips/tipsChar_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (tipsCharAtlas_jp != null) { return; }
                tipsCharAtlas_jp = atlas;
                curLoadAtlasCount++;
                if (curLoadAtlasCount >= targetLoadAtlasCount)
                {
                    delayAction?.Invoke();
                }
            });
        }

        public void UpdateTips(ESystemTipsType t)
        {
            if (tipsCharAtlas_en is null || avatarAtlas is null || tipsCharAtlas is null ||
                tipsCharAtlas_jp is null)
            {
                delayAction = () => { UpdateTips(t); };
                return;
            }

            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, tipsCharAtlas }, {  LanguageEnumType.English, tipsCharAtlas_en },
                {  LanguageEnumType.Japanese, tipsCharAtlas_jp },
            };

            var curLaunageType = GameManager.Instance.language;
            SpriteAtlas charAtlas;
            if (!spriteAtlasData.ContainsKey(curLaunageType))
            {
                // 不存在的语言一律使用英文
                charAtlas = tipsCharAtlas_en;
            }
            else
            {
                charAtlas = spriteAtlasData[curLaunageType];
            }
            var args = new UpdatedSystemTipsArgs();
            args.type = t;
            switch (t)
            {
                case ESystemTipsType.Load: // 加载存档
                    // 修改提示文字
                    args.charSprite = charAtlas.GetSprite("LoadChar");

                    // 修改头像
                    args.avatarSprite = avatarAtlas.GetSprite("LoadAvatar");
                    break;
                case ESystemTipsType.Save:
                    args.charSprite = charAtlas.GetSprite("SaveChar");
                    args.avatarSprite = avatarAtlas.GetSprite("SaveAvatar");
                    break;
                case ESystemTipsType.Delete: // 删除存档
                    args.charSprite = charAtlas.GetSprite("DeleteChar");
                    args.avatarSprite = avatarAtlas.GetSprite("SaveAvatar");
                    break;
                case ESystemTipsType.Cover: // 覆盖存档
                    args.charSprite = charAtlas.GetSprite("CoverChar");
                    args.avatarSprite = avatarAtlas.GetSprite("SaveAvatar");
                    break;
                case ESystemTipsType.Quit:
                    args.charSprite = charAtlas.GetSprite("ExitChar");
                    args.avatarSprite = avatarAtlas.GetSprite("ExitAvatar");
                    break;
                case ESystemTipsType.GoOutHome:
                    args.charSprite = charAtlas.GetSprite("GoOutHomeChar");
                    break;
                case ESystemTipsType.Developing:
                    args.charSprite = charAtlas.GetSprite("DevelopingChar");
                    break;
                case ESystemTipsType.LeavingBedroom:
                    args.charSprite = charAtlas.GetSprite("LeavingBedroomChar");
					break;
            }

            onUpdateTips?.Invoke(args);
        }
        
        public void OnSure()
        {
            var sureAction = onSureEvent;
            ResetCallbacks();
            sureAction?.Invoke();
        }

        public void OnCancel()
        {
            var cancelAction = onCancelEvent;
            ResetCallbacks();
            cancelAction?.Invoke();
        }

        public void ResetCallbacks()
        {
            onSureEvent = null;
            onCancelEvent = null;
        }
    }
}