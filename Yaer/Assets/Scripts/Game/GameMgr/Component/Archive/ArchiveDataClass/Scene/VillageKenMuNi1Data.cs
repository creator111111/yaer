using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    /// <summary>
    /// 村场景 <c>Village_KenMuNi1</c> 专用存档（与 <see cref="ForestSceneData"/> 分离，避免森林语义脏挂）。
    /// </summary>
    /// <remarks>
    /// 原因（0901 宝箱案）：巨树 2 楼 Hp/Mp 箱不可复用西境/卧室旗，否则跨场景串档。
    /// 替代方案：把旗塞进 ForestSceneData——可扩展但语义脏，报告否决。
    /// </remarks>
    [Serializable]
    public class VillageKenMuNi1Data : BaseArchiveData
    {
        /// <summary>巨树 2 楼 WalkArea2 内 Hp/Mp 宝箱是否已开（单次）。</summary>
        public bool tree2fHpMpBoxOpened;

        public override void ParseInternal(MasterGameData masterData)
        {
            tree2fHpMpBoxOpened = masterData.GetValue(
                "VillageKenMuNi1Data_tree2fHpMpBoxOpened", false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue(
                "VillageKenMuNi1Data_tree2fHpMpBoxOpened", tree2fHpMpBoxOpened);
        }
    }
}
