#if UNITY_EDITOR
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.DebugTools
{
    /// <summary>
    /// 验收用刷金小窗：自填正整数 → 累加 AddGold 或减少 TrySpend。
    /// 菜单：Tools / Debug / Player Gold Tool…
    /// </summary>
    /// <remarks>
    /// 原因：测商店钱不够 / ShopNo、菜单个位与 0 需要能抽瘦钱包；仅累加不够。
    /// UI 拍板 U1：并排「累加」「减少」，禁止负数当减少（易误触）。
    /// 硬顶：Add 吃 <see cref="PlayerGoldData.MaxGold"/>；打开窗时 F2 钳超标脏档并 Save。
    /// </remarks>
    public sealed class PlayerGoldDebugWindow : EditorWindow
    {
        private const string OpenMenuPath = "Tools/Debug/Player Gold Tool...";
        private const int DefaultAmount = 9999;

        private int _amount = DefaultAmount;

        /// <summary>本会话是否已跑过 F2 钳回，避免 OnGUI 每帧弹 Dialog。</summary>
        private bool _didClampCheckThisSession;

        [MenuItem(OpenMenuPath)]
        private static void OpenWindow()
        {
            var window = GetWindow<PlayerGoldDebugWindow>(utility: false, title: "Player Gold");
            window.minSize = new Vector2(280f, 160f);
            window.Show();
        }

        private void OnEnable()
        {
            _didClampCheckThisSession = false;
        }

        private void OnGUI()
        {
            // F2：Play 中打开窗发现超标 → 钳回并 Save（防脏 JSON；每会话一次）。
            if (Application.isPlaying && !_didClampCheckThisSession)
            {
                _didClampCheckThisSession = true;
                PlayerGoldDebugUtil.TryClampOverCapAndSave(showDialog: true);
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("刷金工具（仅 Play · 累加或减少）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "累加：AddGold + Save（硬顶 " + PlayerGoldData.MaxGold + "，多余丢弃）。\n" +
                "减少：TrySpendPlayerGold（与商店同门面；不足整笔失败，不钳到 0）。\n" +
                "存档与逻辑上限 " + PlayerGoldData.MaxGold + "；菜单显示与存档一致。",
                MessageType.Info);

            DrawCurrentBalance();

            EditorGUILayout.Space(4f);
            _amount = EditorGUILayout.IntField("金额（正整数）", _amount);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("填 100"))
            {
                _amount = 100;
            }

            if (GUILayout.Button("填 9999"))
            {
                _amount = DefaultAmount;
            }

            if (GUILayout.Button("填 " + PlayerGoldData.MaxGold))
            {
                _amount = PlayerGoldData.MaxGold;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("累加 AddGold", GUILayout.Height(28f)))
            {
                PlayerGoldDebugUtil.TryAddPlayerGold(_amount, showDialogOnFail: true);
                Repaint();
            }

            if (GUILayout.Button("减少 Spend", GUILayout.Height(28f)))
            {
                PlayerGoldDebugUtil.TrySpendPlayerGoldForDebug(_amount, showDialogOnFail: true);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请先 Play（建议 InitScene 正规进游戏）。", MessageType.Warning);
            }
        }

        private void DrawCurrentBalance()
        {
            var gold = PlayerGoldDebugUtil.TryGetCurrentGold();
            if (gold.HasValue)
            {
                EditorGUILayout.LabelField("当前余额", gold.Value.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("当前余额", "请先 Play");
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
