using System;
using System.Collections;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.PureMVC;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.Static.Path.Sound;
using UnityEngine;

namespace Game.GameRuntime.Story.ForestSceneFirstEnter
{
    /// <summary>
    /// 林恩线门口对话结束后的镜头衔接：<c>OnDialogueEnd</c> → 摄像回跟玩家 → <see cref="OnCameraMoveEnd"/>
    /// 与 <see cref="TryNotifyNodeCanvasCameraMoveEndEvent"/>（在 <c>OnCameraMoveEnd</c> 内统一发 <c>CameraMoveEnd</c> 供图「等待」节点继续）。
    /// 下一段剧情 <b>仅由 NodeCanvas 图</b> 在 <c>CameraMoveEnd</c> 之后用「触发剧情 / TriggerStoryActionTask」发起，本脚本不调用
    /// <c>Game.GameRuntime.GameSceneManager.Component.Story.StoryComponentGSM.TriggerStory</c> 打开 <c>ForestSceneYaerAfterLinEnStory</c>。
    /// <para>
    /// 健壮性：<see cref="OnCameraMoveEnd"/> 依赖 <c>SetFollow(玩家, onComplete:…)</c>；若 onComplete 长期不到，由
    /// <see cref="CoSafeTriggerCameraMoveEndIfStuck"/> 在实时秒数后强制 <see cref="OnCameraMoveEnd"/>，不播剧情。
    /// </para>
    /// </summary>
    public class ForestSceneLinEnStory: BaseSceneEntityLogic
    {
        [SerializeField] private GameObject linEn;

        [Header("调试用：CameraMoveEnd 与 NodeCanvas")]
        [Tooltip("镜头移动结束、TryNotify、OnCameraMoveEnd 入口等打印。用于确认 CameraMoveEnd 是否发出、对话「等待」是否已注册、OnCameraMoveEnd 是否被调用。")]
        [SerializeField] private bool debugLogCameraMoveEndFlow = true;

        /// <summary>
        /// 已在 <see cref="OnDialogueEnd"/> 用 <c>SetFollow(玩家, onComplete: OnCameraMoveEnd)</c> 收束一程跟拍时；若
        /// <see cref="OnCameraMoveEnd"/> 内再 <c>forceSnapToTarget:true</c> 会二次重开 SmoothDamp，易顿挫。为 true 时不再对玩家重复 SetFollow。
        /// </summary>
        private bool _skipPlayerResnapOnCameraMoveEnd;

        /// <summary>若 <see cref="SetFollow.onComplete"/> 因故不触发，在若干秒(实时)后仅强制 <see cref="OnCameraMoveEnd"/> 收尾；0=关闭。</summary>
        [SerializeField, Tooltip("onComplete 未触发时在此秒数(实时)后强制走 OnCameraMoveEnd。0=关闭。")]
        private float safeTriggerYaerAfterRealSeconds = 2f;

        private Coroutine _safetyTriggerYaerRoutine;
        private CameraComponentGSM _cameraGsmForSafety;

        /// <summary>无玩家时，下一帧起 <see cref="OnCameraMoveEnd"/> 的协程，需在新一轮 OnDialogueEnd 时停掉，避免重入。</summary>
        private Coroutine _invokeOnCameraMoveEndNextFrameRoutine;
        private CameraComponentGSM _hostForInvokeOnCameraMoveEndNextFrame;

        /// <summary>自本轮 <see cref="OnDialogueEnd"/> 起，是否已至少进入过 <see cref="OnCameraMoveEnd"/>。</summary>
        private bool _onCameraMoveEndInvokedThisChain;

        public void PrepaerPlay()
        {
            linEn.gameObject.SetActive(true);
        }

        public void LinEnStoryLinEnMove()
        {
            linEn.GetComponent<Animator>().Play("LinEnStoryLinEnMove");
        }

        public void OnDialogueEnd()
        {
            var homeBefore = SceneManager.GetArchiveData<ForestSceneData>().homeDoorStoryComplete;
            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    $"[CHAIN] OnDialogueEnd ENTRY  instanceId={GetInstanceID()}  homeDoorStoryComplete(before)={homeBefore}  " +
                    $"timeScale={Time.timeScale:F2}  unscaledTime={Time.unscaledTime:F2}",
                    this);
            }

            var cameraGsm = SceneManager.GetModule<CameraComponentGSM>();
            cameraGsm.SetLock(false);
            StopAllDialogueEndFallbackRoutines();
            _skipPlayerResnapOnCameraMoveEnd = false;
            _onCameraMoveEndInvokedThisChain = false;

