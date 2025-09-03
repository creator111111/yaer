using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Base;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class SaveGameHandleComponent: BaseComponentGM
    {
        private ArchiveComponentGM archiveComponentGM;

        public override void OnInit()
        {
            base.OnInit();
            
            archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
        }

        /// <summary>
        /// 专门处理存档前处理
        /// </summary>
        public void SaveGame()
        {
           HandlePlayerData();
        }

        private void HandlePlayerData()
        {
            var playerSceneData = archiveComponentGM.GetData<PlayerSceneData>();
            playerSceneData.sceneName = GameManager.GetGMComponent<ChangeSceneComponentGM>().NowSceneName;
            playerSceneData.lastSceneName = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            if (playerLogic)
            {
                playerSceneData.pos = playerLogic.transform.position;
                var sceneMgr = GameManager.GetGameSceneManager();

                if (sceneMgr != null)
                {
                    var realSceneMgr = sceneMgr as BaseGameSceneManager;
                    if (realSceneMgr.curStoryPrefab != null && realSceneMgr.curStoryPrefab.name == "WestRappRoadGoblinAndGusha")
                    {
                        // 如果是在特殊事件中保存，则需要强制设置保存时的人物坐标
                        var story = WestRappRoadBossBattleMgr.getInstance().GetBossBattleStory();
                        playerSceneData.pos = story.eventSavePosNode.transform.position;
                    }
                }
                var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
                // 人物在存档中的状态只有站立和蹲下两种状态
                if (csAnimator.GetSign(PlayerStateSign.Squat))
                {
                    playerSceneData.playerState = PlayerStateSign.Squat;
                }
                else
                {
                    // 非下蹲状态都记录为站立状态
                    playerSceneData.playerState = PlayerStateSign.Idle;
                }
                // 记录人物是否处于特殊区域中
                playerSceneData.isInTreeBridge = ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge;
                
            }
        }
    }
}