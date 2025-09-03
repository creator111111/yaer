using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.GameSceneManager.Base;
using System.Linq;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Base
{
    public class BaseMonsterState : BaseState
    {
        protected BaseMonster monsterLogic;
        protected AnimationEventComponent animationEventComponent;
        protected float timeCount = 0;
        protected float timeDistance = 2;

        float pauseAniTimeCount = 0;
        float pauseAniFrameTime = 0; // 暂停当前动画X秒
        int curAnimatorSpeed = 1;// 默认的动画速度
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            monsterLogic = stateMachine.GetEntityLogic<BaseMonster>();
            //if (monsterLogic == null) { return; }
            animationEventComponent = monsterLogic.GetComponent<AnimationEventComponent>();
            // 注册怪物死亡之后的回调函数
            animationEventComponent.RegisterEvent("RemoveMonsterOnDead", RemoveMonsterOnDead);
        }

        public override void Update()
        {
            base.Update();
            if (monsterLogic.GetSceneManager() == null) { return; }
            if (monsterLogic.animationNode == null) { return; }
            var animator = monsterLogic.GetComponent<Animator>();
            if (animator == null) { return; }
            var mgr = monsterLogic.GetSceneManager();
            if (mgr.GetSceneObjAniIsPause())
            {
                if (curAnimatorSpeed != 0)
                {
                    curAnimatorSpeed = 0;
                    if (monsterLogic.componentSystem.TryGetComponent<MoveComponent>() != null)
                    {
                        monsterLogic.componentSystem.GetComponent<MoveComponent>().StopMove();
                    }
                    animator.speed = curAnimatorSpeed;// 暂停动画
                }
            }
            else
            {
                pauseAniTimeCount += Time.deltaTime;
                if (pauseAniTimeCount > pauseAniFrameTime)
                {
                    pauseAniTimeCount = 0;
                    pauseAniFrameTime = 0;
                    
                    if (curAnimatorSpeed != 1)
                    {
                        curAnimatorSpeed = 1;
                        animator.speed = curAnimatorSpeed;// 恢复动画
                    }
                }
            }
        }

        protected override void ChangeState<T>()
        {
            base.ChangeState<T>();
            // 转换状态时需要清除当前状态的攻击碰撞体
            RemoveMAtkCollsion("defalutName");
        }

        // 进入子状态机
        protected override T EnterSubStateMachine<T>()
        {
            // 转换状态时需要清除当前状态的攻击碰撞体
            RemoveMAtkCollsion("defalutName");

            var stateMachine = base.EnterSubStateMachine<T>();

            return stateMachine;
        }

        // 创建攻击碰撞体
        protected virtual void CreateMAtkCollsion(string atkArgs)
        {
            // 在原来的动画事件参数基础上再进行解析
            var valueList = atkArgs.Split(',');
            var monsterName = valueList.Count() > 0 ? valueList[0] : ""; // 怪物名字
            var atkTypeName = valueList.Count() > 1 ? valueList[1] : ""; // 怪物招式类型
            if (monsterLogic.atkCollAreaNodeDict.ContainsKey(atkTypeName))
            {
                //var oldAtkNode = monsterLogic.atkCollAreaNodeDict[atkTypeName];
                monsterLogic.atkCollAreaNodeDict[atkTypeName].SetActive(true);
                var collArea = UIUtils.findChild(monsterLogic.atkCollAreaNodeDict[atkTypeName], "collArea");
                if (collArea == null) { return; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.initAtkDataByName(monsterLogic, monsterLogic.curAtkCollsionType, atkTypeName);
                return;
                //Object.Destroy(monsterLogic.atkCollAreaNode);
                //monsterLogic.atkCollAreaNode = null;
            }
            var resMgr = GameManager.GetGMComponent<ResComponentGM>();
            var prefabPath = "Assets/GameRes/Prefabs/Entity/Effect/Monster/AtkCollsion/{0}/CollArea_{1}.prefab";
            var realPath = string.Format(prefabPath, monsterName, atkTypeName);
            resMgr.LoadAsset<GameObject>(realPath, (obj) =>
            {
                if (monsterLogic.atkCollAreaNodeDict.ContainsKey(atkTypeName)) { return; }
                monsterLogic.atkCollAreaNodeDict[atkTypeName] = Object.Instantiate(obj);
                var parentNode = monsterLogic.atkCollNodeParent == null ? monsterLogic.gameObject : monsterLogic.atkCollNodeParent;
                monsterLogic.atkCollAreaNodeDict[atkTypeName].transform.SetParent(parentNode.transform, false);
                var collArea = UIUtils.findChild(monsterLogic.atkCollAreaNodeDict[atkTypeName], "collArea");
                if (collArea == null) { return; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.initAtkDataByName(monsterLogic, monsterLogic.curAtkCollsionType, atkTypeName);
            });
        }

        // 移除攻击碰撞体
        protected virtual void RemoveMAtkCollsion(string atkArgs)
        {
            // 在原来的动画事件参数基础上再进行解析
            var valueList = atkArgs.Split(',');
            var monsterName = valueList.Count() > 0 ? valueList[0] : ""; // 怪物名字
            var atkTypeName = valueList.Count() > 1 ? valueList[1] : ""; // 怪物招式类型
            foreach(var atkCollAreaNode in monsterLogic.atkCollAreaNodeDict.Values)
            {
                var collArea = UIUtils.findChild(atkCollAreaNode, "collArea");
                if (collArea == null) { continue; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.clearData();
                atkCollAreaNode.SetActive(false);
            }
            //if (monsterLogic.atkCollAreaNodeDict.ContainsKey(atkTypeName))
            //{
            //    //var oldAtkNode = monsterLogic.atkCollAreaNodeDict[atkTypeName];
            //    var collArea = UIUtils.findChild(monsterLogic.atkCollAreaNodeDict[atkTypeName], "collArea");
            //    if (collArea == null) { return; }
            //    var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
            //    baseAtkCollsion.clearData();
            //    monsterLogic.atkCollAreaNodeDict[atkTypeName].SetActive(false);
            //    return;
            //    //Object.Destroy(monsterLogic.atkCollAreaNode);
            //    //monsterLogic.atkCollAreaNode = null;
            //}
        }

        // 怪物死亡之后检测是否移除
        public virtual void RemoveMonsterOnDead(string objName)
        {
            
            monsterLogic.MonsterDeadEndEvent(); // 触发怪物死亡后的事件
        }

        // 怪物逃跑
        public void MonsterEscape(string args)
        {
            // 怪物逃跑动画播完后淡出消失
            var animation = UIUtils.findChild(monsterLogic.gameObject, "Animation");
            var fadeAct = GameActionMgr.runFadeActionSpriteRender(animation, 0, 2f);
            if (fadeAct == null) { return; }
            fadeAct.onComplete = () =>
            {
                monsterLogic.MonsterDeadEndEvent();
                //Object.Destroy(monsterLogic.gameObject);
            };
        }

        // 刷新怪物位置，用于动画帧事件中更新坐标
        public void UpdateMonsterPos(string args)
        {
            //Debug.Break();
            var valueList = args.Split(',');
            var posX = valueList.Count() > 0 ? float.Parse(valueList[0]) : 0; // X坐标
            var posY = valueList.Count() > 1 ? float.Parse(valueList[1]) : 0; // Y坐标
            var oldPos = monsterLogic.transform.position;
            monsterLogic.transform.position = oldPos + new Vector3(posX, posY);
        }

        public bool CheckObjIsPause()
        {
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr != null && sceneMgr.GetSceneObjIsPause())
            {
                return true;
            }
            return false;
        }

        // 在动画某一帧播放一个音效
        public virtual void PlayAudioSfx(string resPathName)
        {
            monsterLogic.commonSfxCpn.ChangeSoundRes(resPathName);
            monsterLogic.PlayAudio(monsterLogic.commonSfxCpn, true);
        }

        // 暂停动画帧多少秒
        public virtual void StopAniFrameWithSec(string args)
        {
            var sec = float.Parse(args);
            var animator = monsterLogic.GetComponent<Animator>();
            if (animator == null) { return; }
            pauseAniFrameTime = sec;
            curAnimatorSpeed = 0;
            animator.speed = curAnimatorSpeed;// 暂停动画
        }

        // 造成屏幕震动
        public virtual void ShowCameraImpluse(string args)
        {
            
        }
    }
}