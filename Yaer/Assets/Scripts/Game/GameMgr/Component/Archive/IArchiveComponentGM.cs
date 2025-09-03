using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive
{
    public interface IArchiveComponentGM
    {
        T GetData<T>() where T : BaseArchiveData, new();
        void SaveTempGameArchive();
        void LoadArchive();
    }
}