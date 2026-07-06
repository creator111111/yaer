using System;
using Game.GameRuntime.UI.Component;
using TMPro;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店列表单行数量输入（购买 / 出售共用）：隐形 TMP_InputField + DigitStrip 图片数字。
    /// 挂在 Shop_Bar 根节点；合计通过 <see cref="QuantityForTotal"/> 参与 Total2 Σ 计算。
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopBuyRowQuantityInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField quantityInput;

        private Transform _quantityNode;

        /// <summary>失焦后的购买数量（空串回退默认值，供阶段四交易用）。</summary>
        public int Quantity => ShopQuantityInputHelper.ParseAndClampQuantity(
            quantityInput != null ? quantityInput.text : string.Empty);

        /// <summary>合计用数量：空串或非法按 0。</summary>
        public int QuantityForTotal => ShopQuantityInputHelper.ParseQuantityForTotal(
            quantityInput != null ? quantityInput.text : string.Empty);

        /// <summary>数量输入框每次变化时触发（含 onValueChanged）。</summary>
        public event Action OnQuantityValueChanged;

        private void Awake()
        {
            BindQuantityInput();
        }

        private void OnEnable()
        {
            RegisterInputListeners();
        }

        private void OnDisable()
        {
            UnregisterInputListeners();
        }

        /// <summary>打开商店或切 Tab 时重置为默认数量（ST：0）并同步图片。</summary>
        public void ResetToDefault(int defaultQuantity = ShopQuantityInputHelper.DefaultQuantity)
        {
            BindQuantityInput();
            ShopQuantityInputHelper.ApplyQuantityText(quantityInput, defaultQuantity);
            RefreshDigitDisplay();
            OnQuantityValueChanged?.Invoke();
        }

        public TMP_InputField GetQuantityInput()
        {
            BindQuantityInput();
            return quantityInput;
        }

        public void EnsureListening()
        {
            BindQuantityInput();
            RegisterInputListeners();
        }

        private void BindQuantityInput()
        {
            if (quantityInput != null)
            {
                return;
            }

            _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
            if (_quantityNode != null)
            {
                quantityInput = ShopQuantityInputHelper.EnsureTmpIntegerInputField(
                    _quantityNode,
                    ShopQuantityInputHelper.DefaultQuantity);
            }

            if (quantityInput == null)
            {
                Debug.LogWarning(
                    $"[ShopBuyRowQuantityInput] 未找到 TxtStock 或 Number：{GetHierarchyPath(transform)}",
                    this);
            }
        }

        /// <summary>把 TMP 当前文本同步到 Number/DigitStrip 图片层。</summary>
        private void RefreshDigitDisplay()
        {
            if (_quantityNode == null)
            {
                _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
            }

            var text = quantityInput != null ? quantityInput.text : string.Empty;
            ShopQuantityInputHelper.SyncNumberDigitDisplay(_quantityNode, text);
        }

        private void RegisterInputListeners()
        {
            if (quantityInput == null)
            {
                return;
            }

            quantityInput.onEndEdit.AddListener(OnQuantityEndEdit);
            quantityInput.onValueChanged.AddListener(OnQuantityValueChangedInternal);
        }

        private void UnregisterInputListeners()
        {
            if (quantityInput == null)
            {
                return;
            }

            quantityInput.onEndEdit.RemoveListener(OnQuantityEndEdit);
            quantityInput.onValueChanged.RemoveListener(OnQuantityValueChangedInternal);
        }

        private void OnQuantityValueChangedInternal(string text)
        {
            ShopQuantityInputHelper.SyncNumberDigitDisplay(_quantityNode, text);
            OnQuantityValueChanged?.Invoke();
        }

        private void OnQuantityEndEdit(string _)
        {
            if (quantityInput == null)
            {
                return;
            }

            var sanitized = ShopQuantityInputHelper.ParseQuantityForTotal(quantityInput.text);
            ShopQuantityInputHelper.ApplyQuantityText(quantityInput, sanitized);
            RefreshDigitDisplay();
            OnQuantityValueChanged?.Invoke();
        }

        private static string GetHierarchyPath(Transform node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            return node.parent == null ? node.name : $"{GetHierarchyPath(node.parent)}/{node.name}";
        }
    }
}
