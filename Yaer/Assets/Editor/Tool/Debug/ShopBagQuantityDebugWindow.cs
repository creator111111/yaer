#if UNITY_EDITOR
using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.Static.Enum.Goods;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.DebugTools
{
    /// <summary>
    /// 商店货单背包数量调试窗：Buy∪Sell 列表、「设为 N」、清空、全满。
    /// 菜单：Tools / Debug / Shop Bag Quantity Tool…
    /// </summary>
    /// <remarks>
    /// 原因：测空包可买 / 堆满拒买 / 差 1 件须精确拧数量，不能一把塞全图鉴。
    /// 样板：Player Gold Tool（Play 门禁 · Save · 即时刷新）。
    /// 反例：一键全部主道具（过宽）。
    /// </remarks>
    public sealed class ShopBagQuantityDebugWindow : EditorWindow
    {
        private const string OpenMenuPath = "Tools/Debug/Shop Bag Quantity Tool...";

        private Vector2 _scroll;
        private List<ShopBagQuantityDebugUtil.ShopBagRow> _rows;
        private readonly Dictionary<EMainItemName, int> _targetByItem = new Dictionary<EMainItemName, int>();

        [MenuItem(OpenMenuPath)]
        private static void OpenWindow()
        {
            var window = GetWindow<ShopBagQuantityDebugWindow>(utility: false, title: "Shop Bag Qty");
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshRowCache();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("商店背包数量（仅 Play）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "列表 = GetShopBuyCandidates ∪ GetShopSellCandidates（动态，勿写死）。\n" +
                "「应用」= 设为 N（正好持有，非再 +N）；清空仅店货；不管金币。\n" +
                "堆叠上限 MaxStackPerItem=" + PlayerBagData.MaxStackPerItem + "。",
                MessageType.Info);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请先 Play（建议 InitScene 正规进游戏）。", MessageType.Warning);
            }

            DrawToolbar();
            DrawScrollList();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            if (GUILayout.Button("清空商店货", GUILayout.Height(24f)))
            {
                ShopBagQuantityDebugUtil.TryClearAllShopCandidates(showDialogOnFail: true);
                SyncTargetsFromBag();
                Repaint();
            }

            if (GUILayout.Button("商店货全满(Max)", GUILayout.Height(24f)))
            {
                ShopBagQuantityDebugUtil.TryFillAllShopCandidatesToMax(showDialogOnFail: true);
                SyncTargetsFromBag();
                Repaint();
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("刷新列表", GUILayout.Height(24f)))
            {
                RefreshRowCache();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScrollList()
        {
            if (_rows == null || _rows.Count == 0)
            {
                EditorGUILayout.HelpBox("无商店候选（检查 MainItemDatabase / EnsureLoaded）。", MessageType.Warning);
                return;
            }

            var bag = Application.isPlaying
                ? ShopBagQuantityDebugUtil.TryResolveBag(showDialogOnFail: false)
                : null;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                var held = bag != null ? bag.GetMainItemCount(row.ItemId) : 0;
                if (!_targetByItem.ContainsKey(row.ItemId))
                {
                    _targetByItem[row.ItemId] = held;
                }

                EditorGUILayout.BeginHorizontal();
                var side = FormatSide(row);
                EditorGUILayout.LabelField($"{row.ItemId} ({side})", GUILayout.MinWidth(160f));
                EditorGUILayout.LabelField($"持有 {held}", GUILayout.Width(56f));
                _targetByItem[row.ItemId] = EditorGUILayout.IntField(_targetByItem[row.ItemId], GUILayout.Width(48f));

                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                if (GUILayout.Button("应用", GUILayout.Width(48f)))
                {
                    ShopBagQuantityDebugUtil.TrySetCountAndSave(
                        row.ItemId,
                        _targetByItem[row.ItemId],
                        showDialogOnFail: true);
                    // 应用后把目标同步为实际持有（含钳顶）
                    var bagAfter = ShopBagQuantityDebugUtil.TryResolveBag(showDialogOnFail: false);
                    if (bagAfter != null)
                    {
                        _targetByItem[row.ItemId] = bagAfter.GetMainItemCount(row.ItemId);
                    }

                    Repaint();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private static string FormatSide(ShopBagQuantityDebugUtil.ShopBagRow row)
        {
            if (row.IsBuy && row.IsSell)
            {
                return "Buy+Sell";
            }

            return row.IsBuy ? "Buy" : "Sell";
        }

        private void RefreshRowCache()
        {
            _rows = ShopBagQuantityDebugUtil.BuildShopUnionRows();
            SyncTargetsFromBag();
        }

        private void SyncTargetsFromBag()
        {
            if (_rows == null)
            {
                return;
            }

            var bag = Application.isPlaying
                ? ShopBagQuantityDebugUtil.TryResolveBag(showDialogOnFail: false)
                : null;

            for (var i = 0; i < _rows.Count; i++)
            {
                var id = _rows[i].ItemId;
                _targetByItem[id] = bag != null ? bag.GetMainItemCount(id) : 0;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
