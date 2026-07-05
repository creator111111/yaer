using Game.DataTable.MainItem;
using Game.Static.Enum.Goods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// Shop_Bar 单行视图：按 MainItemDef 刷新 Icon / Name / Price。
    /// 挂在 Shop_Bar.prefab 根节点；ItemId / Price 供合计与阶段四交易读取。
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopBarRowView : MonoBehaviour
    {
        private const string IconNodeName = "Icon";
        private const string NameNodeName = "Name";
        private const string PriceNodeName = "Price";
        private const string NumberNodeName = "Number";
        private const string TxtStockNodeName = "TxtStock";

        [Header("可选：Inspector 预绑，留空则 Awake 时 Find")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private TextMeshProUGUI nameTextTmp;
        [SerializeField] private Text priceText;
        [SerializeField] private TextMeshProUGUI priceTextTmp;

        /// <summary>本行道具 ID，供 GetBuyQuantity / 交易逻辑匹配。</summary>
        public EMainItemName ItemId { get; private set; }

        /// <summary>本行商店单价（买价或卖价，由所在列表决定语义）。</summary>
        public int Price { get; private set; }

        private void Awake()
        {
            CacheUiReferences();
        }

        /// <summary>
        /// 按 MainItemDatabase 条目写 UI；价格来自 def.BuyPrice / def.SellPrice。
        /// 购买行 Number 列由 ShopBuyRowQuantityInput 接管；出售行显示占位 "1"。
        /// </summary>
        public void Bind(MainItemDef def, bool isBuyRow)
        {
            if (def == null)
            {
                Debug.LogWarning("[ShopBarRowView] Bind 收到空 def。", this);
                return;
            }

            CacheUiReferences();

            ItemId = def.ItemId;
            Price = isBuyRow ? def.BuyPrice : def.SellPrice;

            // Fix-I2：Bind 时实时 ResolveIcon，避免 def.Icon 在图集异步加载前被缓存为 null。
            var icon = MainItemDefProvider.ResolveIcon(def.ItemId);
            ApplyIcon(icon);

            // Fix-I4：Icon 仍 null 时输出诊断，便于策划补 Database 或图集资源。
            if (icon == null)
            {
                Debug.LogWarning(
                    $"[ShopBarRowView] Icon 未解析：{def.ItemId}；请在 MainItemDatabase 拖 icon 或补 enum 名 PNG/图集。",
                    this);
            }

            ApplyName(def.DisplayName);
            ApplyPrice(Price);

            if (!isBuyRow)
            {
                ApplySellQuantityPlaceholder();
            }
        }

        private void CacheUiReferences()
        {
            if (iconImage == null)
            {
                iconImage = transform.Find(IconNodeName)?.GetComponent<Image>();
            }

            if (nameText == null && nameTextTmp == null)
            {
                var nameNode = transform.Find(NameNodeName);
                if (nameNode != null)
                {
                    nameText = nameNode.GetComponent<Text>();
                    nameTextTmp = nameNode.GetComponent<TextMeshProUGUI>();
                }
            }

            if (priceText == null && priceTextTmp == null)
            {
                var priceNode = transform.Find(PriceNodeName);
                if (priceNode != null)
                {
                    priceText = priceNode.GetComponent<Text>();
                    priceTextTmp = priceNode.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private void ApplyIcon(Sprite sprite)
        {
            if (iconImage == null)
            {
                return;
            }

            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        private void ApplyName(string displayName)
        {
            SetLabelText(nameText, nameTextTmp, displayName ?? string.Empty);
        }

        private void ApplyPrice(int price)
        {
            SetLabelText(priceText, priceTextTmp, price.ToString());
        }

        /// <summary>出售页本阶段：Number 列显示持有数占位 "1"（阶段六接背包）。</summary>
        private void ApplySellQuantityPlaceholder()
        {
            var numberNode = transform.Find(NumberNodeName) ?? transform.Find(TxtStockNodeName);
            if (numberNode == null)
            {
                return;
            }

            var legacyText = numberNode.GetComponent<Text>();
            if (legacyText != null)
            {
                legacyText.text = "1";
            }

            var tmpText = numberNode.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "1";
            }
        }

        private static void SetLabelText(Text legacy, TextMeshProUGUI tmp, string value)
        {
            if (tmp != null)
            {
                tmp.text = value;
            }

            if (legacy != null)
            {
                legacy.text = value;
            }
        }
    }
}
