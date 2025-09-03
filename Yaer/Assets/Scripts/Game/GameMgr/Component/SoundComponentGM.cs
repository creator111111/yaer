using Game.GameMgr.Component.Base;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.Static.Path.Sound;
using GameFramework.Sound;
using GameFramework.UnityRuntime.Sound;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class SoundComponentGM : BaseComponentGM
    {
        private SoundComponent soundComponent;
        private SettingsConfigData settingConfig;

        private int CurrentBGMID = -1;
        private int CurrentSFXID = -1;
        Dictionary<string, int> sfxPathDatas = new Dictionary<string, int>(); // 每个音效路径对应当前短时间内播放的音效数量
        float sameSfxPlayDistance = 0.1f; // 同一音效播放时间间隔
        Dictionary<string, float> sfxTimeCountData = new Dictionary<string, float>(); // 每种音效的播放计时器
        bool canCountSfxTime; // 是否开始计时音效播放时间

        public bool IsPlayingBGM => CurrentBGMID > 0;

        public Action<string, float> OnChangeAudioVolume;
        public override void OnInit()
        {
            base.OnInit();
            soundComponent = GameManager.GetGFComponent<SoundComponent>();
            settingConfig = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (canCountSfxTime)
            {
                var timeCountEndNum = 0;
                var newDict = new Dictionary<string, float>(sfxTimeCountData);
                foreach (var resName in newDict.Keys)
                {
                    if (sfxTimeCountData[resName] <= 0) {
                        timeCountEndNum++;
                        continue; 
                    }
                    sfxTimeCountData[resName] -= Time.deltaTime;
                    if (sfxTimeCountData[resName] <= 0)
                    {
                        sfxTimeCountData[resName] = 0;
                        sfxPathDatas[resName] = 0;// 冷却时间到了则清空播放列表
                        timeCountEndNum++;
                    }
                }
                if (timeCountEndNum >= sfxTimeCountData.Count)
                {
                    canCountSfxTime = false;
                }
            }
            
        }

        public int PlaySound(SoundType soundType, string resName, bool loop, float fadeOutTime=0.7f, float fadeInTime = 0, float volume=-1)
        {
            if (soundType == SoundType.SFX)
            {
                if (sfxPathDatas.ContainsKey(resName) && sfxPathDatas[resName] > 0)
                {
                    return -1;// 同一种音效短时间内只能播放一个
                }
            }
            string path = SoundPath.GetSoundPath(soundType, resName);
            if (soundType == SoundType.BGM)
            {
                StopBGM(fadeOutTime);
            }

            if (volume < 0) { volume = settingConfig.allVolume; }
            switch (soundType)
            {
                case SoundType.BGM:
                    volume *= settingConfig.bgmVolume;
                    break;
                case SoundType.SFX:
                    if (!sfxPathDatas.ContainsKey(resName)) { sfxPathDatas[resName] = 1; }
                    else { sfxPathDatas[resName] += 1; }
                    sfxTimeCountData[resName] = sameSfxPlayDistance;
                    volume *= settingConfig.soundVolume * 2;
                    canCountSfxTime = true;
                    break;
            }

            var soundParam = new PlaySoundParams();
            soundParam.Loop = loop;
            soundParam.VolumeInSoundGroup = volume;
            soundParam.FadeInSeconds = fadeInTime;
            int soundID = soundComponent.PlaySound(path, soundType.ToString(), soundParam);
            if (soundType == SoundType.BGM)
            {
                CurrentBGMID = soundID;
            }else if (soundType == SoundType.SFX)
            {
                CurrentSFXID = soundID;
            }
            return soundID;
        }

        public void StopBGM(float fadeOutTime = 0.7f)
        {
            if (IsPlayingBGM)
            {
                StopSound(CurrentBGMID, fadeOutTime);
                CurrentBGMID = -1;
            }
        }

        public void StopSound(int id, float fadeOutTime = 0.7f)
        {
            GameManager.GetGFComponent<SoundComponent>().StopSound(id, fadeOutTime);
        }

        public void PauseSound(int id, float fadeOutTime = 0.7f)
        {
            GameManager.GetGFComponent<SoundComponent>().PauseSound(id, fadeOutTime);
        }

        public void ResumeSound(int id, float fadeOutTime = 0.7f)
        {
            GameManager.GetGFComponent<SoundComponent>().ResumeSound(id, fadeOutTime);
        }

    }
}
