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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.GameRuntime.Entities.SceneEntities.HomeScene2
{
    public class WestRappRoadBox : BaseSceneEntityLogic
    {
        public Animator animator;

        private bool opened; // 已经打开过标识

        public SoundToggleComponent soundSfxCpn;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            animator = GetComponent<Animator>();
            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent += OpenBox;

            if (SceneManager.GetArchiveData<WestRappRoadData>().BoxOpened)
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
        public void OnWestRappRoadBox_OpenBox()
        {
            SceneManager.GetArchiveData<WestRappRoadData>().BoxOpened = true;
            animator.SetBool("Open", true);
            soundSfxCpn.PlaySound();


            SceneManager.GetArchiveData<PlayerBagData>().AddMainItem(EMainItemName.HpBall, 3);
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetHpBall");
            SceneManager.GetArchiveData<PlayerBagData>().AddMainItem(EMainItemName.MpBall, 3);
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetMpBall");
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("ImportantEvent");
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
            OnWestRappRoadBox_OpenBox();
        }
    }
}

