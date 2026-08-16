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
            // Database/图集异步晚于入包时：数据层已由 PlayerBagData 重刷；此处刷新打开中的贵重物品页 Icon
            MainItemDefProvider.DefinitionsRebuilt -= OnMainItemDefinitionsRebuilt;
            MainItemDefProvider.DefinitionsRebuilt += OnMainItemDefinitionsRebuilt;
        }

        /// <summary>
        /// Provider 缓存重建后，若贵重物品页已订阅 onUpdateMainItem，则立即重刷格子。
        /// Archive 尚未就绪时跳过，避免启动早期空引用。
        /// </summary>
        private void OnMainItemDefinitionsRebuilt()
        {
            if (GameManager.Instance == null || onUpdateMainItem == null)
            {
                return;
            }

            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive == null || archive.GetData<PlayerBagData>() == null)
            {
                return;
            }

            UpdateItemPage();
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