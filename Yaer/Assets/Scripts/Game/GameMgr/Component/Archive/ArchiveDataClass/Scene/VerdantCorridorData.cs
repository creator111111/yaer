using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    public class VerdantCorridorData : BaseArchiveData
    {
        public bool PickGushaNacklace;
        public override void ParseInternal(MasterGameData masterData)
        {
            PickGushaNacklace = masterData.GetValue<bool>("VerdantCorridorData_PickGushaNacklace", false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue<bool>("VerdantCorridorData_PickGushaNacklace", PickGushaNacklace);
        }
    }
}

