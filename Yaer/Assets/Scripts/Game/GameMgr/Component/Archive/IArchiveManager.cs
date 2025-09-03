using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Manager.Base;

namespace Game.GameMgr.Component.Archive
{
    public interface IArchiveManager : IManager
    {
        List<ArchiveInfo> GetAllArchiveInfo();
        ArchiveInfo GetNowArchiveInfo();
        void SetNowArchiveInfo(ArchiveInfo archiveInfo);
        T GetArchiveData<T>() where T : BaseArchiveData, new();
        void UnloadArchive();

        void DeleteArchive(string guid);

        // void NewArchive();
        void LoadArchive(string guid);
        void SaveArchive();
        void SaveAsNewArchive();
        void CoverArchive(string toGuid);
    }
}