using System;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Enum;

namespace Game.GameRuntime.UI.FormLogic.SelectHard
{
    public class SelectHardFormProxy: BaseFormProxy
    {
        public Action<EGameHard> onSelect { get; set; }
        
        public void SelectHard(EGameHard hard)
        {
            onSelect?.Invoke(hard);
            onSelect = null;
        }
    }
}