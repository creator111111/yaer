using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive;
using Game.GameMgr;
using UnityEngine;
using Game.GameRuntime.BagPack;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    public class MenuFormMainItemInfo
    {
        public int id;
        public int index;
        public string name;
        [ES3NonSerializable]
        public Sprite icon;
        [ES3NonSerializable]
        public string detail { get; set; }
        public string detail_en;
        public string detail_jp;
        public int num;
        public BagItemType itemType;
    }
}