using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Fighting
{
    public class FightingPanelProxy : BaseFormProxy
    {
        public PlayerStateValue GetPlayerStateValue()
        {
            // return GetProxy<PlayerProxy>().GetPlayerStateValue();
            return null;
        }

        public Sprite GetAvatar()
        {
            // 
            return null;
        }
    }
}