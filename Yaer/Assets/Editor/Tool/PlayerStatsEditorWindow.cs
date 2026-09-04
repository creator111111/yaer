using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Component;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Player;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 人物状态调试工具（Tools → 人物状态调试工具）。
/// 源码须以 UTF-8 无 BOM 保存；曾因错误编码另存导致菜单/窗口中文乱码。
/// </summary>
public class PlayerStatsEditorWindow : EditorWindow
{
    private PlayerLogic playerLogic;

    private float HPValue;
    private float StaminaValue;

    /// <summary>与 <see cref="PlayerLogic.EditorInvincible"/> 同步；Find 玩家后刷新勾选状态。</summary>
    private bool editorInvincibleToggle;

    [MenuItem("Tools/人物状态调试工具")]
    public static void Open()
    {
        PlayerStatsEditorWindow window = GetWindow<PlayerStatsEditorWindow>("Player Stats Tool");
        window.FindPlayer();
    }

    private void OnGUI()
    {
        if (playerLogic == null)
        {
            GUILayout.Label("未找到PlayerLogic");
        }
        if (GUILayout.Button("Find PlayerLogic"))
        {
            FindPlayer();
        }
        if (playerLogic != null)
        {
            var healthCpnt = playerLogic.componentSystem.GetComponent<HealthComponent>();
            var staminaCpnt = playerLogic.componentSystem.GetComponent<StaminaComponent>();

            // 勾选写入 PlayerLogic.EditorInvincible；血量/体力仍可靠滑条改数值
            editorInvincibleToggle = EditorGUILayout.ToggleLeft("无敌开关", editorInvincibleToggle);
            playerLogic.EditorInvincible = editorInvincibleToggle;

            if (GUILayout.Button("修复服装"))
            {
                playerLogic.FixClothes();
            }
            if (GUILayout.Button("受到10点伤害"))
            {
                playerLogic.TakeDamage(10);
                HPValue = healthCpnt.hp;
            }
            else
            {
                float MaxHP = healthCpnt.maxHp;
                float MaxStamina = staminaCpnt.MaxStamina;

                GUILayout.BeginHorizontal();
                GUILayout.Label("血量：");
                HPValue = GUILayout.HorizontalSlider(HPValue, 0, MaxHP, GUILayout.Width(200));
                GUILayout.Label($"{HPValue:N2}");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("体力：");
                StaminaValue = GUILayout.HorizontalSlider(StaminaValue, 0, MaxStamina, GUILayout.Width(200));
                GUILayout.Label($"{StaminaValue:N2}");
                GUILayout.EndHorizontal();

                healthCpnt.SetData(HPValue, MaxHP);
                staminaCpnt.SetData(StaminaValue, MaxStamina);
            }
        }
    }

    private void FindPlayer()
    {
        playerLogic = null;
        if (Application.isPlaying)
        {
            var entityGM = GameManager.GetGMComponent<EntityComponentGM>();
            if (entityGM != null)
                playerLogic = entityGM.GetEntityLogic<PlayerLogic>();
        }
        if (playerLogic == null)
            playerLogic = Object.FindObjectOfType<PlayerLogic>(true);
        if (playerLogic == null) return;
        HPValue = playerLogic.componentSystem.GetComponent<HealthComponent>().hp;
        StaminaValue = playerLogic.componentSystem.GetComponent<StaminaComponent>().Stamina;
        editorInvincibleToggle = playerLogic.EditorInvincible;
    }
}
