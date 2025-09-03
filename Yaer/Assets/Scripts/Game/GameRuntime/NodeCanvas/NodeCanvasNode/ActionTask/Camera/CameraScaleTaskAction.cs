using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Camera")]
    [Name("拉伸相机")]
    public class CameraScaleTaskAction : ActionTask
    {
        public BBParameter<float> curSize; // 相机的当前尺寸
        public BBParameter<float> targetSize; // 相机的正交尺寸需要缩放的目标值,游戏默认大小7.9
        public BBParameter<float> Duration;
        public BBParameter<bool> isUseDefaultBaseSize; // 是否使用当前摄像机的尺寸

        private CameraComponentGSM cameraMgr;
        private Tween scaleCameraTween;
        private float curCameraSize = 0; //
        protected override string OnInit()
        {
            cameraMgr = GameManager.GetGameSceneManager().GetModule<CameraComponentGSM>();
            
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            Scale().Forget();
        }

        private async UniTask Scale()
        {
            await UniTask.WaitUntil(() => !cameraMgr.IsLock);
            if (isUseDefaultBaseSize.value == false)
            {
                // 没选使用默认值则设置一下摄像机的起始大小
                curCameraSize = curSize.value;
                //cameraMgr.ChangeVirtualCameraShowSize(curCameraSize);
            }
            else
            {
                curCameraSize = cameraMgr.GetVirtualCameraShowSize();
            }
            cameraMgr.SetLock(true);
            scaleCameraTween = DOTween.To(
                () => curCameraSize,
                (x) => { curCameraSize = x; },
                targetSize.value,
                Duration.value
            )
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() =>
            {
                cameraMgr.ChangeVirtualCameraShowSize(curCameraSize);
            });
            await scaleCameraTween.AsyncWaitForCompletion();
            cameraMgr.SetLock(false);
            EndAction();
        }
    }
}