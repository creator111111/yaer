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
        /// OnGetAvatar 不再激活旧 actorPortrait，避免与 Mask 双影；Loader 仍跑，供历史列表用图集。
        /// 默认 false：其它未挂 Mask 的对话面板保持旧 Portrait 行为；NormalDialogueNewPanel Prefab 显式开 true。
        /// </summary>
        [SerializeField] private bool useMaskAvatar = false;
        public SubtitleDelays subtitleDelays = new SubtitleDelays();
        public List<AudioClip> typingSounds;
        private AudioSource playSource;
        public CanvasGroup subtitlesCanvasGroup { get; private set; }

        //Group...
        [Header("Multiple Choice")]
        public RectTransform OptionContainerRtf;
        public RectTransform DialogueOptionsGroup;
        public Button optionButton;
        private Dictionary<Button, int> cachedButtons;
        private Vector2 originalSubsPosition;
        private bool isWaitingChoice;

        private AudioSource _localSource;
        private AudioSource localSource {
            get { return _localSource != null ? _localSource : _localSource = gameObject.AddComponent<AudioSource>(); }
        }
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
        }

        void OnDialoguePaused(DialogueTree dlg) {
            subtitlesGroup.gameObject.SetActive(false);
            ClearOptionBtn();
            StopAllCoroutines();
            if ( playSource != null ) playSource.Stop();
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
            if ( playSource != null ) playSource.Stop();
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
            else if (actor != null)
            {
                actor.RefreshAvatar(info.FaceType, (sprite) => OnGetAvatar(sprite, text));
                OnGetNewStatement?.Invoke(actor.RoleName, info.FaceType, text);
            }
            else
            {
                // 旧框保持关；通知 Mask Presenter 清空（role=None），避免残留上一角色立绘
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

        private void OnGetAvatar(Sprite sprite, string text)
        {
            if (actorPortrait == null)
            {
                return;
            }

            // Mask 真源：旧 Image 保持关闭；仍可写入 sprite 供调试查看，但不激活
            if (useMaskAvatar)
            {
                actorPortrait.gameObject.SetActive(false);
                if (sprite != null)
                {
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
