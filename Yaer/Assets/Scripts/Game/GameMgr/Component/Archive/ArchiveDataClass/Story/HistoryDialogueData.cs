
using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameRuntime.UI.FormLogic.Story.Base.Control;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass
{
    public class HistoryDialogueData : BaseArchiveData
    {
        public List<HistoryDialogueInfo> HistoryDialogueInfos;

        public override void ParseInternal(MasterGameData masterData)
        {
            var bytes = masterData.GetValue<byte[]>("HistoryDialogueData_HistoryDialogueInfos");
            if (bytes != null)
            {
                HistoryDialogueInfos = ES3.Deserialize<List<HistoryDialogueInfo>>(bytes);
            }
            else
            {
                HistoryDialogueInfos = new List<HistoryDialogueInfo>();
            }
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("HistoryDialogueData_HistoryDialogueInfos", ES3.Serialize(HistoryDialogueInfos));
        }
    }
}