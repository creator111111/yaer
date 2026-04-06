using System.Collections.Generic;
using UnityEngine;
using Game.GameRuntime.UI.Control;
using Game.GameRuntime.UI.Component;

namespace Game.GameRuntime.UI.FormLogic.SelectHard
{
    /// <summary>
    /// 在任一难度按钮开始播放 Highlighted 时，强制将其他按钮的 ReturnToNormal 动画中断并立即切回 Normal 状态。
    /// </summary>
    public class SelectHardButtonGroup : MonoBehaviour
    {
        private List<UIListener> _buttons;
        private bool _initialized;

        public void Init(UIListener btnEasy, UIListener btnNormal, UIListener btnHard, UIListener btnHardest)
        {
            if (_initialized) return;
            _buttons = new List<UIListener> { btnEasy, btnNormal, btnHard, btnHardest };
            foreach (var btn in _buttons)
                btn.OnHighlighted += OnAnyHighlighted;
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_buttons == null) return;
            foreach (var btn in _buttons)
            {
                if (btn != null)
                    btn.OnHighlighted -= OnAnyHighlighted;
            }
        }

        private void OnAnyHighlighted(UIListener highlighted)
        {
            foreach (var btn in _buttons)
            {
                if (btn == null || btn == highlighted) continue;
                ForceToNormal(btn);
            }
        }

        private static void ForceToNormal(UIListener listener)
        {
            var animator = listener.GetComponent<Animator>();
            if (animator != null)
            {
                animator.ResetTrigger("ReturnToNormal");
                animator.Play("Normal", 0, 1f);
            }
            var stateMachine = listener.GetComponent<UIStateMachine>();
            if (stateMachine != null)
                stateMachine.ChangeTo("Normal");
        }
    }
}
