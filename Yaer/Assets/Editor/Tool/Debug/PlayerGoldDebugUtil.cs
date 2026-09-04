#if UNITY_EDITOR
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.UI.FormLogic.Menu;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.DebugTools
{
    /// <summary>
    /// 刷金共享逻辑：Play 门禁 + AddGold / TrySpend + 刷新已开 Menu Money。
    /// 供 <see cref="AddPlayerGoldDebugMenu"/> 一键快捷与 <see cref="PlayerGoldDebugWindow"/> 共用。
    /// </summary>
    /// <remarks>
    /// 原因：禁止窗/菜单各写一套加减金，避免漏 Save、双 Save 或裸写 gold。
    /// Add 吃 <see cref="PlayerGoldData.MaxGold"/> 硬顶（可测触顶，禁止再造超额档）。
    /// 减少走商店同门面 <see cref="QuestManager.TrySpendPlayerGold"/>（成功已 Save，Util 勿再 Save）。
    /// </remarks>
    public static class PlayerGoldDebugUtil
    {
        /// <summary>
        /// 累加指定正整数金币并落盘；结果 ≤ <see cref="PlayerGoldData.MaxGold"/>。
        /// </summary>
        /// <param name="amount">须 &gt; 0；≤0 拒绝且不改档。可大于剩余额度，多余由 AddGold 丢弃。</param>
        /// <param name="showDialogOnFail">未 Play / 失败 / 触顶时是否弹 Dialog。</param>
        /// <returns>是否成功调用加金并 Save（触顶仍算成功写入上限）。</returns>
        public static bool TryAddPlayerGold(int amount, bool showDialogOnFail = true)
        {
            if (!TryResolveSpendOrAddContext(amount, showDialogOnFail, out var questMgr, out var goldData))
            {
                return false;
            }

            var before = goldData.gold;
            goldData.AddGold(amount);
            questMgr.SavePlayerGold();
            var after = goldData.gold;

            // 触顶：请求加额无法全部入账（含已在 Max 再加 → 实加 0）。
            var actualGain = after - before;
            if (actualGain < amount)
            {
                var msg =
                    $"已达金币上限 {PlayerGoldData.MaxGold}。请求 +{amount}，实加 {actualGain}（{before} → {after}）。";
                Debug.LogWarning($"[DebugGold] 触顶：{msg}");
                if (showDialogOnFail)
                {
                    EditorUtility.DisplayDialog("Player Gold", msg, "OK");
                }
            }

            Debug.Log($"[DebugGold] AddGold({amount})：{before} → {after}（已 SavePlayerGold）");

            RefreshOpenMenuMoney();
            return true;
        }

        /// <summary>
        /// 调试减少金币：走 <see cref="QuestManager.TrySpendPlayerGold"/>（与商店同门面）。
        /// 不足整笔失败、不钳到 0；成功时门面已 Save，本方法不再 Save。
        /// </summary>
        public static bool TrySpendPlayerGoldForDebug(int amount, bool showDialogOnFail = true)
        {
            if (!TryResolveSpendOrAddContext(amount, showDialogOnFail, out var questMgr, out var goldData))
            {
                return false;
            }

            var before = goldData.gold;
            if (!questMgr.TrySpendPlayerGold(amount))
            {
                var msg = $"余额不足：需要 {amount}，当前 {before}。整笔失败，未改档。";
                if (showDialogOnFail)
                {
                    EditorUtility.DisplayDialog("Player Gold", msg, "OK");
                }

                Debug.LogWarning($"[DebugGold] Spend 失败：need={amount}, have={before}（未改档）");
                return false;
            }

            var after = goldData.gold;
            Debug.Log($"[DebugGold] Spend({amount})：{before} → {after}（TrySpendPlayerGold 已 Save）");

            RefreshOpenMenuMoney();
            return true;
        }

        /// <summary>
        /// F2：若内存 gold 超标则钳回 MaxGold 并立刻 Save，避免脏 JSON 残留。
        /// </summary>
        /// <returns>是否执行了钳回并落盘。</returns>
        public static bool TryClampOverCapAndSave(bool showDialog = true)
        {
            if (!Application.isPlaying || GameManager.Instance == null)
            {
                return false;
            }

            var questMgr = QuestManager.getInstance();
            var goldData = questMgr?.GetPlayerGoldData();
            if (goldData == null)
            {
                return false;
            }

            if (goldData.gold <= PlayerGoldData.MaxGold && goldData.gold >= 0)
            {
                return false;
            }

            var before = goldData.gold;
            if (!goldData.ClampGoldToLegalRange())
            {
                return false;
            }

            questMgr.SavePlayerGold();
            var msg = $"检测到超标余额 {before}，已钳回 {goldData.gold} 并 Save。";
            Debug.LogWarning($"[DebugGold] F2 钳回：{msg}");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Player Gold", msg, "OK");
            }

            RefreshOpenMenuMoney();
            return true;
        }

        /// <summary>
        /// Add / Spend 共用门禁：Play、amount&gt;0、GM / Quest / GoldData 可用。
        /// </summary>
        private static bool TryResolveSpendOrAddContext(
            int amount,
            bool showDialogOnFail,
            out QuestManager questMgr,
            out PlayerGoldData goldData)
        {
            questMgr = null;
            goldData = null;

            if (!Application.isPlaying)
            {
                if (showDialogOnFail)
                {
                    EditorUtility.DisplayDialog(
                        "Player Gold",
                        "请先 Play 进入游戏（建议从 InitScene 正规进，有存档）后再执行。",
                        "OK");
                }

                Debug.LogWarning("[DebugGold] 未 Play，拒绝改金（不改档）。");
                return false;
            }

            if (amount <= 0)
            {
                if (showDialogOnFail)
                {
                    EditorUtility.DisplayDialog(
                        "Player Gold",
                        "金额须为正整数（>0）。0 / 负数不会改档。",
                        "OK");
                }

                Debug.LogWarning($"[DebugGold] amount={amount} ≤ 0，拒绝改金。");
                return false;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("[DebugGold] GameManager 不可用，请从 InitScene 正规进游戏。");
                return false;
            }

            questMgr = QuestManager.getInstance();
            if (questMgr == null)
            {
                Debug.LogError("[DebugGold] QuestManager 不可用。");
                return false;
            }

            goldData = questMgr.GetPlayerGoldData();
            if (goldData == null)
            {
                Debug.LogError(
                    "[DebugGold] PlayerGoldData 为空，请确认已加载存档（InitScene → 进游戏）。");
                return false;
            }

            return true;
        }

        /// <summary>Play 时读取当前存档金币；失败返回 null。</summary>
        public static int? TryGetCurrentGold()
        {
            if (!Application.isPlaying || GameManager.Instance == null)
            {
                return null;
            }

            var data = QuestManager.getInstance()?.GetPlayerGoldData();
            return data != null ? data.gold : (int?)null;
        }

        /// <summary>菜单已开则即时刷 Money 图片数字。</summary>
        public static void RefreshOpenMenuMoney()
        {
            var menu = Object.FindObjectOfType<MenuFormLogic>();
            if (menu != null)
            {
                menu.RefreshMoneyFromArchive();
                Debug.Log("[DebugGold] 已刷新打开中的 Menu Money。");
            }
            else
            {
                Debug.Log("[DebugGold] 当前无打开 MenuPanel，请 ESC 开菜单查看余额。");
            }
        }
    }
}
#endif
