using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Date
{
    public class DateData : BaseArchiveData
    {
        private DateTime date;
        public string Date => $"龙历{date.Year}年{date.Month}月{date.Day}日";

        public void ThroughDate(int year, int month, int day)
        {
            date = date.AddYears(year);
            date = date.AddMonths(month);
            date = date.AddDays(day);
        }

        public override void ParseInternal(MasterGameData masterData)
        {
            date = masterData.GetValue("DateData_date", new DateTime(271, 7, 2));
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("DateData_date", date);
        }
    }
}
