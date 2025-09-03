using Cysharp.Threading.Tasks;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.Static.Path.Sound;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VolumeChangedListener : MonoBehaviour
{
    [SerializeField]
    private SoundType soundType;
    
    private AudioSource audioSource;
    private SettingManager settingManager;
    private SoundComponentGM soundComponent;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private async UniTask InitVolume()
    {
        while (true) 
        {
            if (settingManager != null) break;
            await UniTask.Yield();
            settingManager = GameManager.GetManager<SettingManager>();
            if (settingManager != null)
            {
                settingManager.OnVolumeChange += UpdateVolume;
            }
        }
        var data = settingManager.LoadSetting<SettingsConfigData>();
        var args = new VolumeChangedArgs(data.allVolume, data.bgmVolume, data.soundVolume);
        UpdateVolume(args);

        while (true)
        {
            if (soundComponent != null) break ;
            await UniTask.Yield();
            soundComponent = GameManager.GetGMComponent<SoundComponentGM>();
            soundComponent.OnChangeAudioVolume += ChangeSoundVolume;
        }
    }

    private void UpdateVolume(VolumeChangedArgs volumeSetting)
    {
        float volume = volumeSetting.MainVolume;
        switch (soundType)
        {
            case SoundType.BGM:
                volume *= volumeSetting.BGMVolume;
                break;
            case SoundType.SFX:
                volume *= volumeSetting.SoundVolume * 2; // 音效默认还要在放大一倍
                break;
        }
        audioSource.volume = volume;
    }

    public void SetSoundType(SoundType soundType)
    {
        this.soundType = soundType;
        InitVolume().Forget();
    }

    void ChangeSoundVolume(string clipName, float newVolume)
    {
        if (audioSource == null || audioSource.clip == null) { return; }
        if (audioSource.clip.name == clipName)
        {
            audioSource.volume = newVolume;
        }
    }
}
