using System;
using TMPro;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 购买列表单行：第四列 TxtStock 的数量输入。
    /// 挂在 Row_HpBall / Row_MpBall 根节点，或在 Awake 时自动 Find("TxtStock")。
    /// 阶段二：整数输入与失焦校验；阶段三通过 <see cref="QuantityForTotal"/> 参与合计。
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopBuyRowQuantityInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField quantityInput;

        /// <summary>失焦后的购买数量（空串回退默认值，供阶段四交易用）。</summary>
        public int Quantity => ShopQuantityInputHelper.ParseAndClampQuantity(
            quantityInput != null ? quantityInput.text : string.Empty);

        /// <summary>合计用数量：空串或非法按 0（阶段三 TxtTotal 公式）。</summary>
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

        /// <summary>打开商店或切回购买 Tab 时重置为默认数量。</summary>
        public void ResetToDefault(int defaultQuantity = ShopQuantityInputHelper.DefaultQuantity)
        {
            BindQuantityInput();
            ShopQuantityInputHelper.ApplyQuantityText(quantityInput, defaultQuantity);
            OnQuantityValueChanged?.Invoke();
        }

        /// <summary>供 ShopFormLogic 在初始化后绑定合计刷新。</summary>
        public TMP_InputField GetQuantityInput()
        {
            BindQuantityInput();
            return quantityInput;
        }

        /// <summary>供 ShopFormLogic 在 Tab 切换后确保 InputField 已绑定监听。</summary>
        public void EnsureListening()
        {
            BindQuantityInput();
            RegisterInputListeners();
        }

        /// <summary>
        /// 数量列节点：优先 TxtStock（0629 约定），Prefab 实际名为 Number 时兜底（SD-4）。
        /// </summary>
        private void BindQuantityInput()
        {
            if (quantityInput != null)
            {
                return;
            }

            var quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
            if (quantityNode != null)
            {
                quantityInput = ShopQuantityInputHelper.EnsureTmpIntegerInputField(
                    quantityNode,
                    ShopQuantityInputHelper.DefaultQuantity);
            }

            if (quantityInput == null)
            {
                Debug.LogWarning(
                    $"[ShopBuyRowQuantityInput] 未找到 TxtStock 或 Number：{GetHierarchyPath(transform)}",
                    this);
            }
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

        private void OnQuantityValueChangedInternal(string _)
        {
            OnQuantityValueChanged?.Invoke();
        }

        private void OnQuantityEndEdit(string _)
        {
            if (quantityInput == null)
            {
                return;
            }

            // 合计口径：空或非法 → 0；合法整数原样保留（含 0）。
            var sanitized = ShopQuantityInputHelper.ParseQuantityForTotal(quantityInput.text);
            ShopQuantityInputHelper.ApplyQuantityText(quantityInput, sanitized);
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
