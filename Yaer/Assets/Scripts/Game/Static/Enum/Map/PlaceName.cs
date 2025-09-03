using Game.GameMgr;
using Game.Static.Name.Res;
using Game.Static.Path;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.Static.Enum.Map
{
    public static class PlaceName
    {
        public const string Home = "Home";
        public const string JingLingVillage = "JingLingVillage";
        public const string WangZheMountain = "WangZheMountain";
        public const string AoGuShiCity = "AoGuShiCity";
        public const string Stream = "Stream";
        public const string Hole = "Hole";
        public const string SeaDesert = "SeaDesert";
        public const string HuiJinForest = "HuiJinForest";
        public const string WaLunVillage = "WaLunVillage";
        public const string DiTeVillage = "DiTeVillage";
        public const string MoZuPlace = "MoZuPlace";
        public const string MoFaForest = "MoFaForest";
        public const string ManHuangPlace = "ManHuangPlace";
        public const string HomeToJingLingVillage = "HomeToJingLingVillage";
        public const string HomeToJingLingVillage2 = "HomeToJingLingVillage2";

        private static Dictionary<string, string> PlaceChsName = new Dictionary<string, string>()
        {
            { Home, "新诺塞尔城" },
            { PlaceName.HomeToJingLingVillage, "龙城郊" },
            { PlaceName.HomeToJingLingVillage2, "龙城东郊" },
            { SceneName.VerdantCorridor, "苍翠走廊"},
            { SceneName.WestRappRoad, "拉普路西" }
        };
        private static Dictionary<string, string> PlaceChsName_en = new Dictionary<string, string>()
        {
            { Home, "New Nosel City" },
            { PlaceName.HomeToJingLingVillage, "Outskirts of Dragon City" },
            { PlaceName.HomeToJingLingVillage2, "Eastern Outskirts of Dragon City" },
            { SceneName.VerdantCorridor, "Verdant Corridor"},
            { SceneName.WestRappRoad, "Laplucie" }
        };
        private static Dictionary<string, string> PlaceChsName_jp = new Dictionary<string, string>()
        {
            { Home, "新ノーセル城" },
            { PlaceName.HomeToJingLingVillage, "ドラゴンシテ郊外" },
            { PlaceName.HomeToJingLingVillage2, "ドラゴンシテイ東の外れ" },
            { SceneName.VerdantCorridor, "翠緑の回廊"},
            { SceneName.WestRappRoad, "ラプルシ" }
        };

        public static string GetPlaceChsName(string placeName)
        {
            var curLaunageType = GameManager.Instance.language;
            Dictionary<string, string> placeNameData = new Dictionary<string, string>();
            if (curLaunageType == LanguageEnumType.Chinese)
            {
                placeNameData = PlaceChsName;
            }
            else if (curLaunageType == LanguageEnumType.English)
            {
                placeNameData = PlaceChsName_en;
            }
            else if (curLaunageType == LanguageEnumType.Japanese)
            {
                placeNameData = PlaceChsName_jp;
            }
            else
            {
                // 不存在的语言一律使用英文
                placeNameData = PlaceChsName;
            }
            if (placeNameData.TryGetValue(placeName, out var chs))
            {
                return chs;
            }
            Debug.LogWarning($"地名未注册: {placeName}");
            return placeName;
        }
    }
}