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
    [Name("ÒÆ¶¯Ïà»ú")]
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
            GameObject go = new GameObject();
            go.transform.position = StartPos.value.transform.position;
            cameraMgr.SetFollow(go.transform);
            cameraMgr.SetLock(true);
            await go.transform.DOMove(EndPos.value.transform.position, Duration.value).AsyncWaitForCompletion();
            cameraMgr.SetLock(false);
            GameObject.Destroy(go);
            EndAction();
        }
    }
}