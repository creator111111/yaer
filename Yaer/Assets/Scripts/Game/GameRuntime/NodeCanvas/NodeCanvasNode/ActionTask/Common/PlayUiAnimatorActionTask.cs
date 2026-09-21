using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 播放 UGUI 容器上的 Animator（如 Village_KenMuNiStart 的 Anim_Gusha / Anim_Yaer）。
    /// </summary>
    /// <remarks>
    /// 原因：现网仅有「等待 Animation 事件」，没有「主动播 Clip」任务；角/翅膀帧动画需 Action 显式 Play。
    /// 绑定：优先 BBParameter&lt;Animator&gt;（导入器写 animator.name = Extra）；若未绑则按物体名在 agent 下查找。
    /// 等待结束用 OnUpdate 轮询，避免 Editor 程序集 / UniTask 依赖带来的编译顺序问题。
    /// 替代方案：五帧轮流 SetActive——层级脏、难维护，不采用。
    ///
    /// loopUntilContinue（默认关）：字幕还挂着时动画循环，玩家点继续（或整段被停）再藏。
    /// 为什么不能等 Clip 的 normalizedTime：循环动画第一圈结束就 &gt;= 1，拿它当「等点击」会在字幕出来前藏掉；
    /// 删掉这个判断又继续等播完，循环永远等不完，字幕出不来，大约 5 秒还会被旧超时掐掉。
    /// 播放节点和字幕节点是排队的，本任务必须立刻 EndAction，隐藏只能包在下一句的 Continue 上。
    /// 替代方案（本期不用）：
    /// 1. 不等待、再在图上插一个「藏动画」节点——成品图节点多，容易把前奏连线弄断，而且 waitUntilFinish=false 仍会立刻藏。
    /// 2. 只把 Clip 勾成 Loop、不改本任务——第一圈结束照样被藏。
    /// </remarks>
    [Category("Animation")]
    [Name("播放UI Animator")]
    public class PlayUiAnimatorActionTask : ActionTask
    {
        /// <summary>Prefab Blackboard 上的 Animator（变量名通常与容器同名，如 Anim_Gusha）。</summary>
        public BBParameter<Animator> animator;

        /// <summary>可选兜底：容器物体名；BB 未绑定时用 agent 下 Find。</summary>
        public BBParameter<string> fallbackObjectName;

        /// <summary>Controller 状态名；默认 Play（与 Anim_*_Horn/Wing.controller 一致）。</summary>
        public BBParameter<string> stateName;

        /// <summary>为 true 时等 Clip 播完再 EndAction；为 false 则立即进入下一节点（可与字幕并行）。</summary>
        public BBParameter<bool> waitUntilFinish;

        /// <summary>播完后是否隐藏 Animator 所在物体（避免挡立绘）。</summary>
        public BBParameter<bool> hideWhenFinished;

        /// <summary>
        /// 字幕期间循环，点继续再藏。默认关。
        /// 为真时忽略 waitUntilFinish / hideWhenFinished：不能靠把 waitUntilFinish 改成 false，
        /// 那个分支会立刻 FinishAndMaybeHide，hideWhenFinished 仍为真时动画同一帧就没了。
        /// </summary>
        public BBParameter<bool> loopUntilContinue;

        Animator _playing;
        string _state;
        float _timeoutAt;
        bool _waiting;

        /// <summary>循环到点击时要藏的容器。不能复用 _playing：EndAction 会进 OnStop 把 _playing 清掉，那时下一句字幕还没来。</summary>
        GameObject _loopHideTarget;

        bool _loopArmed;
        DialogueTree _loopOwnerTree;
        System.Action<SubtitlesRequestInfo> _onLoopSubtitles;
        System.Action<DialogueTree> _onLoopFinished;

        protected override string info
        {
            get
            {
                var name = animator != null && !string.IsNullOrEmpty(animator.name)
                    ? animator.name
                    : (fallbackObjectName != null ? fallbackObjectName.ToString() : "?");
                var st = stateName != null && !string.IsNullOrEmpty(stateName.value) ? stateName.value : "Play";
                if (loopUntilContinue != null && loopUntilContinue.value)
                {
                    return string.Format("播 UI Animator(循环到点击): {0} / {1}", name, st);
                }

                return string.Format("播 UI Animator: {0} / {1}", name, st);
            }
        }

        protected override void OnExecute()
        {
            _playing = ResolveAnimator();
            if (_playing == null)
            {
                Debug.LogError(
                    $"[PlayUiAnimator] 未找到 Animator。bb={animator?.name} fallback={fallbackObjectName?.value}",
                    agent as Object);
                EndAction(false);
                return;
            }

            // 播前显示容器（Prefab 默认 Inactive，避免入场叠五帧）。
            if (!_playing.gameObject.activeSelf)
            {
                _playing.gameObject.SetActive(true);
            }

            _state = stateName != null && !string.IsNullOrEmpty(stateName.value) ? stateName.value : "Play";
            _playing.Play(_state, 0, 0f);
            _playing.Update(0f);

            // 新开关为真：马上放行字幕，不要进下面的等待，也不要武装 5 秒超时。
            if (loopUntilContinue != null && loopUntilContinue.value)
            {
                ArmLoopUntilContinue(_playing.gameObject);
                EndAction(true);
                return;
            }

            var shouldWait = waitUntilFinish == null || waitUntilFinish.value;
            if (!shouldWait)
            {
                FinishAndMaybeHide();
                EndAction(true);
                return;
            }

            _waiting = true;
            _timeoutAt = Time.unscaledTime + 5f;
        }

        protected override void OnUpdate()
        {
            // 只留给旧的「播一次就藏」。循环到点击已经 EndAction，不会进这里。
            // 循环动画第一圈结束 normalizedTime 就 >= 1，不能拿它当「等玩家点击」。
            if (!_waiting || _playing == null)
            {
                return;
            }

            if (Time.unscaledTime >= _timeoutAt)
            {
                FinishAndMaybeHide();
                EndAction(true);
                return;
            }

            var infoState = _playing.GetCurrentAnimatorStateInfo(0);
            if (infoState.IsName(_state) && infoState.normalizedTime >= 1f && !_playing.IsInTransition(0))
            {
                FinishAndMaybeHide();
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            _waiting = false;
            _playing = null;
            // 不要在这里退订循环钩子，也不要藏物体。
            // EndAction 会立刻调用 OnStop，而下一句的 OnSubtitlesRequest 要等本次 Execute 返回后才发生。
        }

        /// <summary>
        /// 订一次「下一句字幕」和「整段结束」。必须在 EndAction 之前调用。
        /// 播放任务不能一边活着一边等点击，否则字幕节点排不进去。
        /// </summary>
        void ArmLoopUntilContinue(GameObject target)
        {
            DisarmLoopHooks();
            _loopHideTarget = target;
            _loopArmed = true;
            _loopOwnerTree = ownerSystem as DialogueTree;
            _onLoopSubtitles = OnLoopSubtitlesRequest;
            _onLoopFinished = OnLoopDialogueFinished;
            DialogueTree.OnSubtitlesRequest += _onLoopSubtitles;
            DialogueTree.OnDialogueFinished += _onLoopFinished;
        }

        /// <summary>
        /// 下一次字幕请求就是紧挨着的那句（古莎卷角 / 雅尔扇翅膀）。
        /// info.Continue 此时已是 OnStatementFinish。先藏，再调用原来的继续。
        /// </summary>
        void OnLoopSubtitlesRequest(SubtitlesRequestInfo info)
        {
            if (!_loopArmed || info == null)
            {
                return;
            }

            // 静态事件全树共用。别的对话树插进来就先不包，继续等本树的下一句。
            if (_loopOwnerTree != null && DialogueTree.currentDialogue != _loopOwnerTree)
            {
                return;
            }

            var original = info.Continue;
            info.Continue = () =>
            {
                HideLoopTarget();
                if (original != null)
                {
                    original();
                }
            };

            // 只包这一次。退订放在玩家点击之前，避免下一句字幕的 Continue 又被包住。
            if (_onLoopSubtitles != null)
            {
                DialogueTree.OnSubtitlesRequest -= _onLoopSubtitles;
                _onLoopSubtitles = null;
            }
        }

        /// <summary>
        /// 这句还没点、整段就被停掉（跳过 / 中断）时也藏一次。
        /// 整棵剧情物体被关掉只能兜「对话已经结束」，兜不住下一句立绘还在、动画还挡着；所以不能省这一下。
        /// </summary>
        void OnLoopDialogueFinished(DialogueTree dlg)
        {
            if (!_loopArmed)
            {
                return;
            }

            if (_loopOwnerTree != null && dlg != _loopOwnerTree)
            {
                return;
            }

            HideLoopTarget();
        }

        void HideLoopTarget()
        {
            if (_loopHideTarget != null)
            {
                _loopHideTarget.SetActive(false);
            }

            DisarmLoopHooks();
        }

        void DisarmLoopHooks()
        {
            if (_onLoopSubtitles != null)
            {
                DialogueTree.OnSubtitlesRequest -= _onLoopSubtitles;
                _onLoopSubtitles = null;
            }

            if (_onLoopFinished != null)
            {
                DialogueTree.OnDialogueFinished -= _onLoopFinished;
                _onLoopFinished = null;
            }

            _loopHideTarget = null;
            _loopArmed = false;
            _loopOwnerTree = null;
        }

        void FinishAndMaybeHide()
        {
            _waiting = false;
            if (_playing != null && hideWhenFinished != null && hideWhenFinished.value)
            {
                _playing.gameObject.SetActive(false);
            }

            _playing = null;
        }

        Animator ResolveAnimator()
        {
            if (animator != null && animator.value != null)
            {
                return animator.value;
            }

            var objectName = fallbackObjectName != null ? fallbackObjectName.value : null;
            if (string.IsNullOrEmpty(objectName) && animator != null)
            {
                objectName = animator.name;
            }

            if (string.IsNullOrEmpty(objectName) || agent == null)
            {
                return null;
            }

            // 在对话 Prefab 根下按名查找（对齐 Extra=Anim_Gusha）。
            var transforms = agent.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == objectName)
                {
                    return t.GetComponent<Animator>();
                }
            }

            return null;
        }
    }
}
