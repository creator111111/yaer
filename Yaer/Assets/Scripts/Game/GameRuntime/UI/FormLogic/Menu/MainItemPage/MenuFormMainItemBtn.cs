using System;
using Game.DataTable.MainItem;
using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.BagPack;
using Game.Static.Enum.Goods;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Menu.MainItemPage
{
    public class MenuFormMainItemBtn : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private GameObject root;
        [SerializeField] private DetailFormLogic detailForm;
        /// <summary>用于描述框锚点的道具格外框；为空则使用本按钮的 RectTransform。</summary>
        [SerializeField] private RectTransform detailAnchorRect;
        [SerializeField] private Image imgIcon;
        [SerializeField] private TextMeshProUGUI num;
        [SerializeField] private GameObject mask;
        [SerializeField] private GridLayoutGroup grid;

        public MenuFormMainItemInfo item;
        public Canvas uiCanvas;

        public void OnInit(object userData)
        {
            imgIcon.gameObject.SetActive(false);
        }

        /// <summary>
        /// 刷新单格展示。无 Icon 时仍绑定 item 与数量，避免「账本有货、格子空白」；
        /// 末尾再调 ResolveIcon 一次，覆盖 Database/图集晚于入包的窗口期。
        /// 替代方案：坚持无 icon 不显示，则必须保证入包前 EnsureLoaded 同步成功——更脆，不推荐。
        /// </summary>
        public void UpdateInfo(MenuFormMainItemInfo item, MenuFormLogic menuFormLogic)
        {
            // 无论 Icon 是否就绪，都先绑定数据，保证数量与点击/悬停有对象
            this.item = item;

            // Icon 为空时再向 Provider 要一次（异步晚到后 DefinitionsRebuilt 也会重刷；此处兜底打开瞬间）
            if (item.icon == null && !string.IsNullOrEmpty(item.name)
                && Enum.TryParse(item.name, out EMainItemName itemId))
            {
                var resolved = MainItemDefProvider.ResolveIcon(itemId);
                if (resolved != null)
                {
                    item.icon = resolved;
                }
            }

            if (item.icon != null)
            {
                imgIcon.gameObject.SetActive(true);
                imgIcon.sprite = item.icon;
                imgIcon.SetNativeSize();
            }
            else
            {
                // 图标槽隐藏，但数量仍显示，玩家能确认「包里有货」
                imgIcon.gameObject.SetActive(false);
            }

            num.text = $"{item.num}";
            GetComponent<Button>().onClick.AddListener(() =>
            {
                UIUtils.PlayBtnAudio(menuFormLogic);
                ItemBase.OnClick(item.name, menuFormLogic);
            });
        }

        public void ShowDetail()
        {
            // 不依赖 imgIcon 显隐：无 Icon 时仍要能悬停读 Database 详情（BAG-V4）
            if (item == null || detailForm == null)
            {
                return;
            }

            // 悬停协程每帧调用：刷新文案与锚点位置（滚动列表时格子会移动）
            var anchor = detailAnchorRect != null ? detailAnchorRect : GetComponent<RectTransform>();
            detailForm.gameObject.SetActive(true);

            var languageType = GameManager.Instance.language;
            var itemDesc = "";
            if (languageType == LanguageEnumType.Chinese)
            {
                itemDesc = item.detail;
            }
            else if (languageType == LanguageEnumType.English)
            {
                itemDesc = item.detail_en;
            }
            else if (languageType == LanguageEnumType.Japanese)
            {
                itemDesc = item.detail_jp;
            }

            detailForm.UpdateInfo(itemDesc, anchor);
        }

        public void HideDetail()
        {
            if (detailForm != null)
            {
                detailForm.gameObject.SetActive(false);
            }
        }

        #region 拖拽

        RectTransform rt;
        private Vector3 pos;
        private Vector3 start;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            pos = rt.localPosition;
            grid.enabled = true;
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item == null || item.num == 0 || eventData.button != PointerEventData.InputButton.Left)
                return;
            GetComponent<Button>().interactable = false;
            GetComponent<Image>().raycastTarget = false;
            grid.enabled = false;
            root.transform.SetAsLastSibling();
            start = Input.mousePosition;
            mask.SetActive(false);
        }

        /// <summary>
        /// 拖拽中
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            if (item == null || item.num == 0 || eventData.button != PointerEventData.InputButton.Left)
                return;
            rt.localPosition = pos + (Input.mousePosition - start) / uiCanvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (root == null) { return; }
            if (item == null) { return; }

            // 先还原拖拽造成的本地偏移，再改 sibling / 开 Grid；否则 Grid 只排布 Content 子节点时，子物体仍偏移会与邻格视觉堆叠。
            rt.localPosition = pos;

            root.transform.SetSiblingIndex(item.index);

            MenuFormMainItemBtn cell = null;
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                cell = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<MenuFormMainItemBtn>();
            }

            // 命中自身或同一数据项时不交换，避免多余 Swap 与布局抖动。
            if (cell != null && cell.item != null && cell != this && !ReferenceEquals(cell.item, this.item))
            {
                GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerBagData>().SwapItemsIndex(item, cell.item);
                cell.root.transform.SetSiblingIndex(cell.item.index);
                root.transform.SetSiblingIndex(item.index);
            }

            // 数据与 sibling 顺序确定后再开启布局，且只开一次，减少中间态重叠。
            grid.enabled = true;
            if (grid != null)
            {
                var gridRect = grid.transform as RectTransform;
                if (gridRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
                }
            }

            mask.SetActive(true);
            GetComponent<Button>().interactable = true;
            GetComponent<Image>().raycastTarget = true;
        }

        #endregion
    }
}