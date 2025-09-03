using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Name("获取存档数据int")]
    public class GetArchiveIntActionTask<T> : GetArchiveFieldActionTask<T, int> where T : BaseArchiveData
    {

    }
}

