using System;
using Game.DataTable;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Dialogue
{
    public class SelectDialogueFormSelectItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text txContent;
        [SerializeField] private Button btnSelect;
        [SerializeField] private Transform signTsf;
        
        public Action<int> onSelected;

        public void OnInit(object userData)
        {
            signTsf.gameObject.SetActive(false);
        }

        private void Start()
        {
            signTsf.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            signTsf.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            signTsf.gameObject.SetActive(false);
        }
    }
}