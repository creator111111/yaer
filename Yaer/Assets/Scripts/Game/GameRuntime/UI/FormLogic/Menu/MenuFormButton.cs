using System;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.Control;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    public class MenuFormButton : MonoBehaviour
    {
        [SerializeField] private UIListener listener;
        [SerializeField] private UIStateMachine stateMachine;

        private void OnValidate()
        {
            listener = GetComponent<UIListener>();
            stateMachine = GetComponent<UIStateMachine>();
        }

        private void Awake()
        {
            listener.OnHighlighted += uiListener =>
            {
                stateMachine.ChangeTo("HighLighted");
            };
            listener.OnNormal += uiListener =>
            {
                stateMachine.ChangeTo("Normal");
            };
            listener.OnPressed += uiListener =>
            {
                stateMachine.ChangeTo("Pressed");
            };
        }
    }
}