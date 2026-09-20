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
        /// 民居椅子 Quest_002。与老农分开常量，避免改一条菜单时误清另一条。
        /// </summary>
        private const string VineFruitQuestId = "Quest_002";

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

        /// <summary>
        /// 清掉妈妈藤蔓果任务，同一局再点椅子可重播接任务长对白。
        /// 只删内存里 Quest_002 的状态和进度，再走现有落盘。不改背包，不改 Quest_003。
        /// 为什么不改正式读档：接之前另存的档，现网读出来已经是未接。
        /// 没有那份档时，才用本菜单在这一局清掉。退出再读刚才这一档，已接还会在——
        /// 序列化不会删掉磁盘上的旧键，老农菜单也一样，本票不改。
        /// 替代方案：改 PlayerQuestData 序列化时 RemoveField。会连带改变老农 Reset 的落盘，本票否决。
        /// </summary>
        [MenuItem("Editor/Quest/ResetQuest_002 妈妈的藤蔓果")]
        private static void ResetVineFruitQuest()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Quest] ResetQuest 仅 Play 模式可用，请先进入游戏再点菜单。");
                return;
            }

            QuestManager.getInstance().ResetQuest(VineFruitQuestId);
        }
    }
}
