using Game.GameMgr;
using Game.GameMgr.Component;
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

    /// <summary>�� <see cref="PlayerLogic.EditorInvincible"/> ͬ�������� Find ��Һ�ָ���ѡ״̬��</summary>
    private bool editorInvincibleToggle;

    [MenuItem("Tools/����״̬���Թ���")]
    public static void Open()
    {
        PlayerStatsEditorWindow window = GetWindow<PlayerStatsEditorWindow>("Player Stats Tool");
        window.FindPlayer();
    }

    private void OnGUI()
    {
        if (playerLogic == null)
        {
            GUILayout.Label("δ�ҵ�PlayerLogic");
        }
        if (GUILayout.Button("Find PlayerLogic"))
        {
            FindPlayer();
        }
        if (playerLogic != null)
        {
            var healthCpnt = playerLogic.componentSystem.GetComponent<HealthComponent>();
            var staminaCpnt = playerLogic.componentSystem.GetComponent<StaminaComponent>();

            // ��ѡ��д�� PlayerLogic.EditorInvincible��Ѫ��/���������Կ��ֶ�����ֵ
            editorInvincibleToggle = EditorGUILayout.ToggleLeft("����޵п���", editorInvincibleToggle);
            playerLogic.EditorInvincible = editorInvincibleToggle;

            if (GUILayout.Button("�޸���װ"))
            {
                playerLogic.FixClothes();
            }
            if (GUILayout.Button("�ܵ�10���˺�"))
            {
                playerLogic.TakeDamage(10);
                HPValue = healthCpnt.hp;
            }
            else
            {
                float MaxHP = healthCpnt.maxHp;
                float MaxStamina = staminaCpnt.MaxStamina;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѫ����");
                HPValue = GUILayout.HorizontalSlider(HPValue, 0, MaxHP, GUILayout.Width(200));
                GUILayout.Label($"{HPValue:N2}");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("������");
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
