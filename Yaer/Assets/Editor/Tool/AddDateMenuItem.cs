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

        // 存档日期 +1
        archive.GetData<DateData>().AddOneDay();

        // 刷新文本日期
        var textDisplay = Object.FindObjectOfType<CalendarDateDisplay>(true);
        textDisplay?.RefreshFromArchive();

        // 刷新菜单里数字图片日期
        var spriteDisplay = Object.FindObjectOfType<MenuCalendarDayNumDisplay>(true);
        spriteDisplay?.RefreshFromArchive();
    }
}
