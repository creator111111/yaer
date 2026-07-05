using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 在受击后的下落阶段追加额外向下加速度，与 <see cref="KnockBackComponent"/> 击退表现配合。
    /// 生效范围：<b>Break 击飞</b>（<c>IsBreakUp</c>，DamageFly）与 <b>普通受击</b>（<c>IsDamaging</c>，如 Damage1/Damage2）；
    /// 普通受击且击退把角色抬离地面时，即使竖直速度尚未转为向下也会尝试叠加（击退使用 MovePosition 时速度与位置可能不同步）。
    /// 必须写在 <see cref="MoveComponent.OnFixedUpdate"/> 之后（通过 DefaultExecutionOrder），
    /// 并同步修改 <see cref="MoveComponent.Velocity"/> 与 <see cref="Rigidbody2D.velocity"/>。
    /// 挂在与 <see cref="KnockBackComponent"/> 同一物体上。
    /// </summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(KnockBackComponent))]
    public class PlayerKBFallDown : MonoBehaviour
    {
        [Header("受击后下落额外加速度（Break 击飞 / 普通受击）")]
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
        private KnockBackComponent _knockBack;
        private BaseGameSceneManager _sceneMgr;

        private void Awake()
        {
            _playerLogic = GetComponentInParent<PlayerLogic>();
            _move = GetComponentInParent<PlayerMoveComponent>();
            _rb = GetComponentInParent<Rigidbody2D>();
            _knockBack = GetComponent<KnockBackComponent>();
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

            var cs = _playerLogic.componentSystem != null
                ? _playerLogic.componentSystem.GetComponent<BaseCsAnimator>()
                : null;
            if (cs == null)
            {
                return;
            }

            // Break 击飞（DamageFly）或普通受击（Damage1/Damage2 等 BasePlayerDamageState）；避免跳跃等非受击下落误触发
            bool isBreakFly = cs.GetSign("IsBreakUp");
            bool isNormalDamage = cs.GetSign("IsDamaging");
            if (!isBreakFly && !isNormalDamage)
            {
                return;
            }

            if (_move.IsGrounded)
            {
                return;
            }

            // Break 击飞下落：与原先一致，需竖直速度已向下
            // 普通受击 + 击退：击退用 MovePosition 时速度可能未反映下落，允许在击退进行中放宽「已向下」判定
            bool falling = _move.moveSpeedY < 0f;
            bool normalKnockbackAir = isNormalDamage && !isBreakFly && _knockBack != null && _knockBack.IsKnockbackInProgress;
            if (!falling && !normalKnockbackAir)
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
