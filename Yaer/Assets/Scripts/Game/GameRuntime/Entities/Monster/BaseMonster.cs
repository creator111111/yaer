using System.Collections.Generic;
using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Effect;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.Physics;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.UnityRuntime.Base;
using System;

public enum MonsterState
{
    Anger, // 愤怒
    Weak, // 虚弱状态
    Escape, // 逃跑
}
// 所处地面的类型，地面分为上中下三层线，玩家默认中，怪物按需求设置不同类型
public enum GroundType
{
    Up,
    Center, // 玩家默认的类型
    Down,
}

namespace Game.GameRuntime.Entities.Monster
{
    public class BaseMonster : BaseSceneEntityLogic, IMonster
    {
        public Collider2D bodyCld;
        public Collider2D footCld;
        public Collider2D groundCld; // 用来和地形检测碰撞的碰撞体,脚本初始化时强制设置图层
        [SerializeField] protected Animator animator;
        [SerializeField] protected DepthComponent depthCpn;
        [SerializeField] protected Transform woundPosParentTsf;
        public BaseGameSceneManager sceneManager;
        public GameObject buffArea; // BUFF标识显示区域
        public GameObject angryTag; //愤怒状态标志
        public GameObject weakTag; // 虚弱标志
        public GameObject escapeTag; // 逃跑标志
        public GameObject dropItem; // 掉落物
        public GameObject animationNode; // 图片显示节点
        public GroundType groundType; // 当前所处的地面类型，在Unity编辑器设置值
        public bool canRandomMove = false; // 是否能随机移动，在Unity编辑器里面设置
        private bool hasPickUpDropItem = false; // 掉落物是否被拾取了
        public bool hasDropItem = true; // 当前怪物是否拥有掉落物
        public bool deadIsToObjectPool = false; // 是否需要回收到对象池
        public bool hasMonsterState = true; // 是否拥有怪物的基础状态：愤怒，逃跑等状态
        public SoundToggleComponent commonSfxCpn; // 怪物身上音效管理组件,不会同时触发的音效可以用这个来播放
        private readonly List<Transform> woundPosList = new List<Transform>();
        [HideInInspector]
        public event Action OnDeadEventFunc; // 怪物死亡后的回调方法
        [HideInInspector]
        public float escapeTimeCount; // 逃跑时间计数器
        [HideInInspector]
        public float escapeTime = 6; // 逃跑状态触发后持续时间
        [HideInInspector]
        public bool hasTriggerEscState = false; // 是否触发过逃跑状态
        [HideInInspector]
        public int escapeRate = 10; // HP低于X%触发逃跑状态
        [HideInInspector]
        public bool hasFirstCheckAngry = true; // 是否首次检测触发愤怒状态，只判断一次
        // 基础属性值
        [HideInInspector]
        public int monsterId;// 怪物类型ID
        [HideInInspector]
        public int maxHp; // 最大血量
        [HideInInspector]
        public int curHp; // 当前血量
        [HideInInspector]
        public int baseAtkValue; // 基础攻击力
        [HideInInspector]
        public int atkValue; // 攻击力
        [HideInInspector]
        public float baseAtkDistance; // 基础攻击间隔
        [HideInInspector]
        public float atkDistance; // 攻击间隔
        [HideInInspector]
        public float attackCdTimer; // 攻击间隔计时器
        [HideInInspector]
        public float baseMoveSpeed = 1f; // 移动速度
        [HideInInspector]
        protected int angerRate = 20; // 愤怒状态概率，百分比
        [HideInInspector]
        public int weakRate = 40; // HP低于X%触发虚弱状态
        [HideInInspector]
        public int sceneMonsterTag = 0; // 已经添加到场景中的怪物标记，用来记录当前怪物是否已经死亡

        protected int curMonsterLayer; // 当前图层
        protected int onlyMapObjLayer = 7; // 只和地图碰撞的对象图层
        protected int atkCheckLayer = 19; // 进行攻击碰撞检测的图层
        
        // 当前不同状态是否触发
        Dictionary<MonsterState, bool> stateData = new Dictionary<MonsterState, bool>();
        Dictionary<MonsterState, GameObject> stateTagDict = new Dictionary<MonsterState, GameObject>();

        // 所处地面类型对应的图层名字字典
        Dictionary<GroundType, string> groundToMLayerData = new Dictionary<GroundType, string>() {
            { GroundType.Up, "MonsterUp" }, { GroundType.Center, "MonsterCenter" },{ GroundType.Down, "MonsterDown" },
        };
        // 所处地面类型对应的精灵排序名字字典
        Dictionary<GroundType, string> groundToSpriteSortData = new Dictionary<GroundType, string>() {
            { GroundType.Up, "Monster1" }, { GroundType.Center, "Monster2" },{ GroundType.Down, "Monster3" },
        };

