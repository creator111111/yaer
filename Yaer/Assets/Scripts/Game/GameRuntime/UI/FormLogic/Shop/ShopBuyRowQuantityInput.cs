using System;
using Game.GameRuntime.UI.Component;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店列表单行数量输入（购买 / 出售共用）：隐形 TMP + DigitStrip。
    /// 挂在 Shop_Bar 根节点。
    /// <para>
    /// 0923 公式审计（T1）：编辑中只 Sync 显示，不 TryClamp；失焦/Enter/切行/决定前
    /// <see cref="CommitQuantity"/> 再跑上限。买卖同一时序。禁止关 Form 联合公式。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopBuyRowQuantityInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField quantityInput;

        private Transform _quantityNode;

        private Func<int> _resolveMaxQuantity;

        private bool _isClampingQuantity;

        /// <summary>防止 BeginQuantityEdit / onSelect 清空时重入。</summary>
        private bool _isClearingForEdit;

        /// <summary>Commit 进行中：避免 EndEdit/Begin 重入。</summary>
        private bool _isCommitting;

        private Button _rowButton;

        private QuantityPointerClickRelay _quantityClickRelay;

        private Action _beforeBeginEdit;

        public int Quantity => ShopQuantityInputHelper.ParseAndClampQuantity(
            quantityInput != null ? quantityInput.text : string.Empty);

        public int QuantityForTotal => ShopQuantityInputHelper.ParseQuantityForTotal(
            quantityInput != null ? quantityInput.text : string.Empty);

        /// <summary>数量框是否正在编辑（聚焦）。联合 Reclamp 应跳过本行。</summary>
        public bool IsQuantityFocused =>
            quantityInput != null && quantityInput.isFocused && !_isCommitting;

        /// <summary>正式提交后（失焦/Enter/切行/决定前）：买侧联合回扫+Total2。</summary>
        public event Action OnQuantityValueChanged;

        /// <summary>清空进编辑：静默刷合计，不 Reclamp。</summary>
        public event Action OnQuantityClearedForEdit;

        /// <summary>编辑中键入：只 Sync 后软刷合计（不 Reclamp）。</summary>
        public event Action OnQuantityEditPreview;

        /// <summary>
        /// Commit 完成：beforeParsed → afterClamped。Form 打 [ShopBuyMax] / max=0 Tips。
        /// </summary>
        public event Action<int, int> OnQuantityCommitted;

        public void SetMaxQuantityResolver(Func<int> resolver)
        {
            _resolveMaxQuantity = resolver;
        }

        /// <summary>Begin 前先 Commit 其它聚焦行（Form 注入）。</summary>
        public void SetBeforeBeginEdit(Action beforeBeginEdit)
        {
            _beforeBeginEdit = beforeBeginEdit;
        }

        /// <summary>Form Unwire 时清掉所有外部回调，避免重复订阅闭包。</summary>
        public void ClearFormCallbacks()
        {
            OnQuantityValueChanged = null;
            OnQuantityClearedForEdit = null;
            OnQuantityEditPreview = null;
            OnQuantityCommitted = null;
            _beforeBeginEdit = null;
            _resolveMaxQuantity = null;
        }

        private void Awake()
        {
            BindQuantityInput();
        }

        private void OnEnable()
        {
            RegisterInputListeners();
            WireRowButtonClick();
        }

        private void OnDisable()
        {
            UnregisterInputListeners();
            UnwireRowButtonClick();
        }

        /// <summary>打开商店或切 Tab 时重置为默认数量并同步图片。</summary>
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
            WireRowButtonClick();
        }

        /// <summary>
        /// 统一进编辑：先 Commit 其它行 → 清空 → Activate。
        /// </summary>
        public void BeginQuantityEdit()
        {
            BindQuantityInput();
            if (quantityInput == null)
            {
                return;
            }

            // T3：切商品行前显式提交旧行（不依赖失焦副作用）。
            _beforeBeginEdit?.Invoke();

            ClearForEditInternal();
            quantityInput.Select();
            quantityInput.ActivateInputField();
            OnQuantityClearedForEdit?.Invoke();
        }

        /// <summary>
        /// 提交节点：合法化 → 业务上限钳 → DigitStrip → 正式通知。
        /// 覆盖：失焦、Enter、切行前、决定前。
        /// </summary>
        public void CommitQuantity()
        {
            if (quantityInput == null || _isCommitting || _isClearingForEdit)
            {
                return;
            }

            _isCommitting = true;
            try
            {
                var beforeParsed = ShopQuantityInputHelper.ParseQuantityForTotal(quantityInput.text);
                ShopQuantityInputHelper.ApplyQuantityText(quantityInput, beforeParsed);
                TryClampQuantityToBusinessMax(invokeChanged: false);
                RefreshDigitDisplay();
                var afterClamped = ShopQuantityInputHelper.ParseQuantityForTotal(quantityInput.text);

                OnQuantityCommitted?.Invoke(beforeParsed, afterClamped);
                OnQuantityValueChanged?.Invoke();
            }
            finally
            {
                _isCommitting = false;
            }
        }

        private void ClearForEditInternal()
        {
            if (quantityInput == null)
            {
                return;
            }

            _isClearingForEdit = true;
            try
            {
                ShopQuantityInputHelper.ClearQuantityForEdit(quantityInput, _quantityNode, this);
            }
            finally
            {
                _isClearingForEdit = false;
            }
        }

        private void BindQuantityInput()
        {
            if (quantityInput != null)
            {
                ShopQuantityInputHelper.ApplyInvisibleInputTextStyle(quantityInput);
                if (_quantityNode == null)
                {
                    _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
                }

                quantityInput.onValidateInput = null;
                EnsureQuantityHitArea();
                EnsureQuantityClickRelay();
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
                return;
            }

            quantityInput.onValidateInput = null;
            EnsureQuantityHitArea();
            EnsureQuantityClickRelay();
        }

        private void EnsureQuantityHitArea()
        {
            if (quantityInput == null)
            {
                return;
            }

            if (_quantityNode == null)
            {
                _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
            }

            var numberRt = _quantityNode as RectTransform;
            var inputRt = quantityInput.transform as RectTransform;
            if (numberRt != null && inputRt != null && inputRt != numberRt && inputRt.IsChildOf(numberRt))
            {
                StretchFull(inputRt);
            }

            var root = _quantityNode != null ? _quantityNode : quantityInput.transform;
            var graphics = root.GetComponentsInChildren<Graphic>(true);
            for (var i = 0; i < graphics.Length; i++)
            {
                var graphic = graphics[i];
                if (graphic == null)
                {
                    continue;
                }

                graphic.raycastTarget = graphic.gameObject == quantityInput.gameObject;
            }

            var hitImage = quantityInput.GetComponent<Image>();
            if (hitImage == null)
            {
                hitImage = quantityInput.gameObject.AddComponent<Image>();
                hitImage.color = new Color(1f, 1f, 1f, 0.02f);
            }

            hitImage.raycastTarget = true;
            if (quantityInput.targetGraphic == null)
            {
                quantityInput.targetGraphic = hitImage;
            }
        }

        private void EnsureQuantityClickRelay()
        {
            if (quantityInput == null)
            {
                return;
            }

            _quantityClickRelay = quantityInput.GetComponent<QuantityPointerClickRelay>();
            if (_quantityClickRelay == null)
            {
                _quantityClickRelay = quantityInput.gameObject.AddComponent<QuantityPointerClickRelay>();
            }

            _quantityClickRelay.Owner = this;
        }

        private void WireRowButtonClick()
        {
            if (_rowButton == null)
            {
                _rowButton = GetComponent<Button>();
            }

            if (_rowButton == null)
            {
                return;
            }

            _rowButton.onClick.RemoveListener(OnRowButtonClicked);
            _rowButton.onClick.AddListener(OnRowButtonClicked);
        }

        private void UnwireRowButtonClick()
        {
            if (_rowButton == null)
            {
                return;
            }

            _rowButton.onClick.RemoveListener(OnRowButtonClicked);
        }

        private void OnRowButtonClicked()
        {
            BeginQuantityEdit();
        }

        private void RefreshDigitDisplay()
        {
            if (_quantityNode == null)
            {
                _quantityNode = transform.Find("TxtStock") ?? transform.Find("Number");
            }

            var text = quantityInput != null ? quantityInput.text : string.Empty;
            ShopQuantityInputHelper.SyncNumberDigitDisplay(_quantityNode, text, this);
        }

        private void RegisterInputListeners()
        {
            if (quantityInput == null)
            {
                return;
            }

            quantityInput.onEndEdit.RemoveListener(OnQuantityEndEdit);
            quantityInput.onValueChanged.RemoveListener(OnQuantityValueChangedInternal);
            quantityInput.onSelect.RemoveListener(OnQuantitySelected);

            quantityInput.onEndEdit.AddListener(OnQuantityEndEdit);
            quantityInput.onValueChanged.AddListener(OnQuantityValueChangedInternal);
            quantityInput.onSelect.AddListener(OnQuantitySelected);
            quantityInput.onValidateInput = null;
        }

        private void UnregisterInputListeners()
        {
            if (quantityInput == null)
            {
                return;
            }

            quantityInput.onEndEdit.RemoveListener(OnQuantityEndEdit);
            quantityInput.onValueChanged.RemoveListener(OnQuantityValueChangedInternal);
            quantityInput.onSelect.RemoveListener(OnQuantitySelected);
            quantityInput.onValidateInput = null;
        }

        private void OnQuantitySelected(string _)
        {
            if (quantityInput == null || _isClearingForEdit || _isClampingQuantity || _isCommitting)
            {
                return;
            }

            if (!string.IsNullOrEmpty(quantityInput.text))
            {
                ClearForEditInternal();
                OnQuantityClearedForEdit?.Invoke();
            }
            else
            {
                RefreshDigitDisplay();
            }
        }

        private void OnQuantityValueChangedInternal(string text)
        {
            if (_isClampingQuantity || _isClearingForEdit || _isCommitting)
            {
                return;
            }

            ApplyValueChangedPipeline(text);
        }

        /// <summary>
        /// T1 编辑锁：聚焦中禁止 TryClamp；只刷 DigitStrip + 软合计。
        /// 提交钳制只走 <see cref="CommitQuantity"/>。
        /// </summary>
        private void ApplyValueChangedPipeline(string text)
        {
            RefreshDigitDisplay();

            if (quantityInput != null && quantityInput.isFocused)
            {
                // 编辑中：允许短暂超钱包/超持有显示；合计软刷，不联合 Reclamp。
                if (string.IsNullOrWhiteSpace(quantityInput.text))
                {
                    OnQuantityClearedForEdit?.Invoke();
                }
                else
                {
                    OnQuantityEditPreview?.Invoke();
                }

                return;
            }

            // 未聚焦却收到 ValueChanged（少见）：按提交处理，避免漏钳。
            CommitQuantity();
        }

        private void OnQuantityEndEdit(string _)
        {
            // 失焦 + Enter 均到此 → Commit。
            CommitQuantity();
        }

        /// <summary>
        /// 按 Form 注入的 max 写回。Commit / 联合回扫（非编辑行）调用。
        /// </summary>
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

        /// <summary>Form 联合回扫：静默钳（编辑中行应由 Form 跳过）。</summary>
        public bool ApplyBusinessMaxClampSilent()
        {
            if (IsQuantityFocused)
            {
                return false;
            }

            return TryClampQuantityToBusinessMax(invokeChanged: false);
        }

        private static void StretchFull(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private static string GetHierarchyPath(Transform node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            return node.parent == null ? node.name : $"{GetHierarchyPath(node.parent)}/{node.name}";
        }

        private sealed class QuantityPointerClickRelay : MonoBehaviour, IPointerClickHandler
        {
            public ShopBuyRowQuantityInput Owner;

            public void OnPointerClick(PointerEventData eventData)
            {
                if (Owner == null || eventData == null)
                {
                    return;
                }

                if (eventData.button != PointerEventData.InputButton.Left)
                {
                    return;
                }

                Owner.BeginQuantityEdit();
            }
        }
    }
}
