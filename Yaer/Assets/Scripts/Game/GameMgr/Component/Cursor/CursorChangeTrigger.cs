using Game.GameMgr.Manager.Settings.Helper;
using Game.GameMgr.Manager.Settings;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GameMgr.Component.Cursor
{
    [RequireComponent(typeof(Collider2D))]
    public class CursorChangeTrigger : MonoBehaviour
    {
        [SerializeField]
        private CursorState TargetState;

        [SerializeField]
        private int Priority = 1;

        private CursorComponentGM cursorComponentGM;
        private new Collider2D collider2D;

        private Guid CursorChangeID;

        private bool PointerIsEnter = false;

        private void Start()
        {
            cursorComponentGM = GameManager.GetGMComponent<CursorComponentGM>();
            collider2D = GetComponent<Collider2D>();
        }

        private void Update()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (collider2D.OverlapPoint(mousePosition))
            {
                if (!PointerIsEnter)
                {
                    OnPointerEnter();
                }
            }
            else
            {
                if (PointerIsEnter)
                {
                    OnPointerExit();
                }
            }
        }

        public void OnPointerEnter()
        {
            PointerIsEnter = true;
            CursorChangeID = Guid.NewGuid();
            cursorComponentGM.OnEnterChangeTrigger(new CursorChangeArgs(TargetState, CursorChangeID, Priority));
            // 鼠标进入目标时，如果此时鼠标左键同时是攻击键则禁用普通攻击
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr != null)
            {
                var entity = sceneMgr.GetPlayerEntity();
                if (entity != null)
                {
                    var playerLogic = entity.Logic as PlayerLogic;
                    var configData = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
                    if (configData.KeyboardMouseInputConfig[ControlInputType.NormalAttack] == KeyCode.Mouse0)
                    {
                        playerLogic.isEnableNorAtk = false;
                    }
                }
            }
            
        }

        public void OnPointerExit()
        {
            PointerIsEnter = false;
            cursorComponentGM.OnExitChangeTrigger(CursorChangeID);
            // 鼠标移除目标时，如果此时鼠标左键同时是攻击键则开启普通攻击
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr != null)
            {
                var entity = sceneMgr.GetPlayerEntity();
                if (entity != null)
                {
                    var playerLogic = entity.Logic as PlayerLogic;
                    var configData = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
                    if (configData.KeyboardMouseInputConfig[ControlInputType.NormalAttack] == KeyCode.Mouse0)
                    {
                        playerLogic.isEnableNorAtk = true;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (PointerIsEnter)
            {
                OnPointerExit();
            }
        }

        private void OnDisable()
        {
            if (PointerIsEnter)
            {
                OnPointerExit();
            }
        }
    }
}