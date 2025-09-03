using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameRuntime.UI.FormLogic.Base;

namespace Game.GameRuntime.UI.FormLogic.Archive.SaveGamePanel
{
    public class SaveGameFormProxy : BaseFormProxy
    {
        private ArchiveComponentGM archiveComponentGM;

        public Action onSaveNewArchiveAction;
        public Action onSaveOldArchiveAction;
        public Action<string> onCoverArchiveAction;

        public override void OnInit()
        {
            base.OnInit();

            archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
        }

        /// <summary>
        /// 获取所有存档信息
        /// </summary>
        public List<ArchiveDirectoryInfo> GetAllArchiveInfos() => archiveComponentGM.LoadAllArchiveInfo();

        /// <summary>
        /// 获取当前正在使用的存档信息
        /// </summary>
        /// <returns></returns>
        public ArchiveInfo GetNowArchiveInfo() => archiveComponentGM.GetNowArchiveInfo();

        /// <summary>
        /// 保存新存档
        /// </summary>
        public void SaveNewArchive() => onSaveNewArchiveAction?.Invoke();

        public void SaveOldArchive() => onSaveOldArchiveAction?.Invoke();

        public void CoverArchive(string selectedArchiveGuid) => onCoverArchiveAction?.Invoke(selectedArchiveGuid);

        public void DeleteArchive(string selectedArchiveGuid)
        {
            archiveComponentGM.DeleteArchive(selectedArchiveGuid);
        }
    }
}