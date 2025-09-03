using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Newtonsoft.Json.Linq;
using NodeCanvas.Tasks.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.SceneObjData
{
    // 记录所有环境类场景对象的状态的数据类
    [Serializable]
    public class SceneObjectData : BaseArchiveData
    {
        public bool forestEastTenWanHasBreak; // 龙城郊东的藤蔓是否被砍断了
        public bool verdantTenWanHasBreak; // 翠绿走廊的藤蔓是否被砍断了

        string forestEastTenwanKey = "forestEastTenwan";
        string verdantTenwanKey = "verdantTenwan";

       

        // =========================场景中的藤蔓障碍物相关逻辑
        public void RecordTenWanBreakState(string tenWanName, bool hasBreak=true)
        {
            switch (tenWanName)
            {
                case "forestEastTenwan":
                    forestEastTenWanHasBreak = hasBreak;
                    return;
                case "verdantTenwan":
                    verdantTenWanHasBreak = hasBreak;
                    return;
                default:
                    return;
            }
        }

        public bool GetTenWanHasBreak(string tenWanName)
        {
            Dictionary<string, bool> tenWanBreakStateDatas = new Dictionary<string, bool>() {
                 {forestEastTenwanKey, forestEastTenWanHasBreak}, {verdantTenwanKey, verdantTenWanHasBreak}
             };
            if (!tenWanBreakStateDatas.ContainsKey(tenWanName)) { return false; }
            return tenWanBreakStateDatas[tenWanName];
        }

        // ================数据存取
        public override void ParseInternal(MasterGameData masterData)
        {
            forestEastTenWanHasBreak = masterData.GetValue(forestEastTenwanKey, false);
            verdantTenWanHasBreak = masterData.GetValue(verdantTenwanKey, false);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue(forestEastTenwanKey, forestEastTenWanHasBreak);
            masterData.SetValue(verdantTenwanKey, verdantTenWanHasBreak);
        }
    }
}