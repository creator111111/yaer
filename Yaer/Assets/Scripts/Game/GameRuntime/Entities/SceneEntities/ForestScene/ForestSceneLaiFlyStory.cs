using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Game.GameRuntime.Story.ForestSceneFirstEnter
{
    /// <summary>
    /// 林恩/莱飞行前「国王与士兵」相关演出。
    /// <b>士兵回头</b>仅由场景内 <see cref="PlayableDirector"/> 绑定的 Timeline（Animation/Signal 轨）控制，不在 C# 中 SetTrigger("Turn")。
    /// 仍保留 <see cref="OnSoldierTurnEnd"/> 供 Signal/动画事件衔接国王等。
    /// </summary>
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

        [Tooltip("未使用 Timeline 回退为协程时，等待该秒数后再显隐国王/莱（不驱动士兵，士兵仅由 TimeLine 控制）。")]
        [SerializeField] private float delayBeforeShowKingLaiWhenNoTimeline = 1.5f;

        [Header("国王演出：Timeline 接入")]
        [Tooltip("为 true：PreparePlay 只播放 TimeLine。关闭则只走回退协程（延迟+显隐国王/莱，不播士兵）。")]
        [SerializeField] private bool useKingPerformanceTimeline = true;

        [Tooltip("带 Playable Director，Playable 已绑定 .playable。若开 Timeline 但留空，将回退协程并打警告。")]
        [SerializeField] private PlayableDirector kingPerformanceDirector;

        [Tooltip("整段 TimeLine 自然播放结束时触发。")]
        [SerializeField] private UnityEvent onKingPerformanceTimelineComplete;

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

        private void OnDestroy()
        {
            if (kingPerformanceDirector != null)
            {
                kingPerformanceDirector.stopped -= OnKingPerformanceTimelineStopped;
            }
        }

        /// <summary>对话图入口：优先播 TimeLine；否则仅延迟后显国王/莱（不控制士兵动作）。</summary>
        public void PreparePlay()
        {
            if (useKingPerformanceTimeline)
            {
                if (kingPerformanceDirector != null)
                {
                    PlayKingPerformanceTimeline();
                    return;
                }

                Debug.LogWarning(
                    "[ForestSceneLaiFlyStory] 已开启 useKingPerformanceTimeline 但未指定 kingPerformanceDirector，将回退为协程。请绑定 TimeLine 的 Playable Director。",
                    this);
            }

            StartCoroutine(CoFallbackShowAfterDelayNoSoldierCode());
        }

        private void PlayKingPerformanceTimeline()
        {
            var d = kingPerformanceDirector;
            if (d == null) { return; }

            d.stopped -= OnKingPerformanceTimelineStopped;
            if (d.state == PlayState.Playing) { d.Stop(); }

            d.stopped += OnKingPerformanceTimelineStopped;
            d.time = 0d;
            d.Play();
        }

        private void OnKingPerformanceTimelineStopped(PlayableDirector _)
        {
            if (kingPerformanceDirector != null)
            {
                kingPerformanceDirector.stopped -= OnKingPerformanceTimelineStopped;
            }
            onKingPerformanceTimelineComplete?.Invoke();
        }

        public void ShowKing() { }

        /// <summary>旧对话节点可留空。士兵仅由 TimeLine 控制，此处不做事。</summary>
        public void BeginSoldierTurnEarly() { }

        /// <summary>无 TimeLine/未绑 Director 回退：不播士兵，仅延迟后显国王/莱。</summary>
        private IEnumerator CoFallbackShowAfterDelayNoSoldierCode()
        {
            yield return new WaitForSeconds(delayBeforeShowKingLaiWhenNoTimeline);
            if (king != null) { king.SetActive(true); }
            if (lai != null) { lai.SetActive(true); }
            if (NormalLai != null) { NormalLai.SetActive(false); }
        }

        public void LaiFly()
        {
            lai.GetComponent<Animator>().Play("LaiFly");
        }

        /// <summary>旧节点可留空。士兵仅由 TimeLine 控制，此处不做事。</summary>
        public void StartSoldierTurn() { }

        /// <summary>TimeLine 的 Signal/动画事件：衔接国王显示。</summary>
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
            string baseFilePath = "主角跑步走路音效/{0}";
            var baseName = "土地跑{0}.mp3";
            var audioNum = 10;
            audioIndex++;
            audioIndex = audioIndex > audioNum ? 1 : audioIndex;
            var resName = string.Format(baseName, audioIndex);
            var realResPath = string.Format(baseFilePath, resName);
            kingSoundSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(kingSoundSfxCpn, true);
        }

        void PlayKingShowAnim()
        {
            if (king != null && !king.activeSelf)
            {
                king.SetActive(true);
            }

            king.GetComponent<Animator>().Play("ShowKing");
        }
    }
}
