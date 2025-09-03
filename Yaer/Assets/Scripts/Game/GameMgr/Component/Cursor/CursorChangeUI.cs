using Game.GameRuntime.UI.FormLogic.Base;
using System;
using UnityEngine;

namespace Game.GameMgr.Component.Cursor
{
    [RequireComponent(typeof(BaseUIFormLogic))]
    public class CursorChangeUI : MonoBehaviour
    {
        [SerializeField]
        private CursorState TargetState = CursorState.Normal;

        [SerializeField]
        private int Priority = 100;

        private CursorComponentGM cursorComponentGM;

        private Guid CursorChangeID;

        private void Awake()
        {
            cursorComponentGM = GameManager.GetGMComponent<CursorComponentGM>();
        }

        private void OnEnable()
        {
            CursorChangeID = Guid.NewGuid();
            cursorComponentGM.OnEnterChangeTrigger(new CursorChangeArgs(TargetState, CursorChangeID, Priority));
        }

        private void OnDisable()
        {
            cursorComponentGM.OnExitChangeTrigger(CursorChangeID);
        }

        private void OnDestroy()
        {
            cursorComponentGM.OnExitChangeTrigger(CursorChangeID);
        }
    }
}

