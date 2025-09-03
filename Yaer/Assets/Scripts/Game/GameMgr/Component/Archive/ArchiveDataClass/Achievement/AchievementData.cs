using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class AchievementData: BaseArchiveData
    {
        [HideInInspector]
        public Dictionary<AchievementType, int> achievementProData = new Dictionary<AchievementType, int>();// 成就数据，ID:成就是否完成

        string achievenBaseName = "Achievement_{0}";

        // ================数据存取
        public override void ParseInternal(MasterGameData masterData)
        {
            var achieveCount = AchievementDataMgr.getInstance().GetAchievementCount();
            for (var i = AchievementType.KillSlime_1; (int)i <= achieveCount; i++)
            {
                var realKeyName = string.Format(achievenBaseName, (int)i);
                achievementProData[i] = masterData.GetValue(realKeyName, 0);
            }
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            var achieveCount = AchievementDataMgr.getInstance().GetAchievementCount();
            for(var i = AchievementType.KillSlime_1; (int)i <= achieveCount; i++)
            {
                var realKeyName = string.Format(achievenBaseName, i);
                if (!achievementProData.ContainsKey(i)) { achievementProData[i] = 0; }
                var progressValue = achievementProData[i];
                masterData.SetValue(realKeyName, progressValue);
            }
        }
    }
}