using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.CldController;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima;
using Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima.State;
using Game.GameRuntime.Entities.Player;
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
            // GroundCld 不在 CldController.nodes，SetActiveAll 关不到；基类 OnDead 已关，这里再断言防动画/子物体改回。
            EnsureGroundCldDisabled("OnDead");
            // 洞内宽爬区可能仍 isCrawling；碎卵路径原先不调 StopAutoCrawl，会叠成完全没响应。
            UnlockCrawlAfterEggBreak();
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
            EnsureGroundCldDisabled("MonsterDeadEndEvent");
            // 孵虫必须放在玩家身后（同侧），不能放「远离玩家」——左口前进方向是 +X，
            // 放右侧会正好堵死前进路（0914 复验：上版 Away 导致「修了仍无法前进」）。
            PlaceHatchedWormBehindPlayer();
            woodWormObj.SetActive(true);
            var woodWormLogic = woodWormObj.GetComponent<WoodWormLogic>();
            woodWormLogic.OnBounceFromWormEgg(this);
        }

        /// <summary>
        /// 死后及碎壳动画结束：关 GroundCld，并关掉卵上所有非 Trigger 实心盒。
        /// PlayerFoot 会撞 OnlyMapObj；只 enabled=false 若引用丢了仍会挡。活卵开局仍靠该盒挡路。
        /// </summary>
        void EnsureGroundCldDisabled(string from)
        {
            if (groundCld != null)
            {
                groundCld.enabled = false;
                if (groundCld.gameObject != null && groundCld.gameObject.activeSelf)
                {
                    groundCld.gameObject.SetActive(false);
                }
                Debug.Log($"[WormEggBreak] GroundCld off+inactive at {from} egg={name} pos={transform.position}");
            }
            else
            {
                Debug.LogWarning($"[WormEggBreak] groundCld is null at {from} egg={name}");
            }

            // 兜底：卵层级里任何实心盒都关（Body/Foot 本就是 Trigger，不受影响）
            var cols = GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (c == null || c.isTrigger) { continue; }
                // 孵虫子物体上的实心盒不关（木虫自己的 GroundCld 已是 Trigger）
                if (woodWormObj != null && c.transform.IsChildOf(woodWormObj.transform)) { continue; }
                c.enabled = false;
                Debug.Log($"[WormEggBreak] disabled solid collider {c.name} at {from}");
            }
        }

        /// <summary>
        /// 停自动爬并恢复位移意图。StopAutoCrawl 会打开站起，洞内须立刻再关上。
        /// </summary>
        void UnlockCrawlAfterEggBreak()
        {
            var entityCpn = GameManager.GetGMComponent<EntityComponentGM>();
            var player = entityCpn != null ? entityCpn.GetEntityLogic<PlayerLogic>() : null;
            if (player == null) { return; }
            CanNotSomeActionArea.StopAutoCrawlForPlayer(player);
            player.canInStateSetPos = true;
            bool inTree = ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge;
            if (inTree)
            {
                player.isEnableSquatUp = false;
            }
            Debug.Log($"[WormEggBreak] StopAutoCrawl after break inTree={inTree} squatUp={player.isEnableSquatUp} playerY={player.transform.position.y}");
        }

        /// <summary>
        /// 把孵虫放到卵中心「玩家所在一侧」（身后），前进方向让开。
        /// Prefab 默认本地 x=-3.19 对左口刚好在身后；右口则须翻到右侧。
        /// </summary>
        void PlaceHatchedWormBehindPlayer()
        {
            if (woodWormObj == null) { return; }
            var entityCpn = GameManager.GetGMComponent<EntityComponentGM>();
            var player = entityCpn != null ? entityCpn.GetEntityLogic<PlayerLogic>() : null;
            if (player == null) { return; }
            const float behind = 3.2f;
            float eggX = transform.position.x;
            // 与玩家同侧：玩家在卵左 → 虫也去左（负向）
            float sign = player.transform.position.x < eggX ? -1f : 1f;
            var p = woodWormObj.transform.position;
            float newX = eggX + sign * behind;
            woodWormObj.transform.position = new Vector3(newX, p.y, p.z);
            Debug.Log($"[WormEggBreak] hatch worm BEHIND player to x={newX} playerX={player.transform.position.x} eggX={eggX}");
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