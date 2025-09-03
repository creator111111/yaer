using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    [Serializable]
    public class ForestSceneData : BaseArchiveData
    {
        public bool laiFirstDialogue; // 莱的第一次对话完成
        public bool laiFlyAway;
        public bool homeDoorStoryComplete;

        /// <summary>
        /// 是否第一次与兔子对话
        /// </summary>
        public bool rabbitFirstDialogue;
        /// <summary>
        /// 是否选择了带走兔子
        /// </summary>
        public bool chooseTakeRabbit;
        
        public override void ParseInternal(MasterGameData masterData)
        {
            laiFirstDialogue = masterData.GetValue<bool>("ForestSceneData_laiFirstDialogue", false);
            laiFlyAway = masterData.GetValue<bool>("ForestSceneData_laiFlyAway", false);
            homeDoorStoryComplete = masterData.GetValue<bool>("ForestSceneData_homeDoorStoryComplete", false);
            rabbitFirstDialogue = masterData.GetValue<bool>("ForestSceneData_rabbitFirstDialogue", true);
            chooseTakeRabbit = masterData.GetValue<bool>("ForestSceneData_chooseTakeRabbit", false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("ForestSceneData_laiFirstDialogue", laiFirstDialogue);
            masterData.GetValue<bool>("ForestSceneData_laiFlyAway", laiFlyAway);
            masterData.SetValue("ForestSceneData_homeDoorStoryComplete", homeDoorStoryComplete);
            masterData.SetValue("ForestSceneData_rabbitFirstDialogue", rabbitFirstDialogue);
            masterData.SetValue("ForestSceneData_chooseTakeRabbit", chooseTakeRabbit);
        }
    }
}