using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class PlayerClothesData : BaseArchiveData
    {
        private Dictionary<string, string> clothesDataDic = new Dictionary<string, string>();

        public string GetClothesName(string bone)
        {
            if (clothesDataDic.TryGetValue(bone, out var clothesName)) return clothesName;
            Debug.LogError("没有找到" + bone);
            return null;
        }

        public string AddClothes(string bone, string clothesName)
        {
            clothesDataDic[bone] = clothesName;
            return clothesName;
        }

        public Dictionary<string, string> GetAllClothesName()
        {
            var d = new Dictionary<string, string>();
            foreach (var pair in clothesDataDic) d[pair.Key] = pair.Value;
            return d;
        }

        public override void ParseInternal(MasterGameData masterData)
        {
            var json = masterData.GetValue<string>("PlayerClothesData_clothesDataDic");
            if (json == null) return;
            clothesDataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("PlayerClothesData_clothesDataDic", JsonConvert.SerializeObject(clothesDataDic));
        }
    }
}