#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop.Editor
{
    /// <summary>
    /// 编辑器一键把 Row 下 TxtStock 设为 TMP 整数输入框（阶段二 Prefab 检查清单）。
    /// 菜单：Tools / Shop / Setup Row Quantity Inputs
    /// </summary>
    public static class ShopQuantityInputSetupEditor
    {
        private const string MenuPath = "Tools/Shop/Setup Row Quantity Inputs";

        [MenuItem(MenuPath)]
        private static void SetupSelectedRows()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                EditorUtility.DisplayDialog("商店数量输入", "请在 Hierarchy 中选中 Row_HpBall / Row_MpBall（或其父节点）后再执行。", "确定");
                return;
            }

            var setupCount = 0;
            foreach (var root in selected)
            {
                setupCount += SetupUnderRoot(root.transform);
            }

            EditorUtility.DisplayDialog(
                "商店数量输入",
                setupCount > 0
                    ? $"已处理 {setupCount} 个 TxtStock 节点（TMP Integer，默认 1）。"
                    : "未找到名为 TxtStock 的节点；请确认选中购买列表行。",
                "确定");
        }

        [MenuItem(MenuPath, true)]
        private static bool SetupSelectedRowsValidate()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        private static int SetupUnderRoot(Transform root)
        {
            var count = 0;

            if (root.name == "TxtStock")
            {
                ShopQuantityInputHelper.EnsureTmpIntegerInputField(root);
                EnsureRowQuantityComponent(root.parent);
                EditorUtility.SetDirty(root.gameObject);
                return 1;
            }

            foreach (Transform child in root)
            {
                count += SetupUnderRoot(child);
            }

            if (root.name == "Row_HpBall" || root.name == "Row_MpBall")
            {
                var txtStock = root.Find("TxtStock");
                if (txtStock != null)
                {
                    ShopQuantityInputHelper.EnsureTmpIntegerInputField(txtStock);
                    EnsureRowQuantityComponent(root);
                    EditorUtility.SetDirty(root.gameObject);
                    count++;
                }
            }

            return count;
        }

        private static void EnsureRowQuantityComponent(Transform row)
        {
            if (row == null)
            {
                return;
            }

            if (row.GetComponent<ShopBuyRowQuantityInput>() == null)
            {
                row.gameObject.AddComponent<ShopBuyRowQuantityInput>();
            }
        }
    }
}
#endif
