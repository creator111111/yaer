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
        [SerializeField] private Image imgIcon;
        [SerializeField] private TextMeshProUGUI num;
        [SerializeField] private GameObject mask;
        [SerializeField] private GridLayoutGroup grid;

        private bool isOpenDetail;
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
            if (!imgIcon.gameObject.activeSelf) return;

            if (!isOpenDetail)
            {
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
                detailForm.UpdateInfo(itemDesc);
                isOpenDetail = true;
            }
        }

        public void HideDetail()
        {
            detailForm.gameObject.SetActive(false);
            isOpenDetail = false;
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
            root.transform.SetSiblingIndex(item.index);
            grid.enabled = true;
            MenuFormMainItemBtn cell = null;
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                cell = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<MenuFormMainItemBtn>();
            }
            if (cell != null && cell.item != null)
            {
                GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerBagData>().SwapItemsIndex(item, cell.item);
                cell.root.transform.SetSiblingIndex(cell.item.index);
                root.transform.SetSiblingIndex(item.index);
            }

            rt.localPosition = pos;
            mask.SetActive(true);
            GetComponent<Button>().interactable = true;
            GetComponent<Image>().raycastTarget = true;
        }

        #endregion
    }
}