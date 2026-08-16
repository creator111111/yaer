using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.CldController;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Monster.TenWan.Components.Anima;
using Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State;

namespace Game.GameRuntime.Entities.Monster.TenWan
{
    public class TenWanLogic : BaseMonster
    {
        public bool defaultAwake;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            // GroundCld：Prefab 实心 + OnlyMapObj + 整株细高盒，尖顶也能托住 PlayerFoot；
            // 又不进 GroundLayerMask → JumpFall/DamageFlyFall 死等 IsGrounded（与史莱姆同族残留）。
            // 怪落地靠 GroundChecker + GravityScale=0，GroundCld 本意「只和地图碰」；改 Trigger 后不再当玩家踏板。
            // 替代方案：改 Physics2D 矩阵 / GroundLayerMask（OPEN_QUESTIONS Q1/Q2，本期不采用）；
            // 或恢复 PlayerBodyCollider 挤出（Q5，本期不恢复）。
            // 禁止：对场景障碍 TenWanSceneObjLogic 套同款（砍断前实心挡路是设计）。
            if (groundCld != null)
            {
                groundCld.isTrigger = true;
            }

            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnDead;

            if (defaultAwake)
            {
                componentSystem.GetComponent<BaseCsAnimator>().ChangeState<TenWanIdleState>();
            }
            else
            {
                componentSystem.GetComponent<BaseCsAnimator>().ChangeState<TenWanSleepState>();
            }

            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;
            curAtkCollsionType = AtkCollsionType.Neutral;// 藤蔓为中立单位

            initBaseData(2);// 初始化基础数据并设置怪物ID
            baseMoveSpeed = 0;// 不能移动

            // 初始记录攻击碰撞体
            var skillInfo = UIUtils.findChild(gameObject, "SkillInfos");
            if (skillInfo != null)
            {
                var atkNode_1 = UIUtils.findChild(skillInfo, "CollArea_NorAtk");
                if (atkNode_1 != null) { atkCollAreaNodeDict["NorAtk"] = atkNode_1; }
            }
        }


        #region BattleComponent

        private void OnApplyStatusEffects(DamageData data)
        {
            if (isDead == false)
            {
                // 播放动画
                componentSystem.GetComponent<TenWanCsAnimator>().ChangeState<TenWanDamageState>();
                componentSystem.GetComponent<HealthComponent>().TakeDamage(data.baseDamage);
            }
        }

        private void OnPlayImpactEffects(DamageData data)
        {
        }

        private void OnApplyFinalDamage(DamageData data)
        {
        }

        #endregion


        #region HealthComponent

        public override void OnDead()
        {
            base.OnDead();
            isDead = true;
            isProtect = true;
            componentSystem.GetComponent<TenWanCsAnimator>().ChangeState<TenWanDeadState>();
            componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(false);
        }

        #endregion

        public override void PlayAttackSfx(bool isPlay = true)
        {
            var realResName = "藤蔓攻击.WAV";
            commonSfxCpn.ChangeSoundRes(realResName);
            PlayAudio(commonSfxCpn, isPlay);
        }

        
    }
}