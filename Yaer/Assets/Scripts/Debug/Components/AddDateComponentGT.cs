using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using UnityEngine;

namespace GameDebug
{
    public class AddDateComponentGT : BaseGTComponent
    {
        protected override void OnInit()
        {
            base.OnInit();
        }

        public void AddOneDay()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("AddDate: 请先运行游戏");
                return;
            }
            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive == null)
            {
                Debug.Log("AddDate: 存档不可用");
                return;
            }
            archive.GetData<DateData>().AddOneDay();
            var display = Object.FindObjectOfType<Game.GameRuntime.UI.Component.CalendarDateDisplay>(true);
            display?.RefreshFromArchive();
        }
    }
}
