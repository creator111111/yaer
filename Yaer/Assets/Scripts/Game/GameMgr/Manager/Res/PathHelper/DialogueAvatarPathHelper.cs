using Game.Static.Enum.Dialogue;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.PathHelper
{
    public class DialogueAvatarPathHelper
    {
        private const string path = "Assets/GameRes/Atlas/Avatar";

        public static string GetPath(string roleName, string clothes = "", string headWear = "")
        {
            // 无头像
            if (roleName == "None")
            {
                return null;
            }

            if (roleName == DialogueRoleName.Yaer.ToString())
            {
                return $"{path}/Avatar_{roleName}_{clothes}_{headWear}.spriteatlas";
            }

            return $"{path}/Avatar_{roleName}.spriteatlas";
        }
    }
}