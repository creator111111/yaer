using Game.GameMgr;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// DialogDebug 专用：保证 <see cref="GameManager.Instance"/> 存在且 <c>language</c> 有默认值，
    /// 供 <see cref="NodeCanvasExtend.DialogueTMPUGUI"/> 选中/英/日字幕。不调用 <see cref="GameManager.OnInit"/>，不注册 GF 组件。
    /// </summary>
    /// <remarks>
    /// 替代方案：场景内手动摆带 GameManager 的空物体 — 与本组件等价；首版用 Bootstrap 减少漏配。
    /// 不使用 DontDestroyOnLoad，避免污染其它 Open Scene 测试（见架构文档 §16）。
    /// </remarks>
    public class DialogDebugRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private LanguageEnumType defaultLanguage = LanguageType.Chinese;

        private void Awake()
        {
            if (GameManager.Instance == null)
            {
                var go = new GameObject("GameManager");
                go.transform.SetParent(transform);
                var gm = go.AddComponent<GameManager>();
                gm.language = defaultLanguage;
            }
            else
            {
                GameManager.Instance.language = defaultLanguage;
            }
        }
    }
}