        public object userData;
        public IDepthComponent DepthComponent => depthCpn;

        public bool IsDead
        {
            get => isDead;
            protected set
            {
                isDead = value;

                // 设置层级最低
                if (value && depthCpn) depthCpn.SetLower();
            }
        }

        public void setHasDead(bool value)
        {
            isDead = value;
            gameObject.SetActive(!value);
        }

        public bool AllowControl { get; set; }

        // 初始化某个怪物的基础数据
        public virtual void initBaseData(int monsterId)
        {
            this.monsterId = monsterId;
            maxHp = MonsterDataMgr.getInstance().getMonsterHp(monsterId);
            curHp = maxHp;
            atkValue = MonsterDataMgr.getInstance().getMonsterAtkValue(monsterId);
            baseAtkValue = atkValue;
            atkDistance = MonsterDataMgr.getInstance().getMonsterAtkDistance(monsterId);
            baseAtkDistance = atkDistance;
            attackCdTimer = 0; // 第一次攻击前不需要冷却
            var helthCpn = componentSystem.TryGetComponent<HealthComponent>();
            if (helthCpn != null)
            {
                helthCpn.hp = maxHp;
                helthCpn.maxHp = maxHp;
            }
        }

        public BaseGameSceneManager GetSceneManager()
        {
            if (sceneManager != null) { return sceneManager; }
            else { return SceneManager; }
        }

        protected virtual void OnValidate()
        {
            FindCpn();
        }

        protected internal override void OnInit(object userData)
        {
            this.userData = userData;
            base.OnInit(userData);
            FindCpn();
            if (dropItem != null) { dropItem.SetActive(false); }
            if (woundPosParentTsf)
            {
                for (var i = 0; i < woundPosParentTsf.childCount; i++)
                {
                    // 相对根节点方向
                    woundPosList.Add(woundPosParentTsf.GetChild(i));
                }
            }
            curAtkCollsionType = AtkCollsionType.Enemy; // 默认攻击类型为敌方单位

            stateTagDict[MonsterState.Anger] = angryTag;
            stateTagDict[MonsterState.Weak] = weakTag;
            stateTagDict[MonsterState.Escape] = escapeTag;
            foreach (var stateTag in stateTagDict.Values)
            {
                if (stateTag != null) { stateTag.SetActive(false); }
            }
            updateMonsterGroundType(groundType);
            // 初始化部分会修改数据
            angerRate = 20;
            isProtect = false;
            isDead = false;
            hasPickUpDropItem = false;
            hasTriggerEscState = false;
            hasFirstCheckAngry = true;
            deadIsToObjectPool = false;
            stateData.Clear();
            curMonsterLayer = gameObject.layer;
            if (bodyCld != null)
            {
                // 修改身体组件图层让其始终作为受伤碰撞检测区域
                bodyCld.gameObject.layer = atkCheckLayer; 
            }
            if (groundCld != null)
            {
                groundCld.gameObject.layer = onlyMapObjLayer;
            }
            var moveCpn = componentSystem.TryGetComponent<MoveComponent>();
            if (moveCpn != null)
            {
                moveCpn.onTurnAction += OnTurnAction;
            }

            //gameObject.SetActive(false); 
            // 默认怪物设置为不可见

        }

