using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.BornSubState
{
    public class SlimeBornFallState : BaseSlimeState
    {
        private float downY;
        

        public override void Enter()
        {
            base.Enter();
            animationEventComponent.RegisterEvent("StopAniFrameWithSec", StopAniFrameWithSec);
            moveCpn.canGravity = true;
            // 计算落地点
            downY = slime.transform.position.y + slime.BornDownY;
            slime.isFallDownAtk = true;
            slime.isProtect = true;
        }

        public override void Update()
        {
            base.Update();
            if (moveCpn.IsGrounded) { ChangeState<SlimeBornDownState>(); }
            //if (slime.transform.position.y < downY) ChangeState<SlimeBornDownState>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsExit) slime.SetVelocity(new Vector3(0, slime.FallSpeed, 0));
        }
    }
}