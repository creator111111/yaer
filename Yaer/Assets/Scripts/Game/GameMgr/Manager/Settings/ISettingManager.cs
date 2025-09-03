using Game.GameMgr.Manager.Base;

namespace Game.GameMgr.Manager.Settings
{
    public interface ISettingManager : IManager
    {
        void SaveSetting(object data);
        T LoadSetting<T>() where T : class;
        void SetDefault();

        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        void SetBool(string key, bool value);
        void SetString(string key, string value);
        int GetInt(string key);
        float GetFloat(string key);
        bool GetBool(string key);
        string GetString(string key);
        int GetInt(string key, int defaultValue);
        float GetFloat(string key, float defaultValue);
        bool GetBool(string key, bool defaultValue);
        string GetString(string key, string defaultValue);
    }
}