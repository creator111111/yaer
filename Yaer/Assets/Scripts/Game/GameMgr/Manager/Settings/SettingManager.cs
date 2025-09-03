using System;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameMgr.Manager.Settings.interf;
using GameFramework.UnityRuntime.Setting;
using UnityEngine;

namespace Game.GameMgr.Manager.Settings
{
    public struct VolumeChangedArgs
    {
        public float MainVolume;
        public float BGMVolume;
        public float SoundVolume;

        public VolumeChangedArgs(float mainVolume, float bGMVolume, float soundVolume)
        {
            MainVolume = mainVolume;
            BGMVolume = bGMVolume;
            SoundVolume = soundVolume;
        }
    }

    public class SettingManager : MonoBehaviour, ISettingManager
    {
        private ISettingDataHelper dataHelper;
        private SettingComponent _settingSystem;
        private SettingComponent settingSystem
        {
            get
            {
                if (_settingSystem == null)
                    _settingSystem = GameManager.GetGFComponent<SettingComponent>();
                return _settingSystem;
            }
        }
        private ISettingManagerVisionHelper visionHelper;

        public Action<VolumeChangedArgs> OnVolumeChange;
        public Action<float> OnTextShowTimeChange;
        public Action<float> OnAutoShowTimeChange;
        public Action<bool> OnBattleImageChange;
        public Action<bool> OnShowWoundChange;

        public Action OnSetKeyDefault;
        public Action OnSetSettingsDefault;

        public SettingsConfigData SettingData;
        public void Init()
        {
            SetDataHelper(new SettingsDataHelper(this));
        }

        public void SaveSetting(object data)
        {
            if (dataHelper == null)
            {
                Debug.LogError("请先设置dataHelper");
                return;
            }

            dataHelper.SaveSettings(data);
            onSettingsUpdated?.Invoke(dataHelper.GetData());
        }

        public T LoadSetting<T>() where T : class
        {
            if (dataHelper == null)
            {
                Debug.LogError("请先设置dataHelper");
                return null;
            }

            return dataHelper.LoadSettings<T>();
        }

        public void SetDefault()
        {
            if (dataHelper == null)
            {
                Debug.LogError("请先设置dataHelper");
                return;
            }

            dataHelper.SetDefaultSettings();
            onSettingsUpdated?.Invoke(dataHelper.GetData());
        }

        public void SetInt(string key, int value)
        {
            settingSystem.SetInt(key, value);
        }

        public void SetFloat(string key, float value)
        {
            settingSystem.SetFloat(key, value);
        }

        public void SetBool(string key, bool value)
        {
            settingSystem.SetBool(key, value);
        }

        public void SetString(string key, string value)
        {
            settingSystem.SetString(key, value);
        }

        public int GetInt(string key, int defaultValue)
        {
            return settingSystem.GetInt(key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue)
        {
            return settingSystem.GetFloat(key, defaultValue);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return settingSystem.GetBool(key, defaultValue);
        }

        public string GetString(string key, string defaultValue)
        {
            return settingSystem.GetString(key, defaultValue);
        }

        public int GetInt(string key)
        {
            return settingSystem.GetInt(key);
        }

        public float GetFloat(string key)
        {
            return settingSystem.GetFloat(key);
        }

        public bool GetBool(string key)
        {
            return settingSystem.GetBool(key);
        }

        public string GetString(string key)
        {
            return settingSystem.GetString(key);
        }

        public event Action<object> onSettingsUpdated;

        public void SetDataHelper(ISettingDataHelper helper)
        {
            dataHelper = helper;
        }

        public void SetVisionHelper(ISettingManagerVisionHelper helper)
        {
            onSettingsUpdated += helper.UpdateVision;
        }
    }
}