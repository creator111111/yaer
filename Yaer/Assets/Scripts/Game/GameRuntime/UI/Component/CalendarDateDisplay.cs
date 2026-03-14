using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using TMPro;
using UnityEngine;

namespace Game.GameRuntime.UI.Component
{
    /// <summary>
    /// 绑定日期 Text，可从存档刷新显示；Tools「增加日期」后会查找并刷新本组件。
    /// </summary>
    public class CalendarDateDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dateText;

        public void RefreshFromArchive()
        {
            if (dateText == null) return;
            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive == null) return;
            dateText.text = archive.GetData<DateData>().Date;
        }
    }
}
