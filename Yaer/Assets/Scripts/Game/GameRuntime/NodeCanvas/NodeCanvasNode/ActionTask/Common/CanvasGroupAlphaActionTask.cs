using Cysharp.Threading.Tasks;
using DG.Tweening;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("UI")]
[Name("CanvasGroup透明度渐变动画")]
public class CanvasGroupAlphaActionTask : ActionTask
{
    public BBParameter<UnityEngine.CanvasGroup> canvasGroup;
    public BBParameter<float> StartAlpha;
    public BBParameter<float> EndAlpha;
    public BBParameter<float> Duration;
    public BBParameter<bool> EndActionOnAnimationEnd;

    protected override void OnExecute()
    {
        Do().Forget();
    }

    private async UniTask Do()
    {
        canvasGroup.value.alpha = StartAlpha.value;
        if (EndActionOnAnimationEnd.value)
        {
            await canvasGroup.value.DOFade(EndAlpha.value, Duration.value).AsyncWaitForCompletion();
            EndAction();
        }
        else
        {
            canvasGroup.value.DOFade(EndAlpha.value, Duration.value);
            EndAction();
        }
    }

    protected override string info
    {
        get
        {
            return string.Format("<i>' {3}透明度: {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration, canvasGroup);
        }
    }
}
