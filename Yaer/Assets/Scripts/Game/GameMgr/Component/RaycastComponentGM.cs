using Game.GameMgr.Component.Base;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class RaycastComponentGM : BaseComponentGM
    {
        public bool RayCast { get; set; }
        private Camera mainCamera;
        private bool Pause => GameManager.GetGMComponent<ProcedureComponentGM>().Pause;

        public override void OnUpdate()
        {
            base.OnUpdate();

            Raycast();
        }

        /// <summary>
        ///     鼠标交互射线检测
        /// </summary>
        private void Raycast()
        {
            if (!RayCast || Pause) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (!mainCamera) return;

                var hit2D = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 1000,
                    1 << LayerMask.NameToLayer("Interactive"));

                if (hit2D.transform is null == false) hit2D.transform.GetComponent<IBaseInteractive>()?.OnClick();
            }
        }

        public void FindMainCamera()
        {
            mainCamera = Camera.main;
        }
    }
}