using System;
using System.Collections.Generic;
using Game.GameMgr.Manager.Settings.interf;
using Game.Static.Enum;
using UnityEngine;

namespace Game.GameMgr.Manager.Settings.Helper
{
    public class SettingsDataHelper : ISettingDataHelper
    {
        private readonly ISettingManager manager;
        private SettingsConfigData data;
        private readonly Dictionary<string, string> infoDic; // 临时信息
        
        // EasySave保存按键配置的键名
        private const string KEY_BINDING_SAVE_KEY = "KeyBindingConfig";

        public SettingsDataHelper(ISettingManager manager)
        {
            this.manager = manager;
            infoDic = new Dictionary<string, string>();
        }

        // 保存设置数据
        public object GetData()
        {
            return data;
        }

        public void SaveSettings(object source)
        {
            if (source is SettingsConfigData sdata)
            {
                data.windowMode = sdata.windowMode;
                data.resolvingPower = sdata.resolvingPower;
                data.allVolume = sdata.allVolume;
                data.bgmVolume = sdata.bgmVolume;
                data.soundVolume = sdata.soundVolume;
                data.textSpeed = sdata.textSpeed;
                data.autoPlaySpeed = sdata.autoPlaySpeed;
                data.showBattleImage = sdata.showBattleImage;
                data.showWound = sdata.showWound;                
                data.KeyboardMouseInputConfig = sdata.KeyboardMouseInputConfig;

                manager.SetString("WindowMode", data.windowMode.ToString());
                manager.SetString("ResolvingPower", data.resolvingPower.ToString());
                manager.SetFloat("AllVolume", data.allVolume);
                manager.SetFloat("BgmVolume", data.bgmVolume);
                manager.SetFloat("SoundVolume", data.soundVolume);
                manager.SetFloat("TextSpeed", data.textSpeed);
                manager.SetFloat("AutoPlaySpeed", data.autoPlaySpeed);
                manager.SetBool("ShowBattleImage", data.showBattleImage);
                manager.SetBool("ShowWound", data.showWound);
                
                //保存按键绑定配置
                SaveKeyBinding(data.KeyboardMouseInputConfig);

                infoDic.Clear();
                AddInfo("WindowMode", data.windowMode.ToString());
                AddInfo("ResolvingPower", data.resolvingPower.ToString());
                AddInfo("AllVolume", data.allVolume.ToString());
                AddInfo("BgmVolume", data.bgmVolume.ToString());
                AddInfo("SoundVolume", data.soundVolume.ToString());
                AddInfo("TextSpeed", data.textSpeed.ToString());
                AddInfo("AutoPlaySpeed", data.autoPlaySpeed.ToString());
                AddInfo("ShowBattleImage", data.showBattleImage.ToString());
                AddInfo("ShowWound", data.showWound.ToString());
            }
        }

        // 加载设置数据
        public T LoadSettings<T>() where T : class
        {
            if (typeof(T) == typeof(SettingsConfigData))
            {
                if (data == null) data = new SettingsConfigData();
                Enum.TryParse(manager.GetString("WindowMode"), out SettingsConfigData.EWindowMode windowMode);
                Enum.TryParse(manager.GetString("ResolvingPower"), out SettingsConfigData.EResolvingPower resolvingPower);

                data.windowMode = windowMode;
                data.resolvingPower = resolvingPower;
                data.allVolume = manager.GetFloat("AllVolume", data.allVolume);
                data.bgmVolume = manager.GetFloat("BgmVolume", data.bgmVolume);
                data.soundVolume = manager.GetFloat("SoundVolume", data.soundVolume);
                data.textSpeed = manager.GetFloat("TextSpeed", data.textSpeed);
                data.autoPlaySpeed = manager.GetFloat("AutoPlaySpeed", data.autoPlaySpeed);
                data.showBattleImage = manager.GetBool("ShowBattleImage", data.showBattleImage);
                data.showWound = manager.GetBool("ShowWound", data.showWound);
                
                // 加载按键绑定配置
                LoadKeyBinding(data);
                AddInfo("WindowMode", data.windowMode.ToString());
                AddInfo("ResolvingPower", data.resolvingPower.ToString());
                AddInfo("AllVolume", data.allVolume.ToString());
                AddInfo("BgmVolume", data.bgmVolume.ToString());
                AddInfo("SoundVolume", data.soundVolume.ToString());
                AddInfo("TextSpeed", data.textSpeed.ToString());
                AddInfo("AutoPlaySpeed", data.autoPlaySpeed.ToString());
                AddInfo("ShowBattleImage", data.showBattleImage.ToString());
                AddInfo("ShowWound", data.showWound.ToString());

                return data as T;
            }

            Debug.LogError("不支持的类型");
            return null;
        }

        // 返回默认设置
        public T GetDefaultSettings<T>() where T : class
        {
            if (typeof(T) == typeof(SettingsConfigData)) return new SettingsConfigData() as T;

            Debug.LogError("不支持的类型");
            return null;
        }

        public void SetDefaultSettings()
        {
            SaveSettings(GetDefaultSettings<SettingsConfigData>());
        }

        public Dictionary<string, string> UpdateVision()
        {
            return infoDic;
        }

        private void AddInfo(string key, string value)
        {
            infoDic[key] = value;
        }
        
        /// <summary>
        /// 保存按键绑定配置
        /// </summary>
        private void SaveKeyBinding(Dictionary<ControlInputType, KeyCode> keyBindingConfig)
        {
            // 将字典序列化为字符串格式保存
            foreach (var kvp in keyBindingConfig)
            {
                string key = "KeyBinding_" + kvp.Key.ToString();
                manager.SetString(key, kvp.Value.ToString());
            }
        }
        
        /// <summary>
        /// 加载按键绑定配置
        /// </summary>
        private void LoadKeyBinding(SettingsConfigData data)
        {
            // 从保存的字符串中恢复按键绑定配置
            foreach (ControlInputType inputType in System.Enum.GetValues(typeof(ControlInputType)))
            {
                string key = "KeyBinding_" + inputType.ToString();
                string keyCodeString = manager.GetString(key, "");
                
                if (!string.IsNullOrEmpty(keyCodeString))
                {
                    if (System.Enum.TryParse(keyCodeString, out KeyCode keyCode))
                    {
                        data.KeyboardMouseInputConfig[inputType] = keyCode;
                    }
                }
            }
        }
    }
}