using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.UI.FormLogic.Story;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
using Game.GameRuntime.Story;
using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.Story.NodeCanvasExtend
{

    public class DialogueTMPUGUI : MonoBehaviour, IPointerClickHandler
    {

        [System.Serializable]
        public class SubtitleDelays
        {
            public float characterDelay = 0.05f;
            public float sentenceDelay = 0.5f;
            public float commaDelay = 0.1f;
            public float finalDelay = 1.2f;
        }

        //Options...
        /// <summary>
        /// 点击后跳过文本动画
        /// </summary>
        [Header("Input Options")]
        public bool skipOnInput;
        /// <summary>
        /// 控制自动播放
        /// </summary>
        public bool AutoNext;

        //Group...
        [Header("Subtitles")]
        public RectTransform subtitlesGroup;
        public TextMeshProUGUI actorSpeech;
        public TextMeshProUGUI actorName;
        public Image actorPortrait;
        /// <summary>
        /// true = 字幕头像以 Mask 立绘为真源（DialogueMaskAvatarPresenter）。
        /// Mask 已支持角色：旧 actorPortrait 保持关，避免双影；Loader 仍跑，供历史列表用图集。
        /// 白名单旧角色（King/Lai/Xiaer/LinEn）：Mask 无 Painting 时按 A′ 回亮图集 Portrait（0911）。
        /// 默认 false：其它未挂 Mask 的对话面板保持旧 Portrait 行为；NormalDialogueNewPanel Prefab 显式开 true。
        /// </summary>
        [SerializeField] private bool useMaskAvatar = false;

        /// <summary>
        /// 本句字幕正在等待的 Avatar 角色。异步 Loader 晚到时若已切句，则禁止再抢 actorPortrait Active（防串脸）。
        /// 旁白 / 店 / 村长专用口写 None。
        /// </summary>
        DialogueRoleName _subtitleAvatarRole = DialogueRoleName.None;

        public SubtitleDelays subtitleDelays = new SubtitleDelays();
        public List<AudioClip> typingSounds;
        private AudioSource playSource;
        public CanvasGroup subtitlesCanvasGroup { get; private set; }

        /// <summary>
        /// Mask 模式下仍走旧图集 Portrait 的角色白名单（方案 A′）。
        /// 不含 Goblin*（产品不要求）；不含 Yaer/Gusha/Amy/Aliy/Chief（走 Mask）。
        /// 替代方案 A（凡 Resolve=null 就亮）会把哥布林也亮出来，本期不用。
        /// </summary>
        static bool IsAtlasPortraitFallbackRole(DialogueRoleName role)
        {
            switch (role)
            {
                case DialogueRoleName.King:
                case DialogueRoleName.Lai:
                case DialogueRoleName.Xiaer:
                case DialogueRoleName.LinEn:
                    return true;
                default:
                    return false;
            }
        }

        //Group...
        [Header("Multiple Choice")]
        public RectTransform OptionContainerRtf;
        public RectTransform DialogueOptionsGroup;
        public Button optionButton;
        private Dictionary<Button, int> cachedButtons;
        private Vector2 originalSubsPosition;
        private bool isWaitingChoice;

        private AudioSource _localSource;
        /// <summary>
        /// 对话框自带人声源。惰性 AddComponent 时必须关 playOnAwake：
        /// Unity 默认 true，对白结束若只 Stop 不清 clip，关壳再开会自动复读最后一句（0911 H1）。
        /// </summary>
        private AudioSource localSource
        {
            get
            {
                if (_localSource == null)
                {
                    _localSource = gameObject.AddComponent<AudioSource>();
                    _localSource.playOnAwake = false;
                }
                return _localSource;
            }
        }

        /// <summary>
        /// 本场对白实际播过人声的 AudioSource（含 Actor 源与 localSource）。
        /// 结束时按名单 Stop+清 clip，避免只清当前 playSource 漏掉早期 Actor 源。
        /// 禁止全局 FindObjectsOfType 乱停。
        /// </summary>
        readonly List<AudioSource> _voiceSourcesThisDialogue = new List<AudioSource>();

        /// <summary>
        /// 是否为跳过当前剧情状态
        /// </summary>
        [HideInInspector]
        public bool Skipping;
        public bool IsAlphaHide = false;
        private bool anyKeyDown;

        public event System.Action<DialogueRoleName, DialogueFaceType, string> OnGetNewStatement;

        public event System.Action OnDialoguePreEnd;
        /// <summary>
        /// 对话框UI广播对话结束事件
        /// </summary>
        public event System.Action OnDialogueEnd;

        public void OnPointerClick(PointerEventData eventData) => anyKeyDown = true && !IsAlphaHide;

        public void setAntKeyDown(bool isKeyDown=true) { anyKeyDown = isKeyDown; }
        void LateUpdate()
        {
            if (Skipping)
            {
                anyKeyDown = true;
            }
            else
            {
                anyKeyDown = false;
            } 
        }


        void Awake() 
        { 
            Subscribe(); 
            Hide();
            subtitlesCanvasGroup = subtitlesGroup.GetComponent<CanvasGroup>();
        }
        void OnEnable() 
        { 
            UnSubscribe(); 
            Subscribe(); 
        }

        void OnDisable() 
        { 
            UnSubscribe(); 
            ClearOptionBtn(); 
        }

        void Subscribe() {
            DialogueTree.OnDialogueStarted += OnDialogueStarted;
            DialogueTree.OnDialoguePaused += OnDialoguePaused;
            DialogueTree.OnDialogueFinished += OnDialogueFinished;
            DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;
        }

        void UnSubscribe() {
            DialogueTree.OnDialogueStarted -= OnDialogueStarted;
            DialogueTree.OnDialoguePaused -= OnDialoguePaused;
            DialogueTree.OnDialogueFinished -= OnDialogueFinished;
            DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
        }

        void Hide() {
            subtitlesGroup.gameObject.SetActive(false);
            DialogueOptionsGroup.gameObject.SetActive(false);
            optionButton.gameObject.SetActive(false);
            originalSubsPosition = subtitlesGroup.anchoredPosition;
        }

        void OnDialogueStarted(DialogueTree dlg) {
            subtitlesCanvasGroup.DOKill();
            DialogueOptionsGroup.gameObject.SetActive(false);
            // 没有淡入节点的图（如 Village_ShopRepeat）走不到淡入里的藏起。
            // 面板复用时 Presenter.Awake 不再跑，必须在对话一开始藏掉上一场的脸。
            // 第一句仍在 OnSubtitlesRequest 里 Apply，换脸顺序不变。
            HideLeftoverMaskAvatars();
        }

        /// <summary>
        /// 新一场对话开始时藏 Mask 小头像。
        /// Presenter 挂在本组件子层级（GetComponentInParent 能找到本组件）。
        /// 旧面板没有 Presenter 就跳过，不影响旧头像图。
        /// 替代方案：等第一句再藏——框已经打开时会先闪上一张脸。
        /// </summary>
        void HideLeftoverMaskAvatars()
        {
            var presenter = GetComponentInChildren<DialogueMaskAvatarPresenter>(true);
            if (presenter == null)
            {
                return;
            }

            presenter.HideAllMaskAvatars();
        }

        void OnDialoguePaused(DialogueTree dlg) {
            subtitlesGroup.gameObject.SetActive(false);
            ClearOptionBtn();
            StopAllCoroutines();
            // 暂停也须清 clip：PauseDialogue 后关壳再开同样会 PlayOnAwake 复读
            StopDialogueVoiceClean();
        }
        /// <summary>
        /// 对话树组件通知对话框UI对话结束事件
        /// </summary>
        void OnDialogueFinished(DialogueTree dlg) {
            DialogueEndSubtitlesCanvasGroupFade().Forget();
            DialogueOptionsGroup.gameObject.SetActive(false);
            if ( cachedButtons != null ) {
                foreach ( var tempBtn in cachedButtons.Keys ) {
                    if ( tempBtn != null ) {
                        Destroy(tempBtn.gameObject);
                    }
                }
                cachedButtons = null;
            }
            StopAllCoroutines();
            // 真源清理：须在 CloseForm/SetActive(false) 之前清掉残留 clip
            StopDialogueVoiceClean();
        }

        /// <summary>
        /// 对白人声停干净：Stop + clip=null + playOnAwake=false。
        /// 根因修复（0911）：残留 clip + 默认 PlayOnAwake，关壳再 Open 会复读最后一句 VO。
        /// 在 Finished/Paused、StartDialogue 防御、以及 Form 的 OnDialogueEnd 兜底调用。
        /// 替代方案 B（二期）：VO 只走独立人声通道，不写 Actor/UI 默认源。
        /// </summary>
        public void StopDialogueVoiceClean()
        {
            for (int i = 0; i < _voiceSourcesThisDialogue.Count; i++)
            {
                CleanVoiceSource(_voiceSourcesThisDialogue[i]);
            }
            _voiceSourcesThisDialogue.Clear();

            CleanVoiceSource(playSource);
            CleanVoiceSource(_localSource);
            playSource = null;
        }

        void RegisterVoiceSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            if (!_voiceSourcesThisDialogue.Contains(source))
            {
                _voiceSourcesThisDialogue.Add(source);
            }
        }

        static void CleanVoiceSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.playOnAwake = false;
        }
        /// <summary>
        /// 对话结束后subtitlesCanvasGroup淡出
        /// </summary>
        /// <returns></returns>
        private async UniTask DialogueEndSubtitlesCanvasGroupFade()
        {
            OnDialoguePreEnd?.Invoke();
            subtitlesGroup.gameObject.SetActive(true);
            subtitlesCanvasGroup.DOKill();
            await subtitlesCanvasGroup.DOFade(0, 0.7f).AsyncWaitForCompletion();
            subtitlesGroup.gameObject.SetActive(false);
            OnDialogueEnd?.Invoke();
        }

        ///----------------------------------------------------------------------------------------------

        void OnSubtitlesRequest(SubtitlesRequestInfo info) {
            Internal_OnSubtitlesRequestInfo(info).Forget();
        }

        /// <summary>
        /// 与 NodeCanvas 官方 DialogueUGUI 一致：本协程/UniTask 只负责本句的展示与等待输入/自动，结束时调用
        /// <see cref="SubtitlesRequestInfo.Continue"/> 把“下一句/下一节点”交还给 <see cref="DialogueTree"/>，不在此脚本内插入额外分支关窗、跳转或强制取消。
        /// </summary>
        private async UniTask Internal_OnSubtitlesRequestInfo(SubtitlesRequestInfo _info) 
        {
            var info = _info as SubtitlesRequestInfoEx;
            if (info == null) { return; }

            string text = "";
            // 处理文本翻译问题
            var languageType = GameManager.Instance.language;
            if (languageType == LanguageEnumType.Chinese) { text = info.statement.text; }
            else if (languageType == LanguageEnumType.English) { text = info.statement.text_en; }
            else if (languageType == LanguageEnumType.Japanese) { text = info.statement.text_jp; }

            var audio = info.statement.audio;
            var actor = info.actor as DialogueActorEx;

            subtitlesGroup.gameObject.SetActive(true);
            subtitlesGroup.anchoredPosition = originalSubsPosition;
            actorSpeech.text = "";

            // 与官方 DialogueUGUI 一致：每句刷新演员名（渐入阶段会先清空，避免 Prefab 残留「雅尔」）
            if (actorName != null)
            {
                actorName.text = actor != null ? actor.name : string.Empty;
            }

            // 旁白「—」等未绑定 DialogueActorEx 的 dummy Actor：仅字幕、不刷立绘，避免 RefreshAvatar 空引用卡死
            if (info.UseShopkeeperPortrait)
            {
                // 店旗走 Mask：作废任何在途图集回调，并关旧 Portrait，防与 Merchant 双影
                _subtitleAvatarRole = DialogueRoleName.None;
                var shopFaceController = ShopkeeperFaceRegistry.Instance;
                if (shopFaceController != null)
                {
                    shopFaceController.Apply(info.ShopBody, info.ShopFace);
                }
                else
                {
                    Debug.LogWarning("[DialogueTMPUGUI] 店句但 ShopkeeperFaceController 未注册。", this);
                }

                if (actorPortrait != null)
                {
                    actorPortrait.gameObject.SetActive(false);
                }

                var maskPresenter = GetComponentInChildren<DialogueMaskAvatarPresenter>(true);
                if (maskPresenter != null)
                {
                    maskPresenter.ApplyShopkeeperPortrait(info.ShopBody, info.ShopFace);
                }

                OnGetNewStatement?.Invoke(DialogueRoleName.None, DialogueFaceType.None, text);
            }
            else if (info.UseChiefPortrait)
            {
                // 门口村长：DialogueSceneContainer 下大立绘 + Mask 同帧 Apply；Invoke(None) 写历史勿关刚亮的脸
                _subtitleAvatarRole = DialogueRoleName.None;
                ApplyChiefBigPortrait(info.ChiefFace);

                if (actorPortrait != null)
                {
                    actorPortrait.gameObject.SetActive(false);
                }

                var maskPresenter = GetComponentInChildren<DialogueMaskAvatarPresenter>(true);
                if (maskPresenter != null)
                {
                    maskPresenter.ApplyChiefPortrait(info.ChiefFace);
                }

                OnGetNewStatement?.Invoke(DialogueRoleName.None, DialogueFaceType.None, text);
            }
            else if (actor != null)
            {
                // 记录本句 Role：异步 OnGetAvatar 晚到时与此比对，避免串脸抢槽
                var roleForAvatar = actor.RoleName;
                _subtitleAvatarRole = roleForAvatar;

                // Mask 句（雅儿等）：本帧立刻关旧 Portrait，勿等 Loader，否则上一句父亲脸会与 Mask 叠一帧
                // 白名单句：等 sprite 再亮；上一句若也是白名单可短暂留旧脸直到新图到
                if (useMaskAvatar
                    && actorPortrait != null
                    && !IsAtlasPortraitFallbackRole(roleForAvatar))
                {
                    actorPortrait.gameObject.SetActive(false);
                }

                actor.RefreshAvatar(info.FaceType, (sprite) => OnGetAvatar(sprite, text, roleForAvatar));
                OnGetNewStatement?.Invoke(roleForAvatar, info.FaceType, text);
            }
            else
            {
                // 旁白 / 无 Actor：关旧槽 + 作废在途回调；通知 Mask Presenter 清空
                _subtitleAvatarRole = DialogueRoleName.None;
                if (actorPortrait != null)
                {
                    actorPortrait.gameObject.SetActive(false);
                }
                OnGetNewStatement?.Invoke(DialogueRoleName.None, DialogueFaceType.None, text);
            }

            if ( audio != null ) 
            {
                var actorSource = actor != null && actor.transform != null
                    ? actor.transform.GetComponent<AudioSource>()
                    : null;
                playSource = actorSource != null ? actorSource : localSource;
                // 登记本场源，结束按名单清；播前关 playOnAwake，防中途关开壳误触
                RegisterVoiceSource(playSource);
                playSource.playOnAwake = false;
                playSource.clip = audio;
                playSource.Play();
                actorSpeech.text = text;

                UniTask audioEndTask = UniTask.WaitForSeconds(audio.length);
                UniTask waitForInput = UniTask.WaitUntil(() => skipOnInput && anyKeyDown);
                await UniTask.WhenAny(audioEndTask, waitForInput);
            }
            else
            {
                await TextAnimation(text);
            }

            await WaitForInputToMoveNext();

            subtitlesGroup.gameObject.SetActive(false);
            if (info.Continue != null)
            {
                try
                {
                    info.Continue();
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[DialogueTMPUGUI] Continue 时异常（可能对话树已停）：" + e.Message, this);
                }
            }
        }

        /// <summary>
        /// 门口村长大立绘：只在 DialogueSceneContainer 下找 <see cref="ChiefMaskPainting"/>，
        /// 避免 GetComponentInChildren 误伤 Mask 内同脚本实例。
        /// 替代方案：对话级 Registry（店式）——多一处全局态，门口单 Prefab 不必。
        /// </summary>
        private void ApplyChiefBigPortrait(ChiefFaceType face)
        {
            var form = GetComponentInParent<NormalDialogueFormNewLogic>();
            var bigChief = form != null ? form.FindInDialogueScene<ChiefMaskPainting>() : null;
            if (bigChief == null)
            {
                Debug.LogWarning(
                    "[DialogueTMPUGUI] 村长门口句但 DialogueSceneContainer 下无 ChiefPainting（ChiefMaskPainting）。",
                    this);
                return;
            }

            bigChief.gameObject.SetActive(true);
            var cg = bigChief.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha < 1f)
            {
                cg.alpha = 1f;
            }

            bigChief.Apply(face);
        }

        /// <summary>
        /// 播放文本的动画
        /// </summary>
        private async UniTask TextAnimation(string text)
        {
            var stringBuilder = new StringBuilder();
            var inputDown = false;
            if (skipOnInput)
            {
                CheckInput(() => { inputDown = true; }).Forget();
            }

            float characDelay = subtitleDelays.characterDelay;
            characDelay = Mathf.Max(characDelay, 0);

            for (int i = 0; i < text.Length; i++)
            {
                if (skipOnInput && inputDown)
                {
                    actorSpeech.text = text;
                    await UniTask.Yield();
                    break;
                }

                if (subtitlesGroup.gameObject.activeSelf == false)
                {
                    return;
                }

                char c = text[i];
                stringBuilder.Append(c);
                await UniTask.WaitForSeconds(characDelay);
                PlayTypeSound();
                actorSpeech.text = stringBuilder.ToString();

                if (c == '.' || c == '!' || c == '?')
                {
                    await UniTask.WaitForSeconds(subtitleDelays.sentenceDelay);
                }
                if (c == ',')
                {   
                    await UniTask.WaitForSeconds(subtitleDelays.commaDelay);
                }
            }
            if (!Skipping && AutoNext)
            {
                await UniTask.WaitForSeconds(subtitleDelays.finalDelay);
            }
        }
        /// <summary>
        /// 等待输入后进入下一个对话树节点（与 NodeCanvas 预期一致，由 <see cref="AutoNext"/> / 键入推进）
        /// </summary>
        private async UniTask WaitForInputToMoveNext()
        {
            if (!AutoNext)
            {
                await UniTask.WaitUntil(() =>
                {
                    if (!gameObject.activeInHierarchy) return true;
                    return AutoNext || anyKeyDown;
                }, PlayerLoopTiming.PreLateUpdate);
            }
            await UniTask.Yield();
        }

        /// <summary>
        /// Loader 回调。useMaskAvatar 时：
        /// - 白名单四人（A′）且 sprite≠null → 亮旧 Portrait（Mask 无对应 Painting）；
        /// - 其余角色 → Portrait 保持关（Mask / 旁白 / 哥布林等）。
        /// role 用于异步防串：已切到别句则不再 SetActive。
        /// </summary>
        private void OnGetAvatar(Sprite sprite, string text, DialogueRoleName role)
        {
            if (actorPortrait == null)
            {
                return;
            }

            // 句已切换（含切到旁白/店/村长）：丢弃过期回调，禁止残留抢亮
            if (role != _subtitleAvatarRole)
            {
                return;
            }

            if (useMaskAvatar)
            {
                // A′：仅 King/Lai/Xiaer/LinEn 在有图时回亮旧槽；Mask 角色与哥布林等保持关
                if (IsAtlasPortraitFallbackRole(role) && sprite != null)
                {
                    actorPortrait.sprite = sprite;
                    actorPortrait.gameObject.SetActive(true);
                    return;
                }

                actorPortrait.gameObject.SetActive(false);
                if (sprite != null)
                {
                    // 仍写入，便于 Hierarchy 调试 / 与历史图集同源
                    actorPortrait.sprite = sprite;
                }
                return;
            }

            actorPortrait.gameObject.SetActive(sprite != null);
            actorPortrait.sprite = sprite;
        }

        void PlayTypeSound() {
            if ( typingSounds.Count > 0 ) {
                var sound = typingSounds[UnityEngine.Random.Range(0, typingSounds.Count)];
                if ( sound != null ) {
                    localSource.PlayOneShot(sound, UnityEngine.Random.Range(0.6f, 1f));
                }
            }
        }

        private async UniTask CheckInput(System.Action Callback) 
        {
            await UniTask.WaitUntil(() => anyKeyDown, PlayerLoopTiming.PreLateUpdate);
            Callback?.Invoke();
        }

        ///----------------------------------------------------------------------------------------------

        void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info) 
        {
            DialogueOptionsGroup.gameObject.SetActive(true);
            var buttonHeight = optionButton.GetComponent<RectTransform>().rect.height;
            OptionContainerRtf.sizeDelta = new Vector2(OptionContainerRtf.sizeDelta.x, ( info.options.Values.Count * buttonHeight ) + 20);

            cachedButtons = new Dictionary<Button, int>();
            int i = 0;
            // 处理文本翻译问题
            var languageType = GameManager.Instance.language;
            
            foreach ( KeyValuePair<IStatement, int> pair in info.options ) {
                var btn = (Button)Instantiate(optionButton);
                btn.gameObject.SetActive(true);
                btn.transform.SetParent(OptionContainerRtf.transform, false);
                btn.transform.localPosition = (Vector3)optionButton.transform.localPosition - new Vector3(0, buttonHeight * i, 0);
                var statement = pair.Key;
                var text = "";
                if (languageType == LanguageEnumType.Chinese) { text = statement.text; }
                else if (languageType == LanguageEnumType.English) { text = statement.text_en; }
                else if (languageType == LanguageEnumType.Japanese) { text = statement.text_jp; }
                btn.GetComponentInChildren<TextMeshProUGUI>().text = text;
                cachedButtons.Add(btn, pair.Value);
                btn.onClick.AddListener(() => { Finalize(info, cachedButtons[btn]); });
                i++;
            }

            if ( info.availableTime > 0 ) {
                CountDown(info).ToUniTask().Forget();
            }
        }
        /// <summary>
        /// 选项限时，超时自动选择
        /// </summary>
        IEnumerator CountDown(MultipleChoiceRequestInfo info) {
            isWaitingChoice = true;
            var timer = 0f;
            while ( timer < info.availableTime ) {
                if ( isWaitingChoice == false ) {
                    yield break;
                }
                timer += Time.deltaTime;
                SetMassAlpha(OptionContainerRtf, Mathf.Lerp(1, 0, timer / info.availableTime));
                yield return null;
            }

            if ( isWaitingChoice ) {
                Finalize(info, info.options.Values.Last());
            }
        }
        /// <summary>
        /// 选择选项
        /// </summary>
        void Finalize(MultipleChoiceRequestInfo info, int index) {
            isWaitingChoice = false;
            SetMassAlpha(OptionContainerRtf, 1f);
            DialogueOptionsGroup.gameObject.SetActive(false);
            subtitlesGroup.gameObject.SetActive(false);
            ClearOptionBtn();
            info.SelectOption(index);
        }

        private void ClearOptionBtn()
        {
            DialogueOptionsGroup.gameObject.SetActive(false);
            if (cachedButtons != null)
            {
                foreach (var tempBtn in cachedButtons.Keys)
                {
                    Destroy(tempBtn.gameObject);
                }
            }
        }

        void SetMassAlpha(RectTransform root, float alpha) {
            foreach ( var graphic in root.GetComponentsInChildren<CanvasRenderer>() ) {
                graphic.SetAlpha(alpha);
            }
        }
    }
}
