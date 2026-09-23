using System;
using Game.GameRuntime.UI.Component;
using TMPro;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店列表单行数量输入（购买 / 出售共用）：隐形 TMP_InputField + DigitStrip 图片数字。
    /// 挂在 Shop_Bar 根节点；合计通过 <see cref="QuantityForTotal"/> 参与 Total2 Σ 计算。
    /// <para>
    /// 0922：失焦/输入超限时按 <see cref="SetMaxQuantityResolver"/> 钳回上限
    /// （卖=持有，买=金币购买力∩堆叠空位），并同步 DigitStrip + 合计。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopBuyRowQuantityInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField quantityInput;

        private Transform _quantityNode;

        /// <summary>
        /// 当前行可填最大数量（由 <see cref="ShopFormLogic"/> 按买/卖公式注入）。
        /// 未绑定时不钳业务上限（仅 ≥0），避免孤立 Prefab 预览误砍。
        /// </summary>
        private Func<int> _resolveMaxQuantity;

        /// <summary>防钳制写回触发 onValueChanged 重入。</summary>
        private bool _isClampingQuantity;

        /// <summary>失焦后的购买数量（空串回退默认值，供阶段四交易用）。</summary>
        public int Quantity => ShopQuantityInputHelper.ParseAndClampQuantity(
            quantityInput != null ? quantityInput.text : string.Empty);

        /// <summary>合计用数量：空串或非法按 0。</summary>
        public int QuantityForTotal => ShopQuantityInputHelper.ParseQuantityForTotal(
            quantityInput != null ? quantityInput.text : string.Empty);

        /// <summary>数量输入框每次变化时触发（含 onValueChanged）。</summary>
        public event Action OnQuantityValueChanged;

        /// <summary>
        /// 注入本行上限回调（买=金币/堆叠，卖=持有）。
        /// 原因：行组件不直读存档，公式集中在 Form，切 Tab/旁路语义由 Form 定。
        /// </summary>
        public void SetMaxQuantityResolver(Func<int> resolver)
        {
            _resolveMaxQuantity = resolver;
        }

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

        /// <summary>
        /// 绑定数量框：无引用则 Ensure；已有引用也强制刷隐形样式（关 caret 闪烁）。
        /// </summary>
        /// <remarks>
        /// 原因：Prefab 预绑 quantityInput 时旧逻辑直接 return，不跑 Ensure → 磁盘上 width/blink 仍闪。
        /// 施工要求：已有引用也 Apply 一次，运行时覆盖旧序列化，不必手改 11 行 YAML。
        /// </remarks>
        private void BindQuantityInput()
        {
            // 已绑定：仍刷隐形样式（关 caret），再保证 _quantityNode 给 DigitStrip 用。
            if (quantityInput != null)
            {
                ShopQuantityInputHelper.ApplyInvisibleInputTextStyle(quantityInput);
                if (_quantityNode == null)
                {
                    _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
                }

                return;
            }

            _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
            if (_quantityNode != null)
            {
                // Ensure 内部已含 ApplyInvisible；此处走完整升级路径。
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
            if (_isClampingQuantity)
            {
                return;
            }

            // 边输边钳：超持有/超购买力立刻写回，DigitStrip 与 Total2 同步变。
            if (TryClampQuantityToBusinessMax(invokeChanged: false))
            {
                RefreshDigitDisplay();
                OnQuantityValueChanged?.Invoke();
                return;
            }

            ShopQuantityInputHelper.SyncNumberDigitDisplay(_quantityNode, text);
            OnQuantityValueChanged?.Invoke();
        }

        private void OnQuantityEndEdit(string _)
        {
            if (quantityInput == null)
            {
                return;
            }

            // 失焦：先规整非负整数，再按业务上限钳（空串→0）。
            var sanitized = ShopQuantityInputHelper.ParseQuantityForTotal(quantityInput.text);
            ShopQuantityInputHelper.ApplyQuantityText(quantityInput, sanitized);
            TryClampQuantityToBusinessMax(invokeChanged: false);
            RefreshDigitDisplay();
            OnQuantityValueChanged?.Invoke();
        }

        /// <summary>
        /// 按 Form 注入的 max 钳 TMP；超限则写回并刷图。
        /// </summary>
        /// <returns>是否发生了写回（调用方可据此决定是否再 Sync）。</returns>
        private bool TryClampQuantityToBusinessMax(bool invokeChanged)
        {
            if (quantityInput == null || _resolveMaxQuantity == null)
            {
                return false;
            }

            var parsed = ShopQuantityInputHelper.ParseQuantityForTotal(quantityInput.text);
            int maxQty;
            try
            {
                maxQty = _resolveMaxQuantity.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ShopBuyRowQuantityInput] max 回调异常，跳过钳制：{e.Message}", this);
                return false;
            }

            var clamped = ShopQuantityInputHelper.ClampQuantityToCap(parsed, maxQty);
            if (clamped == parsed)
            {
                return false;
            }

            _isClampingQuantity = true;
            try
            {
                ShopQuantityInputHelper.ApplyQuantityText(quantityInput, clamped);
                RefreshDigitDisplay();
                if (invokeChanged)
                {
                    OnQuantityValueChanged?.Invoke();
                }
            }
            finally
            {
                _isClampingQuantity = false;
            }

            return true;
        }

        /// <summary>
        /// Form 在其它行改数量后回扫本行：按最新联合总价再钳一次，不触发 OnQuantityValueChanged（防循环）。
        /// </summary>
        public bool ApplyBusinessMaxClampSilent()
        {
            return TryClampQuantityToBusinessMax(invokeChanged: false);
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
