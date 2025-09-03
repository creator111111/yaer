using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Name("获取存档数据Bool")]
    public class GetArchiveBoolActionTask<T> : GetArchiveFieldActionTask<T, bool> where T : BaseArchiveData
    {

    }
}

