using Game.DataTable.AchievenmentConfig;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Settings;
using Game.Static.Name.Settings;
using Game.Static.Path;
using GameFramework.CoreExtend.Systems.Setting;
using GameFramework.DataTable;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class SceneMonsterData: BaseArchiveData
    {
        [HideInInspector]
        public Dictionary<string, bool> sceneMonsterSafeStates = new Dictionary<string, bool>();// 当前场景中的怪物存活情况

        string dataBaseName = "SceneMonsterSafeData_{0}";

        // ================数据存取
        public override void ParseInternal(MasterGameData masterData)
        {
            foreach (var data in masterData.data)
            {
                var key = data.Key;
                if (key.StartsWith("SceneMonsterSafeData_"))
                {
                    sceneMonsterSafeStates[key] = masterData.GetValue(key, false);
                }
            }
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            foreach(var data in sceneMonsterSafeStates)
            {
                var key = data.Key;
                var value = data.Value;
                masterData.SetValue(key, value);
            }
        }

        public bool GetMonsterHasDeadByTag(int tag)
        {
            var key = string.Format(dataBaseName, tag);
            if (sceneMonsterSafeStates.ContainsKey(key))
            {
                return sceneMonsterSafeStates[key];
            }
            return false;
        }

        public void RecordMonsterByTag(int tag)
        {
            var key = string.Format(dataBaseName, tag);
            sceneMonsterSafeStates[key] = true;
        }
    }
}