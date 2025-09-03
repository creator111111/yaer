using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    // 树洞右侧悬崖处的彩蛋对话事件
    public class ForestEastTreeTriggerSecretTigger : SimpleStoryTrigger
    {

        protected override void TriggerStory()
        {
            base.TriggerStory();
            //// 记录成就
            //AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.FindOneSecret, 1);
        }
    }
}

