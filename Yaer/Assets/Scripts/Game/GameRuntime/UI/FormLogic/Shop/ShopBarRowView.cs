using Game.DataTable.MainItem;
using Game.GameRuntime.UI.Component;
using Game.Static.Enum.Goods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// Shop_Bar 单行视图：Editor Bake 写入 Icon / Name(名图) / Price 与 baked 序列化字段；
    /// 运行时 Awake 只读 baked 字段；Play 进店 / 切语言时按当前语 ResolveShopNameSprite 贴 Name。
    /// 挂在 Shop_Bar.prefab 根节点；ItemId / Price 供合计与阶段四交易读取。
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopBarRowView : MonoBehaviour
    {
        private const string IconNodeName = "Icon";
        private const string NameNodeName = "Name";
        private const string PriceNodeName = "Price";

        [Header("可选：Inspector 预绑，留空则 Awake 时 Find")]
        [SerializeField] private Image iconImage;
        /// <summary>商店名图节点（原 TMP Name，现为 Image）。</summary>
        [SerializeField] private Image nameImage;
        [SerializeField] private Text priceText;
        [SerializeField] private TextMeshProUGUI priceTextTmp;

        private UiSpriteNumberDisplay _priceDigitDisplay;

        [Header("Editor Bake 写入（运行时只读）")]
        [SerializeField] private EMainItemName bakedItemId;
        [SerializeField] private int bakedPrice;
        [SerializeField] private bool bakedIsBuyRow;

        /// <summary>本行道具 ID，供 GetBuyQuantity / 交易逻辑匹配。</summary>
        public EMainItemName ItemId { get; private set; }

        /// <summary>本行商店单价（买价或卖价，由所在列表决定语义）。</summary>
        public int Price { get; private set; }

        /// <summary>Awake / Bind 已写出行身份后才能用 ItemId 解析名图。</summary>
        private bool _hasRuntimeIdentity;

        private void Awake()
        {
            CacheUiReferences();
            // EB-2：从场景烘焙的序列化字段恢复，不依赖 MainItemDefProvider / 图集异步加载。
            ItemId = bakedItemId;
            Price = bakedPrice;
            _hasRuntimeIdentity = true;
            ApplyPrice(Price);
            // SN-7：Play 时按当前语言重贴名图（Bake 仅为中文预览）。
            RefreshShopNameForLanguage();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor Bake 工具写入行身份与单价；替代方案为仅改 Text/Sprite 而不序列化 itemId，运行时无法匹配数量输入。
        /// </summary>
        public void EditorSetBakedData(EMainItemName itemId, int price, bool isBuyRow)
        {
            bakedItemId = itemId;
            bakedPrice = price;
            bakedIsBuyRow = isBuyRow;
            EditorUtility.SetDirty(this);
        }
#endif

        /// <summary>
        /// 按 MainItemDatabase 条目写 UI；价格来自 def.BuyPrice / def.SellPrice。
        /// EB 商店以 Bake 为准；Bind 仅供动态商店等非 Bake 路径。
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
            _hasRuntimeIdentity = true;

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

            // 禁止再写 displayName 到 Name；整图路径 + 当前语言。
            ApplyShopName(MainItemDefProvider.ResolveShopNameSprite(def.ItemId));
            ApplyPrice(Price);
        }

        /// <summary>
        /// 按当前游戏语言重刷本行商店名图；切语言 / 进店时幂等调用。
        /// 原因：Bake 死贴中文预览，Play Resolve 才是真语种。
        /// 父级 ShopFormLogic.Awake 可能早于本行 Awake：未就绪时回退 bakedItemId。
        /// </summary>
        public void RefreshShopNameForLanguage()
        {
            CacheUiReferences();
            var itemId = _hasRuntimeIdentity ? ItemId : bakedItemId;
            ApplyShopName(MainItemDefProvider.ResolveShopNameSprite(itemId));
        }

        private void CacheUiReferences()
        {
            if (iconImage == null)
            {
                iconImage = transform.Find(IconNodeName)?.GetComponent<Image>();
            }

            if (nameImage == null)
            {
                nameImage = transform.Find(NameNodeName)?.GetComponent<Image>();
            }

            if (priceText == null && priceTextTmp == null)
            {
                var priceNode = transform.Find(PriceNodeName);
                if (priceNode != null)
                {
                    priceText = priceNode.GetComponent<Text>();
                    priceTextTmp = priceNode.GetComponent<TextMeshProUGUI>();
                    _priceDigitDisplay = UiSpriteNumberDisplay.FindUnder(priceNode);
                }
            }
            else if (_priceDigitDisplay == null)
            {
                _priceDigitDisplay = UiSpriteNumberDisplay.FindUnder(transform.Find(PriceNodeName));
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

        /// <summary>
        /// 贴商店名图；sprite=null 时 Image 禁用且不回退写 displayName。
        /// </summary>
        public void ApplyShopName(Sprite sprite)
        {
            if (nameImage == null)
            {
                Debug.LogWarning(
                    $"[ShopNameSprite] Name 节点缺少 Image（row={name}）；请将 Shop_Bar.Name 换成 Image 后 Bake。",
                    this);
                return;
            }

            nameImage.sprite = sprite;
            nameImage.enabled = sprite != null;
            if (sprite == null)
            {
                Debug.LogWarning($"[ShopNameSprite] Name 空图：{ItemId}", this);
            }
        }

        /// <summary>优先 UiSpriteNumberDisplay 图片价；无 Display 时回退 Legacy Text（兼容旧场景）。</summary>
        private void ApplyPrice(int price)
        {
            if (_priceDigitDisplay == null)
            {
                var priceNode = transform.Find(PriceNodeName);
                if (priceNode != null)
                {
                    _priceDigitDisplay = UiSpriteNumberDisplay.FindUnder(priceNode);
                }
            }

            if (_priceDigitDisplay != null)
            {
                _priceDigitDisplay.SetNumber(price);
                return;
            }

            SetLabelText(priceText, priceTextTmp, price.ToString());
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
