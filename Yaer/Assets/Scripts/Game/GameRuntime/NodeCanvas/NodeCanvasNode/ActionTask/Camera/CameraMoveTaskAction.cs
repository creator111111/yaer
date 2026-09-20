using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("Camera")]
    [Name("????")]
    public class CameraMoveTaskAction : ActionTask
    {
        public BBParameter<Transform> StartPos;
        public BBParameter<Transform> EndPos;
        public BBParameter<float> Duration;

        private CameraComponentGSM cameraMgr;

        protected override string OnInit()
        {
            cameraMgr = GameManager.GetGameSceneManager().GetModule<CameraComponentGSM>();
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            Move().Forget();
        }

        private async UniTask Move()
        {
            await UniTask.WaitUntil(() => !cameraMgr.IsLock);
            GameObject go = new GameObject("CameraMoveTempFollow");
            go.transform.position = StartPos.value.transform.position;

            // 0913 ????????? forceSnap??? smoothTime ??? DOMove ??????/???
            // ???forceSnap=true????????smoothTime=0 ?????????
            cameraMgr.SetFollow(go.transform, onComplete: null, forceSnapToTarget: false);
            cameraMgr.SetLock(true);

            // InOutSine??????????? OutQuad ????????
            await go.transform
                .DOMove(EndPos.value.transform.position, Duration.value)
                .SetEase(Ease.InOutSine)
                .AsyncWaitForCompletion();

            cameraMgr.SetLock(false);

            // ??????????????? Destroy ????? CM ? Follow ??????
            if (EndPos.value != null)
            {
                cameraMgr.SetFollow(EndPos.value, onComplete: null, forceSnapToTarget: false);
            }

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            if (go != null)
            {
                Object.Destroy(go);
            }

            EndAction();
        }
    }
}
