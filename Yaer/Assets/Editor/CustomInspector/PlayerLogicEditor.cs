using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerLogic))]
public class PlayerLogicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var playerLogic = (PlayerLogic)target;
        if (GUILayout.Button("ÊÜµ½ÉËº¦"))
        {
            playerLogic.TakeDamage(10);
        }
        if (GUILayout.Button("»Ö¸´ÑªÁ¿"))
        {
            playerLogic.componentSystem.GetComponent<HealthComponent>().AddHp(10);
        }
    }
}
