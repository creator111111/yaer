using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Path;
using UnityEngine;
using static Game.GameRuntime.Entities.Player.Components.PlayerInputComponent;

namespace Game.GameRuntime.Entities.SceneEntities
{
    // 玩家按键指引触发器
    public class PlayerKeyBoardGuideTrigger : MonoBehaviour
    {
        public string guideKeyName; // 按键指引的动作名称
        public bool isOnceTrigger = false; // 是否是只触发一次

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (PlayerGuideMgr.getInstance().hasAnyKeyTips()) { return; }
            PlayerGuideMgr.getInstance().PraseActName(guideKeyName);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 离开指引区域后去掉按键提示
            if (!PlayerGuideMgr.getInstance().hasAnyKeyTips()) { return; }
            PlayerGuideMgr.getInstance().RemoveKeyTips(guideKeyName);
            if (isOnceTrigger)
            {
                gameObject.SetActive(false);
            }
        }

        protected void Update()
        {
            
        }
    }

}

