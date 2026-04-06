using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.BagPack;
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

        public void UpdateInfo(MenuFormMainItemInfo item, MenuFormLogic menuFormLogic)
        {
            if (!item.icon)
            {
                imgIcon.gameObject.SetActive(false);
                return;
            }

            this.item = item;
            imgIcon.gameObject.SetActive(true);
            imgIcon.sprite = item.icon;
            num.text = $"{item.num}";
            imgIcon.SetNativeSize();
            GetComponent<Button>().onClick.AddListener(() =>
            {
                UIUtils.PlayBtnAudio(menuFormLogic);
                ItemBase.OnClick(item.name, menuFormLogic);
            });
        }

        public void ShowDetail()
        {
            if (!imgIcon.gameObject.activeSelf || item == null || detailForm == null)
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