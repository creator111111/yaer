using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class PlayerCommonData : BaseArchiveData
    {
        public int CurChapter;
        public float MaxHP;
        public float CurrentHP;
        public float MaxStamina;
        public float CurrentStamina;
        public bool ClothesBroken;

        public override void ParseInternal(MasterGameData masterData)
        {
            MaxHP = masterData.GetValue("PlayerCommonData_MaxHP", 100);
            CurrentHP = masterData.GetValue("PlayerCommonData_CurrentHP", 100);
            MaxStamina = masterData.GetValue("PlayerCommonData_MaxStamina", 100);
            CurrentStamina = masterData.GetValue("PlayerCommonData_CurrentStamina", 100);
            ClothesBroken = masterData.GetValue("PlayerCommonData_ClothesBroken", false);
            CurChapter = masterData.GetValue("PlayerCommonData_CurChapter", 0);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("PlayerCommonData_MaxHP", MaxHP);
            masterData.SetValue("PlayerCommonData_CurrentHP", CurrentHP);
            masterData.SetValue("PlayerCommonData_MaxStamina", MaxStamina);
            masterData.SetValue("PlayerCommonData_CurrentStamina", CurrentStamina);
            masterData.SetValue("PlayerCommonData_ClothesBroken", ClothesBroken);
            masterData.SetValue("PlayerCommonData_CurChapter", CurChapter);
        }
    }
}
