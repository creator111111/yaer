using System;

namespace Game.GameRuntime.UI.FormLogic.Black
{
    public enum BlackFadeType
    {
        RawShow,
        RawHide,
        FadeShow,
        FadeHide
    }
    
    public class ShowBlackFormArgs
    {
        public BlackFadeType showType;
        public BlackFadeType hideType;
        public Action<BlackFormLogic> onShowEnd;
        public Action<BlackFormLogic> onHideEnd;
    }
}