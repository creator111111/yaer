using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    public class ForestEastSceneData : BaseArchiveData
    {
        public bool TreeBridgeFall;

        public override void ParseInternal(MasterGameData masterData)
        {
            TreeBridgeFall = masterData.GetValue("ForestEastSceneData_TreeBridgeFall", false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("ForestEastSceneData_TreeBridgeFall", TreeBridgeFall);
        }
    }
}