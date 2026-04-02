using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.ForestSceneFirstEnter
{
    public class ForestSceneLaiFlyStory : BaseSceneEntityLogic
    {
        [SerializeField] private GameObject king;
        [SerializeField] private GameObject lai;

        [SerializeField] private GameObject NormalLai;
        [SerializeField] private Animator soldierTurnAnimator;
        public SoundToggleComponent soundSfxCpn;
        public SoundToggleComponent kingSoundSfxCpn;

        public AnimationEventComponent kingShowAniEventCpn;
        public AnimationEventComponent LaiFlyAniEventCpn;
        int audioIndex;

        [Tooltip("士兵先开始回头，再过此秒数再激活国王/莱伊。默认 1.5 = 原 0.5 基础上再提前约 1 秒间隔；国王行走仍由 OnSoldierTurnEnd→PlayKingShowAnim")]
        [SerializeField] private float soldierTurnLeadBeforeKingSeconds = 1.5f;

        private bool soldierTurnTriggered;
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
            StartCoroutine(CoPreparePlayAfterSoldierLead());
        }

        /// <summary>
        /// 对话图里若仍绑定旧节点可保留空实现；士兵回头已并入 <see cref="PreparePlay"/>。
        /// </summary>
        public void ShowKing()
        {
        }

        /// <summary>
        /// 与 <see cref="StartSoldierTurn"/> 相同。若要在「PreparePlay 调用时刻」不变的前提下让士兵再早约 1 秒，
        /// 请在对话图中于 PreparePlay 之前约 1 秒单独调用本方法（或 StartSoldierTurn），PreparePlay 内将只负责国王/莱伊激活与延迟。
        /// </summary>
        public void BeginSoldierTurnEarly()
        {
            StartSoldierTurn();
        }

        private IEnumerator CoPreparePlayAfterSoldierLead()
        {
            if (soldierTurnAnimator == null)
            {
                king.SetActive(true);
                lai.SetActive(true);
                NormalLai.SetActive(false);
                PlayKingShowAnim();
                yield break;
            }

            // 若对话已提前调用了 BeginSoldierTurnEarly/StartSoldierTurn，此处不再重复触发
            if (!soldierTurnTriggered)
            {
                StartSoldierTurn();
            }

            yield return new WaitForSeconds(soldierTurnLeadBeforeKingSeconds);

            king.SetActive(true);
            lai.SetActive(true);
            NormalLai.SetActive(false);
        }

        public void LaiFly()
        {
            lai.GetComponent<Animator>().Play("LaiFly");
        }

        public void StartSoldierTurn()
        {
            if (soldierTurnTriggered)
            {
                return;
            }

            if (soldierTurnAnimator == null)
            {
                PlayKingShowAnim();
                return;
            }

            soldierTurnTriggered = true;
            soldierTurnAnimator.SetTrigger("Turn");
        }

        public void OnSoldierTurnEnd()
        {
            PlayKingShowAnim();
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

        void PlayKingShowAnim()
        {
            if (king != null && !king.activeSelf)
            {
                king.SetActive(true);
            }

            // king移动
            king.GetComponent<Animator>().Play("ShowKing");
        }
    }
}