using Game.GameRuntime.Story.NodeCanvasExtend;
using NodeCanvas.DialogueTrees;
using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// DialogDebug 解耦沙盒：Inspector 拖入对话 prefab，直接 Instantiate + <see cref="DialogueTreeController.StartDialogue"/>。
    /// 不经过 <c>StoryComponentGSM</c> / GF 换场；Open 本场景 Play 即可测。
    /// </summary>
    /// <remarks>
    /// 替代方案 B：Editor 下用字符串 + AssetDatabase 加载 — 不如拖引用直观。
    /// 替代方案 C：<c>StoryComponentGSM.TriggerStory</c> — 已否决（见架构文档附录 A）。
    /// </remarks>
    public class DialogDebugPlayground : MonoBehaviour
    {
        [Tooltip("从 Project 拖入 GameRes/Prefabs/Dialogue/*.prefab")]
        [SerializeField] private GameObject dialoguePrefab;

        [Tooltip("对话实例化父节点，通常为 DialogueInstanceRoot")]
        [SerializeField] private Transform dialogueContainer;

        [Tooltip("场景内常驻 DialogueTMPUGUI（来自 NormalDialogueNewPanel 子树）")]
        [SerializeField] private DialogueTMPUGUI dialogueUI;

        [SerializeField] private bool playOnStart = true;

        [SerializeField] private KeyCode replayKey = KeyCode.T;

        [Tooltip("重播前销毁上一实例，避免 Hierarchy 堆积")]
        [SerializeField] private bool destroyPreviousOnReplay = true;

        private DialogueTreeController runningTree;
        private bool isRunning;

        private void Start()
        {
            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueEnd += OnDialogueUIEnd;
            }
            else
            {
                Debug.LogWarning("[DialogDebugPlayground] 未绑定 dialogueUI，字幕/选项可能不显示。");
            }

            if (playOnStart && dialoguePrefab != null)
            {
                PlayDialogue();
            }
        }

        private void OnDestroy()
        {
            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueEnd -= OnDialogueUIEnd;
            }
        }

        private void Update()
        {
            if (replayKey != KeyCode.None && Input.GetKeyDown(replayKey) && !isRunning)
            {
                PlayDialogue();
            }
        }

        [ContextMenu("Play Dialogue")]
        public void PlayDialogue()
        {
            if (dialoguePrefab == null)
            {
                Debug.LogError("[DialogDebugPlayground] 请在 Inspector 拖入 dialoguePrefab。");
                return;
            }

            if (dialogueContainer == null)
            {
                Debug.LogError("[DialogDebugPlayground] 请指定 dialogueContainer（DialogueInstanceRoot）。");
                return;
            }

            if (destroyPreviousOnReplay && runningTree != null)
            {
                Destroy(runningTree.gameObject);
                runningTree = null;
            }

            if (isRunning)
            {
                Debug.LogWarning(
                    "[DialogDebugPlayground] 对话进行中，请等待结束；或开启 destroyPreviousOnReplay 后重试。");
                return;
            }

            // 先实例化，再挂到 DialogUI Canvas 下（与正式管线 DialogueSceneContainer 一致），否则 BG/立绘 UI 不渲染
            var instance = Instantiate(dialoguePrefab);
            AttachToDialogueSceneCanvas(instance);
            runningTree = instance.GetComponentInChildren<DialogueTreeController>(true);
            if (runningTree == null)
            {
                Debug.LogError("[DialogDebugPlayground] prefab 上未找到 DialogueTreeController。");
                Destroy(instance);
                return;
            }

            isRunning = true;
            runningTree.StartDialogue();
            Debug.Log($"[DialogDebugPlayground] 开始播放: {dialoguePrefab.name}");
        }

        private void OnDialogueUIEnd()
        {
            isRunning = false;
            if (destroyPreviousOnReplay && runningTree != null)
            {
                Destroy(runningTree.gameObject);
                runningTree = null;
            }
        }

        /// <summary>
        /// 将对话 prefab 挂到场景 DialogUI 的 Canvas 下，模拟正式游戏中
        /// <c>NormalDialogueNewPanel.DialogueSceneContainer</c> 的层级（背景/立绘在字幕条后面）。
        /// </summary>
        /// <remarks>
        /// 重要修改原因：对话 prefab 根节点只有 RectTransform，不含 Canvas；
        /// 若挂在场景根级空物体（DialogueInstanceRoot）下，Unity UI 不会绘制 BG 与立绘。
        /// 替代方案：给 DialogueInstanceRoot 单独加 Canvas — 会与 DialogUI 双 Canvas 抢排序，故优先复用已有 Canvas。
        /// </remarks>
        private void AttachToDialogueSceneCanvas(GameObject instance)
        {
            var canvas = ResolveDialogueCanvas();
            if (canvas == null)
            {
                Debug.LogWarning(
                    "[DialogDebugPlayground] 未找到 DialogUI Canvas，背景/立绘可能仍不可见。请运行 Tools/Dialogue/Setup DialogDebug Scene。");
                if (dialogueContainer != null)
                {
                    instance.transform.SetParent(dialogueContainer, false);
                }

                return;
            }

            var container = GetOrCreateDialogueSceneContainer(canvas.transform);
            var rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.SetParent(container, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }
            else
            {
                instance.transform.SetParent(container, false);
            }

            // 保证立绘 CanvasGroup 可见（部分图可能在结束时 Fade 过）
            foreach (var group in instance.GetComponentsInChildren<CanvasGroup>(true))
            {
                group.alpha = 1f;
            }
        }

        /// <summary>优先用 Playground 绑定的 dialogueUI 所在 Canvas，与字幕 UI 同屏。</summary>
        private Canvas ResolveDialogueCanvas()
        {
            if (dialogueUI != null)
            {
                var uiCanvas = dialogueUI.GetComponentInParent<Canvas>();
                if (uiCanvas != null)
                {
                    return uiCanvas;
                }
            }

            return FindObjectOfType<Canvas>();
        }

        /// <summary>
        /// 与 NormalDialogueNewPanel 一致：Canvas 下首个全屏子节点，专门承载 Instantiate 的对话 prefab。
        /// </summary>
        private static RectTransform GetOrCreateDialogueSceneContainer(Transform canvasTransform)
        {
            const string containerName = "DialogueSceneContainer";
            var existing = canvasTransform.Find(containerName) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(containerName, typeof(RectTransform));
            var container = go.GetComponent<RectTransform>();
            container.SetParent(canvasTransform, false);
            container.SetAsFirstSibling();
            container.anchorMin = Vector2.zero;
            container.anchorMax = Vector2.one;
            container.offsetMin = Vector2.zero;
            container.offsetMax = Vector2.zero;
            container.localScale = Vector3.one;
            return container;
        }
    }
}
