using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Name("获取存档数据string")]
    public class GetArchiveStringActionTask<T> : GetArchiveFieldActionTask<T, string> where T : BaseArchiveData
    {

    }
}

