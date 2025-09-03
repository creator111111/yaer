using Game.GameMgr.Component.Archive.ArchiveDataClass;

namespace Game.GameMgr.Component.Archive
{
    public class ArchiveDirectoryInfo
    {
        public string path;
        public ArchiveInfo info;
        
        public string Guid => info.guid;
    }
}