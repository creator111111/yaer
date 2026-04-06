using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// Break 击飞（DamageFly，IsBreakUp）阶段，在竖直速度向下时追加额外向下加速度。
    /// 必须写在 <see cref="MoveComponent.OnFixedUpdate"/> 之后（通过 DefaultExecutionOrder），
    /// 并同步修改 <see cref="MoveComponent.Velocity"/> 与 <see cref="Rigidbody2D.velocity"/>，
    /// 避免单独 AddForce 被下一帧 MoveComponent 覆盖。
    /// 挂在与 <see cref="KnockBackComponent"/> 同一物体上，便于与击退相关参数并列配置。
    /// </summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(KnockBackComponent))]
    public class PlayerKBFallDown : MonoBehaviour
    {
        [Header("Break 击飞下落额外加速度")]
        [Tooltip("正值：在原有重力之外，每秒额外增加的下落加速度大小（与 MoveComponent.Gravity 量级相近）。")]
        [SerializeField]
        private float additionalDownAcceleration = 5f;

        [Header("可选：最大下落速度")]
        [Tooltip("勾选后，将竖直速度限制在 maxFallSpeedY（应为负数，例如 -40）。")]
        [SerializeField]
        private bool clampMaxFallSpeed;

        [Tooltip("竖直速度下限（负数）。例如 -50 表示最快向下 50 单位/秒。")]
        [SerializeField]
        private float maxFallSpeedY = -50f;

        private PlayerLogic _playerLogic;
        private PlayerMoveComponent _move;
        private Rigidbody2D _rb;
        private BaseGameSceneManager _sceneMgr;

        private void Awake()
        {
            _playerLogic = GetComponentInParent<PlayerLogic>();
            _move = GetComponentInParent<PlayerMoveComponent>();
            _rb = GetComponentInParent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_playerLogic == null || _move == null || _rb == null)
            {
                return;
            }

            // 与 KnockBackComponent 一致：场景对象动画暂停时不改速度，避免与暂停逻辑打架
            if (_sceneMgr == null && _playerLogic.sceneManager is BaseGameSceneManager mgr)
            {
                _sceneMgr = mgr;
            }

            if (_sceneMgr != null && _sceneMgr.GetSceneObjAniIsPause())
            {
                return;
            }

            // 仅 Break 击飞（DamageFlyUpState 置位），避免普通跳跃下落也吃到额外加速度
            var cs = _playerLogic.componentSystem != null
                ? _playerLogic.componentSystem.GetComponent<BaseCsAnimator>()
                : null;
            if (cs == null || !cs.GetSign("IsBreakUp"))
            {
                return;
            }

            // 仅空中且竖直速度已向下时生效（与 DamageFlyUpState → Fall 的切换条件一致）
            if (_move.IsGrounded || _move.moveSpeedY >= 0f)
            {
                return;
            }

            // 在 MoveComponent 本帧已积分重力并写入 rg.velocity 之后，再叠加额外向下分量并回写 Velocity，保持二者一致
            var v = _move.Velocity;
            v.y += -additionalDownAcceleration * Time.fixedDeltaTime;

            if (clampMaxFallSpeed && v.y < maxFallSpeedY)
            {
                v.y = maxFallSpeedY;
            }

            _move.Velocity = v;
            _rb.velocity = v;
        }
    }
}
