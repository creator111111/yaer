using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using Game.GameRuntime.UI.Component;
using UnityEditor;
using UnityEngine;

public static class AddDateMenuItem
{
    [MenuItem("Tools/增加日期")]
    public static void AddOneDay()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("请先运行游戏");
            return;
        }
        var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
        if (archive == null)
        {
            Debug.Log("增加日期: 存档不可用");
            return;
        }
        archive.GetData<DateData>().AddOneDay();
        var display = Object.FindObjectOfType<CalendarDateDisplay>(true);
        display?.RefreshFromArchive();
    }
}
