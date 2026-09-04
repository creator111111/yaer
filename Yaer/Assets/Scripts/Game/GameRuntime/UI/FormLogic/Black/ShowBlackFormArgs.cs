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

        /// <summary>可选：本次淡入黑幕时长（秒）；未设则用 BlackPanel 默认 showTime。</summary>
        public float? showDuration;

        /// <summary>可选：本次淡出黑幕时长（秒）；未设则用 BlackPanel 默认 hideTime。</summary>
        public float? hideDuration;
    }
}