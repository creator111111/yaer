namespace Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass
{
    public interface IArchiveData
    {
        void ParseInternal(MasterGameData masterData);
        void SerializeInternal(MasterGameData masterData);
    }
}