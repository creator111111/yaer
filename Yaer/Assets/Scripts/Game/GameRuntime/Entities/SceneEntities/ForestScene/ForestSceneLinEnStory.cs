using System;
using System.Collections;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Path;
using Game.Static.Path.Sound;
using UnityEngine;

namespace Game.GameRuntime.Story.ForestSceneFirstEnter
{
    /// <summary>
    /// 林恩线门口对话结束后的镜头衔接：<c>OnDialogueEnd</c> → 摄像回跟玩家 → <see cref="OnCameraMoveEnd"/>
    /// 与 <see cref="EnsureNotifyCameraMoveEndWhenRegistered"/>（在机位收束后发 <c>CameraMoveEnd</c>，供图「等待」节点继续）。
    /// 下一段剧情 <b>仅由 NodeCanvas 图</b> 在 <c>CameraMoveEnd</c> 之后用「触发剧情 / TriggerStoryActionTask」发起，本脚本不调用
    /// <c>Game.GameRuntime.GameSceneManager.Component.Story.StoryComponentGSM.TriggerStory</c> 打开 <c>ForestSceneYaerAfterLinEnStory</c>。
    /// <para>
    /// 健壮性：<see cref="OnCameraMoveEnd"/> 依赖 <c>SetFollow(玩家, onComplete:…)</c>；若 onComplete 长期不到，由
    /// <see cref="CoSafeTriggerCameraMoveEndIfStuck"/> 在实时秒数后强制 <see cref="OnCameraMoveEnd"/>，不播剧情。
    /// </para>
    /// <para>
    /// 0912 产品纠偏：<b>M3</b> 保留场景默认 <c>smoothTime</c> 平滑手推 +「已 Register 再 Notify」；
    /// <b>M1</b>（本轮迭加）：手推前短黑幕掩护、到位后揭幕再 Notify，消可见滑动闪；禁止再 A′ <c>smoothTime=0</c>。
    /// B′ 只挡二次 snap，不得吞唯一有效 Notify。禁止套用龙宫 Stairs 定格。
    /// </para>
    /// </summary>
    public class ForestSceneLinEnStory: BaseSceneEntityLogic
    {
        private const string CameraMoveEndEventName = "CameraMoveEnd";

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
        [SerializeField, Tooltip("onComplete 未触发时在此秒数(实时)后强制走 OnCameraMoveEnd。0=关闭。从开始手推起算。")]
        private float safeTriggerYaerAfterRealSeconds = 2.5f;

        /// <summary>
        /// 手推结束后若图尚未 Register <c>CameraMoveEnd</c>，最长等待秒数(实时)再强制发事件。
        /// 原因：错过的事件无 sticky；发早则 id41 永久傻等。替代：固定 Wait 硬匹配机位（SPEC 禁止）。
        /// </summary>
        [SerializeField, Tooltip("等图 Register CameraMoveEnd 的最长实时秒数；超时仍发并打 Error。")]
        private float waitRegisterCameraMoveEndRealSeconds = 3f;

        [Header("M1 手推黑幕掩护（消闪）")]
        [Tooltip("开：全黑后再手推回玩家，到位揭幕再发 CameraMoveEnd。关：纯 M3（移动可见，可能闪）。禁止用 smoothTime=0 替代。")]
        [SerializeField] private bool useShortBlackCoverForCameraReturn = true;

        [SerializeField, Tooltip("掩护黑幕淡入秒数（短一点更干脆）。")]
        private float cameraReturnBlackShowSeconds = 0.15f;

        [SerializeField, Tooltip("手推到位后黑幕淡出秒数。")]
        private float cameraReturnBlackHideSeconds = 0.2f;

        private Coroutine _safetyTriggerYaerRoutine;
        private CameraComponentGSM _cameraGsmForSafety;

        /// <summary>无玩家时，下一帧起 <see cref="OnCameraMoveEnd"/> 的协程，需在新一轮 OnDialogueEnd 时停掉，避免重入。</summary>
        private Coroutine _invokeOnCameraMoveEndNextFrameRoutine;
        private CameraComponentGSM _hostForInvokeOnCameraMoveEndNextFrame;

        /// <summary>等待 id41 Register 后再发 <c>CameraMoveEnd</c> 的协程。</summary>
        private Coroutine _waitNotifyRoutine;
        private CameraComponentGSM _hostForWaitNotify;

        /// <summary>
        /// B′（收紧语义）：本轮是否已处理过「回玩家 snap / 解锁」业务。
        /// 仅为 true 时跳过二次 SetFollow，<b>不</b>阻止补发尚未送达的 Notify。
        /// </summary>
        private bool _cameraMoveSnapHandledThisChain;

