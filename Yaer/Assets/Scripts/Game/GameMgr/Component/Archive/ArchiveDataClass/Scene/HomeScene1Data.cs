using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Scene
{
    [Serializable]
    public class HomeScene1Data : BaseArchiveData
    {
        public bool firstEnter;
        public bool xiaerDialogue;
        public bool getMap;
        public override void ParseInternal(MasterGameData masterData)
        {
            firstEnter = masterData.GetValue("HomeScene1Data_firstEnter", true);
            xiaerDialogue = masterData.GetValue<bool>("HomeScene1Data_xiaerDialogue");
            getMap = masterData.GetValue<bool>("HomeScene1Data_getMap");
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("HomeScene1Data_firstEnter", firstEnter);
            masterData.SetValue("HomeScene1Data_xiaerDialogue", xiaerDialogue);
            masterData.SetValue("HomeScene1Data_getMap", getMap);
        }
    }
}