using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.ForestSceneFirstEnter
{
    public class ForestSceneLaiFlyStory : BaseSceneEntityLogic
    {
        [SerializeField] private GameObject king;
        [SerializeField] private GameObject lai;

        [SerializeField] private GameObject NormalLai;
        public SoundToggleComponent soundSfxCpn;
        public SoundToggleComponent kingSoundSfxCpn;

        public AnimationEventComponent kingShowAniEventCpn;
        public AnimationEventComponent LaiFlyAniEventCpn;
        int audioIndex;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            var sceneData = SceneManager.GetArchiveData<ForestSceneData>();
            if (sceneData.laiFlyAway)
            {
                this.gameObject.SetActive(false);
            }
            kingShowAniEventCpn.RegisterEvent("ShowWalkAudio", ShowWalkAudio);
            LaiFlyAniEventCpn.RegisterEvent("ShowLaiFlyAudio", ShowLaiFlyAudio);
        }

        public void PreparePlay()
        {
            king.SetActive(true);
            lai.SetActive(true);
            NormalLai.SetActive(false);
        }

        public void ShowKing()
        {
            // king移动
            king.GetComponent<Animator>().Play("ShowKing");
        }

        public void LaiFly()
        {
            lai.GetComponent<Animator>().Play("LaiFly");
        }

        void ShowLaiFlyAudio(string arg)
        {
            soundSfxCpn.PlaySound();
        }

        void ShowWalkAudio(string arg)
        {
            // 根据当前场景播放不同类型的音效
            string baseFilePath = "主角跑步走路音效/{0}";
            var baseName = "土地跑{0}.mp3";
            var audioNum = 10;
            audioIndex++;
            audioIndex = audioIndex > audioNum ? 1 : audioIndex;
            var resName = string.Format(baseName, audioIndex);
            var realResPath = string.Format(baseFilePath, resName);
            kingSoundSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(kingSoundSfxCpn, true); // 播放一次走路音效
        }
    }
}