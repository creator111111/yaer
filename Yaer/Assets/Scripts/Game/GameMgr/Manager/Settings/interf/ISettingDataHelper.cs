namespace Game.GameMgr.Manager.Settings.interf
{
    public interface ISettingDataHelper
    {
        object GetData();
        void SaveSettings(object source);
        T LoadSettings<T>() where T : class;
        T GetDefaultSettings<T>() where T : class;
        void SetDefaultSettings();
    }
}