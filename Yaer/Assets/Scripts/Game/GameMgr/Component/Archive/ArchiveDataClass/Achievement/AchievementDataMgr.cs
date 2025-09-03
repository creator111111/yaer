using Game.DataTable.AchievenmentConfig;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Settings;
using Game.Static.Name.Settings;
using Game.Static.Path;
using GameFramework.DataTable;
using GameFramework.UnityRuntime.Setting;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum AchievementType
{
    KillSlime_1 = 1, // 击杀史莱姆
    KillSlime_2, // 击杀史莱姆
    FirstOutfitChange, // 首次换装
    WoodWormKill_1, // 击杀幼虫
    WoodWormKill_2, // 击杀幼虫
    WormHomeKill_1, // 击杀野外的虫巢和虫卵
    WormHomeKill_2,
    SaveGoblin, // 救下哥布林
    FindOneSecret, //触发彩蛋对话
}

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class AchievementDataMgr: ISettingManager
    {
        [HideInInspector]
        public Dictionary<AchievementType, bool> achievementTagData = new Dictionary<AchievementType, bool>();// 成就数据，ID:成就是否完成

        string achievenBaseName = "Achievement_{0}";
        //AchievementData achievementData { get; set; }
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
        public Action onLoadConfigCallFunc { get; set; } // 数据加载完成之后的回调方法
        public static AchievementDataMgr instance = null;
        public static AchievementDataMgr getInstance()
        {
            if (instance == null)
            {
                instance = new AchievementDataMgr();
            }
            return instance;
        }
        public static IDataTable<AchievenmentDataTableRow> table;

        #region 初始化

        public void Init()
        {
            if (table != null) { return; }
            var resComponent = GameManager.GetGMComponent<ResComponentGM>();
            resComponent.LoadConfig<AchievenmentDataTableRow>("Assets/GameRes/Config/AchievementConfig/AchievementConfig.json", (rows) =>
            {
                table = rows;
                LoadAchievementData();
            });
        }

        #endregion

        public AchievementData GetAchievementData()
        {
            return GameManager.GetGameSceneManager().GetArchiveData<AchievementData>();
            //if (achievementData == null)
            //{
            //    var sceneMgr = GameManager.GetGameSceneManager();
            //    if (sceneMgr == null) { return null; }
            //    achievementData = GameManager.GetGameSceneManager().GetArchiveData<AchievementData>();
            //}
            //return achievementData;
        }


        public void RecordAchievementProgress(AchievementType achieveId, int changeValue)
        {
            if (CheckAchievementHasComplete(achieveId)) { return; }// 已经完成的成就就不处理了

            if (GetAchievementData().achievementProData.ContainsKey(achieveId))
            {
                GetAchievementData().achievementProData[achieveId] += changeValue;
            }
            else
            {
                GetAchievementData().achievementProData[achieveId] = changeValue;
            }
            // 目标值变化后再次检测成就进度是否达标
            if (CheckAchievementHasComplete(achieveId, true))
            {
                // 达成成就
                ShowAchievementTips(achieveId);
                // 保存成就完成情况
                SaveAchievementData();
            }
            SaveAchievementProgress();// 实时保存成就进度至当前存档
        }

        public bool CheckAchievementHasComplete(AchievementType achieveId, bool hasValueChange=false)
        {
            if (hasValueChange)
            {
                // 数值变化时需要计算一次成就进度是否达标
                var curValue = GetAchievementProValue(achieveId);
                var targetValue = GetAchievementTargetValue(achieveId);
                var hasFinsh = curValue >= targetValue;
                GetAchievementData().achievementProData[achieveId] = Math.Min(curValue, targetValue);
                achievementTagData[achieveId] = hasFinsh;
                return hasFinsh;
            }
            else
            {
                if (!achievementTagData.ContainsKey(achieveId)) { achievementTagData[achieveId] = false; }
                return achievementTagData[achieveId];
            }
        }

        // 显示获得某个成就
        public void ShowAchievementTips(AchievementType achieveId)
        {
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("AchievementTipsPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
            if (uiForm == null)
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
                {
                    userData = achieveId,
                });
            }
        }
        
        // 获取某个成就的名称
        public string GetAchievementName(AchievementType achieveId)
        {
            var data = table.GetDataRow(condition: row => row.id == (int)achieveId);
            var languageType = GameManager.Instance.language;
            switch (languageType)
            {
                case LanguageType.Chinese: return data.name;
                case LanguageType.English: return data.name_en;
                case LanguageType.Japanese: return data.name_jp;
                default:
                    return "";
            }
        }

        // 获取某个成就的提示
        public string GetAchievementTips(AchievementType achieveId)
        {
            var data = table.GetDataRow(condition: row => row.id == (int)achieveId);
            var languageType = GameManager.Instance.language;
            switch (languageType)
            {
                case LanguageType.Chinese: return data.condition;
                case LanguageType.English: return data.condition_en;
                case LanguageType.Japanese: return data.condition_jp;
                default:
                    return "";
            }
        }

        // 获取某个成就的描述
        public string GetAchievementDesc(AchievementType achieveId)
        {
            var data = table.GetDataRow(condition: row => row.id == (int)achieveId);
            var languageType = GameManager.Instance.language;
            switch (languageType)
            {
                case LanguageType.Chinese: return data.desc;
                case LanguageType.English: return data.desc_en;
                case LanguageType.Japanese: return data.desc_jp;
                default:
                    return "";
            }
        }

        // 获取某个成就的目标值
        public int GetAchievementTargetValue(AchievementType achieveId)
        {
            var data = table.GetDataRow(condition: row => row.id == (int)achieveId);
            return data.value;
        }

        // 获取成就对应的图片tag
        public int GetAchievementYaerTag(AchievementType achieveId)
        {
            var data = table.GetDataRow(condition: row => row.id == (int)achieveId);
            return data.tag;
        }

        // 获取成就数量
        public int GetAchievementCount()
        {
            return table.Count;
        }

        // 获取某个成就当前的进度
        public int GetAchievementProValue(AchievementType achieveId)
        {
            if (GetAchievementData().achievementProData.ContainsKey(achieveId))
            {
                return GetAchievementData().achievementProData[achieveId];
            }
            else
            {
                return 0;
            }
        }

        // 保存当前成就进度
        public void SaveAchievementProgress()
        {
            var archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archiveComponentGM != null)
            {
                archiveComponentGM.SaveSpcData<AchievementData>();
            }
        }

        // 保存当前所有成就完成是否数据
        public void SaveAchievementData()
        {
            for (AchievementType i = AchievementType.KillSlime_1; (int)i <= table.Count; i++)
            {
                var hasFinsh = achievementTagData.ContainsKey(i) ? achievementTagData[i] : false;
                var realKeyName = string.Format(achievenBaseName, i);
                SetBool(realKeyName, hasFinsh);
            }
        }

        // 读取当前所有成就数据
        public void LoadAchievementData()
        {
            for (AchievementType i = AchievementType.KillSlime_1; (int)i <= table.Count; i++)
            {
                var realKeyName = string.Format(achievenBaseName, i);
                achievementTagData[i] = GetBool(realKeyName, false);
            }
        }

        // ==================testFunction
        public void ResetAllAchievementData()
        {
            // 清空当前成就进度
            GetAchievementData().achievementProData.Clear();
            SaveAchievementProgress();
            // 设置所有成就为未完成状态
            achievementTagData.Clear();
            SaveAchievementData();
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

        public void SaveSetting(object data)
        {
        }

        public T LoadSetting<T>() where T : class
        {
            return default;
        }

        public void SetDefault()
        {

        }
    }
}