#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.DebugTools
{
    /// <summary>
    /// 验收快捷：Play 中一键累加 9999 金币（内部走 <see cref="PlayerGoldDebugUtil"/>）。
    /// 菜单：Tools / Debug / Add 9999 Player Gold
    /// 自定义金额请用：Tools / Debug / Player Gold Tool…
    /// </summary>
    /// <remarks>
    /// 原因：保留一键路径；金额与窗体共用 TryAdd，避免双份逻辑漂移。
    /// </remarks>
    public static class AddPlayerGoldDebugMenu
    {
        private const string MenuPath = "Tools/Debug/Add 9999 Player Gold";
        private const int AddAmount = 9999;

        [MenuItem(MenuPath)]
        private static void Add9999Gold()
        {
            PlayerGoldDebugUtil.TryAddPlayerGold(AddAmount, showDialogOnFail: true);
        }

        /// <summary>仅 Play 时启用，避免 Edit 误点改档。</summary>
        [MenuItem(MenuPath, true)]
        private static bool ValidateAdd9999Gold()
        {
            return Application.isPlaying;
        }
    }
}
#endif
