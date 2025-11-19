using System;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.Static.Utility;
using UnityEngine;
using static Game.GameRuntime.Entities.Player.Components.PlayerInputComponent;

namespace Game.GameRuntime.Entities.Player.Components
{
    public class PlayerMoveComponent : MoveComponent, IPlayerComponent
    {
        public PlayerLogic PlayerLogic { get; set; }

        [SerializeField]
        private CldInteractiveListener bodyCollider;

        [Header("射线检测")]
        [SerializeField] private float forwardDetDistance = 0.5f;
        [SerializeField] private Transform forwardDetTsf;

        [Header("移动")]
        [SerializeField] private float runSpeed;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float climbSpeed;
        [SerializeField] private float climbSpeedScale;

        [Header("跳跃")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpDistance = 1f;

        [Header("击飞")]
        [SerializeField] private float damageFlyHeight = 3f;
        [SerializeField] private float damageFlyDistance = 10f;

        public void ChangeMoveSpeed(float newSpeed)
        {
            runSpeed = newSpeed;
            walkSpeed = newSpeed;
            climbSpeed = newSpeed;
            climbSpeedScale = newSpeed * 2;
        }

        public AutoInputMove AutoMoveState = AutoInputMove.None;


        public Action<Vector2> onMoveAction;

        #region 属性

        public CldInteractiveListener BodyCollider => bodyCollider;

        public float JumpHeight => jumpHeight;
        public float JumpDistance => jumpDistance;
        #endregion

        public void SetDamageFlyHeight(float damageFlyHeight)
        {
            if (damageFlyHeight <= 0) { return; }
            this.damageFlyHeight = damageFlyHeight;
        }

        public void SetDamageFlyDistance(float damageFlyDistance)
        {
            if (damageFlyDistance <= 0) { return; }
            this.damageFlyDistance = damageFlyDistance;
        }

        private void OnDrawGizmos()
        {
            // forward wall detect
            Gizmos.color = Color.red;
            var position1 = forwardDetTsf.position;
            Gizmos.DrawLine(position1, position1 + (Vector3)DirV2 * forwardDetDistance);
        }

        protected override void OnInit()
        {
            base.OnInit();
            bodyCollider.GetComponent<ColliderResponder>().entityLogic = PlayerLogic;
            LoadMoveCurveData("Assets/GameRes/Config/MoveCurveConfig/PlayerJumpUp.asset");

            GroundedEvent += PlayerLogic.OnGroundedEvent;
            UnGroundedEvent += PlayerLogic.OnUnIsGround;
        }

        public override void OnUpdate()
        {
            if (!PlayerLogic.AllowControl)
            {
                return;
            }

            base.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            if (!PlayerLogic.AllowControl)
            {
                return;
            }

            base.OnFixedUpdate();
        }

        public bool DetectForwardWall()
        {
            return Physics2DUtility.Raycast(forwardDetTsf.position, DirV2, forwardDetDistance, "Map", "Wall");
        }

        /// <summary>
        ///     玩家前方是否有怪
        /// </summary>
        /// <returns></returns>
        public bool DetectForwardMonster()
        {
            return Physics2DUtility.Raycast(forwardDetTsf.position, DirV2, forwardDetDistance, "Monster");
        }

        public void SetWalkSpeed()
        {
            moveSpeedX = walkSpeed * DirectionSign;
            //moveSpeedX = 20 * DirectionSign;
        }

        public void SetRunSpeed()
        {
            moveSpeedX = runSpeed * DirectionSign;
            //moveSpeedX = 20 * DirectionSign;
        }

        public void SetClimbSpeed()
        {
            moveSpeedX = climbSpeedScale * climbSpeed * DirectionSign;
        }

        public void SetJumpSpeed()
        {
            SetParabolaSpeed(JumpHeight, JumpDistance);
            moveSpeedX *= DirectionSign;
            OnJumpUp?.Invoke();
        }

        public void SetDamageFlySpeed()
        {
            SetParabolaSpeed(damageFlyHeight, damageFlyDistance);
            moveSpeedX *= -DirectionSign;
        }

        public void TFMovePosition(Vector3 position)
        {
            //PlayerLogic.transform.position = position;
            PlayerLogic.SetPos(position);
        }

        // --------------------------------------------------------------------------------
        // 跳跃

        public void JumpUpMove(float normalized)
        {
            // 抛物线上升
            CurveMove("Assets/GameRes/Config/MoveCurveConfig/PlayerJumpUp.asset", jumpDistance, jumpHeight, DirV2, PlayerLogic.transform.position, normalized);
        }
    }
}