using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima;
using Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima.State;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot
{
    public class WoodWormRootLogic : BaseMonster
    {
        public bool defaultAwake = true;
        public bool canCreateWoodWorm { get; set; } // 是否能产生虫子
        public int maxCount = 5; // 同一个巢穴最大出现的虫子个数
        public float timeCount = 0; // 产生虫子的计时器
        public float timeDistance = 5; // 产生虫子的时间间隔
        public GameObject childAreaNode; 

        public int curBornWormCount = 0; // 当前巢穴产生虫子个数
        string woodWormPrefabPath = "Assets/GameRes/Prefabs/Entity/Monster/WoodWorm_1.prefab";
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            if (defaultAwake)
            {
                componentSystem.GetComponent<WoodWormRootCsAnimator>().ChangeState<WoodWormRootIdleState>();
            }
            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnDead;
            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;
            initBaseData(5);// 初始化基础数据并设置怪物ID
            hasDropItem = false;
        }

        public override void OnDead()
        {
            base.OnDead();
            isDead = true;
            canCreateWoodWorm = false;
            var csAnimator = componentSystem.GetComponent<WoodWormRootCsAnimator>();
            var stateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
            stateMachine.ChangeState<WoodWormRootDeadState>();
            WoodWormRootBattleMgr.getInstance().CheckEventHasEnd();
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.WormHomeKill_1, 1);
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.WormHomeKill_2, 1);
        }
        protected override void Update()
        {
            base.Update();
            var sceneMgr = GetSceneManager();
            if (sceneMgr.GetSceneObjIsPause()) { return; }

            if (!canCreateWoodWorm) { return; }
            timeCount += Time.deltaTime;
            if (timeCount >= timeDistance )
            {
                timeCount = 0;
                // 创建蠕虫
                CreateWoodWorm();
            }
        }

        public void CreateWoodWorm()
        {
            if (curBornWormCount >= maxCount) { return; }
            curBornWormCount++;
            // 创建蠕虫怪物
            //woodWormObj.SetActive(true);
            //var woodWormLogic = woodWormObj.GetComponent<WoodWormLogic>();
            var entityComponentGM = GameManager.GetGMComponent<EntityComponentGM>();
            entityComponentGM.ShowMonsterEntity<WoodWormLogic>(woodWormPrefabPath, 0, SceneManager, (newWormObj) =>
            {
                newWormObj.transform.SetParent(childAreaNode.transform, false);
                newWormObj.transform.localPosition = new Vector2(2, -0.7f);
                newWormObj.initComponentData(userData);
                newWormObj.OnBounceFromWormRoot(this);
                WoodWormRootBattleMgr.getInstance().wormBattleStory.addNewWoodWorm(newWormObj);
            });
        }
        public override void MonsterRealRemove()
        {
            // 虫巢是否需要移除等管理器判断
            //if (curBornWormCount <= 0)
            //{
            //    gameObject.SetActive(false);
            //}
        }

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

    }
}