using System;
using Game.GameRuntime.UI.FormLogic.Base;

namespace Game.GameRuntime.UI.FormLogic.Init
{
    public class InitFormProxy: BaseFormProxy
    {
        public Action onHideEnd;

        public void OnHideEnd() => onHideEnd?.Invoke();
    }
}