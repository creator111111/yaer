using System.Collections;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component.Story;
using UnityEngine;

/// <summary>
/// 在指定剧情名触发后订阅一次 <see cref="StoryComponentGSM.onStoryEnd"/>，结束时生成/激活物体。
/// <para>
/// 重要：<see cref="GameManager.GetGameSceneManager"/> 在场景 <c>OnGameSceneManagerReady</c> 之前一直为 null，
/// 若仅在 OnEnable/Start 里取 GSM，会从未订阅到 <c>onStoryTriggered</c>（不是事件「假」，而是根本没绑上）。
/// 本脚本通过 <c>onGameSceneManagerReady</c> + 协程兜底确保绑定到真正的 <see cref="StoryComponentGSM"/>。
/// </para>
/// <para>
/// 不要在 <see cref="OnDisable"/> 里取消 <c>onStoryEnd</c>，否则剧情播放中物体被 SetActive(false) 会误删结束回调。
/// </para>
/// <para>
/// Boss 战前「保存提示」若已用 <see cref="Game.GameRuntime.Story.DialoguePreBossSaveTipGate"/> + <see cref="Game.GameRuntime.Story.DialoguePreBossSaveTipSettings"/> 在对话中途弹出 SystemTipsPanel2，
/// 请勿再在本脚本里于剧情结束时激活同一面板，否则会重复弹出。
/// </para>
/// </summary>
public class StoryEndSpawnOnPrefabName : MonoBehaviour
{
    [SerializeField] private string targetStoryName = "WestRappRoadGoblinAndGusha";
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject sceneObjectToActivate;

    [Header("调试")]
    [Tooltip("勾选后在 Console 输出绑定 GSM、onStoryTriggered、onStoryEnd 相关信息。")]
    [SerializeField] private bool debugLog = true;

    private StoryComponentGSM gsm;
    private bool pendingEnd;
    private bool subscribedToSceneManagerReady;
    private bool storyTriggeredHookRegistered;

    private void OnEnable()
    {
        TryBindGsmAndSubscribe();
    }

    private void Start()
    {
        StartCoroutine(CoRetryBindUntilGsmReady());
    }

    /// <summary>
    /// 若 OnEnable 时场景管理器尚未注册，协程每帧重试，避免错过一次性事件后永远无法绑定。
    /// </summary>
    private IEnumerator CoRetryBindUntilGsmReady()
    {
        var maxFrames = 600;
        var frames = 0;
        while (gsm == null && frames < maxFrames)
        {
            TryBindGsmAndSubscribe();
            if (gsm != null)
            {
                yield break;
            }

            frames++;
            yield return null;
        }

        if (gsm == null && debugLog)
        {
            Debug.LogError(
                "[StoryEndSpawnOnPrefabName] 超时仍未拿到 StoryComponentGSM（GetGameSceneManager 一直为 null？）。请确认已进入游玩场景且场景管理器已初始化。物体=" +
                gameObject.name,
                this);
        }
    }

    /// <summary>获取当前场景的 StoryComponentGSM 并订阅 onStoryTriggered；若管理器未就绪则挂到 onGameSceneManagerReady。</summary>
    private void TryBindGsmAndSubscribe()
    {
        gsm = GameManager.GetGameSceneManager()?.GetModule<StoryComponentGSM>();
        if (gsm != null)
        {
            UnsubscribeSceneManagerReady();
            if (!storyTriggeredHookRegistered)
            {
                RegisterStoryTriggered();
                storyTriggeredHookRegistered = true;
                if (debugLog)
                {
                    Debug.Log(
                        "[StoryEndSpawnOnPrefabName] 已绑定 StoryComponentGSM 并订阅 onStoryTriggered | " + gameObject.name,
                        this);
                }
            }

            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (!subscribedToSceneManagerReady)
        {
            GameManager.Instance.onGameSceneManagerReady += OnGameSceneManagerReady;
            subscribedToSceneManagerReady = true;
            if (debugLog)
            {
                Debug.LogWarning(
                    "[StoryEndSpawnOnPrefabName] GetGameSceneManager 为空，已订阅 onGameSceneManagerReady，就绪后自动绑定。物体=" +
                    gameObject.name,
                    this);
            }
        }
    }

    private void OnGameSceneManagerReady(IGameSceneManager _)
    {
        UnsubscribeSceneManagerReady();
        TryBindGsmAndSubscribe();
    }

    private void UnsubscribeSceneManagerReady()
    {
        if (!subscribedToSceneManagerReady || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.onGameSceneManagerReady -= OnGameSceneManagerReady;
        subscribedToSceneManagerReady = false;
    }

    private void RegisterStoryTriggered()
    {
        gsm.onStoryTriggered -= OnStoryTriggered;
        gsm.onStoryTriggered += OnStoryTriggered;
    }

    private void OnDisable()
    {
        UnsubscribeSceneManagerReady();
        storyTriggeredHookRegistered = false;
        if (gsm == null)
        {
            return;
        }

        gsm.onStoryTriggered -= OnStoryTriggered;
    }

    private void OnDestroy()
    {
        UnsubscribeSceneManagerReady();
        if (gsm == null)
        {
            return;
        }

        gsm.onStoryTriggered -= OnStoryTriggered;
        gsm.onStoryEnd -= OnStoryEndOnce;
    }

    private void OnStoryTriggered()
    {
        if (gsm == null || pendingEnd)
        {
            return;
        }

        var current = gsm.CurrentRunningStoryName;
        if (current != targetStoryName)
        {
            if (debugLog)
            {
                Debug.Log(
                    $"[StoryEndSpawnOnPrefabName] onStoryTriggered 名字不匹配 | current={current} | target={targetStoryName}",
                    this);
            }

            return;
        }

        pendingEnd = true;
        gsm.onStoryEnd += OnStoryEndOnce;
        if (debugLog)
        {
            Debug.LogError(
                "[StoryEndSpawnOnPrefabName] 已订阅 onStoryEnd，等待剧情结束 | target=" + targetStoryName,
                this);
        }
    }

    private void OnStoryEndOnce()
    {
        gsm.onStoryEnd -= OnStoryEndOnce;
        pendingEnd = false;

        Debug.LogError(
            $"<color=lime>[StoryEndSpawnOnPrefabName] OnStoryEndOnce 已执行</color> | story={targetStoryName} | go={gameObject.name}",
            this);

        if (prefabToSpawn != null)
        {
            var parent = spawnPoint != null ? spawnPoint : null;
            Instantiate(prefabToSpawn, parent != null ? parent.position : transform.position, Quaternion.identity, parent);
        }

        if (sceneObjectToActivate != null)
        {
            sceneObjectToActivate.SetActive(true);
        }
    }
}
