using UnityEngine;

namespace Game.GameMgr.Manager.Path
{
    public abstract class PathManager : IPathManager
    {
        public static string ARCHIVE_PATH = Application.persistentDataPath + "/Save/"; // 存档路径
        public static string ATLAS_PATH = "Atlas/"; // 图集资源
        public static string ANIMATOR_PATH = "Animation/";
        public static string UI_CONTROL_PREFABS_PATH = "UI/Control/";
    }
}