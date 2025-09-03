using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    // 这个方法主要是设置相机所在对象的移动,控制的是场景中Camera这个GameObject
    [Category("Camera")]
    [Name("相机整体移动")]
    public class CameraAllMoveTaskAction : ActionTask
    {
        public BBParameter<Vector2> StartPos;
        public BBParameter<Vector2> EndPos;
        public BBParameter<float> Duration;
        public BBParameter<bool> isUseDefualtStartPos; // 是否使用默认的基础起始坐标

        private CameraComponentGSM cameraMgr;
        private Tween moveCameraTween;

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
            cameraMgr.SetLock(true);
            var gameObject = cameraMgr.CameraComponent.gameObject;
            if (isUseDefualtStartPos.value == false)
            {
                gameObject.transform.position = StartPos.value;// 设置起始坐标
            }
            
            moveCameraTween = GameActionMgr.runMoveToAction(gameObject, EndPos.value, Duration.value);
            await moveCameraTween.AsyncWaitForCompletion();
            cameraMgr.SetLock(false);
            EndAction();
        }
    }
}