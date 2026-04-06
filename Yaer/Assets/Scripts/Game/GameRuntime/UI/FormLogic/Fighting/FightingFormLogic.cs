using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.Entities.Component;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Fighting;
using Game.Static.Name.Clothes;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic
{
    public class FightingFormLogic : BaseUIFormLogic
    {
        private Animator animator;
        private Image imgAvatar;
        private Slider HPSlider;
        private Slider MPSlider;

        private FightingBroken broken;
        private FightingIllustration illustration;

        private bool isShowWound = true;

        [SerializeField]
        private Animator WoundEffectAnimator;
        private CanvasGroup WoundEffectCanvasGroup;

        private SettingManager settingManager;
        private PlayerLogic playerLogic;

        private float MaxHP;
        private float MaxStamina;

        private bool FirstRefresh;

        bool hasUpdateIllustration = false; // 是否刷新当前战斗立绘
        bool hasInitUI = false; // 是否初始化某些UI
        private bool isBattleImageVisible; // 当前战斗立绘是否处于显示状态（逻辑层记录）
        GameObject itemImgBgArea;
        [SerializeField]
        private Sprite[] AvatarSprites;
        private ForestSceneData ForestSceneData
        {
            get
            {
                if (GameManager.GetGameSceneManager() != null)
                {
                    return GameManager.GetGameSceneManager().GetArchiveData<ForestSceneData>();
                }else { return null; }
            }
        }


        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            animator = transform.GetComponent<Animator>();
            imgAvatar = transform.Find("Info/ImageBG/ImageAvatar_ref").GetComponent<Image>();
            HPSlider = transform.Find("Info/ImageBG/HPSlider").GetComponent<Slider>();
            MPSlider = transform.Find("Info/ImageBG/MPSlider").GetComponent<Slider>();
            itemImgBgArea = UIUtils.findChild(gameObject, "imgItemBg");
            broken = transform.Find("Broken").GetComponent<FightingBroken>();
            illustration = transform.Find("Illustration").GetComponent<FightingIllustration>();
            WoundEffectCanvasGroup = WoundEffectAnimator.GetComponent<CanvasGroup>();

            HPSlider.value = 1;
            MPSlider.value = 1;

            settingManager = GameManager.GetManager<SettingManager>();

        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            settingManager.OnBattleImageChange += this.UpdateBattleImageVisiable;
            settingManager.OnShowWoundChange += this.UpdateWoundVisiable;

            playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            // 切换场景后下面的回调方法会失效，改成直接在PlayerLogic里面调用对应的方法
            //playerLogic.componentSystem.GetComponent<HealthComponent>().onHpChange += UpdateHp;
            //playerLogic.componentSystem.GetComponent<StaminaComponent>().OnStaminaChanged += UpdateMp;
            //playerLogic.OnTakeDamage += PlayerUnderAttack;
            //playerLogic.OnClothesBrokenChanged += OnClothesBrokenChanged;
            MaxHP = playerLogic.componentSystem.GetComponent<HealthComponent>().maxHp;
            MaxStamina = playerLogic.componentSystem.GetComponent<StaminaComponent>().MaxStamina;
            var curHp = playerLogic.componentSystem.GetComponent<HealthComponent>().hp;
            var curMp = playerLogic.componentSystem.GetComponent<StaminaComponent>().Stamina;
            UpdateHp(curHp);
            UpdateMp(curMp);
            var configData = settingManager.LoadSetting<SettingsConfigData>();
            UpdateBattleImageVisiable(configData.showBattleImage);
            UpdateWoundVisiable(configData.showWound);
            FirstRefresh = false;

            hasUpdateIllustration = false;
            //UpdateIllustrationState();
            itemImgBgArea.GetComponent<CanvasGroup>().alpha = 1f;
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            WoundEffectCanvasGroup.DOKill();
            settingManager.OnBattleImageChange -= this.UpdateBattleImageVisiable;
            hasUpdateIllustration = false;
            //playerLogic.componentSystem.GetComponent<HealthComponent>().onHpChange -= UpdateHp;
            //playerLogic.componentSystem.GetComponent<StaminaComponent>().OnStaminaChanged -= UpdateMp;
            //playerLogic.OnTakeDamage -= PlayerUnderAttack;
            //playerLogic.OnClothesBrokenChanged -= OnClothesBrokenChanged;
        }


        protected override void Start()
        {
            base.Start();

            //UpdateIllustrationState();
        }

        private void OnDisable()
        {
            hasInitUI = false;
        }

        void showUIOnEnable()
        {
            if (hasInitUI) { return; }
            hasInitUI = true;
            var configData = settingManager.LoadSetting<SettingsConfigData>();
            UpdateBattleImageVisiable(configData.showBattleImage);
            UpdateWoundVisiable(configData.showWound);
            itemImgBgArea.GetComponent<CanvasGroup>().alpha = 1f;
        }

        public void UpdateIllustrationState()
        {
            if (GameManager.GetGameSceneManager() != null)
            {
                hasUpdateIllustration = true;
                var clothesData = GameManager.GetGameSceneManager().GetArchiveData<PlayerClothesData>();
                string headWear = clothesData.GetClothesName(BoneName.Headwear);
                illustration.Initialize(FightingIllustration.IllustrationState.Normal, headWear);

                broken.Initialize(headWear);
            }
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (!FirstRefresh)
            {
                var health = playerLogic.componentSystem.GetComponent<HealthComponent>();
                UpdateHp(health.hp);
                FirstRefresh = true;
            }
            if (!hasUpdateIllustration)
            {
                UpdateIllustrationState();
            }
            if (ForestSceneData != null)
            {
                showUIOnEnable();
            }
        }

        /// <summary>
        /// 更新血条
        /// </summary>
        /// <param name="hp">血量的百分比</param>
        public void UpdateHp(float hp)
        {
            float hpPercent = hp / MaxHP;
            HPSlider.value = hpPercent;
            UpdateIllustration(hpPercent, playerLogic.ClothesBroken);
            WoundEffectAnimator.SetBool("Wound", hpPercent < 0.25f);

            UpdateAvatar(hpPercent);
        }

        private void UpdateAvatar(float hpPercent)
        {
            if (hpPercent > 0.5f)
            {
                imgAvatar.sprite = AvatarSprites[0];
            }
            else if (hpPercent > 0.25f)
            {
                imgAvatar.sprite = AvatarSprites[1];
            }
            else
            {
                if (isShowWound)
                {
                    imgAvatar.sprite = AvatarSprites[2];
                }
                else
                {
                    imgAvatar.sprite = AvatarSprites[1];
                }
            }
        }

        private void UpdateIllustration(float hp, bool clothesBroken)
        {
            if (hp > 0.5f)
            {
                if (clothesBroken)
                {
                    illustration.SetState(FightingIllustration.IllustrationState.Damaged);
                }
                else
                {
                    illustration.SetState(FightingIllustration.IllustrationState.Normal);
                }
            }
            else if (hp > 0.25f && hp <= 0.5f)
                illustration.SetState(FightingIllustration.IllustrationState.Damaged);
            else if (hp <= 0.25f)
            {
                if (isShowWound)
                {
                    illustration.SetState(FightingIllustration.IllustrationState.DamagedAndWounded);
                }
                else
                {
                    illustration.SetState(FightingIllustration.IllustrationState.Damaged);
                }
            }
        }

        /// <summary>
        ///     更新体力
        /// </summary>
        /// <param name="mp">体力的百分比</param>
        public void UpdateMp(float mp)
        {
            MPSlider.value = mp / MaxStamina;
        }

        public void UpdateAvatar(Sprite sprite)
        {
            imgAvatar.sprite = sprite;
        }

        public void OnClothesBrokenChanged(bool isBroken)
        {
            float hp = HPSlider.value;
            UpdateIllustration(hp, isBroken);
            if (isBroken)
            {
                broken.PlayBrokenAnimation();
                playerLogic.PlayClothingBreakAudio(isBroken);
            }
        }

        public void PlayerUnderAttack(float damage)
        {
            illustration.Attacked(true);
        }

        public void PlayerNotUnderAttack()
        {
            illustration.Attacked(false);
        }

        public void Hide()
        {
            animator.SetTrigger("Hide");
            WoundEffectCanvasGroup.DOKill();
            WoundEffectCanvasGroup.DOFade(0, 0.5f);
            illustration.gameObject.SetActive(false);
        }

        public void Show()
        {
            animator.SetTrigger("Show");
            WoundEffectCanvasGroup.DOKill();
            WoundEffectCanvasGroup.DOFade(1, 0.5f);

            UpdateBattleImageVisiable(settingManager.LoadSetting<SettingsConfigData>().showBattleImage);
        }

        public void UpdateBattleImageVisiable(bool isShow)
        {
            if (ForestSceneData == null) { return; }

            bool targetVisible = isShow && ForestSceneData.homeDoorStoryComplete;

            // 从显示变为隐藏
            if (isBattleImageVisible && !targetVisible)
            {
                illustration.gameObject.SetActive(false);
                isBattleImageVisible = false;
                return;
            }

            // 从隐藏变为显示时，根据当前血量和受伤状态重算一次立绘状态
            if (!isBattleImageVisible && targetVisible)
            {
                illustration.gameObject.SetActive(true);

                float hpPercent = HPSlider.value;
                UpdateIllustration(hpPercent, playerLogic.ClothesBroken);
                WoundEffectAnimator.SetBool("Wound", hpPercent < 0.25f);
                UpdateAvatar(hpPercent);

                isBattleImageVisible = true;
                return;
            }

            // 状态未变化时，保持当前显示状态一致
            illustration.gameObject.SetActive(targetVisible);
            isBattleImageVisible = targetVisible;
        }

        private void UpdateWoundVisiable(bool showWound)
        {
            isShowWound = showWound;
            UpdateIllustration(HPSlider.value, playerLogic.ClothesBroken);
            UpdateAvatar(HPSlider.value);
        }

        public void SetIllustrationAlpha(float alpha)
        {
            illustration.SetAlpha(alpha);
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}