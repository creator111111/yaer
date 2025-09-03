using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.HomeScene2
{
    public class HomeScene2Box : BaseSceneEntityLogic
    {
        public Animator animator;

        private bool opened; // 已经打开过标识

        public SoundToggleComponent soundSfxCpn;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            animator = GetComponent<Animator>();
            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent += OpenBox;

            if (SceneManager.GetArchiveData<HomeScene2Data>().boxOpened)
            {
                animator.SetBool("Open", true);
                opened = true;
            }
            // 设置宝箱是否可与玩家交互
            componentSystem.GetComponent<InteractiveComponent>().entityControll.canTouchWithPlayer = !opened;
        }

        public override void OnShutDown()
        {
            base.OnShutDown();

            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent -= OpenBox;
        }

        /// <summary>
        /// 打开箱子
        /// </summary>
        public void OnHomeScene2Box_OpenBox()
        {
            SceneManager.GetArchiveData<HomeScene2Data>().boxOpened = true;
            animator.SetBool("Open", true);
            soundSfxCpn.PlaySound();
        }
        /// <summary>
        /// 获得艾琳之剑
        /// </summary>
        public void OnHomeScene2Box_GetSword()
        {
            // 储存进背包
            SceneManager.GetArchiveData<PlayerBagData>().AddMainItem(EMainItemName.AiLinSword);
            // 提示信息
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetAiLinSword");
        }

        private void OpenBox(InteractiveComponent component)
        {
            if (opened) return;

            opened = true;
            componentSystem.GetComponent<InteractiveComponent>().entityControll.canTouchWithPlayer = !opened;
            // 设置按键提示消失
            var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            if (playerLogic)
            {
                if (playerLogic.keyTipsNode && playerLogic.keyTipsNode.activeSelf)
                {
                    playerLogic.showKeyTipsNode(false);
                }
            }
            // 注册剧情事件

            // 触发剧情
            SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene2Box");
        }
    }
}