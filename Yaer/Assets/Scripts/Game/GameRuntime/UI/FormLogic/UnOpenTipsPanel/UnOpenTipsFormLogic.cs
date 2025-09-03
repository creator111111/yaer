using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Game.Static.Name.Settings;

namespace Game.GameRuntime.UI.FormLogic.ChapterEndPanel
{
    // 章节结束界面
    public class UnOpenTipsFormLogic : BaseUIFormLogic
    {
        public GameObject root;
        public GameObject imgUnOpenTips;
        SpriteAtlas spriteAtlas;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            LoadAtlas(1);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var resCpnGM = GameManager.GetGMComponent<ResComponentGM>();
            var path = "Assets/GameRes/Atlas/SettingPanel/tipImg.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            var baseKeyName = "按键绑定未开放{0}";
            var curLaunageType = GameManager.Instance.language;
            var tag = LanguageType.GetLanaguageResTag(curLaunageType);
            var realKeyName = string.Format(baseKeyName, tag);
            GameTools.loadTextureByAtlas(imgUnOpenTips, spriteAtlas, realKeyName);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            root.GetComponent<CanvasGroup>().alpha = 0;
            List<Tween> tweens = new List<Tween>() {
                GameActionMgr.runFadeAction(root, 1f, 0.5f),
                GameActionMgr.runFadeAction(root, 0f, 0.5f, 1f),
            };
            var seqAct = GameActionMgr.runSequenceAction(root, tweens);
            seqAct.onComplete = () =>
            {
                // 淡出消失后自动关闭界面
                CloseForm();
            };
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }
        public override void PlayerOpenAudio()
        {

        }
    }
}