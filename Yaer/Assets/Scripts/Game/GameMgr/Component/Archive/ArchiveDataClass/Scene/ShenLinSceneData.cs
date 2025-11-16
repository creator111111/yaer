using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    public class ShenLinSceneData : BaseArchiveData
    {
        // 可以根据需要添加场景特定的数据字段
        
        public override void ParseInternal(MasterGameData masterData)
        {
            // 从存档数据中解析场景特定数据
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            // 将场景特定数据序列化到存档中
        }
    }
}