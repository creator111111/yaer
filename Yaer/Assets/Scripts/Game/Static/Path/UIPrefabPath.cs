using UnityEngine;

namespace Game.Static.Path
{
    public class UIPrefabPath : ScriptableObject
    {
        public const string InitPanel = "Assets/GameRes/Prefabs/UI/InitPanel.prefab";
        public const string StartPanel = "Assets/GameRes/Prefabs/UI/StartPanel.prefab";
        public const string LoadGamePanel = "Assets/GameRes/Prefabs/UI/LoadGamePanel.prefab";
        public const string SaveGamePanel = "Assets/GameRes/Prefabs/UI/SaveGamePanel.prefab";
        public const string SettingPanel = "Assets/GameRes/Prefabs/UI/SettingPanel.prefab";
        public const string AchievementPanel = "Assets/GameRes/Prefabs/UI/AchievementPanel.prefab";
        public const string SelectHardPanel = "Assets/GameRes/Prefabs/UI/SelectHardPanel.prefab";
        public const string LoadingPanel = "Assets/GameRes/Prefabs/UI/LoadingPanel.prefab";
        /// <summary>左下角常驻版本号 Form（0922）；与 <see cref="GetUIPrefabPath"/>("VersionPanel") 同源。</summary>
        public const string VersionPanel = "Assets/GameRes/Prefabs/UI/VersionPanel.prefab";

        public static string GetUIPrefabPath(string prefabName)
        {
            return "Assets/GameRes/Prefabs/UI/" + prefabName + ".prefab";
        }
    }
}