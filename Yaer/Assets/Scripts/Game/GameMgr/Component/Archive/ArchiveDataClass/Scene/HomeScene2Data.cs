using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    [Serializable]
    public class HomeScene2Data : BaseArchiveData
    {
        public bool firstCompleteChangeClothes;
        public bool boxOpened;

        public override void ParseInternal(MasterGameData masterData)
        {
            boxOpened = masterData.GetValue<bool>("HomeScene2Data_boxOpened", false);
            firstCompleteChangeClothes = masterData.GetValue<bool>("HomeScene2Data_firstCompleteChangeClothes", true);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("HomeScene2Data_boxOpened", boxOpened);
            masterData.SetValue("HomeScene2Data_firstCompleteChangeClothes", firstCompleteChangeClothes);
        }
    }
}