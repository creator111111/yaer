using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    public class WestRappRoadData : BaseArchiveData
    {
        public bool KillBossMogut;
        public bool AwakeBossMogut;

        public override void ParseInternal(MasterGameData masterData)
        {
            KillBossMogut = masterData.GetValue("WestRappRoadData_KillBossMogut", false);
            AwakeBossMogut = masterData.GetValue("WestRappRoadData_AwakeBossMogut", false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("WestRappRoadData_KillBossMogut", KillBossMogut);
            masterData.SetValue("WestRappRoadData_AwakeBossMogut", AwakeBossMogut);
        }
    }
}

