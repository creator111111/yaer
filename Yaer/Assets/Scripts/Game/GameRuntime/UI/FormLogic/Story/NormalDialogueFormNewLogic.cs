using Cysharp.Threading.Tasks;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using Game.GameRuntime.UI.FormLogic.Story.Base.Control;
using Game.Static.Enum.Dialogue;
using Game.Static.Path;
using GameFramework.UnityRuntime.Utility;
using NodeCanvas.DialogueTrees;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Dialogue
{
    public class NormalDialogueFormNewLogic : BaseUIFormLogic
    {
        [SerializeField]
        private DialogueTMPUGUI dialogueUI;
        [SerializeField]
        private StoryFormHistoryPage historyPage;
        [SerializeField]
        private RectTransform DialogueSceneContainer;

        public DialogueTMPUGUI DialogueUI => dialogueUI;
        public CanvasGroup dialogueUICanvasGroup => dialogueUI.subtitlesCanvasGroup;
        private Button dialogueUICanvasGroupBtn;

        [Header("Controll Button")]
        [SerializeField] private Button btnSave;
        [SerializeField] private Button btnLoad;
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnHistory;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Toggle tgAuto;
        [SerializeField] private Toggle tgSkip;

        [Header("Parameters")]
        [SerializeField] private float autoNextTime = 0.5f;

        public CanvasGroup BlackFadeCanvasGroup { get; private set; }
        public Image FullscreenRaycastMask { get; private set; }
        private ResComponentGM ResMgr;
        private UIComponentGM UIMgr;
        private StoryComponentGSM StoryMgr;
        private SettingManager SettingManager;
        private DialogueTreeController dialogueTree;

        private bool isEnd;

        private HistoryDialogueData historyDialogueData;

        private List<StoryFormPainting> CurrentPaintings;


        KeyCode nextTextKeyCode;
        KeyCode skipKeyCode;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            BlackFadeCanvasGroup = transform.Find("BlackMask").GetComponent<CanvasGroup>();
            FullscreenRaycastMask = transform.GetComponent<Image>();
            ResMgr = GameManager.GetGMComponent<ResComponentGM>();
            UIMgr = GameManager.GetGMComponent<UIComponentGM>();
            SettingManager = GameManager.GetManager<SettingManager>();
            nextTextKeyCode = GameManager.GetKeyCodeByInputType(Static.Enum.ControlInputType.NextSentence);
            skipKeyCode = GameManager.GetKeyCodeByInputType(Static.Enum.ControlInputType.SkipDialogue);
            dialogueUICanvasGroupBtn = dialogueUI.GetComponent<Button>();

            btnSave.onClick.AddListener(OnClickSave);
            btnLoad.onClick.AddListener(OnClickLoad);
            btnClose.onClick.AddListener(OnClickClose);
            btnHistory.onClick.AddListener(OnClickHistory);
            btnSettings.onClick.AddListener(OnClickSetting);
            tgAuto.onValueChanged.AddListener(OnTgAutoChanged);
            tgSkip.onValueChanged.AddListener(OnTgSkipChanged);
            GameTools.setObjectClickFunc(tgAuto.gameObject, () =>
            {
                UIUtils.PlayBtnAudio(this);
            }, null, true, 1);
            GameTools.setObjectClickFunc(tgSkip.gameObject, () =>
            {
                UIUtils.PlayBtnAudio(this);
            }, null, true, 1);

            dialogueUI.OnDialoguePreEnd += OnDialoguePreEnd;
            dialogueUI.OnDialogueEnd += OnDialogueEnd;
            dialogueUI.OnGetNewStatement += RecordHistoryDialogue;
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            SettingManager.OnTextShowTimeChange += UpdateTextShowTime;
            SettingManager.OnAutoShowTimeChange += UpdateAutoShowTime;

            var settingData = SettingManager.LoadSetting<SettingsConfigData>();
            UpdateTextShowTime(settingData.textSpeed);
            UpdateAutoShowTime(settingData.autoPlaySpeed);
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            
            if (Input.GetKeyUp(nextTextKeyCode))
            {
                // 下一文本
                dialogueUI.setAntKeyDown();
            }
            if (Input.GetKeyUp(skipKeyCode))
            {
                // 跳过文本
                tgSkip.isOn = !tgSkip.isOn;
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            ClearDialogueScene();

            SettingManager.OnTextShowTimeChange -= UpdateTextShowTime;
            SettingManager.OnAutoShowTimeChange -= UpdateAutoShowTime;
        }

        /// <summary>
        /// 加载对话资源，并开始对话
        /// </summary>
        /// <param name="dialogueID"></param>
        /// <param name="blockOtherInteraction"></param>
        public void StartDialogue(string dialogueID, bool blockOtherInteraction = true)
        {
            tgAuto.isOn = false;
            tgSkip.isOn = false;
            ResMgr.LoadAsset<GameObject>(DialoguePath.GetPath(dialogueID), (go) => OnDialogueLoaded(go, blockOtherInteraction).Forget());
        }
        /// <summary>
        /// 已经加载好的对话资源
        /// </summary>
        /// <param name="go"></param>
        /// <param name="blockOtherInteraction"></param>
        public void StartDialogue(GameObject go, bool blockOtherInteraction = true)
        {
            tgAuto.isOn = false;
            tgSkip.isOn = false;
            OnDialogueLoaded(go, blockOtherInteraction).Forget();
        }

        private async UniTask OnDialogueLoaded(GameObject dialogueGO, bool blockOtherInteraction)
        {
            ClearDialogueScene();
            await UniTask.Yield();
            dialogueTree = GameObject.Instantiate(dialogueGO, DialogueSceneContainer).GetComponent<DialogueTreeController>();
            if (dialogueTree == null)
            {
                Log.Error($"未在GameObject {dialogueGO.name}上找到DialogueTreeController");
            }
            else
            {
                BlockOtherInteraction(blockOtherInteraction);

                StoryMgr = GameManager.GetGameSceneManager().GetModule<StoryComponentGSM>();

                historyDialogueData = GameManager.GetGameSceneManager().GetArchiveData<HistoryDialogueData>();

                dialogueTree.StartDialogue();

                CurrentPaintings = dialogueTree.GetComponentsInChildren<StoryFormPainting>().ToList();
            }
        }

        private void OnDialoguePreEnd()
        {
            if (CurrentPaintings != null)
            {
                foreach (var painting in CurrentPaintings)
                {
                    painting.Fade();
                }
            }
        }

        private void OnDialogueEnd()
        {
            // 对话框控制器通知剧情管理器当前对话结束
            if (StoryMgr != null) StoryMgr.OnStoryEnd();
            if (dialogueTree != null) { dialogueTree.PauseDialogue(); }
            CloseForm();
        }

        public void ClearDialogueScene()
        {
            if (dialogueTree != null)
            {
                GameObject.Destroy(dialogueTree.gameObject);
                dialogueTree = null;
            }
        }

        public void BlockOtherInteraction(bool block)
        {
            FullscreenRaycastMask.raycastTarget = block;
        }

        public void SetDialogueOptionsGroupPosition(Vector3 worldPos)
        {
            Vector2 screenPos = GameManager.GetGameSceneManager().GetModule<CameraComponentGSM>().MainCamera.WorldToScreenPoint(worldPos);
            Debug.Log("SetUIPos:" + screenPos);

            // 将屏幕坐标转换为 UI 里的局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 localPos);

            // 设置 UI 位置
            DialogueUI.DialogueOptionsGroup.anchoredPosition = localPos;
        }

        private void RecordHistoryDialogue(DialogueRoleName roleName, DialogueFaceType faceType, string text)
        {
            historyDialogueData.HistoryDialogueInfos.Add(new HistoryDialogueInfo(roleName, faceType, text));
        }

        #region button controll
        private void OnClickSave()
        {
            UIUtils.PlayBtnAudio(this);
            if (isEnd) return;
            UIMgr.OpenUIForm(UIPrefabPath.GetUIPrefabPath("SaveGamePanel"), EUIGroup.Top, new OpenFormArgs());
        }

        private void OnClickLoad()
        {
            UIUtils.PlayBtnAudio(this);
            if (isEnd) return;
            UIMgr.OpenUIForm(UIPrefabPath.GetUIPrefabPath("LoadGamePanel"), EUIGroup.Top, new OpenFormArgs());
        }

        private void OnClickClose()
        {
            UIUtils.PlayBtnAudio(this);
            if (isEnd) return;
            dialogueUICanvasGroup.alpha = 0;
            dialogueUICanvasGroup.interactable = false;
            dialogueUI.IsAlphaHide = true;

            AllowOpenMenu(false);
            WaitRecoverDialogueUICanvasGroup().Forget();
        }

        private async UniTask WaitRecoverDialogueUICanvasGroup()
        {
            UniTask waitClickTask = dialogueUICanvasGroupBtn.OnClickAsync();
            UniTask waitEsc = WaitEscTask();
            await UniTask.WhenAny(waitClickTask, waitEsc);
            await OnClickdialogueUICanvasGroup();
        }

        private async UniTask WaitEscTask()
        {
            while (true) 
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    break;
                }
                await UniTask.Yield();
            }
        }

        private async UniTask OnClickdialogueUICanvasGroup()
        {
            dialogueUICanvasGroup.alpha = 1;
            dialogueUICanvasGroup.interactable = true;
            await UniTask.Yield();
            dialogueUI.IsAlphaHide = false;
            AllowOpenMenu(true);
        }

        private void OnClickHistory()
        {
            UIUtils.PlayBtnAudio(this);
            if (isEnd) return;

            tgAuto.isOn = false;
            tgSkip.isOn = false;

            historyPage.Show();
            // 更新历史记录
            historyPage.UpdateDialogue(historyDialogueData.HistoryDialogueInfos);
        }

        private void OnClickSetting()
        {
            UIUtils.PlayBtnAudio(this);
            if (isEnd) return;
            UIMgr.OpenUIForm(UIPrefabPath.GetUIPrefabPath("SettingPanel"), EUIGroup.Middle, new OpenFormArgs());
        }

        private void OnTgSkipChanged(bool value)
        {
            //UIUtils.PlayBtnAudio(this);
            dialogueUI.Skipping = value;
        }

        private void OnTgAutoChanged(bool value)
        {
            //UIUtils.PlayBtnAudio(this);
            dialogueUI.AutoNext = value;
        }
        #endregion

        private void UpdateTextShowTime(float time)
        {
            dialogueUI.subtitleDelays.characterDelay = time;
        }

        private void UpdateAutoShowTime(float time)
        {
            dialogueUI.subtitleDelays.finalDelay = time;
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}