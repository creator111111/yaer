using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.SceneObjData;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

// 场景中的藤蔓，不是怪物，仅使用怪物的受击逻辑，所以其他地方逻辑和通用怪物不一样
namespace Game.GameRuntime.Entities.Monster.TenWanSceneObj
{
    public class TenWanSceneObjLogic : BaseMonster
    {
        public BaseGameSceneManager gameSceneMgr;
        public GameObject tenWanStoryTrigger;
        int curHitCount = 0; // 当前受击次数
        public int maxHitCount = 2; // 最大受击次数

        public string tenWanName;

        protected override void Start()
        {
            baseMoveSpeed = 0;// 不能移动
            if (GetSceneObjectData() != null && GetSceneObjectData().GetTenWanHasBreak(tenWanName))
            {
                animator.Play("TenwanBroken2", 0, 1);
                animator.speed = 0;
                groundCld.isTrigger = true; // 取消阻挡
                if (tenWanStoryTrigger !=null) { tenWanStoryTrigger.SetActive(false); }
            }
        }
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        public override void HasHurt(DamageData damageData)
        {
            if (damageData.atkCollsionType == AtkCollsionType.Player)
            {
                curHitCount++;
                isProtect = true;
                // 受到来自玩家的伤害时播放受伤音效
                PlayBeHurtSfx(damageData.atkSkillName);
                // 播放动画
                if (curHitCount == 1)
                {
                    animator.Play("TenwanBroken1");
                }
                else
                {
                    animator.Play("TenwanBroken2");
                }
            }
        }

        // 在动画编辑器中添加帧事件
        void OnBreak1StateEnd()
        {
            isProtect = false;
        }

        void OnBreak2StateEnd()
        {
            // 藤蔓完全被砍断后设置相关事件结束
            if (tenWanStoryTrigger != null) { tenWanStoryTrigger.SetActive(false); }
            GetSceneObjectData().RecordTenWanBreakState(tenWanName, true);
            isDead = true;
           
            groundCld.isTrigger = true; // 取消阻挡
            animator.speed = 0;
        }

        public SceneObjectData GetSceneObjectData()
        {
            if (gameSceneMgr == null) { return null; }
            return gameSceneMgr.GetArchiveData<SceneObjectData>();
        }


        public override void OnDead()
        {
            base.OnDead();
            isDead = true;
            isProtect = true;
        }
        
    }
}