        public void updateMonsterGroundType(GroundType newGroundType = GroundType.Center)
        {
            groundType = newGroundType;
            // 计算精灵的排序顺序
            var sceneMgr = GetSceneManager();
            int spriteSortIndex = 0;
            if (sceneMgr != null)
            {
                spriteSortIndex = sceneMgr.monsterAniSortData[groundType];
                // 因为怪物图片影子和掉落物图片都需要排序，所以这里计数+3
                sceneMgr.monsterAniSortData[groundType] += 3; 
            }
            // 设置精灵的排序层级
            if (groundToSpriteSortData.TryGetValue(groundType, out var sortName))
            {
                if (showdowArea != null)// 影子显示在最后面
                {
                    showdowArea.GetComponent<SpriteRenderer>().sortingLayerName = sortName;
                    showdowArea.GetComponent<SpriteRenderer>().sortingOrder = spriteSortIndex;
                }
                if (animationNode != null)
                {
                    animationNode.GetComponent<SpriteRenderer>().sortingLayerName = sortName;
                    animationNode.GetComponent<SpriteRenderer>().sortingOrder = spriteSortIndex + 1;
                }
                if (dropItem != null)
                {
                    dropItem.GetComponent<SpriteRenderer>().sortingLayerName = sortName;
                    dropItem.GetComponent<SpriteRenderer>().sortingOrder = spriteSortIndex + 2;
                }
            }
            // 设置怪物所处的图层
            if (groundToMLayerData.TryGetValue(groundType, out var layerName))
            {
                var layer = LayerMask.NameToLayer(layerName);
                if (layer == -1)
                {
                    Debug.LogWarning("==============没有设置怪物碰撞地形类型，将使用默认类型,怪物:" + gameObject);
                    return;
                }
                gameObject.layer = layer;
                // 同时修改移动组件中的地面碰撞检测脚本的值
                var moveCompoent = componentSystem.TryGetComponent<MoveComponent>();
                if (moveCompoent != null && moveCompoent.groundChecker != null)
                {
                    Dictionary<GroundType, string> groundToLayerData = new Dictionary<GroundType, string>() {
                        { GroundType.Up, "GroundUp" }, { GroundType.Center, "GroundCenter" },{ GroundType.Down, "GroundDown" },
                    };
                    var groundName = groundToLayerData[groundType];
                    var groundLayer = LayerMask.NameToLayer(groundName);
                    //moveCompoent.groundChecker.GroundLayerMask = 1 << groundLayer;
                    moveCompoent.groundChecker.GroundLayerMask = LayerMask.GetMask(groundName, "GroundCommon");
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            componentSystem.CheckComponents();
        }

        protected virtual void Update()
        {
            componentSystem.OnUpdate();
            // 记录逃跑状态持续的时间
            if (HasMonsterState(MonsterState.Escape))
            {
                escapeTimeCount += Time.deltaTime;
                if (escapeTimeCount >= escapeTime)
                {
                    escapeTimeCount = 0;
                    // 结束逃跑状态
                    EnterMonsterState(MonsterState.Escape, false);
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if (isDead) { return; }
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr != null && sceneMgr.GetSceneObjIsPause()) { return; }
            componentSystem.OnFixedUpdate();
        }

        //-----------------------------------------------------------------------------------

        public virtual Vector2 Wound(int value, Vector2 dir, float backDistance)
        {
            // 返回受伤点
            return Vector2.zero;
        }


        public bool IsInSameDepth(float y, float width = 0)
        {
            if (!depthCpn)
            {
                Debug.LogError(name + "没有设置深度组件");
                return false;
            }

            return depthCpn.IsInSameDepth(y, width);
        }

        public bool IsInSameDepth(IDepthObject other, float multiple = 0)
        {
            return depthCpn.IsInSameDepth(other, multiple);
        }

        protected virtual void FindCpn()
        {
            animator = GetComponent<Animator>();
            depthCpn = GetComponent<DepthComponent>();
            woundPosParentTsf = transform.Find("WoundPos");
        }

        protected Vector2 GetWoundPosTsf(Vector2 dir)
        {
            foreach (var pos in woundPosList)
                if (new Vector2(transform.position.x - pos.transform.position.x, 0).normalized == dir)
                    return pos.position;

            return Vector2.zero;
        }

        public override void HasHurt(DamageData damageData)
        {
            //if (damageData.atkCollsionType == AtkCollsionType.Player)
            //{
            //    // 受到来自玩家的伤害时播放受伤音效
            //    PlayBeHurtSfx(damageData.atkSkillName);
            //}
            // 受到伤害时播放受伤音效
            PlayBeHurtSfx(damageData.atkSkillName);
            if (componentSystem.TryGetComponent<BattleComponent>())
            {
                componentSystem.GetComponent<BattleComponent>().TakeDamage(damageData);
            }
            if (isDead) { return; }
            // 受伤时有概率进入愤怒状态
            if (hasFirstCheckAngry && !HasMonsterState(MonsterState.Anger))
            {
                hasFirstCheckAngry = false;
                var hasAnger = GameTools.randomRateHasGet(angerRate);
                if (hasAnger) { EnterMonsterState(MonsterState.Anger, hasAnger); }
            }
            // 检测当前HP
            var helthCompont = componentSystem.GetComponent<HealthComponent>();
            if (helthCompont != null && !HasMonsterState(MonsterState.Weak))
            {
                // HP低于某个比例则触发虚弱状态
                if (helthCompont.hp * 1.0 / maxHp * 100 <= weakRate)
                {
                    EnterMonsterState(MonsterState.Weak, true);
                }
            }
            else if (helthCompont != null && !hasTriggerEscState)
            {
                // HP低于某个比例则触发逃跑状态
                if (helthCompont.hp * 1.0 / maxHp * 100 <= escapeRate)
                {
                    EnterMonsterState(MonsterState.Escape, true);
                }
            }
        }

        public virtual void OnDead()
        {
            isProtect = true;
            isDead = true;
            bodyCld.isTrigger = true;
            footCld.isTrigger = true;
            // GroundCld 不在 CldController.nodes 里，SetActiveAll 关不到。
            // 死后必须显式关掉，否则 OnlyMapObj 实心盒会继续挡 PlayerFoot（树洞虫卵打碎后仍卡住）。
            // 替代方案：把 GroundCld 挂进 CldController；或改 Physics2D 矩阵 Ignore PlayerFoot↔OnlyMapObj（会影响天琬等挡板，本期不做）。
            if (groundCld != null)
            {
                groundCld.enabled = false;
            }
            foreach(var key in atkCollAreaNodeDict.Keys)
            {
                atkCollAreaNodeDict[key].SetActive(false);
            }
            if (atkCollNodeParent != null) { atkCollNodeParent.SetActive(false); }
            // 隐藏状态标志
            foreach (var tag in stateTagDict.Values)
            {
                if (tag != null) { tag.SetActive(false); }
            }
            StopMoveOnPosX();
            PlayDeadSfx();
            OnDeadEventFunc?.Invoke();

            if (!deadIsToObjectPool)
            {
                // 记录自己的死亡
                GetSceneManager().recordMonsterHasDead(this);
            }

            // 统一击杀任务上报：由 QuestManager 按 MonsterConfig.name 过滤，子类勿重复调用。
            if (monsterId > 0)
            {
                var monsterName = MonsterDataMgr.getInstance().GetMonsterName(monsterId);
                if (!string.IsNullOrEmpty(monsterName))
                {
                    QuestManager.getInstance().OnMonsterKilled(monsterName);
                }
            }
        }

        public virtual void StopMoveOnPosX()
        {
            var moveCpn = componentSystem.TryGetComponent<MoveComponent>();
            if (moveCpn != null)
            {
                moveCpn.StopMoveInX(); // 水平方向停止移动
            }
        }

        // 攻击后进入冷却
        public void EnterAttackCd()
        {
            attackCdTimer = atkDistance;
        }

        // 触发不同状态
        public void EnterMonsterState(MonsterState monsterState, bool isEnter=true)
        {
            
            switch(monsterState)
            {
                case MonsterState.Anger:
                    // 愤怒状态能力值变化
                    if (isEnter) { 
                        atkValue *= 2;
                        atkDistance /= 2f; // 攻击频率加快
                    } 
                    else { 
                        atkValue = baseAtkValue;
                        atkDistance = baseAtkDistance;

                    }
                    break;
                case MonsterState.Weak:
                    break;
                case MonsterState.Escape:
                    if (hasTriggerEscState && isEnter)
                    {
                        // 已经触发过逃跑状态了则不能继续触发
                        return;
                    }
                    hasTriggerEscState = true;
                    break;
                default:
                    break;
            }
            stateData[monsterState] = isEnter;
            //对应的怪物设置状态显示
            OnMonsterStateChange();
        }

        public virtual void OnMonsterStateChange()
        {
            foreach(var data in stateData)
            {
                var state = data.Key;
                var isEnter = data.Value;
                if (stateTagDict.TryGetValue(state, out var tag))
                {
                    if (tag != null)
                    {
                        tag.SetActive(isEnter);
                        PlayerStateAni(tag);
                    }
                }
            }
        }

        // 播放状态动画
        void PlayerStateAni(GameObject aniNode)
        {
            var animaEffectCompont = aniNode.GetComponent<AnimaEffectComponent>();
            if (animaEffectCompont != null)
            {
                animaEffectCompont.Play(2);
            }
        }

        // 是否触发某种怪物状态
        public bool HasMonsterState(MonsterState monsterState)
        {
            if (stateData.TryGetValue(monsterState, out var isTrigger))
            {
                return isTrigger;
            }
            return false;
        }

        // 显示掉落物
        public void ShowDropItem()
        {
            if (dropItem != null && hasDropItem)
            {
                dropItem.SetActive(true);
            }
            
        }

        // 移除掉落物
        public void RemoveDropItem()
        {
            if (dropItem != null)
            {
                dropItem.SetActive(false);
            }
            // 移除掉落物后X秒尸体消失
            MonsterDelayRemove();
        }

        // 怪物逐渐消失逻辑
        public virtual void MonsterDelayRemove()
        {
            var animation = UIUtils.findChild(gameObject, "Animation");
            if (animationNode != null)
            {
                var fadeAct = GameActionMgr.runFadeActionSpriteRender(animationNode, 0, 2f).SetDelay(3);
                if (fadeAct == null) { return; }
                fadeAct.onComplete = () =>
                {
                    MonsterRealRemove();
                };
            }
            if (showdowArea != null)
            {
                GameActionMgr.runFadeActionSpriteRender(showdowArea, 0, 2f).SetDelay(3);
            }
            
        }

        public virtual void MonsterRealRemove()
        {
            if (deadIsToObjectPool)
            {
                var entityComponentGM = GameManager.GetGMComponent<EntityComponentGM>();
                entityComponentGM.HideEntity(Entity);// 移除实体
            }
            else
            {
                Destroy(gameObject);
            }
            
        }

        // 设置掉落物被拾取
        public void SetDropItemHasPickUp()
        {
            if (dropItem != null)
            {
                hasPickUpDropItem = true;
            }
        }

        public bool GetDropItemHasPickUp()
        {
            return hasPickUpDropItem;
        }

        // 怪物死亡后的逻辑
        public virtual void MonsterDeadEndEvent()
        {
            // 显示掉落物，有掉落物的怪物拾取掉落物后怪物才会被移除
            if (!hasDropItem)
            {
                // 没有掉落物的怪物直接设置消失
                MonsterDelayRemove();
            }
            else
            {
                ShowDropItem();
            }

            // 怪物变成尸体后设置图片排序
            var sceneMgr = GetSceneManager();
            int spriteSortIndex = 0;
            if (sceneMgr != null)
            {
                spriteSortIndex = sceneMgr.deadMonsterSpriteSort;
                // 因为怪物图片影子和掉落物图片都需要排序，所以这里计数变化3
                sceneMgr.deadMonsterSpriteSort -= 3;
            }
            var mosnterLayerName = "Monster1"; // 显示在最下层的怪物层级
            // 这里排序向下减少是为了不影响原本monster1图层其他怪物的显示
            if (showdowArea != null)// 影子显示在最后面
            {
                showdowArea.GetComponent<SpriteRenderer>().sortingLayerName = mosnterLayerName;
                showdowArea.GetComponent<SpriteRenderer>().sortingOrder = spriteSortIndex - 2;
            }
            if (animationNode != null)
            {
                animationNode.GetComponent<SpriteRenderer>().sortingLayerName = mosnterLayerName;
                animationNode.GetComponent<SpriteRenderer>().sortingOrder = spriteSortIndex - 1;
            }
            if (dropItem != null)
            {
                dropItem.GetComponent<SpriteRenderer>().sortingLayerName = mosnterLayerName;
                dropItem.GetComponent<SpriteRenderer>().sortingOrder = spriteSortIndex;
            }
        }

        #region 音效相关
        public virtual void PlayDeadSfx(bool isPlay=true)
        {
        }

        public virtual void PlayBeHurtSfx(string atkName, bool isPlay = true)
        {
            // 踢腿动作命中目标的音效是不一样的
            var realResName = "SmashAtk_1" == atkName ? "踢腿.mp3" : "命中.WAV";
            commonSfxCpn.ChangeSoundRes(realResName);
            PlayAudio(commonSfxCpn, isPlay);
        }

        public virtual void PlayAttackSfx(bool isPlay = true)
        {

        }

        #endregion

        public virtual void OnTurnAction(Vector2 dir)
        {
            if (buffArea != null)
            {
                var rotationY = gameObject.transform.rotation.y;
                var newQuatern = dir.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -rotationY, 0);
                buffArea.gameObject.transform.rotation = newQuatern;
            }
        }

        // 获取怪物的当前生命值
        public virtual float GetMonsterCurHp()
        {
            var helthCpn = componentSystem.TryGetComponent<HealthComponent>();
            if (helthCpn != null)
            {
                return helthCpn.hp;
            }
            else
            {
                return 0;
            }
        }
        // 获取怪物的最大生命值
        public virtual float GetMonsterMaxHp()
        {
            var helthCpn = componentSystem.TryGetComponent<HealthComponent>();
            if (helthCpn != null)
            {
                return helthCpn.maxHp;
            }
            else
            {
                return 0;
            }
        }


    }
}