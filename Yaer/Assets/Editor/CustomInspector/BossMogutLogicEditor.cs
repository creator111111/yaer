using Game.GameRuntime.Entities.Monster.BossMogut;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BossMogutLogic))]
public class BossMogutLogicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var bossMogut = target as BossMogutLogic;

        //if (GUILayout.Button("脚部受击"))
        //{
        //    bossMogut.HitFoot();
        //}
        //if (GUILayout.Button("面部受击"))
        //{
        //    bossMogut.HitFace();
        //}
        //if (GUILayout.Button("重置受击状态"))
        //{
        //    bossMogut.ResetHitState();
        //    bossMogut.csAnimator.CurrentCsRuntimeController
        //        .ExitCurrentSubStateMachine()
        //        .ChangeState<BossMogutMoveState>();
        //}
    }
}
