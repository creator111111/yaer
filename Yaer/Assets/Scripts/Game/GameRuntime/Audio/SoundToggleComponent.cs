using Game.Static.Path.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class SoundToggleComponent : MonoBehaviour
    {
        [SerializeField]
        private SoundType soundType;
        [SerializeField]
        private string SoundResName;
        [SerializeField]
        private bool Loop;

        public bool isAutoPlay = true; // 是否自动播放

        private int soundID = -1;
        public SoundType GetSoundType => soundType;
        public string GetSoundResName() {  return SoundResName; }

        public void SetSoundType(SoundType soundType) { this.soundType = soundType;}
        public void SetIsLoop(bool isLoop) { Loop =  isLoop; }

        private void OnEnable()
        {
            if (!isAutoPlay) { return; }
            soundID = GameManager.GetGMComponent<SoundComponentGM>()
                .PlaySound(soundType, SoundResName, Loop);
        }

        private void OnDisable()
        {
            if (soundID > 0) 
            {
                GameManager.GetGMComponent<SoundComponentGM>().StopSound(soundID);
                soundID = -1;
            }
        }

        public void ChangeSoundRes(string newName)
        {
            if (SoundResName == newName) { return; }
            SoundResName = newName;
        }

        public int GetSoundId() { return soundID; }

        public void PlaySound(float fadeOutTime = 0.7f, float fadeInTime = 0, float volume = -1)
        {
            soundID = GameManager.GetGMComponent<SoundComponentGM>()
                .PlaySound(soundType, SoundResName, Loop, fadeOutTime, fadeInTime, volume);
        }
        public void StopSound()
        {
            if (soundID > 0)
            {
                GameManager.GetGMComponent<SoundComponentGM>().StopSound(soundID);
                soundID = -1;
            }
        }

        public void PauseSound()
        {
            if (soundID > 0)
            {
                GameManager.GetGMComponent<SoundComponentGM>().PauseSound(soundID);
            }
        }
        public void ResumeSound()
        {
            if (soundID > 0)
            {
                GameManager.GetGMComponent<SoundComponentGM>().ResumeSound(soundID);
            }
        }
    }
}