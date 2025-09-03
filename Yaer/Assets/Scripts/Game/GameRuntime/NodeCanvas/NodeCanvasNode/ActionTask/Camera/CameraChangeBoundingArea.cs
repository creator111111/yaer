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
    [Name("相机修改边界区域")]
    public class CameraChangeBoundingArea : ActionTask
    {
        public BBParameter<GameObject> targetColliderArea;

        private CameraComponentGSM cameraMgr;

        protected override string OnInit()
        {
            cameraMgr = GameManager.GetGameSceneManager().GetModule<CameraComponentGSM>();
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            if (targetColliderArea.isNull) { return; }
            if (targetColliderArea.value == null) { return; }
            var colliderArea = targetColliderArea.value.GetComponent<PolygonCollider2D>();
            cameraMgr.ChangeCameraBoundingArea(colliderArea);
            EndAction();
        }
    }
}