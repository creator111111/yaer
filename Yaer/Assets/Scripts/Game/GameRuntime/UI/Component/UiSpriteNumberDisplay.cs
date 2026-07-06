using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.GameRuntime.UI.Component
{
    /// <summary>
    /// 通用 0～9 图片数字条：HorizontalLayoutGroup 横排 Sprite，供 Shop Price / Number / Total 等复用。
    /// 替代方案：固定十位+个位两个 Image（见 MenuCalendarDayNumDisplay）——不适合 1～5 位可变长度。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UiSpriteNumberDisplay : MonoBehaviour
    {
        public const string DigitStripNodeName = "DigitStrip";
        public const string DigitSpriteFolderPath = "Assets/ArtRes/UI/Text/";

        /// <summary>v3：Price 行字间距（定稿 0px）。</summary>
        public const float ShopPriceSpacing = 0f;

        /// <summary>v3：Number 行字间距（定稿 -1px；HLG 允许负值叠紧透明留白）。</summary>
        public const float ShopNumberSpacing = -1f;

        /// <summary>v3+：Total2 合计字间距 Bake 推荐初值（策划可在 Inspector 手调，运行时不会覆盖）。</summary>
        public const float ShopTotalSpacing = -12f;

        /// <summary>v3+：Total2 数字池上限（最多 6 位整数，如 103950）。</summary>
        public const int ShopTotalPoolCapacity = 6;

        /// <summary>Total2 底框（约 144×45）左右留白，fit 时不贴边。</summary>
        public const float ShopTotalFitPadding = 4f;

        [Header("0-9 数字图片，索引即数字本身")]
        [SerializeField] private Sprite[] digitSprites = new Sprite[10];

        [Header("布局（策划可在 Inspector 调 spacing）")]
        [SerializeField] private TextAnchor digitAlignment = TextAnchor.MiddleCenter;
        [SerializeField] private float spacing = ShopPriceSpacing;
        [SerializeField] private bool useNativeSize = true;
        [SerializeField] private int poolCapacity = 5;

        [Header("窄容器适配（Total2）")]
        [Tooltip("为 true 时按父节点 Rect 宽度/高度等比缩放，保证不超出 Total2 底框。")]
        [SerializeField] private bool fitWithinParentWidth;
        [SerializeField] private float fitWidthPadding = ShopTotalFitPadding;

        private HorizontalLayoutGroup _layout;
        private readonly List<Image> _digitImages = new List<Image>();
        private bool _initialized;

        /// <summary>整数 → 自然位数刷图（内部 value.ToString()，禁止 PadLeft / D4）。</summary>
        public void SetNumber(int value)
        {
            if (value < 0)
            {
                value = 0;
            }

            SetDigitString(value.ToString());
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor Bake 专用：重建 Digit 池后刷图，并把各 Digit_* 的 Active 状态写入场景（IMG-R1）。
        /// 替代方案：仅调 SetNumber —— 若旧场景残留重复 Digit_*，可能叠出假前导零。
        /// </summary>
        public void EditorBakeSetNumber(int value)
        {
            ResetDigitPoolForRebuild();
            SetNumber(value);
            MarkDigitChildrenDirty();
        }
#endif

        /// <summary>
        /// 按原始字符刷图（Number 输入过程用）；非数字字符忽略；空串则隐藏全部位图。
        /// </summary>
        public void SetDigitString(string rawDigits)
        {
            EnsureInitialized();

            if (string.IsNullOrEmpty(rawDigits))
            {
                HideAllDigits();
                ResetFitScale();
                return;
            }

            var digits = ExtractDigits(rawDigits);
            if (digits.Length == 0)
            {
                HideAllDigits();
                ResetFitScale();
                return;
            }

            EnsurePoolSize(digits.Length);
            for (var i = 0; i < _digitImages.Count; i++)
            {
                var image = _digitImages[i];
                if (i < digits.Length)
                {
                    ApplyDigitSprite(image, digits[i] - '0');
                    image.gameObject.SetActive(true);
                }
                else
                {
                    image.gameObject.SetActive(false);
                }
            }

            RebuildLayout();
            TryFitWithinParentWidth();
        }

        /// <summary>
        /// 运行时：六位池 + fit 适配；**采纳 Inspector 上 HorizontalLayoutGroup.spacing**，不覆盖策划手调值。
        /// </summary>
        public void ApplyShopTotalLayout()
        {
            poolCapacity = ShopTotalPoolCapacity;
            fitWithinParentWidth = true;
            fitWidthPadding = ShopTotalFitPadding;
            EnsureInitialized();
            SyncSpacingFromLayoutGroup();
            if (_initialized)
            {
                EnsurePoolSize(poolCapacity);
            }
        }

        /// <summary>Bake 初次落盘：写入推荐 spacing 并推到 HorizontalLayoutGroup。</summary>
        public void ApplyShopTotalLayoutForBake()
        {
            spacing = ShopTotalSpacing;
            poolCapacity = ShopTotalPoolCapacity;
            fitWithinParentWidth = true;
            fitWidthPadding = ShopTotalFitPadding;
            EnsureInitialized();
            ApplyLayoutSettings();
            if (_initialized)
            {
                EnsurePoolSize(poolCapacity);
            }
        }

        /// <summary>Total2：以 HLG 手调 spacing 为准，回写脚本字段供宽度测量与 fit 计算。</summary>
        private void SyncSpacingFromLayoutGroup()
        {
            if (!fitWithinParentWidth || _layout == null)
            {
                ApplyLayoutSettings();
                return;
            }

            spacing = _layout.spacing;
        }

        /// <summary>v3+：运行时或 Bake 调整池上限（Total2 须为 6）。</summary>
        public void SetPoolCapacity(int capacity)
        {
            poolCapacity = Mathf.Max(1, capacity);
            if (_initialized)
            {
                EnsurePoolSize(poolCapacity);
            }
        }

        /// <summary>v2/v3：Bake / 运行时校正字间距（同步 HorizontalLayoutGroup，允许负值）。</summary>
        public void SetSpacing(float stripSpacing)
        {
            spacing = stripSpacing;
            EnsureInitialized();
            ApplyLayoutSettings();
            RebuildLayout();
        }

        /// <summary>在 parent 下查找 DigitStrip 上的 Display；无则 null。</summary>
        public static UiSpriteNumberDisplay FindUnder(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            var strip = parent.Find(DigitStripNodeName);
            if (strip != null)
            {
                return strip.GetComponent<UiSpriteNumberDisplay>();
            }

            return parent.GetComponent<UiSpriteNumberDisplay>();
        }

        /// <summary>
        /// 确保 parent 下存在 DigitStrip + UiSpriteNumberDisplay；用于 Price / Number / Total2。
        /// </summary>
        public static UiSpriteNumberDisplay EnsureOn(
            Transform parent,
            TextAnchor alignment,
            float stripSpacing = ShopPriceSpacing,
            int capacity = 5)
        {
            if (parent == null)
            {
                return null;
            }

            var strip = parent.Find(DigitStripNodeName);
            GameObject stripGo;
            if (strip == null)
            {
                stripGo = new GameObject(DigitStripNodeName, typeof(RectTransform));
                stripGo.transform.SetParent(parent, false);
                stripGo.layer = parent.gameObject.layer;
                stripGo.transform.SetAsFirstSibling();

                var rect = stripGo.GetComponent<RectTransform>();
                StretchFull(rect);
            }
            else
            {
                stripGo = strip.gameObject;
                stripGo.transform.SetAsFirstSibling();
            }

            var display = stripGo.GetComponent<UiSpriteNumberDisplay>();
            if (display == null)
            {
                display = stripGo.AddComponent<UiSpriteNumberDisplay>();
            }

            display.digitAlignment = alignment;
            display.spacing = stripSpacing;
            display.poolCapacity = capacity;
            display.TryLoadDefaultSpritesIfEmpty();
            display.EnsureInitialized();
            return display;
        }

        /// <summary>Editor / Bake：从 ArtRes/UI/Text 加载 0～9 写入 digitSprites。</summary>
        public void TryLoadDefaultSpritesIfEmpty()
        {
            if (HasValidSprites())
            {
                return;
            }

#if UNITY_EDITOR
            AssignSprites(LoadDefaultDigitSpritesEditor());
#endif
        }

#if UNITY_EDITOR
        /// <summary>Editor 专用：加载 Assets/ArtRes/UI/Text/0～9.png。</summary>
        public static Sprite[] LoadDefaultDigitSpritesEditor()
        {
            var sprites = new Sprite[10];
            for (var i = 0; i < 10; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{DigitSpriteFolderPath}{i}.png");
            }

            return sprites;
        }

        /// <summary>Editor Bake 写入 Sprite 引用到序列化字段。</summary>
        public void AssignSprites(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length < 10)
            {
                return;
            }

            digitSprites = sprites;
            EditorUtility.SetDirty(this);
        }
#endif

        private void Awake()
        {
            TryLoadDefaultSpritesIfEmpty();
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                SyncSpacingFromLayoutGroup();
                return;
            }

            _layout = GetComponent<HorizontalLayoutGroup>();
            if (_layout == null)
            {
                _layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            ApplyLayoutSettings();
            // IMG-R1：先剔除重复 / 超池 Digit_*，再按 Digit_索引顺序收集，避免 0200 假前导零。
            PruneDuplicateDigitChildren();
            _digitImages.Clear();
            CollectExistingDigitImages();
            EnsurePoolSize(poolCapacity);
            // 建池后立刻隐藏占位位图（须用内部方法：此时尚未 _initialized，不能走 HideAllDigits 守卫）。
            SetAllDigitsActive(false);
            _initialized = true;
        }

        /// <summary>丢弃池缓存，下次 EnsureInitialized 时按子节点重新收集（Bake / 场景校正用）。</summary>
        public void ResetDigitPoolForRebuild()
        {
            _initialized = false;
            _digitImages.Clear();
        }

        /// <summary>
        /// 按 Digit_ 后缀数字排序收集；仅保留每个索引的第一个子节点。
        /// 替代方案：按 sibling 顺序收集 —— 重复 Digit_0 会进池，SetActive(false) 管不到池外节点。
        /// </summary>
        private void CollectExistingDigitImages()
        {
            var sorted = new List<(int index, Image image)>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!TryParseDigitChildIndex(child.name, out var digitIndex))
                {
                    continue;
                }

                var image = child.GetComponent<Image>();
                if (image != null)
                {
                    sorted.Add((digitIndex, image));
                }
            }

            sorted.Sort((a, b) => a.index.CompareTo(b.index));

            var seen = new HashSet<int>();
            foreach (var (index, image) in sorted)
            {
                if (!seen.Add(index))
                {
                    continue;
                }

                _digitImages.Add(image);
            }
        }

        /// <summary>删除同名索引重复项与 index &gt;= poolCapacity 的多余 Digit_*（历史 Bake 残留）。</summary>
        private void PruneDuplicateDigitChildren()
        {
            var indexToChild = new Dictionary<int, Transform>();
            var toRemove = new List<GameObject>();

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!TryParseDigitChildIndex(child.name, out var digitIndex))
                {
                    continue;
                }

                if (indexToChild.TryGetValue(digitIndex, out var existing))
                {
                    // 同名索引保留较早的 sibling，删掉后创建的重复节点。
                    toRemove.Add(child.gameObject);
                    continue;
                }

                indexToChild[digitIndex] = child;
                if (digitIndex >= poolCapacity)
                {
                    toRemove.Add(child.gameObject);
                }
            }

            foreach (var go in toRemove)
            {
                DestroyDigitChild(go);
            }
        }

        private static bool TryParseDigitChildIndex(string childName, out int digitIndex)
        {
            digitIndex = -1;
            const string prefix = "Digit_";
            if (string.IsNullOrEmpty(childName) || !childName.StartsWith(prefix))
            {
                return false;
            }

            var suffix = childName.Substring(prefix.Length);
            return int.TryParse(suffix, out digitIndex) && digitIndex >= 0;
        }

        private static void DestroyDigitChild(GameObject go)
        {
            if (go == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(go);
                return;
            }
#endif
            Object.Destroy(go);
        }

