using System;
using Game.GameMgr.Component.Base;
using UnityEngine.InputSystem;

namespace Game.GameMgr.Component
{
    public class InputComponentGM : BaseComponentGM
    {
        private InputActions inputActions;
        
        /// <summary>
        /// Esc按下
        /// </summary>
        public event Action onEscPressed;
        /// <summary>
        /// E键按下
        /// </summary>
        public event Action onEKeyPressed; 

        public override void OnInit()
        {
            base.OnInit();
            
            inputActions = new InputActions();

            // Esc打开菜单
            inputActions.Generic.Esc.performed += context => onEscPressed?.Invoke();
            //inputActions.Player.Interactive.performed += context => onEKeyPressed?.Invoke();
            
            inputActions.Enable();
        }
    }
}