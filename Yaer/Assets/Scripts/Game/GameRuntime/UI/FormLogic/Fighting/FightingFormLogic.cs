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
using System.Collections;
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

        /// <summary>
        /// 最近一次因 <c>OnStoryEnd</c> 调度「延迟恢复战斗立绘」时的 <see cref="Time.unscaledTime"/>，仅作排查时序/日志用。
        /// </summary>
        private float _lastStoryEndRestoreScheduleTime;

        /// <summary>
        /// 【重要】剧情结束（<c>StoryComponentGSM.OnStoryEnd</c>）后，不立刻把战斗立绘 SetActive，而是等待本秒数再显示。
        /// <para>
        /// <b>为何需要这段延迟（请勿随意删去或改为 0）：</b><br/>
        /// 流程上，某段对话（例如「国王演出」）正常结束时会先触发 <c>OnStoryEnd</c>，随后可能<b>立刻</b>再加载下一段对话（例如战斗教学中的主角自言自语）。
        /// 若在 <c>OnStoryEnd</c> 里立即把战斗立绘重新打开，会与「下一段对话 UI 仍占屏、或刚要再关战斗立绘」叠在同一时间窗内，造成战斗立绘短暂露一帧再被关掉，表现为立绘闪烁。<br/>
        /// 用短延迟可以错开与「下一段教学对话开始」的时序；若下一段对话在窗口内已调用隐藏战斗立绘，协程会被取消，立绘便不会误闪。
        /// </para>
        /// <para><b>维护警告：</b>若删除本延迟、或把时长改为 0，易复现「剧情衔接处战斗立绘闪烁」类回归问题，非明确需求请勿改动。</para>
        /// </summary>
        [SerializeField, Tooltip("OnStoryEnd 后延迟再显战斗立绘，用于错开与紧接教学对话的时序，避免立绘闪屏。勿随意改 0。")]
        [Range(0.3f, 0.5f)]
        private float _storyEndBattleImageDelay = 0.4f;

        /// <summary>承载上述「故事结束后再显」的协程，便于在关对话/隐藏/关界面时统一 <see cref="StopCoroutine"/> 取消，避免已失效仍执行。</summary>
        private Coroutine _deferredShowBattleImageRoutine;
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
            // 关界面时取消「故事结束后再显」的等待，与 Hide 同理，见 _storyEndBattleImageDelay 说明。
            CancelPendingStoryEndBattleImageShow();
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
            // 与强制隐藏立绘统一：避免延迟协程仍会在稍后 SetActive(战斗立绘)
            CancelPendingStoryEndBattleImageShow();
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

        /// <summary>设置、Show 等单参数入口，不经过「OnStoryEnd 后的延迟再显」逻辑（立即按条件显示/隐藏）。</summary>
        public void UpdateBattleImageVisiable(bool isShow)
        {
            UpdateBattleImageVisiable(isShow, fromStoryEndRestore: false);
        }

        /// <param name="fromStoryEndRestore">
        /// 仅当为 <c>true</c> 且目标为「应显示」时，不立刻 <c>SetActive(战斗立绘)</c>，而进入协程，在 <see cref="_storyEndBattleImageDelay"/> 秒后再显。
        /// 由 <c>StoryComponentGSM</c> 在 <b>整段故事结束、恢复立绘</b> 时传入，用于与紧接教学对话错开，防闪烁；其它入口应传 <c>false</c>。
        /// <para>【维护警告】勿随意把「从 OnStoryEnd 调用的路径」改成单参数，否则会失去上述延迟，易再现立绘与对话 UI 的显示冲突。</para>
        /// </param>
        public void UpdateBattleImageVisiable(bool isShow, bool fromStoryEndRestore)
        {
            if (ForestSceneData == null) { return; }

            bool targetVisible = isShow && ForestSceneData.homeDoorStoryComplete;

            // 目标为不可见：取消可能仍在等待的「故事结束补显」协程（例如下一段对话已开始并再次关战斗立绘）
            if (!targetVisible)
            {
                CancelPendingStoryEndBattleImageShow();
            }

            // 从显示变为隐藏
            if (isBattleImageVisible && !targetVisible)
            {
                illustration.gameObject.SetActive(false);
                isBattleImageVisible = false;
                return;
            }

            if (!isBattleImageVisible && targetVisible)
            {
                // 【核心】从隐藏切到显示：非「剧情刚结束补显」则立即应用；若来自 OnStoryEnd，则必须走协程延迟，见 _storyEndBattleImageDelay 注释说明。
                if (fromStoryEndRestore)
                {
                    _lastStoryEndRestoreScheduleTime = Time.unscaledTime;
                    CancelPendingStoryEndBattleImageShow();
                    // 此处不调用 ApplyBattleImageShowNow，避免与紧接 OnStoryEnd 后立刻开启的教学/自言自语对话抢同一帧的显示权导致闪屏。
                    _deferredShowBattleImageRoutine = StartCoroutine(CoDeferredShowBattleImageAfterStory());
                    return;
                }

                CancelPendingStoryEndBattleImageShow();
                ApplyBattleImageShowNow();
                return;
            }

            illustration.gameObject.SetActive(targetVisible);
            isBattleImageVisible = targetVisible;
        }

        /// <summary>
        /// 取消「OnStoryEnd 后延迟再显」的协程。典型触发：下一段对话 OnStoryPrefabLoad 里会先把战斗立绘关为不可见，此处必须停掉，否则延迟结束仍会再打开立绘并闪烁。
        /// </summary>
        private void CancelPendingStoryEndBattleImageShow()
        {
            if (_deferredShowBattleImageRoutine == null) { return; }
            StopCoroutine(_deferredShowBattleImageRoutine);
            _deferredShowBattleImageRoutine = null;
        }

        private void ApplyBattleImageShowNow()
        {
            illustration.gameObject.SetActive(true);

            float hpPercent = HPSlider.value;
            UpdateIllustration(hpPercent, playerLogic.ClothesBroken);
            WoundEffectAnimator.SetBool("Wound", hpPercent < 0.25f);
            UpdateAvatar(hpPercent);

            isBattleImageVisible = true;
        }

        /// <summary>
        /// 在 <c>OnStoryEnd</c> 决定「应显示战斗立绘」后，等待 <see cref="_storyEndBattleImageDelay"/> 再真正执行 <see cref="ApplyBattleImageShowNow"/>。
        /// <para>
        /// 与「国王演出结束 → 立刻教学对话」类流程配合：用 Realtime 等待错开一帧/数帧，使紧接其后的对话已调用关闭战斗立绘时，上文的 <see cref="CancelPendingStoryEndBattleImageShow"/> 已停掉本协程，从而不再误显。<br/>
        /// <b>勿删除此协程与延迟</b>，否则 <c>OnStoryEnd</c> 与下一段对话的立绘显隐会再次打架，易复现闪烁。
        /// </para>
        /// </summary>
        private IEnumerator CoDeferredShowBattleImageAfterStory()
        {
            yield return new WaitForSecondsRealtime(_storyEndBattleImageDelay);
            _deferredShowBattleImageRoutine = null;
            if (ForestSceneData == null) { yield break; }
            var configData = settingManager != null ? settingManager.LoadSetting<SettingsConfigData>() : null;
            if (configData == null) { yield break; }
            if (!configData.showBattleImage || !ForestSceneData.homeDoorStoryComplete) { yield break; }
            if (isBattleImageVisible) { yield break; }
            ApplyBattleImageShowNow();
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