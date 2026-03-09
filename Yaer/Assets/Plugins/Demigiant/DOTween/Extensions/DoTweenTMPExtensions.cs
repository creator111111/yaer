using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;

namespace Library.DOTween.Extensions
{
    public static class DoTweenTMPExtensions
    {
        public static TweenerCore<string, string, StringOptions> DOText(
            this TMP_Text target, 
            string endValue, 
            float duration, 
            bool richTextEnabled = true, 
            ScrambleMode scrambleMode = ScrambleMode.None, 
            string scrambleChars = null)
        {
            if (endValue == null) 
            {
                if (Debugger.logPriority > 0) 
                    Debugger.LogWarning("You can't pass a NULL string to DOText: an empty string will be used instead to avoid errors");
                endValue = "";
            }

            TweenerCore<string, string, StringOptions> t = DG.Tweening.DOTween.To(
                () => target.text, 
                x => target.text = x, 
                endValue, 
                duration
            );

            t.SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetTarget(target);

            return t;
        }
    }
}