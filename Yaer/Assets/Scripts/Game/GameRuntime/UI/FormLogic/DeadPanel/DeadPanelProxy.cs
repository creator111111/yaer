using Game.GameRuntime.UI.FormLogic.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic
{
    public class DeadPanelProxy : BaseFormProxy
    {
        public Action onReturnMainMenuEvent;
        public Action<string> onLoadGameAction;
        public void OnReturnMainMenu() => onReturnMainMenuEvent?.Invoke();
        public void LoadArchive(string selectedArchiveGuid) => onLoadGameAction?.Invoke(selectedArchiveGuid);
    }
}