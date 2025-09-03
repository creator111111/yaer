using Game.GameRuntime.Entities.Component;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Player;
using UnityEditor;
using UnityEngine;

public class PlayerStatsEditorWindow : EditorWindow
{
    private PlayerLogic playerLogic;

    private float HPValue;
    private float StaminaValue;

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
        playerLogic = GameObject.FindObjectOfType<PlayerLogic>();
        HPValue = playerLogic.componentSystem.GetComponent<HealthComponent>().hp;
        StaminaValue = playerLogic.componentSystem.GetComponent<StaminaComponent>().Stamina;
    }
}
