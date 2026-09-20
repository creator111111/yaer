using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class BaseSlimeState : BaseMonsterState
    {
        /// <summary>
        /// 横版战斗场地面轴常量（ForestEast / VerdantCorridor / WestRapp 地面 EnterPos 标尺均为 −6.61）。
        /// 0913：升为 Snap / JumpAtk 落点唯一权威；禁止再抄玩家实时 y（玩家一跳会把史莱姆拽上天）。
        /// 禁止当成村庄「Y=纵深」。
        /// </summary>
        protected const float CombatAxisY = -6.61f;

        /// <summary>Snap 后若 |Δy| 小于此值则视为已同轴，避免无意义写 Transform。</summary>
        private const float CombatAxisYEpsilon = 0.05f;

        protected Slime slime;
        protected MoveComponent moveCpn;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            slime = stateMachine.GetEntityLogic<Slime>();
            moveCpn = slime.componentSystem.GetComponent<MoveComponent>();
        }

        public override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// 把史莱姆 Y 对齐到场景战斗轴常量 <see cref="CombatAxisY"/>。
        /// 调用点：BornDown / JumpAtkDown / Idle / Move 的 Enter；以及 <b>Dead.Enter（先 Snap 再 FreezeAll）</b>。
        /// JumpAtk 升空前勿调用。
        /// <para>
        /// 0912：为掉树同轴引入 Snap；曾优先实时玩家 y → 玩家跳时 Idle/Move 把怪拽上天（0913 推翻）。
        /// 0913 常量案：权威只返回 <see cref="CombatAxisY"/>。
        /// 0913 尸体贴轴：Dead 必须 Snap 常量轴（旧「Dead 禁止 Snap」针对玩家实时 y，已过时）。
        /// 替代：方案 B 仅落地用玩家 y（用户不要）；方案 D Config 每场景轴高（三场同值，本期否）。
        /// 禁止：Update 每帧追 Y；恢复 0723 GroundCld 实心；用村庄 Town 纵深语义。
        /// </para>
        /// </summary>
        /// <param name="alsoClearVerticalVelocity">落地瞬间清掉竖直速度，避免残速把刚体顶开。</param>
        protected void SnapToCombatAxisY(bool alsoClearVerticalVelocity = true)
        {
            if (slime == null)
            {
                return;
            }

            if (!TryResolveCombatAxisY(out var axisY))
            {
                return;
            }

            var pos = slime.transform.position;
            if (Mathf.Abs(pos.y - axisY) <= CombatAxisYEpsilon)
            {
                if (alsoClearVerticalVelocity)
                {
                    ClearVerticalVelocity();
                }
                return;
            }

            pos.y = axisY;
            slime.transform.position = pos;

            var rg = slime.BodyRg;
            if (rg != null)
            {
                // 同步刚体权威坐标，避免下一 Fixed 用旧 position 把 Snap 顶回去
                rg.position = pos;
            }

            if (alsoClearVerticalVelocity)
            {
                ClearVerticalVelocity();
            }
        }

        /// <summary>
        /// 权威 Y = 场景战斗地面轴常量（−6.61）。不再读玩家 / atkTarget 实时 y。
        /// </summary>
        private bool TryResolveCombatAxisY(out float axisY)
        {
            axisY = CombatAxisY;
            return true;
        }

        /// <summary>
        /// 0913 A′：JumpAtk 落点 X 仍追目标，Y 钉战斗轴（避免玩家空中时跳攻飞特别高）。
        /// </summary>
        protected Vector2 ResolveJumpAtkEndPos(Transform atkTarget)
        {
            if (atkTarget == null)
            {
                return new Vector2(slime != null ? slime.transform.position.x : 0f, CombatAxisY);
            }

            var p = atkTarget.position;
            return new Vector2(p.x, CombatAxisY);
        }

        private void ClearVerticalVelocity()
        {
            var rg = slime.BodyRg;
            if (rg == null)
            {
                return;
            }

            var v = rg.velocity;
            if (Mathf.Abs(v.y) < 0.0001f)
            {
                return;
            }

            v.y = 0f;
            rg.velocity = v;

            if (moveCpn != null)
            {
                moveCpn.moveSpeedY = 0f;
            }
        }
    }
}
