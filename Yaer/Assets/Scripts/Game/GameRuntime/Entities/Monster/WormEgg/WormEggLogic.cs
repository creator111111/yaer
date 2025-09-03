using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.CldController;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima;
using Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima.State;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.WormEgg
{
    public class WormEggLogic : BaseMonster
    {
        public GameObject woodWormObj; // 死亡后产生的怪物对象

        protected override void Start()
        {
            woodWormObj.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            OnUpdate(0, 0);
        }

        // --------------------------------------------------------------------------------

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            GetComponent<AnimationEventComponent>().RegisterEvent("Break", CreateWorm);
            
            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnDead;

            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;
            initBaseData(4);// 初始化基础数据并设置怪物ID
            hasDropItem = false;
        }

        public override void OnDead()
        {
            base.OnDead();
            componentSystem.GetComponent<WormEggCsAnimator>().ChangeState<WormEggBreakState>();
            componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(false);
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.WormHomeKill_1, 1);
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.WormHomeKill_2, 1);
        }

        //protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        //{
        //    base.OnUpdate(elapseSeconds, realElapseSeconds);

        //    componentSystem.OnUpdate();
        //}

        private void CreateWorm(string args)
        {
            Debug.Log("CreateWorm");
        }
        
        #region BattleComponent

        private void OnApplyStatusEffects(DamageData data)
        {
            componentSystem.GetComponent<HealthComponent>().TakeDamage(data.baseDamage);
        }

        private void OnPlayImpactEffects(DamageData data)
        {
        }

        private void OnApplyFinalDamage(DamageData data)
        {
        }

        #endregion

        public override void MonsterDeadEndEvent()
        {
            //base.MonsterDeadEndEvent();
            // 产生新的蠕虫怪物
            woodWormObj.SetActive(true);
            var woodWormLogic = woodWormObj.GetComponent<WoodWormLogic>();
            woodWormLogic.OnBounceFromWormEgg(this);
            // 缓慢消失
            //var fadeAct = GameActionMgr.runFadeActionSpriteRender(animationNode, 0, 2f, 2f);
            //fadeAct.onComplete = () =>
            //{
            //    // 这里只隐藏虫蛋，虫蛋真正移除是在产生的蠕虫移除时一起移除
            //    bodyCld.isTrigger = true; // 设置为触发状态
            //    animationNode.SetActive(false);
            //};
        }

        public override void MonsterRealRemove()
        {
            // 虫蛋死亡后不消失
            //base.MonsterRealRemove();
        }

        public override void PlayDeadSfx(bool isPlay = true)
        {
            base.PlayDeadSfx(isPlay);
            var realResName = "卵破的声音.mp3";
            commonSfxCpn.ChangeSoundRes(realResName);
            PlayAudio(commonSfxCpn, isPlay);
        }
    }
}