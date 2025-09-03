using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Component.Physics;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State;
using Game.GameRuntime.Entities.Monster.Slime.Anima;
using System;
using UnityEngine;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using System.Collections.Generic;
using DG.Tweening;
using Game.Static.Enum;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    /*
    * 这个怪的行动力是靠脚上这个石头控制的，玩家主要攻击点是这个地方，
    * 这个东西攻击一定次数会触发怪的瘫痪动画，具体次数可以先定10-15次之间，
    * 当怪处于瘫痪状态，玩家攻击这个怪的面部，大概5次左右，这个怪有一个瘫痪击破动画，
    * 你理解成重伤敌人，触发后怪起身，继续站立姿势的攻击，这个流程大概循环三次吧，
    * 然后触发这个图的一个破碎效果。应该在那个瘫痪坐下里能看见，这个阶段后这个怪就站不起来了，
    * 属于是被动挨打的阶段，播放那个不能起身的瘫痪击破动画，大概循环两次这个阶段，
    * 触发面部破碎那个动画，然后播放那个死亡动画，BOSS流程整体结束，注意的是，这个怪除了它基本动画外，
    * 另一个攻击是召唤虫子入场，这怪可以通过这两个口往外生那个虫子，就你摆放过的那个，用腾空翻滚入场就行，
    * 瘫痪后不能行动的BOSS就只能召唤虫子
    */
    public class BossMogutLogic : BaseMonster
    {
        string woodWormPrefabPath = "Assets/GameRes/Prefabs/Entity/Monster/WoodWorm_1.prefab";
        [HideInInspector]
        public int curBornWormCount = 0;
        public int maxBornWormCount = 2; // 最大同时可以产生的蠕虫数量
        public BossMogutCsAnimator csAnimator { get; private set; }

        public FindTargetComponent findTargetComponent { get; private set; }

        private WestRappRoadData sceneData;

        public GameObject skillNode_1;
        public GameObject skillNode_2;
        public GameObject skillNodeTrample;
        public GameObject wormBornNode_1; // 蠕虫怪物的出现位置
        public GameObject wormBornNode_2; // 蠕虫怪物的出现位置
        public Collider2D otherFootCld; // 该怪物还有另一只脚
        public Collider2D hitPointCld; // 怪物腿上水晶的碰撞体
        public Collider2D faceCld; // 脸部碰撞体
        public GameObject atkCollArea_1;
        public GameObject atkCollArea_2;
        public GameObject atkCollAreaTrample;
        public GameObject effectAtk_1;
        public GameObject effectAtk_2;
        public GameObject effectAtkTrample;
        public SoundToggleComponent talkSfxCpn; // 怪物叫声音效组件


        [Header("触发瘫痪的脚部受击次数")]
        [SerializeField]
        public int ParalysisDownFootHitTimes;
        [Header("瘫痪后触发起身的面部受击次数")]
        [SerializeField]
        public int ParalysisUpFaceHitTimes;
        [Header("触发不能起身的瘫痪次数")]
        [SerializeField]
        public int BrokenLegParalysisTimes;
        [Header("不能起身后触发重伤的受击次数")]
        [SerializeField]
        public int BigHurtTimes;
        [Header("不能起身后触发死亡的重伤次数")]
        [SerializeField]
        public int DeathHitTimes;

        [HideInInspector]
        public int CurrentParalysisDownFootHitTimes; // 怪物腿部水晶的受击次数，达到一定次数后怪物会瘫痪
        [HideInInspector]
        public int CurrentParalysisUpFaceHitTimes; // 怪物瘫痪之后受到的攻击次数，达到一定数量时怪物会站起来
        [HideInInspector]
        public int CurrentParalysisTimes; // 当前瘫痪次数,达到一定数量时击破怪物
        [HideInInspector]
        public int CurrentBigHitTimes; // 触发重伤受到的攻击次数,达到一定次数怪物重伤
        [HideInInspector]
        public int CurrentDeathHitTimes; // 当前击破次数,达到一定次数怪物死亡

        [HideInInspector]
        public bool IsParalysis; // 是否瘫痪倒地
        [HideInInspector]
        public bool IsDefeating; // 是否
        [HideInInspector]
        public bool IsBrokenLeg; // 是否处于腿部被击破的状态

        [HideInInspector]
        public bool IsTest = true;
        [HideInInspector]
        public string lastSkillName; // 上一次使用的技能名称
        GameObject skillNode = null;
        GameObject collArea = null;
        GameObject effectNode = null;
        // 瘫痪后的血量数据
        [HideInInspector]
        Dictionary<EGameHard, int> otherHpDatas = new Dictionary<EGameHard, int>() {
            { EGameHard.Easy, 200 }, { EGameHard.Normal, 250},
            { EGameHard.Hard, 300 }, { EGameHard.Hardest, 350},
        };
        [HideInInspector]
        public float totalCurHp; // 当前剩余的总血量
        [HideInInspector]
        public float totalMaxHp; // 当前总血量的最大值

        public Action<string> OnPerformAttack;

        int effectFadeActEndCount = 0; // 特效淡出动作完成的个数
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            sceneData = SceneManager.GetArchiveData<WestRappRoadData>();
            if (sceneData.KillBossMogut)
            {
                gameObject.SetActive(false);
                return;
            }
            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnBreak;
            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;

            csAnimator = componentSystem.GetComponent<BossMogutCsAnimator>();
            findTargetComponent = componentSystem.GetComponent<FindTargetComponent>();

            csAnimator.ChangeState<BossMogutMoveState>();
            // 初始隐藏部分碰撞体;
            bodyCld.gameObject.SetActive(false);
            faceCld.gameObject.SetActive(false);

            initBaseData(6);// 初始化基础数据并设置怪物ID
            hasDropItem = false;
            hasMonsterState = false;
            attackCdTimer = baseAtkDistance; // BOSS第一次出手之前需要间隔一段时间
            if (sceneData.AwakeBossMogut)
            {
                EnterBattleState();
            }
            else
            {
                EnterStorySubState();
            }
            // 初始计算BOSS所有阶段的总血量
            var tempHp = BrokenLegParalysisTimes * maxHp; // BOSS一阶段站立总血量
            tempHp += (BrokenLegParalysisTimes - 1) * GetInParalysisMaxHp();// BOSS一阶段瘫痪总血量
            tempHp += DeathHitTimes * GetInParalysisMaxHp(); // BOSS二阶段总血量
            totalMaxHp += tempHp;
            totalCurHp = totalMaxHp;
        }

        public void OnBreak()
        {
            if (!IsParalysis)
            {
                CurrentParalysisTimes++;
                paralysisSubSM = csAnimator.CurrentCsRuntimeController.EnterSubStateMachine<BossMogutParalysisSubSM>();
                // 不能起身瘫痪
                if (CurrentParalysisTimes >= BrokenLegParalysisTimes)
                {
                    PlayHeartBreakAudio();// 水晶破碎时播放音效
                    paralysisSubSM.ChangeState<BossMogutBrokenLegParalysisDownState>();
                }
                // 普通瘫痪
                else
                {
                    paralysisSubSM.ChangeState<BossMogutParalysisDownState>();
                }
                // 转换成瘫痪后的血量
                var otherHp = GetInParalysisMaxHp();
                componentSystem.GetComponent<HealthComponent>().maxHp = otherHp;
                componentSystem.GetComponent<HealthComponent>().hp = otherHp;
            }
            else
            {
                if (IsBrokenLeg)
                {

                    CurrentDeathHitTimes++;
                    if (CurrentDeathHitTimes >= DeathHitTimes)
                    {
                        // 怪物死亡
                        paralysisSubSM.ChangeState<BossMogutParalysisFaceBroken1State>();
                    }
                    else
                    {
                        // 触发一次大瘫痪
                        paralysisSubSM.ChangeState<BossMogutBrokenLegParalysisDefeatState>();
                        var otherHp = GetInParalysisMaxHp();
                        componentSystem.GetComponent<HealthComponent>().maxHp = otherHp;
                        componentSystem.GetComponent<HealthComponent>().hp = otherHp;
                    }
                }
                else
                {
                    // 达到一定次数后怪物重新站起来
                    paralysisSubSM.ChangeState<BossMogutParalysisUpState>();
                    var otherHp = maxHp; // 站起来后恢复为正常血量
                    componentSystem.GetComponent<HealthComponent>().maxHp = otherHp;
                    componentSystem.GetComponent<HealthComponent>().hp = otherHp;
                }
            }
        }

        int GetInParalysisMaxHp()
        {
            var hardCompont = GameManager.GetGMComponent<HardComponentGM>();
            var gameHard = hardCompont.Hard;
            if (otherHpDatas.ContainsKey(gameHard))
            {
                return otherHpDatas[gameHard];
            }
            else
            {
                return 0;
            }
        }

        public override void OnDead()
        {
            base.OnDead();
            if (bodyCld != null) { bodyCld.isTrigger = true; }
            if (footCld != null) { footCld.isTrigger = true; }
            if (otherFootCld != null) { otherFootCld.isTrigger = true; }
            if (faceCld != null) { faceCld.isTrigger = true; }

            WestRappRoadBossBattleMgr.getInstance().CheckEventHasEnd();
        }

        
        private void OnApplyStatusEffects(DamageData data)
        {
            if (!isDead)
            {
                // 检测本次伤害攻击的部位
                var hp = componentSystem.GetComponent<HealthComponent>().hp;
                var curReduceHp = (hp >= data.baseDamage ? data.baseDamage : hp);
                totalCurHp -= curReduceHp; // 总血量受伤持续减少
                var collAreaName = data.atkObjName;
                if (collAreaName == "HitPoint")
                {
                    // 攻击的是腿部水晶
                    HitFoot(data);
                }
                else if (collAreaName == "Face")
                {
                    // 攻击的是脸部
                    HitFace(data);
                }
            }
        }

        private void OnPlayImpactEffects(DamageData data)
        {
        }

        private void OnApplyFinalDamage(DamageData data)
        {

        }


        public void EnterStorySubState()
        {
            footCld.isTrigger = true; // 故事状态中不需要有碰撞效果
            faceCld.isTrigger = true;
            csAnimator.CurrentCsRuntimeController.EnterSubStateMachine<BossMogutStorySubSM>();
        }

        public void EnterBattleState()
        {
            footCld.isTrigger = false;
            faceCld.isTrigger = false;
            ResetHitState();
            csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine().ChangeState<BossMogutMoveState>();
        }

        public void ResetHitState()
        {
            CurrentParalysisDownFootHitTimes = 0;
            CurrentParalysisUpFaceHitTimes = 0;
            CurrentParalysisTimes = 0;
            CurrentDeathHitTimes = 0;
            CurrentBigHitTimes = 0;
            IsParalysis = false;
            IsDefeating = false;
            IsBrokenLeg = false;
        }

        private BossMogutParalysisSubSM paralysisSubSM;

        public void HitFoot(DamageData data)
        {
            if (isDead) return;
            if (IsParalysis) return;

            componentSystem.GetComponent<HealthComponent>().TakeDamage(data.baseDamage);

            //CurrentParalysisDownFootHitTimes++;
            //Debug.Log($"bossMogut脚部受击: {CurrentParalysisDownFootHitTimes}/{ParalysisDownFootHitTimes}");
            //if (CurrentParalysisDownFootHitTimes >= ParalysisDownFootHitTimes)
            //{
            //    CurrentParalysisDownFootHitTimes = 0;

            //    CurrentParalysisTimes++;

            //    paralysisSubSM = csAnimator.CurrentCsRuntimeController.EnterSubStateMachine<BossMogutParalysisSubSM>();

            //    // 不能起身瘫痪
            //    if (CurrentParalysisTimes >= BrokenLegParalysisTimes)
            //    {
            //        PlayHeartBreakAudio();// 水晶破碎时播放音效
            //        paralysisSubSM.ChangeState<BossMogutBrokenLegParalysisDownState>();
            //    }
            //    // 普通瘫痪
            //    else
            //    {
            //        paralysisSubSM.ChangeState<BossMogutParalysisDownState>();
            //    }

            //}
        }

        public void HitFace(DamageData data)
        {
            if (isDead) return;
            if (!IsParalysis) return;
            if (IsDefeating) return;
            componentSystem.GetComponent<HealthComponent>().TakeDamage(data.baseDamage);
            //if (IsBrokenLeg)
            //{
            //    CurrentBigHitTimes++;
            //    if (CurrentBigHitTimes >= BigHurtTimes)
            //    {
            //        CurrentBigHitTimes = 0; // 重置次数
            //        Debug.Log($"bossMogut脸部重伤: {CurrentDeathHitTimes}/{DeathHitTimes} ");
            //        CurrentDeathHitTimes++;
            //        if (CurrentDeathHitTimes >= DeathHitTimes)
            //        {
            //            // 怪物死亡
            //            paralysisSubSM.ChangeState<BossMogutParalysisFaceBroken1State>();
            //        }
            //        else
            //        {
            //            // 触发一次大瘫痪
            //            paralysisSubSM.ChangeState<BossMogutBrokenLegParalysisDefeatState>();
            //        }
            //    }
            //}
            //else
            //{
            //    CurrentParalysisUpFaceHitTimes++;
            //    Debug.Log($"bossMogut脸部受击: {CurrentParalysisUpFaceHitTimes}/{ParalysisUpFaceHitTimes}");
            //    if (CurrentParalysisUpFaceHitTimes >= ParalysisUpFaceHitTimes)
            //    {
            //        // 达到一定次数后怪物重新站起来
            //        paralysisSubSM.ChangeState<BossMogutParalysisUpState>();
            //    }
                
            //}
        }


        // 创建蠕虫怪物
        public void CreateWormWood()
        {
            
            if (curBornWormCount >= maxBornWormCount) { return; }
            curBornWormCount++;
            // 创建蠕虫怪物
            //woodWormObj.SetActive(true);
            //var woodWormLogic = woodWormObj.GetComponent<WoodWormLogic>();
            ShowWormWood(wormBornNode_1);
            // 每次同时场景两只蠕虫
            if (curBornWormCount >= maxBornWormCount) { return; }
            curBornWormCount++;
            ShowWormWood(wormBornNode_2);
        }

        void ShowWormWood(GameObject parentNode)
        {
            var entityComponentGM = GameManager.GetGMComponent<EntityComponentGM>();
            entityComponentGM.ShowMonsterEntity<WoodWormLogic>(woodWormPrefabPath, 0, SceneManager, (newWormObj) =>
            {
                //newWormObj.transform.SetParent(parentNode.transform, false);
                newWormObj.transform.position = parentNode.transform.position;
                newWormObj.initComponentData(userData);
                newWormObj.OnBounceFromBossMogut(this);
                newWormObj.moveToPlayer();
                WestRappRoadBossBattleMgr.getInstance().GetBossBattleStory().allWoodWormLogics.Add(newWormObj);
            });
        }

        public void CheckBossAtkCollisonShow(string atkName, bool isShow)
        {
            if (atkName == "Atk1")
            {
                skillNode = skillNode_1;
                collArea = atkCollArea_1;
                effectNode = effectAtk_1;
                WestRappRoadBossBattleMgr.getInstance().StartCameraImpluse(new Vector3(7, 7, 7));
            }
            else if (atkName == "Atk2")
            {
                skillNode = skillNode_2;
                collArea = atkCollArea_2;
                effectNode = effectAtk_2;
                // 重击造成屏幕震动
                WestRappRoadBossBattleMgr.getInstance().StartCameraImpluse(new Vector3(10, 10, 10));
            }
            else if (atkName == "Trample")
            {
                skillNode = skillNodeTrample;
                collArea = atkCollAreaTrample;
                effectNode = effectAtkTrample;
                WestRappRoadBossBattleMgr.getInstance().StartCameraImpluse(new Vector3(7, 7, 7));
            }
            if (skillNode == null) { return; }
            if (effectNode == null) { return; }
            //effectNode.transform.localPosition = Vector3.zero;// 初始特效位置回归原位
            
            if (collArea != null) { collArea.SetActive(isShow); } // 碰撞体区域是瞬间出现和消失的
            if (isShow)
            {
                // 设置碰撞体相关数据
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.initAtkDataByName(this, curAtkCollsionType, atkName);
                baseAtkCollsion.clearData();// 清除之前的攻击目标
            }

            var effectSprite_1 = UIUtils.findChild(effectNode, "stoneIn");
            var effectSprite_2 = UIUtils.findChild(effectNode, "stoneOut");
            //var effectSprite_3 = UIUtils.findChild(effectNode, "dust");
            //var effectSprite_4 = UIUtils.findChild(effectNode, "flyStone");
            List<GameObject> effectSprites = new List<GameObject>() {
                effectSprite_1, effectSprite_2,
            };
            if (isShow)
            {
                // 恢复透明度
                foreach (var obj in effectSprites)
                {
                    if (obj == null) { continue; }
                    var oldColor = obj.GetComponent<SpriteRenderer>().color;
                    oldColor.a = 1f;
                    obj.GetComponent<SpriteRenderer>().color = oldColor;
                    obj.GetComponent<SpriteRenderer>().DOKill();

                }
                effectFadeActEndCount = 0;
                effectNode.transform.SetParent(skillNode.transform, false);// 特效恢复到原节点上
                effectNode.transform.rotation = transform.rotation;// 和怪物的旋转度保持一致
                effectNode.transform.localPosition = Vector3.zero;
                effectNode.SetActive(true); // 特效瞬间出现
                effectNode.transform.SetParent(null);
            }
            else
            {
                foreach (var obj in effectSprites)
                {
                    if (obj == null) { continue; }
                    var oldColor = obj.GetComponent<SpriteRenderer>().color;
                    oldColor.a = 1f;
                    obj.GetComponent<SpriteRenderer>().color = oldColor;
                    obj.GetComponent<SpriteRenderer>().DOKill();
                    // 特效淡出消失
                    var action = GameActionMgr.runFadeActionSpriteRender(obj, 0, 3f);
                    action.onComplete = () =>
                    {
                        effectFadeActEndCount++;
                        if (effectFadeActEndCount >= effectSprites.Count)
                        {
                            effectFadeActEndCount = 0;
                            effectNode.transform.SetParent(skillNode.transform, false);// 特效恢复到原节点上
                            effectNode.transform.rotation = transform.rotation;// 和怪物的旋转度保持一致
                            effectNode.transform.localPosition = Vector3.zero;
                            effectNode.SetActive(false); // 所有特效都消失之后隐藏特效节点区域
                        }
                    };
                }
            }
            
        }

        public override void PlayBeHurtSfx(string atkName, bool isPlay = true)
        {
            var realResName = "攻击石头金属类1.mp3";
            commonSfxCpn.ChangeSoundRes(realResName);
            PlayAudio(commonSfxCpn, isPlay);
        }

        // 音效相关
        public void PlayHeartBreakAudio(bool isPlay = true)
        {
            var realResPath = "装备水晶碎裂.mp3";
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay);
        }
        // BOSS叫声音效
        public void PlayBossCallAudio(bool isPlay = true)
        {
            PlayAudio(talkSfxCpn, isPlay);
        }

        public override float GetMonsterCurHp()
        {
            return totalCurHp;
        }

        public override float GetMonsterMaxHp()
        {
            return totalMaxHp;
        }
    }
}