        /// <summary>本轮是否已成功（或超时强制）把 <c>CameraMoveEnd</c> 投递给已注册监听。</summary>
        private bool _cameraMoveEndEventDelivered;

        /// <summary>M1：掩护手推的系统 BlackPanel；手推期间不为 null。</summary>
        private BlackFormLogic _cameraReturnCoverBlack;

        /// <summary>M1：正在 CloseFormFade，揭幕完成前勿抢发 Notify / 勿叠关。</summary>
        private bool _cameraReturnCoverClosing;

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
                    $"timeScale={Time.timeScale:F2}  unscaledTime={Time.unscaledTime:F2}  " +
                    $"M1_cover={useShortBlackCoverForCameraReturn}",
                    this);
            }

            var cameraGsm = SceneManager.GetModule<CameraComponentGSM>();
            cameraGsm.SetLock(false);
            StopAllDialogueEndFallbackRoutines();
            AbortCameraReturnCoverBlackIfAny();
            _skipPlayerResnapOnCameraMoveEnd = false;
            _cameraMoveSnapHandledThisChain = false;
            _cameraMoveEndEventDelivered = false;

            var player = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();

            // 存档 / BGM 与镜头收束解耦：先落档，再开黑幕或手推（避免等黑幕才写 complete）
            var homeData = SceneManager.GetArchiveData<ForestSceneData>();
            homeData.homeDoorStoryComplete = true;
            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    $"[CHAIN] 存档: homeDoorStoryComplete  {homeBefore} -> {homeData.homeDoorStoryComplete}  (本段门口剧情在流程上视为完成)",
                    this);
            }

            ApplyPostLinEnSoundToggles();

            if (player != null)
            {
                _skipPlayerResnapOnCameraMoveEnd = true;

                if (useShortBlackCoverForCameraReturn)
                {
                    // M1：先淡黑，全黑后再手推（移动仍发生，只是观众看不见滑动闪）
                    if (debugLogCameraMoveEndFlow)
                    {
                        Debug.Log(
                            "[CHAIN] M1 Open BlackPanel → onShowEnd 再 SetFollow（保留 smoothTime 手推）",
                            this);
                    }
                    OpenCameraReturnCoverBlack(black =>
                    {
                        _cameraReturnCoverBlack = black;
                        BeginSetFollowReturnToPlayer(cameraGsm, player);
                    });
                }
                else
                {
                    // 纯 M3：无黑幕，手推可见（可能闪）
                    BeginSetFollowReturnToPlayer(cameraGsm, player);
                }
            }
            else if (debugLogCameraMoveEndFlow)
            {
                Debug.LogWarning(
                    "[ForestSceneLinEnStory] player 为空：无法跟拍，1 帧后仅 OnCameraMoveEnd 兜底。",
                    this);
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

        /// <summary>门口剧情结束后的 BGM/SFX 显隐（与镜头收束无关）。</summary>
        private void ApplyPostLinEnSoundToggles()
        {
            // 原 FindObjectOfType<ForestSceneManager>：肯尼姆等场景根物体挂载的是其它 BaseGameSceneManager 子类时会查不到并 NRE。
            // 改为当前 GameManager 登记的场景管理器根物体（执行说明 §5.1 方案 A）。
            var rootSceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (rootSceneMgr == null)
            {
                Debug.LogWarning(
                    "[ForestSceneLinEnStory] GetGameSceneManager 非 BaseGameSceneManager 或为空，跳过门口剧情结束后的 BGM/SFX 显隐。",
                    this);
                return;
            }

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

        /// <summary>
        /// 真正开始「镜头回玩家」手推。须在已解锁且（M1 下）已全黑后调用。
        /// </summary>
        private void BeginSetFollowReturnToPlayer(CameraComponentGSM cameraGsm, PlayerLogic player)
        {
            if (player == null || cameraGsm == null)
            {
                return;
            }

            if (debugLogCameraMoveEndFlow)
            {
                var smooth = cameraGsm.CameraComponent != null
                    ? cameraGsm.CameraComponent.smoothTime.ToString("F3")
                    : "n/a";
                Debug.Log(
                    $"[ForestSceneLinEnStory] SetFollow(玩家) → onComplete: OnCameraMoveEnd（M3 smoothTime={smooth}；M1 掩护={_cameraReturnCoverBlack != null}）",
                    this);
            }

            // 保留场景默认 smoothTime 多帧手推；禁止临时置 0。
            cameraGsm.SetFollow(player.transform, onComplete: OnCameraMoveEnd, forceSnapToTarget: true);

            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    "[CHAIN] MOVE_CMD  SetFollow(玩家)  forceSnap=true  keepSmoothMove=M3  " +
                    $"coverM1={_cameraReturnCoverBlack != null}  CameraGsm.IsLock={cameraGsm.IsLock}",
                    this);
            }

            if (safeTriggerYaerAfterRealSeconds > 0.01f)
            {
                _cameraGsmForSafety = cameraGsm;
                _safetyTriggerYaerRoutine = _cameraGsmForSafety.StartCoroutine(CoSafeTriggerCameraMoveEndIfStuck());
            }
        }

        /// <summary>打开系统 BlackPanel；全黑回调里再开手推。对齐 BlackFadeTeleport / ChiefNearDoor 写法。</summary>
        private void OpenCameraReturnCoverBlack(Action<BlackFormLogic> onBlackReady)
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(
                uiPath,
                EUIGroup.System,
                new OpenFormArgs
                {
                    userData = new ShowBlackFormArgs
                    {
                        showType = BlackFadeType.FadeShow,
                        showDuration = cameraReturnBlackShowSeconds,
                        hideDuration = cameraReturnBlackHideSeconds,
                        onShowEnd = onBlackReady
                    }
                });
        }

        /// <summary>新一轮 OnDialogueEnd 时若上轮黑幕未关干净，先关掉避免叠层。</summary>
        private void AbortCameraReturnCoverBlackIfAny()
        {
            if (_cameraReturnCoverBlack == null)
            {
                _cameraReturnCoverClosing = false;
                return;
            }

            var black = _cameraReturnCoverBlack;
            _cameraReturnCoverBlack = null;
            _cameraReturnCoverClosing = false;
            try
            {
                black.CloseFormFade(null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ForestSceneLinEnStory] 中止上轮掩护黑幕异常：" + ex.Message, this);
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

        private void StopWaitNotifyRoutine()
        {
            if (_waitNotifyRoutine == null) { return; }
            if (_hostForWaitNotify != null) { _hostForWaitNotify.StopCoroutine(_waitNotifyRoutine); }
            _waitNotifyRoutine = null;
            _hostForWaitNotify = null;
        }

        /// <summary>停止 2s 兜底、「下帧 OnCameraMoveEnd」、以及「等 Register 再 Notify」协程，避免多轮 OnDialogueEnd 叠挂。</summary>
        private void StopAllDialogueEndFallbackRoutines()
        {
            StopSafeCameraMoveEndStuckRoutine();
            StopInvokeOnCameraMoveEndNextFrameRoutine();
            StopWaitNotifyRoutine();
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

        private AnimationEventComponent ResolveAnimationEventComponent(out string source)
        {
            var anima = GetComponent<AnimationEventComponent>();
            source = "本物体 GetComponent";
            if (anima == null)
            {
                anima = GetComponentInChildren<AnimationEventComponent>(true);
                source = "子物体 GetComponentInChildren";
            }
            return anima;
        }

        /// <summary>
        /// 立刻向 AnimationEvent 投递 <c>CameraMoveEnd</c>（调用方须已确认已 Register，或接受超时强制）。
        /// </summary>
        private void FireCameraMoveEndEventNow(AnimationEventComponent anima, string source)
        {
            if (anima == null)
            {
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.LogWarning(
                        "[ForestSceneLinEnStory] FireNotify: 未找到 AnimationEventComponent，无法发 CameraMoveEnd。",
                        this);
                }
                return;
            }

            if (debugLogCameraMoveEndFlow)
            {
                var registered = anima.IsEventRegistered(CameraMoveEndEventName);
                Debug.Log(
                    $"[ForestSceneLinEnStory] FireNotify: 使用 {source} 上的「{anima.gameObject.name}」。" +
                    $" 触发前 RegisterEvent({CameraMoveEndEventName})={registered}。",
                    anima);
            }

            anima.AnimaEventTrigger(CameraMoveEndEventName + ":");
            _cameraMoveEndEventDelivered = true;

            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    "[ForestSceneLinEnStory] FireNotify: 已执行 AnimaEventTrigger(\"CameraMoveEnd:\")",
                    anima);
            }
        }

        /// <summary>
        /// M3 时序闸门：已 Register 则立即发；否则挂协程等到注册（或超时）再发。
        /// 可重入：已送达则 no-op；等待中则不叠挂第二条。
        /// 替代：固定 Wait 硬匹配机位时长（SPEC §3 禁止）。
        /// </summary>
        private void EnsureNotifyCameraMoveEndWhenRegistered()
        {
            if (_cameraMoveEndEventDelivered)
            {
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        "[CHAIN] CameraMoveEnd Notify already delivered；跳过重复投递。",
                        this);
                }
                return;
            }

            if (_waitNotifyRoutine != null)
            {
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        "[CHAIN] CameraMoveEnd Notify 等待协程已在跑；不叠挂。",
                        this);
                }
                return;
            }

            var anima = ResolveAnimationEventComponent(out var source);
            if (anima != null && anima.IsEventRegistered(CameraMoveEndEventName))
            {
                FireCameraMoveEndEventNow(anima, source);
                return;
            }

            var cameraGsm = SceneManager.GetModule<CameraComponentGSM>();
            _hostForWaitNotify = cameraGsm;
            _waitNotifyRoutine = cameraGsm.StartCoroutine(CoWaitRegisterThenNotify());
        }

        private IEnumerator CoWaitRegisterThenNotify()
        {
            var anima = ResolveAnimationEventComponent(out var source);
            var start = Time.unscaledTime;
            var timeout = Mathf.Max(0.01f, waitRegisterCameraMoveEndRealSeconds);

            if (debugLogCameraMoveEndFlow)
            {
                Debug.Log(
                    $"[CHAIN] CameraMoveEnd 尚未 Register（图可能仍在 id40→id41）。等待最多 {timeout:F2}s 实时后再发。",
                    this);
            }

            while (anima == null || !anima.IsEventRegistered(CameraMoveEndEventName))
            {
                if (Time.unscaledTime - start >= timeout)
                {
                    Debug.LogError(
                        $"[ForestSceneLinEnStory] 等待 Register(CameraMoveEnd) 超时 {timeout:F2}s，仍强制 AnimaEventTrigger。" +
                        "若接话仍断，查 NodeCanvas id41 是否进入等待、AnimationEventComponent 是否挂在本实体。",
                        this);
                    break;
                }

                yield return null;
                // 图可能稍后才挂上组件；每帧重解析
                anima = ResolveAnimationEventComponent(out source);
            }

            _waitNotifyRoutine = null;
            _hostForWaitNotify = null;

            if (_cameraMoveEndEventDelivered)
            {
                yield break;
            }

            FireCameraMoveEndEventNow(anima, source);
        }

        /// <summary>
        /// M1：有掩护黑幕则先揭幕，再 Ensure Notify（主角在亮屏后接话）；无黑幕则直接 Ensure（纯 M3）。
        /// 揭幕进行中的重入：等 Close 回调发门铃，避免叠关 / 过早 Notify。
        /// </summary>
        private void UnveilCoverThenEnsureNotify()
        {
            if (_cameraReturnCoverClosing)
            {
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        "[CHAIN] M1 黑幕正在揭幕；等 CloseFormFade 回调再 Ensure Notify。",
                        this);
                }
                return;
            }

            if (_cameraReturnCoverBlack != null)
            {
                var black = _cameraReturnCoverBlack;
                _cameraReturnCoverBlack = null;
                _cameraReturnCoverClosing = true;
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        "[CHAIN] M1 手推到位 → CloseFormFade → 再 EnsureNotify(CameraMoveEnd)",
                        this);
                }
                black.CloseFormFade(() =>
                {
                    _cameraReturnCoverClosing = false;
                    EnsureNotifyCameraMoveEndWhenRegistered();
                });
                return;
            }

            EnsureNotifyCameraMoveEndWhenRegistered();
        }

        public void OnCameraMoveEnd()
        {
            // —— B′（收紧）：只对「解锁 + 二次 snap」幂等；Notify 走 Unveil+Ensure，未送达仍可补发 ——
            if (_cameraMoveSnapHandledThisChain)
            {
                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        $"[CHAIN] OnCameraMoveEnd REENTER  frame={Time.frameCount}  " +
                        "snap 已处理（跳过二次 SetFollow）；仍走 Unveil+Ensure Notify。",
                        this);
                }
            }
            else
            {
                _cameraMoveSnapHandledThisChain = true;
                // 停手推兜底 / 下帧调用；不要停「等 Register」——揭幕后 Ensure 可能才启动它
                StopSafeCameraMoveEndStuckRoutine();
                StopInvokeOnCameraMoveEndNextFrameRoutine();

                if (debugLogCameraMoveEndFlow)
                {
                    Debug.Log(
                        $"[CHAIN] OnCameraMoveEnd ENTER  frame={Time.frameCount}  time={Time.time:F3}  unscaledTime={Time.unscaledTime:F3}  " +
                        $"coverBlack={_cameraReturnCoverBlack != null}  closing={_cameraReturnCoverClosing}",
                        this);
                }

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

            // M1+M3：先揭幕（若有），再等图 Register 后发门铃
            UnveilCoverThenEnsureNotify();
        }
    }
}
