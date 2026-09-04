#if UNITY_EDITOR
using Game.GameRuntime.UI.Component;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Menu.Editor
{
    /// <summary>
    /// 0829：MenuPanel ButtonMoney 挂商店同款 DigitStrip + UiSpriteNumberDisplay，绑 0～9。
    /// 菜单：Tools / UI / Setup MenuPanel Money Digits
    /// Batchmode：MenuMoneyDigitsSetupEditor.ExecuteBatchSetup()
    /// </summary>
    /// <remarks>
    /// 原因：Money/Money(1) 为静态占位图，不能显示可变金币；须复用 UiSpriteNumberDisplay，禁止日历双位/TMP。
    /// 替代方案：仅运行时 EnsureOn —— Editor Play 可补 Sprite，正式包须 Prefab 序列化 digitSprites。
    /// </remarks>
    public static class MenuMoneyDigitsSetupEditor
    {
        private const string MenuPath = "Tools/UI/Setup MenuPanel Money Digits";
        private const string MenuPanelPrefabPath = "Assets/GameRes/Prefabs/UI/MenuPanel.prefab";
        private const string ButtonMoneyName = "ButtonMoney";
        private const string MoneyDigitsHostName = "Money_Digits";
        private const string MoneyStaticZeroName = "Money";
        private const string MoneyCoinName = "Money (1)";

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            if (!SetupPrefab())
            {
                EditorUtility.DisplayDialog("Menu Money Digits", "装配失败，见 Console。", "OK");
                return;
            }

            EditorUtility.DisplayDialog("Menu Money Digits", "已写入 MenuPanel：Money_Digits + DigitStrip。", "OK");
        }

        /// <summary>供 Unity -batchmode -executeMethod 调用。</summary>
        public static void ExecuteBatchSetup()
        {
            if (!SetupPrefab())
            {
                Debug.LogError("[MenuMoneySetup] batch 失败");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[MenuMoneySetup] batch 成功");
        }

        private static bool SetupPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(MenuPanelPrefabPath);
            if (root == null)
            {
                Debug.LogError("[MenuMoneySetup] 无法加载 " + MenuPanelPrefabPath);
                return false;
            }

            try
            {
                var buttonMoney = FindDeep(root.transform, ButtonMoneyName);
                if (buttonMoney == null)
                {
                    Debug.LogError("[MenuMoneySetup] Prefab 内无 ButtonMoney");
                    return false;
                }

                // 隐藏静态 0.png，禁止与 DigitStrip 叠双「0」
                var moneyZero = buttonMoney.Find(MoneyStaticZeroName);
                if (moneyZero != null)
                {
                    moneyZero.gameObject.SetActive(false);
                }

                // 币标（Z.png）靠左；数字条占右侧
                var coin = buttonMoney.Find(MoneyCoinName);
                if (coin != null)
                {
                    var coinRt = coin as RectTransform;
                    if (coinRt != null)
                    {
                        coinRt.anchoredPosition = new Vector2(-78f, 0f);
                        coinRt.sizeDelta = new Vector2(28f, 28f);
                    }
                }

                var host = buttonMoney.Find(MoneyDigitsHostName) as RectTransform;
                if (host == null)
                {
                    var hostGo = new GameObject(MoneyDigitsHostName, typeof(RectTransform));
                    hostGo.layer = buttonMoney.gameObject.layer;
                    host = hostGo.GetComponent<RectTransform>();
                    host.SetParent(buttonMoney, false);
                }

                host.anchorMin = Vector2.zero;
                host.anchorMax = Vector2.one;
                host.pivot = new Vector2(0.5f, 0.5f);
                host.offsetMin = new Vector2(36f, 0f);
                host.offsetMax = Vector2.zero;
                host.localScale = Vector3.one;
                host.localRotation = Quaternion.identity;
                host.SetAsLastSibling();

                var display = UiSpriteNumberDisplay.EnsureOn(
                    host,
                    TextAnchor.MiddleRight,
                    stripSpacing: UiSpriteNumberDisplay.ShopTotalSpacing,
                    capacity: UiSpriteNumberDisplay.ShopTotalPoolCapacity);

                display.AssignSprites(UiSpriteNumberDisplay.LoadDefaultDigitSpritesEditor());
                display.ApplyShopTotalLayoutForBake();
                display.EditorBakeSetNumber(0);

                // 尽量把引用写到 MenuFormLogic，减少运行时 Find
                var menuLogic = root.GetComponent<MenuFormLogic>();
                if (menuLogic != null)
                {
                    var so = new SerializedObject(menuLogic);
                    var prop = so.FindProperty("moneyDigits");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = display;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, MenuPanelPrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[MenuMoneySetup] 已写入 " + MenuPanelPrefabPath, display);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindDeep(parent.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
