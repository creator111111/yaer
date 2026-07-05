using System.Collections;
using UnityEngine;

namespace Game.GameRuntime.Diagnostic
{
    /// <summary>
    /// 最小可运行性诊断：挂在任意可激活的 GameObject 上，用 Console 是否出现本脚本日志来判断
    /// 「组件生命周期是否被调用」「场景是否进 Play 且本物体仍启用」等。
    /// </summary>
    /// <remarks>
    /// 若仍无日志，按顺序检查：1) 物体激活 2) 本脚本与 Behaviour 未禁用 3) 是否在进 Play 后实例化 4) Console 是否过滤了 Log。
    /// </remarks>
    [AddComponentMenu("Game/Diagnostic Tool")]
    public class DiagnosticTool : MonoBehaviour
    {
        [Header("只用于识别日志来源，可改短句")]
        [SerializeField] private string label = "DiagnosticTool";

        [Tooltip("为 false 时不会启用心跳协程，只保留 Awake/OnEnable/Start 三行，便于与崩溃点对照。")]
        [SerializeField] private bool runAliveHeartbeat = true;

        private void Awake()
        {
            Debug.Log($"[{label}] Awake 已执行。GameObject: \"{gameObject.name}\"  activeInHierarchy: {gameObject.activeInHierarchy}  activeSelf: {gameObject.activeSelf}  enabled: {enabled}", this);
        }

        private void OnEnable()
        {
            Debug.Log($"[{label}] OnEnable 已执行。 enabled: {enabled}", this);
        }

        private void Start()
        {
            Debug.Log($"[{label}] Start 已执行。", this);
            if (runAliveHeartbeat)
            {
                StartCoroutine(CoAliveHeartbeat());
            }
        }

        private void OnDisable()
        {
            Debug.Log($"[{label}] OnDisable：组件或物体被关闭。", this);
        }

        /// <summary>每隔约 1 秒打印一次，证明 Update 类逻辑之前的生命周期已正常且协程在跑。</summary>
        private IEnumerator CoAliveHeartbeat()
        {
            // 首条心跳略延后 1 秒，避免与 Start 日志挤在同帧难以区分
            yield return new WaitForSeconds(1f);
            while (enabled && gameObject.activeInHierarchy)
            {
                Debug.Log($"[{label}] 脚本存活中  time: {Time.time:F1}s  frame: {Time.frameCount}", this);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
