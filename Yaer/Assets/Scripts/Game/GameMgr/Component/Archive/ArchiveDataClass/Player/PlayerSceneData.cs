using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass.Struct;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.GameRuntime.UI.FormLogic.Fighting;
using Game.Static.Name.Res;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class PlayerSceneData : BaseArchiveData
    {
        public string sceneName;
        public Vector2 pos { get; set; }
        public string lastSceneName;
        public string playerState;
        public bool isInTreeBridge { get; set; } // 当前人物是否处于东城郊区的树洞中

        public override void ParseInternal(MasterGameData masterData)
        {
            sceneName = masterData.GetValue("PlayerSceneInfo_sceneName", SceneName.InitScene);
            pos = masterData.GetValue<Vector2>("PlayerSceneInfo_pos");
            lastSceneName = masterData.GetValue("PlayerSceneInfo_lastSceneName", SceneName.InitScene);
            playerState = masterData.GetValue("PlayerSceneInfo_playerState", PlayerStateSign.Idle);
            isInTreeBridge = masterData.GetValue("PlayerSceneInfo_isInTreeBridge", false);
            ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge = isInTreeBridge;
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("PlayerSceneInfo_sceneName", sceneName);
            masterData.SetValue("PlayerSceneInfo_pos", pos);
            masterData.SetValue("PlayerSceneInfo_lastSceneName", lastSceneName);
            masterData.SetValue("PlayerSceneInfo_playerState", playerState);
            masterData.SetValue("PlayerSceneInfo_isInTreeBridge", isInTreeBridge);
        }
    }
}