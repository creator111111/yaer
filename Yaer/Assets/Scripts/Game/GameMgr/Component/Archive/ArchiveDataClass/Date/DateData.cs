using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Date
{
    public class DateData : BaseArchiveData
    {
        private DateTime date;
        public string Date => $"??????{date.Year}??{date.Month}??{date.Day}??";

        public int Day => date.Day;

        public void AddOneDay()
        {
            ThroughDate(0, 0, 1);
        }

        public void ThroughDate(int year, int month, int day)
        {
            date = date.AddYears(year);
            date = date.AddMonths(month);
            date = date.AddDays(day);
        }

        public override void ParseInternal(MasterGameData masterData)
        {
            date = masterData.GetValue("DateData_date", new DateTime(639, 4, 17));
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("DateData_date", date);
        }
    }
}
