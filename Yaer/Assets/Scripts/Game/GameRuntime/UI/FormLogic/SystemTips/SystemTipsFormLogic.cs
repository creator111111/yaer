using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.SystemTips.Args;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.SystemTips
{
    public class SystemTipsFormLogic : BaseUIFormLogic
    {
        [SerializeField] private Image imgAvatar; // 头像图片
        [SerializeField] private Image imgTipsContent; // 提示内容
        [SerializeField] private Button btnCancel;
        [SerializeField] private Button btnSure;
        [SerializeField] private Button btnConfirmExit;

        public SystemTipsFormProxy proxy;

        SpriteAtlas spriteAtlas;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            proxy = GetProxy<SystemTipsFormProxy>();

            btnCancel.onClick.AddListener(() =>
            {
                UIUtils.PlayBtnAudio(this);
                proxy.OnCancel();
                CloseForm();
            });
            btnConfirmExit.onClick.AddListener(() =>
            {
                UIUtils.PlayBtnAudio(this);
                proxy.OnSure();
                CloseForm();
            });
            btnSure.onClick.AddListener(() =>
            {
                UIUtils.PlayBtnAudio(this);
                proxy.OnSure();
                CloseForm();
            });

            LoadAtlas(1);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/SystemTips/btn.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            // 设置按钮的多语言
            var baseName = "好呀{0}";
            var baseNameSelect = "好呀点击{0}";
            var languageTag = GameManager.GetCurLanguageResTag();
            var realName = string.Format(baseName, languageTag);
            var realNameSelect = string.Format(baseNameSelect, languageTag);
            var spriteYes = spriteAtlas.GetSprite(realName);
            var spriteYesSelect = spriteAtlas.GetSprite(realNameSelect);
            if (spriteYes == null)
            {
                spriteYes = spriteAtlas.GetSprite("好呀_en");// 没有语言对应的图片则默认使用英文版
            }
            if (spriteYesSelect == null) { spriteYesSelect = spriteAtlas.GetSprite("好呀点击_en"); }
            GameTools.loadBtnSprite(btnSure, spriteYes, spriteYesSelect);
            baseName = "是的{0}";
            baseNameSelect = "是的点击{0}";
            languageTag = GameManager.GetCurLanguageResTag();
            realName = string.Format(baseName, languageTag);
            realNameSelect = string.Format(baseNameSelect, languageTag);
            spriteYes = spriteAtlas.GetSprite(realName);
            spriteYesSelect = spriteAtlas.GetSprite(realNameSelect);
            if (spriteYes == null)
            {
                spriteYes = spriteAtlas.GetSprite("是的_en");// 没有语言对应的图片则默认使用英文版
            }
            if (spriteYesSelect == null) { spriteYesSelect = spriteAtlas.GetSprite("是的点击_en"); }
            GameTools.loadBtnSprite(btnConfirmExit, spriteYes, spriteYesSelect);
            baseName = "再想想{0}";
            baseNameSelect = "再想想点击{0}";
            languageTag = GameManager.GetCurLanguageResTag();
            realName = string.Format(baseName, languageTag);
            realNameSelect = string.Format(baseNameSelect, languageTag);
            spriteYes = spriteAtlas.GetSprite(realName);
            spriteYesSelect = spriteAtlas.GetSprite(realNameSelect);
            if (spriteYes == null)
            {
                spriteYes = spriteAtlas.GetSprite("再想想_en");// 没有语言对应的图片则默认使用英文版
            }
            if (spriteYesSelect == null) { spriteYesSelect = spriteAtlas.GetSprite("再想想点击_en"); }
            GameTools.loadBtnSprite(btnCancel, spriteYes, spriteYesSelect);
        }


        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            proxy.onUpdateTips = UpdateInfo;
            proxy.ResetCallbacks();
            proxy.UpdateTips((ESystemTipsType)userData);
        }


        public void UpdateInfo(UpdatedSystemTipsArgs args)
        {
            // 隐藏退出游戏按钮
            btnConfirmExit.gameObject.SetActive(false);
            imgTipsContent.sprite = args.charSprite;
            imgAvatar.sprite = args.avatarSprite;

            if (args.type == ESystemTipsType.Quit)
            {
                // 隐藏普通确认按钮
                btnSure.gameObject.SetActive(false);
                btnConfirmExit.gameObject.SetActive(true);
            }
            else
            {
                // 显示普通确认按钮
                btnSure.gameObject.SetActive(true);
                btnConfirmExit.gameObject.SetActive(false);
            }

            // 设置原始大小
            imgTipsContent.SetNativeSize();
        }

        public override void CloseFormOnEsc()
        {
            proxy.OnCancel();
            CloseForm();
        }
    }
}