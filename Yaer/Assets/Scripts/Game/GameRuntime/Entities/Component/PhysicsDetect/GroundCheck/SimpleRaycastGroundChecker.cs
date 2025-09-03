using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect
{
    public class SimpleRaycastGroundChecker : BaseGroundChecker
    {
        [SerializeField]
        private Vector3 GroundCheckOffset;
        [SerializeField]
        private float GroundCheckDistance;

        public override bool GroundCheck()
        {
            var hit = Physics2D.Raycast(Root.position + GroundCheckOffset, Vector2.down, GroundCheckDistance, GroundLayerMask);
            return hit.collider != null;
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 position = Root.position + GroundCheckOffset;
            Gizmos.DrawLine(position, position + Vector3.down * GroundCheckDistance);
        }
    }
}