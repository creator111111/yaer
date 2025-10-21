using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Menu.MainItemPage
{
    public class MenuFormMainItemPage : MonoBehaviour
    {
        private MenuFormProxy proxy;

        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;
        private List<GameObject> itemList = new List<GameObject>();
        private MenuFormLogic menuFormLogic;

        public bool IsOpen => gameObject.activeSelf;

        public void OnInit(MenuFormProxy menuFormProxy, MenuFormLogic menuFormLogic)
        {
            proxy = menuFormProxy;
            this.menuFormLogic = menuFormLogic;
            proxy.onUpdateMainItem = UpdateItemInfoList;
        }

        public void OnOpen()
        {
            gameObject.SetActive(true);
            proxy.UpdateItemPage();
        }

        public void OnClose()
        {
            gameObject.SetActive(false);
        }

        private void UpdateItemInfoList(List<MenuFormMainItemInfo> infos)
        {
            // 每次打开都清空
            for (var i = 0; i < itemList.Count; i++) Destroy(itemList[i].gameObject);
            itemList.Clear();

            // 实例化控件
            foreach (var item in infos)
            {
                GameObject go = Instantiate(itemPrefab, content, false);
                var button = go.GetComponentInChildren<MenuFormMainItemBtn>();
                button.OnInit(null);
                button.UpdateInfo(item, menuFormLogic);
                go.SetActive(true);
                itemList.Add(go);
            }

            // 填充到20个
            for (var i = itemList.Count; i < 24; i++)
            {
                GameObject go = Instantiate(itemPrefab, content, false);
                var button = go.GetComponentInChildren<MenuFormMainItemBtn>();
                button.OnInit(null);
                itemList.Add(go);
                go.SetActive(true);
            }
        }
    }
}