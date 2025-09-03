using System;
using System.Collections.Generic;
using GameFramework.UnityRuntime.Base;
using GameFramework.UnityRuntimeExtend.Base;
using UnityEngine;

namespace GameFramework.CoreExtend.Systems.Setting
{
    public class SettingComponent : GameFrameworkComponent, ISettingSystem
    {
        private ISettingSerializerHelper serializerHelper;

        /// <summary>获取游戏配置项数量。</summary>
        public int Count
        {
            get
            {
                if (serializerHelper == null)
                {
                    Debug.LogError("Setting helper is invalid.");
                    return default; // 返回默认值
                }

                return serializerHelper.Count;
            }
        }

        /// <summary>初始化游戏配置管理器的新实例。</summary>
        public SettingComponent() => serializerHelper = null;

        // --------------------------------------------------------------------------------


        /// <summary>设置游戏配置辅助器。</summary>
        /// <param name="settingSerializerHelper">游戏配置辅助器。</param>
        public void SetSettingHelper(ISettingSerializerHelper settingSerializerHelper)
        {
            if (settingSerializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            serializerHelper = settingSerializerHelper;
        }

        /// <summary>加载游戏配置。</summary>
        /// <returns>是否加载游戏配置成功。</returns>
        public bool Load()
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 返回默认失败状态
            }

            return serializerHelper.Load();
        }

        /// <summary>保存游戏配置。</summary>
        /// <returns>是否保存游戏配置成功。</returns>
        public bool Save()
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 返回默认失败状态
            }

            return serializerHelper.Save();
        }

        /// <summary>获取所有游戏配置项的名称。</summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public string[] GetAllSettingNames()
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 返回空数组
            }

            return serializerHelper.GetAllSettingNames();
        }

        /// <summary>获取所有游戏配置项的名称。</summary>
        /// <param name="results">所有游戏配置项的名称。</param>
        public void GetAllSettingNames(List<string> results)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            serializerHelper.GetAllSettingNames(results);
        }

        /// <summary>检查是否存在指定游戏配置项。</summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public bool HasSetting(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 默认返回 false
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default; // 默认返回 false
            }

            return serializerHelper.HasSetting(settingName);
        }

        /// <summary>移除指定游戏配置项。</summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public bool RemoveSetting(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 默认返回 false
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default; // 默认返回 false
            }

            return serializerHelper.RemoveSetting(settingName);
        }

        /// <summary>清空所有游戏配置项。</summary>
        public void RemoveAllSettings()
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return; // 直接返回
            }

            serializerHelper.RemoveAllSettings();
        }

        /// <summary>从指定游戏配置项中读取布尔值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 默认返回 false
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default; // 默认返回 false
            }

            return serializerHelper.GetBool(settingName);
        }

        /// <summary>从指定游戏配置项中读取布尔值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName, bool defaultValue)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetBool(settingName, defaultValue);
        }

        /// <summary>向指定游戏配置项写入布尔值。</summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的布尔值。</param>
        public void SetBool(string settingName, bool value)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return;
            }

            serializerHelper.SetBool(settingName, value);
        }

        /// <summary>从指定游戏配置项中读取整数值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default; // 返回默认值
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default; // 返回默认值
            }

            return serializerHelper.GetInt(settingName);
        }

        /// <summary>从指定游戏配置项中读取整数值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName, int defaultValue)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
                Debug.LogError("Setting name is invalid.");
            return serializerHelper.GetInt(settingName, defaultValue);
        }

        /// <summary>向指定游戏配置项写入整数值。</summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的整数值。</param>
        public void SetInt(string settingName, int value)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return;
            }

            serializerHelper.SetInt(settingName, value);
        }

        /// <summary>从指定游戏配置项中读取浮点数值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return 0f; // 返回默认浮点数值
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return 0f; // 返回默认浮点数值
            }

            return serializerHelper.GetFloat(settingName);
        }

        /// <summary>从指定游戏配置项中读取浮点数值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName, float defaultValue)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetFloat(settingName, defaultValue);
        }

        /// <summary>向指定游戏配置项写入浮点数值。</summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的浮点数值。</param>
        public void SetFloat(string settingName, float value)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return;
            }

            serializerHelper.SetFloat(settingName, value);
        }

        /// <summary>从指定游戏配置项中读取字符串值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetString(settingName);
        }

        /// <summary>从指定游戏配置项中读取字符串值。</summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName, string defaultValue)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetString(settingName, defaultValue);
        }

        /// <summary>向指定游戏配置项写入字符串值。</summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的字符串值。</param>
        public void SetString(string settingName, string value)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return;
            }

            serializerHelper.SetString(settingName, value);
        }

        /// <summary>从指定游戏配置项中读取对象。</summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            return !string.IsNullOrEmpty(settingName) ? serializerHelper.GetObject<T>(settingName) : default;
        }

        /// <summary>从指定游戏配置项中读取对象。</summary>
        /// <param name="objectType">要读取对象的类型。</param>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public object GetObject(Type objectType, string settingName)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (objectType == null)
            {
                Debug.LogError("Object type is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetObject(objectType, settingName);
        }

        /// <summary>从指定游戏配置项中读取对象。</summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName, T defaultObj)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetObject(settingName, defaultObj);
        }

        /// <summary>从指定游戏配置项中读取对象。</summary>
        /// <param name="objectType">要读取对象的类型。</param>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return default;
            }

            if (objectType == null)
            {
                Debug.LogError("Object type is invalid.");
                return default;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return default;
            }

            return serializerHelper.GetObject(objectType, settingName, defaultObj);
        }

        /// <summary>向指定游戏配置项写入对象。</summary>
        /// <typeparam name="T">要写入对象的类型。</typeparam>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject<T>(string settingName, T obj)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return;
            }

            serializerHelper.SetObject(settingName, obj);
        }

        /// <summary>向指定游戏配置项写入对象。</summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject(string settingName, object obj)
        {
            if (serializerHelper == null)
            {
                Debug.LogError("Setting helper is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(settingName))
            {
                Debug.LogError("Setting name is invalid.");
                return;
            }

            serializerHelper.SetObject(settingName, obj);
        }
    }
}