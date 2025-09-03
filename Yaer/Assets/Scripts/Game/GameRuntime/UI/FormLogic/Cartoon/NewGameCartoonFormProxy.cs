using System;
using Game.GameRuntime.UI.FormLogic.Base;

namespace Game.GameRuntime.UI.FormLogic.Cartoon
{
    public class NewGameCartoonFormProxy: BaseFormProxy
    {
        public Action onFinishEvent;
        
        public void OnFinish()
        { 
            onFinishEvent?.Invoke();
            onFinishEvent = null;
        }
    }
}