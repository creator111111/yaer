using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Name("获取存档数据float")]
    public class GetArchiveFloatActionTask<T> : GetArchiveFieldActionTask<T, float> where T : BaseArchiveData
    {

    }
}

