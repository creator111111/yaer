using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    [Serializable]
    public class SelectClothesSceneData : BaseArchiveData
    {
        public int exitTimes;
        public bool pEggEqual3; // 3次换上彩蛋
        public bool pEggMore3; // 3次以上换上彩蛋

        public override void ParseInternal(MasterGameData masterData)
        {
            exitTimes = masterData.GetValue<int>("SelectClothesSceneData_exitTimes", 0);
            pEggEqual3 = masterData.GetValue<bool>("SelectClothesSceneData_pEggEqual3", false);
            pEggMore3 = masterData.GetValue<bool>("SelectClothesSceneData_pEggMore3", false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("SelectClothesSceneData_exitTimes", exitTimes);
            masterData.SetValue("SelectClothesSceneData_pEggEqual3", pEggEqual3);
            masterData.SetValue("SelectClothesSceneData_pEggMore3", pEggMore3);
        }
    }
}