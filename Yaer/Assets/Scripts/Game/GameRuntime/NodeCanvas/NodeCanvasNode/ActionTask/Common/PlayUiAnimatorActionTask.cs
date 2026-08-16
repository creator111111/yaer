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

        Animator _playing;
        string _state;
        float _timeoutAt;
        bool _waiting;

        protected override string info
        {
            get
            {
                var name = animator != null && !string.IsNullOrEmpty(animator.name)
                    ? animator.name
                    : (fallbackObjectName != null ? fallbackObjectName.ToString() : "?");
                var st = stateName != null && !string.IsNullOrEmpty(stateName.value) ? stateName.value : "Play";
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
