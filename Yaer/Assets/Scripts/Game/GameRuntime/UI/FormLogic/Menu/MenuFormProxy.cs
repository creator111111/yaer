using System;
using System.Collections.Generic;
using Game.DataTable.MainItem;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Path;
using GameFramework.DataTable;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    public class MenuFormProxy : BaseFormProxy
    {
        public event Action<bool> onMenuActiveEvent;
        public Action onReturnMainMenuEvent;
        public Action<List<MenuFormMainItemInfo>> onUpdateMainItem;

        public override void OnInit()
        {
            base.OnInit();

            PlayerBagData.Init();
        }

        public void UpdateItemPage()
        {
            // 获取背包数据
            var bagData = GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerBagData>();
            // 根据数据获取Icon
            List<MenuFormMainItemInfo> infos = bagData.GetAllMainItem();

            onUpdateMainItem?.Invoke(infos);
        }

        public void OnMenuActive(bool active) => onMenuActiveEvent?.Invoke(active);

        public void OnReturnMainMenu() => onReturnMainMenuEvent?.Invoke();
    }
}