using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.Entity;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class RaycastComponentGSM : BaseComponentGSM
    {
        private GameFramework.UnityRuntime.Entity.Entity playerEntity;
        private Camera mainCamera;
        private HashSet<InteractiveComponent> interactiveComponents = new HashSet<InteractiveComponent>();

        private RaycastHit2D[] hit2D = new RaycastHit2D[16];
        
        private bool Pause => GameManager.GetGMComponent<ProcedureComponentGM>().Pause;


        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            FindMainCamera();
        }

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
            if (Pause) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (!mainCamera) return;

                int count = Physics2D.RaycastNonAlloc(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, hit2D, 1000);

                if (count == 0)
                {
                    return;
                }

                // 缓存成功触发的不在重复触发相同
                interactiveComponents.Clear();

                // 只处理有效命中的数据
                for (int i = 0; i < count; i++)
                {
                    // 只有点击到有射线脚本的物体才执行
                    var raycastListener = hit2D[i].transform.GetComponent<RaycastListener>();

                    if (raycastListener != null && interactiveComponents.Add(raycastListener.InteractiveComponent))
                    {
                        // 传入玩家实体
                        raycastListener.OnClick(SceneManager.GetPlayerEntity());
                    }
                }
            }
        }

        public void FindMainCamera()
        {
            mainCamera = Camera.main;
        }
    }
}