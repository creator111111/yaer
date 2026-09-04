using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using UnityEditor;
using UnityEngine;

namespace GameDebug.Editor
{
    /// <summary>
    /// 任务 Debug 菜单：验收 <see cref="QuestManager.ResetQuest"/>（无日期系统时的跳日替代）。
    /// 仅 Play 模式有效；不改背包 / 不发奖 / 不自动播对白。
    /// </summary>
    public static class QuestResetDebugMenu
    {
        private const string FarmerQuestId = "Quest_003";

        /// <summary>
        /// 清掉老农打水任务状态与进度，交完后可再 Offer→帮整条重来。
        /// </summary>
        [MenuItem("Editor/Quest/ResetQuest_003 老农打水")]
        private static void ResetFarmerQuest()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Quest] ResetQuest 仅 Play 模式可用，请先进入游戏再点菜单。");
                return;
            }

            QuestManager.getInstance().ResetQuest(FarmerQuestId);
        }
    }
}