            var player = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();

            if (player != null)
            {
                _skipPlayerResnapOnCameraMoveEnd = true;
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        "[ForestSceneLinEnStory] SetFollow(玩家) → onComplete: OnCameraMoveEnd",
                        this);
                }
                cameraGsm.SetFollow(player.transform, onComplete: OnCameraMoveEnd, forceSnapToTarget: true);
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        "[CHAIN] MOVE_CMD  SetFollow(玩家)  forceSnap=true  " +
                        $"CameraGsm.IsLock={cameraGsm.IsLock}  (若未解锁则 onComplete 不会触发)",
                        this);
                }
                if (safeTriggerYaerAfterRealSeconds > 0.01f)
                {
                    _cameraGsmForSafety = cameraGsm;
                    _safetyTriggerYaerRoutine = _cameraGsmForSafety.StartCoroutine(CoSafeTriggerCameraMoveEndIfStuck());
                }
            }
            else if (debugLogCameraMoveEndFlow)
            {
                Debug.LogWarning(
                    "[ForestSceneLinEnStory] player 为空：无法跟拍，1 帧后仅 OnCameraMoveEnd 兜底。",
                    this);
            }

            var homeData = SceneManager.GetArchiveData<ForestSceneData>();
            homeData.homeDoorStoryComplete = true;
            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    $"[CHAIN] 存档: homeDoorStoryComplete  {homeBefore} -> {homeData.homeDoorStoryComplete}  (本段门口剧情在流程上视为完成)",
                    this);
            }

            // 原 FindObjectOfType<ForestSceneManager>：肯尼姆等场景根物体挂载的是其它 BaseGameSceneManager 子类时会查不到并 NRE。
            // 改为当前 GameManager 登记的场景管理器根物体（执行说明 §5.1 方案 A）。
            var rootSceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (rootSceneMgr == null)
            {
                Debug.LogWarning(
                    "[ForestSceneLinEnStory] GetGameSceneManager 非 BaseGameSceneManager 或为空，跳过门口剧情结束后的 BGM/SFX 显隐。",
                    this);
            }
            else
            {
                var sounds = rootSceneMgr.gameObject.GetComponentsInChildren<SoundToggleComponent>();
                foreach (var sound in sounds)
                {
                    if (sound.GetSoundType == SoundType.BGM)
                    {
                        sound.gameObject.SetActive(false);
                    }
                    else if (sound.GetSoundType == SoundType.SFX)
                    {
                        sound.gameObject.SetActive(true);
                    }
                }
            }

            if (player == null)
            {
                if (_invokeOnCameraMoveEndNextFrameRoutine != null && _hostForInvokeOnCameraMoveEndNextFrame != null)
                {
                    _hostForInvokeOnCameraMoveEndNextFrame.StopCoroutine(_invokeOnCameraMoveEndNextFrameRoutine);
                }
                _hostForInvokeOnCameraMoveEndNextFrame = cameraGsm;
                _invokeOnCameraMoveEndNextFrameRoutine = _hostForInvokeOnCameraMoveEndNextFrame
                    .StartCoroutine(CoInvokeOnCameraMoveEndNextFrame());
            }
        }

        private void StopSafeCameraMoveEndStuckRoutine()
        {
            if (_safetyTriggerYaerRoutine == null) { return; }
            if (_cameraGsmForSafety != null) { _cameraGsmForSafety.StopCoroutine(_safetyTriggerYaerRoutine); }
            _safetyTriggerYaerRoutine = null;
            _cameraGsmForSafety = null;
        }

        private void StopInvokeOnCameraMoveEndNextFrameRoutine()
        {
            if (_invokeOnCameraMoveEndNextFrameRoutine == null) { return; }
            if (_hostForInvokeOnCameraMoveEndNextFrame != null) { _hostForInvokeOnCameraMoveEndNextFrame.StopCoroutine(_invokeOnCameraMoveEndNextFrameRoutine); }
            _invokeOnCameraMoveEndNextFrameRoutine = null;
            _hostForInvokeOnCameraMoveEndNextFrame = null;
        }

        /// <summary>停止 2s 兜底与「下帧 OnCameraMoveEnd」协程，避免多轮 OnDialogueEnd 叠挂。</summary>
        private void StopAllDialogueEndFallbackRoutines()
        {
            StopSafeCameraMoveEndStuckRoutine();
            StopInvokeOnCameraMoveEndNextFrameRoutine();
        }

        private IEnumerator CoSafeTriggerCameraMoveEndIfStuck()
        {
            yield return new WaitForSecondsRealtime(safeTriggerYaerAfterRealSeconds);
            _safetyTriggerYaerRoutine = null;
            _cameraGsmForSafety = null;
            if (debugLogCameraMoveEndFlow)
            {
                Debug.LogWarning(
                    "[ForestSceneLinEnStory] 兜底：在 " + safeTriggerYaerAfterRealSeconds
                    + "s 内未见 SetFollow.onComplete，强制调用 OnCameraMoveEnd()（下一段由 NodeCanvas 触发）。",
                    this);
            }
            OnCameraMoveEnd();
        }

        private IEnumerator CoInvokeOnCameraMoveEndNextFrame()
        {
            yield return null;
            _invokeOnCameraMoveEndNextFrameRoutine = null;
            _hostForInvokeOnCameraMoveEndNextFrame = null;
            OnCameraMoveEnd();
        }

        /// <summary>
        /// 发 <c>CameraMoveEnd</c> 给 <see cref="AnimationEventComponent"/>，与图里「等待 … 事件: CameraMoveEnd」一致（参数格式见 <see cref="AnimationEventComponent.AnimaEventTrigger"/>）。
        /// </summary>
        private void TryNotifyNodeCanvasCameraMoveEndEvent()
        {
            const string cameraMoveEndEvent = "CameraMoveEnd";

            var anima = GetComponent<AnimationEventComponent>();
            var source = "本物体 GetComponent";
            if (anima == null)
            {
                anima = GetComponentInChildren<AnimationEventComponent>(true);
                source = "子物体 GetComponentInChildren";
            }

            if (debugLogCameraMoveEndFlow)
            {
                if (anima == null)
                {
                    Debug.LogWarning(
                        "[ForestSceneLinEnStory] TryNotify: 未找到 AnimationEventComponent（本物体与子树）。不会调用 AnimaEventTrigger(CameraMoveEnd)，" +
                        "NodeCanvas 若正在「等待该事件」会一直等下去。",
                        this);
                }
                else
                {
                    var registered = anima.IsEventRegistered(cameraMoveEndEvent);
                    Debug.Log(
                        $"[ForestSceneLinEnStory] TryNotify: 使用 {source} 上的「{anima.gameObject.name}」。" +
                        $" 在触发前，事件「{cameraMoveEndEvent}」是否已被 RegisterEvent: {registered}。",
                        anima);
                    if (!registered)
                    {
                        Debug.LogWarning(
                            "[ForestSceneLinEnStory] 当前无人监听 CameraMoveEnd。接着调用 AnimaEventTrigger 时，" +
                            "可能打出「未注册动画事件: CameraMoveEnd」。请让对话流先进入「等待」节点再发事件。",
                            anima);
                    }
                }
            }

            if (anima == null) { return; }
            anima.AnimaEventTrigger(cameraMoveEndEvent + ":");

            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    "[ForestSceneLinEnStory] TryNotify: 已执行 AnimaEventTrigger(\"CameraMoveEnd:\")",
                    anima);
            }
        }

        public void OnCameraMoveEnd()
        {
            _onCameraMoveEndInvokedThisChain = true;
            StopAllDialogueEndFallbackRoutines();

            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    $"[CHAIN] OnCameraMoveEnd ENTER  frame={Time.frameCount}  time={Time.time:F3}  unscaledTime={Time.unscaledTime:F3}  " +
                    "来源=SetFollow.onComplete/兜底/下帧/图。随后 TryNotify(CameraMoveEnd) 并收尾机位。下一段由 NodeCanvas 在 CameraMoveEnd 后 Trigger。",
                    this);
            }

            // 在「机位已结束」的同一处给 NodeCanvas 发事件，避免旧版依赖 cameraTrack 协程里才发事件导致无 track 时图永远等不到
            TryNotifyNodeCanvasCameraMoveEndEvent();

            var cameraGsm = SceneManager.GetModule<CameraComponentGSM>();
            cameraGsm.SetLock(false);

            var skipResnap = _skipPlayerResnapOnCameraMoveEnd;
            _skipPlayerResnapOnCameraMoveEnd = false;
            var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            if (playerLogic != null && !skipResnap)
            {
                cameraGsm.SetFollow(playerLogic.transform, onComplete: null, forceSnapToTarget: true);
            }
            else if (playerLogic != null && skipResnap && debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    "[ForestSceneLinEnStory] OnCameraMoveEnd：已跳过对玩家的二次 SetFollow(forceSnap)。",
                    this);
            }
        }
    }
}
