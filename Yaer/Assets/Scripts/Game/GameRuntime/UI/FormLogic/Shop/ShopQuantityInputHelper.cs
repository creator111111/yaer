using Game.GameRuntime.UI.Component;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店购买行「数量」列：把 TxtStock 从纯 Text 升级为 TMP 整数输入框。
    /// 阶段二只做 UI 输入，不接合计/扣金币；阶段三通过 <see cref="ShopBuyRowQuantityInput"/> 读数量。
    /// </summary>
    public static class ShopQuantityInputHelper
    {
        /// <summary>与背包单格堆叠上限一致，限制输入位数。</summary>
        public const int MaxQuantityDigits = 2;

        /// <summary>打开商店 / 切 Tab 时 Number 列默认数量（ST：合计初始为 0）。</summary>
        public const int DefaultQuantity = 0;

        private const string TextAreaName = "Text Area";
        private const string PlaceholderName = "Placeholder";
        private const string TextChildName = "Text";

        /// <summary>
        /// 确保 TxtStock 节点上存在可点击的 TMP 整数输入框。
        /// 若 Prefab 仍是 legacy Text，运行时会自动替换（保留节点名 TxtStock 供 Find）。
        /// </summary>
        public static TMP_InputField EnsureTmpIntegerInputField(Transform txtStockTransform, int defaultQuantity = DefaultQuantity)
        {
            if (txtStockTransform == null)
            {
                return null;
            }

            var host = txtStockTransform.gameObject;

            // 阶段一遗留的 UnityEngine.UI.Text 无法输入，需移除以免与 TMP 冲突。
            RemoveLegacyText(host);

            EnsureRaycastImage(host);

            var inputField = host.GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                inputField = host.AddComponent<TMP_InputField>();
                BuildInputFieldHierarchy(host.transform, inputField, defaultQuantity);
            }
            else
            {
                WireExistingHierarchy(host.transform, inputField);
            }

            ApplyIntegerInputSettings(inputField, defaultQuantity);
            ApplyInvisibleInputTextStyle(inputField);
            EnsureNumberDigitStrip(txtStockTransform, defaultQuantity);
            return inputField;
        }

        /// <summary>
        /// Number 列：透明 TMP 仅承载输入逻辑，可见数字由 DigitStrip 图片显示（IMG 方案）。
        /// </summary>
        private static void ApplyInvisibleInputTextStyle(TMP_InputField inputField)
        {
            if (inputField == null)
            {
                return;
            }

            if (inputField.textComponent != null)
            {
                var color = inputField.textComponent.color;
                inputField.textComponent.color = new Color(color.r, color.g, color.b, 0f);
            }

            if (inputField.placeholder != null)
            {
                var placeholderColor = inputField.placeholder.color;
                inputField.placeholder.color = new Color(
                    placeholderColor.r,
                    placeholderColor.g,
                    placeholderColor.b,
                    0f);
            }

            inputField.customCaretColor = true;
            inputField.caretColor = new Color(1f, 1f, 1f, 0f);
        }

        /// <summary>确保 Number 下 DigitStrip 存在并刷默认数量图。</summary>
        private static void EnsureNumberDigitStrip(Transform numberNode, int defaultQuantity)
        {
            if (numberNode == null)
            {
                return;
            }

            var display = UiSpriteNumberDisplay.EnsureOn(
                numberNode,
                TextAnchor.MiddleRight,
                stripSpacing: UiSpriteNumberDisplay.ShopNumberSpacing,
                capacity: MaxQuantityDigits);
            display.TryLoadDefaultSpritesIfEmpty();
            display.SetSpacing(UiSpriteNumberDisplay.ShopNumberSpacing);
            display.SetNumber(defaultQuantity);
        }

        /// <summary>同步 Number 列图片数字（供 ShopBuyRowQuantityInput 在 onValueChanged 调用）。</summary>
        public static void SyncNumberDigitDisplay(Transform numberNode, string rawText)
        {
            if (numberNode == null)
            {
                return;
            }

            var display = UiSpriteNumberDisplay.FindUnder(numberNode);
            if (display == null)
            {
                return;
            }

            display.SetDigitString(rawText ?? string.Empty);
        }

        /// <summary>
        /// 失焦或提交时把字符串规整为非负整数；空串回退默认值。
        /// IntegerNumber 会挡大部分非法键，此处兜底粘贴/脚本写入。
        /// </summary>
        public static int ParseAndClampQuantity(string rawText, int fallback = DefaultQuantity)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return fallback;
            }

            // 只保留数字字符，丢弃负号、小数点、字母等。
            var digitsOnly = string.Empty;
            foreach (var ch in rawText.Trim())
            {
                if (char.IsDigit(ch))
                {
                    digitsOnly += ch;
                }
            }

            if (digitsOnly.Length == 0)
            {
                return fallback;
            }

            if (!int.TryParse(digitsOnly, out var value))
            {
                return fallback;
            }

            return Mathf.Max(0, value);
        }

        /// <summary>
        /// 阶段三合计专用：空串或非法输入按 0 计（不用 DefaultQuantity 兜底）。
        /// </summary>
        public static int ParseQuantityForTotal(string rawText)
        {
            return ParseAndClampQuantity(rawText, fallback: 0);
        }

        /// <summary>把输入框文本设为合法整数显示。</summary>
        public static void ApplyQuantityText(TMP_InputField inputField, int quantity)
        {
            if (inputField == null)
            {
                return;
            }

            var safeValue = Mathf.Max(0, quantity);
            inputField.SetTextWithoutNotify(safeValue.ToString());
        }

        private static void RemoveLegacyText(GameObject host)
        {
            var legacyText = host.GetComponent<Text>();
            if (legacyText == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(legacyText);
            }
            else
            {
                Object.DestroyImmediate(legacyText);
            }
        }

        /// <summary>InputField 需要底图且勾选 Raycast Target，否则点不到框。</summary>
        private static void EnsureRaycastImage(GameObject host)
        {
            var image = host.GetComponent<Image>();
            if (image == null)
            {
                image = host.AddComponent<Image>();
                // 近乎透明底框：能看见原 Prefab 美术，又能接收点击。
                image.color = new Color(1f, 1f, 1f, 0.02f);
            }

            image.raycastTarget = true;
        }

        private static void BuildInputFieldHierarchy(Transform root, TMP_InputField inputField, int defaultQuantity)
        {
            var textAreaGo = CreateUiChild(root, TextAreaName, typeof(RectTransform), typeof(RectMask2D));
            StretchFull(textAreaGo.GetComponent<RectTransform>());

            var placeholderGo = CreateUiChild(textAreaGo.transform, PlaceholderName, typeof(RectTransform), typeof(TextMeshProUGUI));
            StretchFull(placeholderGo.GetComponent<RectTransform>());
            ConfigureTmpLabel(placeholderGo.GetComponent<TextMeshProUGUI>(), new Color(0.5f, 0.5f, 0.5f, 0.5f), string.Empty);

            var textGo = CreateUiChild(textAreaGo.transform, TextChildName, typeof(RectTransform), typeof(TextMeshProUGUI));
            StretchFull(textGo.GetComponent<RectTransform>());
            ConfigureTmpLabel(textGo.GetComponent<TextMeshProUGUI>(), new Color(0.196f, 0.196f, 0.196f, 1f), defaultQuantity.ToString());

            inputField.textViewport = textAreaGo.GetComponent<RectTransform>();
            inputField.textComponent = textGo.GetComponent<TextMeshProUGUI>();
            inputField.placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
        }

        private static void WireExistingHierarchy(Transform root, TMP_InputField inputField)
        {
            if (inputField.textViewport == null)
            {
                var textArea = root.Find(TextAreaName);
                if (textArea != null)
                {
                    inputField.textViewport = textArea.GetComponent<RectTransform>();
                }
            }

            if (inputField.textComponent == null)
            {
                var text = root.Find($"{TextAreaName}/{TextChildName}");
                if (text != null)
                {
                    inputField.textComponent = text.GetComponent<TextMeshProUGUI>();
                }
            }

            if (inputField.placeholder == null)
            {
                var placeholder = root.Find($"{TextAreaName}/{PlaceholderName}");
                if (placeholder != null)
                {
                    inputField.placeholder = placeholder.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private static void ApplyIntegerInputSettings(TMP_InputField inputField, int defaultQuantity)
        {
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.characterLimit = MaxQuantityDigits;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.richText = false;

            if (inputField.textComponent != null)
            {
                inputField.textComponent.alignment = TextAlignmentOptions.MidlineRight;
            }

            ApplyQuantityText(inputField, defaultQuantity);
        }

        private static GameObject CreateUiChild(Transform parent, string childName, params System.Type[] components)
        {
            var go = new GameObject(childName, components);
            go.transform.SetParent(parent, false);
            go.layer = parent.gameObject.layer;
            return go;
        }

        private static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void ConfigureTmpLabel(TextMeshProUGUI label, Color color, string text)
        {
            label.color = color;
            label.fontSize = 22f;
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.text = text;
            label.raycastTarget = false;

            // 优先用 TMP 内置字体；Prefab 阶段可在编辑器里换成 Alibaba 等工程字体。
            if (label.font == null)
            {
                label.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
        }
    }
}
