using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using System.Text;
using TMPro;
using NodeCanvas.DialogueTrees;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Game.Static.Enum.Dialogue;
using Game.GameMgr;

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

        private async UniTask Internal_OnSubtitlesRequestInfo(SubtitlesRequestInfo _info) 
        {
            var info = _info as SubtitlesRequestInfoEx;
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

/*            actorName.text = actor.name;
            actorSpeech.color = actor.dialogueColor;*/
            
            actor.RefreshAvatar(info.FaceType, (sprite) => OnGetAvatar(sprite, text));

            OnGetNewStatement?.Invoke(actor.RoleName, info.FaceType, text);

            if ( audio != null ) 
            {
                var actorSource = actor.transform != null ? actor.transform.GetComponent<AudioSource>() : null;
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
            info.Continue();
        }

        /// <summary>
        /// 播放文本的动画
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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
        /// 等待输入后进入下一个对话树节点
        /// </summary>
        /// <returns></returns>
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
            actorPortrait.gameObject.SetActive(sprite != null);
            actorPortrait.sprite = sprite;
        }

        void PlayTypeSound() {
            if ( typingSounds.Count > 0 ) {
                var sound = typingSounds[Random.Range(0, typingSounds.Count)];
                if ( sound != null ) {
                    localSource.PlayOneShot(sound, Random.Range(0.6f, 1f));
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

/*            if ( info.showLastStatement ) {
                subtitlesGroup.gameObject.SetActive(true);
                var newY = OptionContainerRtf.position.y + OptionContainerRtf.sizeDelta.y + 1;
                subtitlesGroup.position = new Vector3(subtitlesGroup.position.x, newY, subtitlesGroup.position.z);
            }*/

            if ( info.availableTime > 0 ) {
                CountDown(info).ToUniTask().Forget();
            }
        }
        /// <summary>
        /// 选项限时，超时自动选择
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
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
        /// <param name="info"></param>
        /// <param name="index"></param>
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