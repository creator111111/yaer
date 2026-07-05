using System;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component.Story;
using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// DialogDebug 专用：在 Inspector 配置对话 prefab 名，通过按键 / ContextMenu / 进场景可选自动触发
    /// <see cref="StoryComponentGSM.TriggerStory"/>。
    /// </summary>
    /// <remarks>
    /// 替代方案：<c>SimpleStoryTrigger</c> 需登记 SceneEntityComponentGSM，配置更重；本组件可挂任意物体，更适合 Debug 场景。
    /// </remarks>
    [Obsolete("已替换为 DialogDebugPlayground（拖 prefab 引用 + 直接 StartDialogue）。见架构文档 2026-05-25 修订。")]
    public class DialogDebugStoryTester : MonoBehaviour
    {
        [Tooltip("对应 GameRes/Prefabs/Dialogue/{名}.prefab，不含 .prefab 后缀")]
        [SerializeField] private string storyPrefabName = "Village_KenMuNiStar_Test";

        [Tooltip("进场景后 Start 时自动播放一次（需 GSM 已就绪）")]
        [SerializeField] private bool triggerOnEnterScene;

        [Tooltip("Play 模式下按键重播；设为 None 则仅 ContextMenu / UI 按钮触发")]
        [SerializeField] private KeyCode triggerKey = KeyCode.T;

        private StoryComponentGSM storyModule;
        private bool entered;

        private void Start()
        {
            // GSM 在 BaseGameSceneManager Awake/Update 就绪后才有；Start 通常晚于 OnGameSceneManagerReady
            var gsm = GameManager.GetGameSceneManager();
            storyModule = gsm?.GetModule<StoryComponentGSM>();

            if (storyModule == null)
            {
                Debug.LogError(
                    "[DialogDebugStoryTester] StoryComponentGSM 未就绪。请从 InitScene Play 后通过 Tools/Dialogue/Enter DialogDebug Scene 进入。");
                return;
            }

            if (triggerOnEnterScene && !entered)
            {
                entered = true;
                TriggerStory();
            }
        }

        private void Update()
        {
            if (triggerKey == KeyCode.None)
            {
                return;
            }

            if (Input.GetKeyDown(triggerKey))
            {
                TriggerStory();
            }
        }

        [ContextMenu("Trigger Story")]
        public void TriggerStory()
        {
            if (storyModule == null)
            {
                storyModule = GameManager.GetGameSceneManager()?.GetModule<StoryComponentGSM>();
            }

            if (storyModule == null)
            {
                Debug.LogError("[DialogDebugStoryTester] 无法获取 StoryComponentGSM");
                return;
            }

            if (!storyModule.TriggerStory(storyPrefabName))
            {
                Debug.LogWarning(
                    "[DialogDebugStoryTester] TriggerStory 被拒绝（可能上一段对话尚未结束，需等 OnStoryEnd）");
            }
        }
    }
}
