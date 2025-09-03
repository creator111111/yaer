using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.Entities.Player;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.HomeScene2
{
    // 绑定在场景对象上的音乐播放脚本,自动检测玩家距离来播放不同音量的音乐
    public class BaseSoundEntity : MonoBehaviour
    {
        SettingsConfigData settingConfig;
        public PlayerLogic playerLogic;
        public SoundToggleComponent soundCpn;// 音乐组件
        public bool openGizmos;
        public string detectorName;
        public float radius;

        float baseVolume; // 基础音量大小
        float curVolume; // 当前音量大小
        SoundComponentGM soundComponentGM;
        string sfxResName;
        private void Start()
        {
            playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            settingConfig = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
            soundComponentGM = GameManager.GetGMComponent<SoundComponentGM>();
            baseVolume = settingConfig.allVolume;
            baseVolume *= settingConfig.soundVolume * 2; // 基础音效大小
            curVolume = baseVolume;
            var realResName = soundCpn.GetSoundResName();
            var resStrs = realResName.Split('.');
            sfxResName = resStrs.Length > 0 ? resStrs[0] : "";
        }
        private void Update()
        {
            if (playerLogic == null) {
                playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
                return; 
            }
            var distance = Vector2.Distance(playerLogic.gameObject.transform.position, transform.position);
            if (distance <= radius)
            {
                // 激活音乐
                PlayBgmWithDistance(distance);
            }
            else
            {
                soundCpn.StopSound();
            }
        }

        void PlayBgmWithDistance(float distance)
        {
            var distanceRate = distance / radius;
            var volumeChangeRate = Math.Max(0.1f, 1 - distanceRate);
            var volume = curVolume * volumeChangeRate;
            if (soundCpn.GetSoundId() > 0)
            {
                if (sfxResName != "")
                {
                    soundComponentGM.OnChangeAudioVolume(sfxResName, volume);
                }
                return;
            }
            soundCpn.PlaySound(0.7f, 0, volume);
        }

        // 调整音量通过百分比
        public void ChangeVolumeByRate(float volumeRate)
        {
            // 设置音量逐渐变化到baseVolume * volumeRate的大小
            DOTween.To(
                () => curVolume,
                (x) => { curVolume = x; },
                baseVolume * volumeRate,
                1f
            )
            .SetEase(Ease.InOutQuad);
        }

        public void ResetCurVolume()
        {
            ChangeVolumeByRate(1);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(detectorName))
            {
                detectorName = gameObject.name;
            }
        }

        private void OnDrawGizmos()
        {
            if (openGizmos == false)
            {
                return;
            }

            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}