using System;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Enum;

namespace Game.GameRuntime.UI.FormLogic.Start
{
    public class StartFormProxy : BaseFormProxy
    {
        public event Action onStart;
        
        public void OnStart()
        {
            onStart?.Invoke();
            onStart = null;
        }
    }
}