#if UNITY_EDITOR
        private void MarkDigitChildrenDirty()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorUtility.SetDirty(this);
            foreach (var image in _digitImages)
            {
                if (image != null)
                {
                    EditorUtility.SetDirty(image.gameObject);
                }
            }
        }
#endif

        private void ApplyLayoutSettings()
        {
            if (_layout == null)
            {
                return;
            }

            _layout.spacing = spacing;
            _layout.childAlignment = digitAlignment;
            _layout.childControlWidth = false;
            _layout.childControlHeight = false;
            _layout.childForceExpandWidth = false;
            _layout.childForceExpandHeight = false;
        }

        private void EnsurePoolSize(int requiredCount)
        {
            var target = Mathf.Max(requiredCount, poolCapacity, 1);
            while (_digitImages.Count < target)
            {
                var index = _digitImages.Count;
                var digitGo = new GameObject($"Digit_{index}", typeof(RectTransform), typeof(CanvasRenderer));
                digitGo.transform.SetParent(transform, false);
                digitGo.layer = gameObject.layer;

                var image = digitGo.AddComponent<Image>();
                image.raycastTarget = false;
                image.preserveAspect = true;
                digitGo.SetActive(false);
                _digitImages.Add(image);
            }
        }

        private void ApplyDigitSprite(Image image, int digit)
        {
            if (image == null)
            {
                return;
            }

            if (digitSprites == null || digit < 0 || digit >= digitSprites.Length)
            {
                image.enabled = false;
                return;
            }

            var sprite = digitSprites[digit];
            image.sprite = sprite;
            image.enabled = sprite != null;

            if (sprite != null && useNativeSize)
            {
                image.SetNativeSize();
            }
        }

        /// <summary>
        /// 隐藏全部位图。仅在外部 API（SetDigitString 等）已初始化后调用；
        /// 禁止在此调用 EnsureInitialized（CRASH-1：与 EnsureInitialized 末尾互调会栈溢出闪退）。
        /// </summary>
        private void HideAllDigits()
        {
            if (!_initialized)
            {
                return;
            }

            SetAllDigitsActive(false);
            ResetFitScale();
        }

        /// <summary>Total2 等窄容器：按父 Rect 等比缩放数字条，六位总价不超出底框。</summary>
        private void TryFitWithinParentWidth()
        {
            if (!fitWithinParentWidth)
            {
                return;
            }

            var parentRect = transform.parent as RectTransform;
            if (parentRect == null)
            {
                return;
            }

            var contentWidth = MeasureActiveDigitsWidth();
            if (contentWidth <= 0f)
            {
                ResetFitScale();
                return;
            }

            var maxWidth = parentRect.rect.width - fitWidthPadding * 2f;
            if (maxWidth <= 0f)
            {
                return;
            }

            var scale = contentWidth <= maxWidth ? 1f : maxWidth / contentWidth;

            var contentHeight = MeasureActiveDigitsHeight();
            if (contentHeight > 0f)
            {
                var maxHeight = parentRect.rect.height - 4f;
                if (maxHeight > 0f)
                {
                    scale = Mathf.Min(scale, maxHeight / contentHeight);
                }
            }

            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private float MeasureActiveDigitsWidth()
        {
            var width = 0f;
            var count = 0;
            foreach (var image in _digitImages)
            {
                if (image == null || !image.gameObject.activeSelf)
                {
                    continue;
                }

                width += image.rectTransform.rect.width;
                count++;
            }

            if (count > 1)
            {
                width += spacing * (count - 1);
            }

            return width;
        }

        private float MeasureActiveDigitsHeight()
        {
            var height = 0f;
            foreach (var image in _digitImages)
            {
                if (image == null || !image.gameObject.activeSelf)
                {
                    continue;
                }

                height = Mathf.Max(height, image.rectTransform.rect.height);
            }

            return height;
        }

        private void ResetFitScale()
        {
            if (!fitWithinParentWidth)
            {
                return;
            }

            transform.localScale = Vector3.one;
        }

        /// <summary>遍历池内 Image 统一开关，供初始化与 HideAllDigits 共用。</summary>
        private void SetAllDigitsActive(bool active)
        {
            foreach (var image in _digitImages)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(active);
                }
            }
        }

        private void RebuildLayout()
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        private bool HasValidSprites()
        {
            if (digitSprites == null || digitSprites.Length < 10)
            {
                return false;
            }

            foreach (var sprite in digitSprites)
            {
                if (sprite != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ExtractDigits(string raw)
        {
            var builder = new StringBuilder(raw.Length);
            foreach (var ch in raw)
            {
                if (char.IsDigit(ch))
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
