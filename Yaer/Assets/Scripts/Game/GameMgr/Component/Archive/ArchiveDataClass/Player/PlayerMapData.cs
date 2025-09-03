using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.Static.Enum.Map;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class PlayerMapData : BaseArchiveData
    {
        private string nowPlace = PlaceName.Home;
        private readonly List<string> unlockPlaces = new List<string>();
        private readonly List<string> unlockRoad = new List<string>();

        public string GetNowPlace()
        {
            return nowPlace;
        }

        public void SetNowPlace(string placeName)
        {
            nowPlace = placeName;
        }

        public List<string> GetUnlockPlaces()
        {
            var l = new List<string>();
            l.AddRange(unlockPlaces);
            return l;
        }

        public bool AddUnlockPlace(string placeName)
        {
            if (unlockPlaces.Contains(placeName))
            {
                Debug.LogWarning("重复解锁地点：" + placeName);
                return false;
            }

            unlockPlaces.Add(placeName);
            return true;
        }

        public bool IsUnlockPlace(string placeName)
        {
            return unlockPlaces.Contains(placeName);
        }

        public bool AddUnlockRoad(string roadName)
        {
            if (unlockRoad.Contains(roadName))
            {
                Debug.LogWarning("重复解锁道路：" + roadName);
                return false;
            }

            unlockRoad.Add(roadName);
            return true;
        }

        public List<string> GetUnlockRoad()
        {
            var l = new List<string>();
            l.AddRange(unlockRoad);
            return l;
        }

        public bool IsUnlockRoad(string roadName)
        {
            return unlockRoad.Contains(roadName);
        }

        public override void ParseInternal(MasterGameData masterData)
        {
            
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            
        }
    }
}