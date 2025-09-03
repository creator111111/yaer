using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameRuntime.UI.FormLogic.Base;

namespace Game.GameRuntime.UI.FormLogic.Archive.LoadGamePanel
{
    public class LoadGameFormProxy: BaseFormProxy
    {
        private ArchiveComponentGM archiveComponentGM;

        public event Action<string> onLoadGameAction;
        
        public override void OnInit()
        {
            base.OnInit();
            
            archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
        }

        /// <summary>
        /// 获取所有存档信息
        /// </summary>
        public List<ArchiveDirectoryInfo> GetAllArchiveInfos()
        {
            return archiveComponentGM.LoadAllArchiveInfo();
        }
        
        /// <summary>
        /// 获取当前正在使用的存档信息
        /// </summary>
        public ArchiveInfo GetNowArchiveInfo()
        {
            return archiveComponentGM.GetNowArchiveInfo();
        }

        public void DeleteArchive(string selectedArchiveGuid)
        {
            archiveComponentGM.DeleteArchive(selectedArchiveGuid);
        }
        
        public void LoadArchive(string selectedArchiveGuid) => onLoadGameAction?.Invoke(selectedArchiveGuid);
    }
}