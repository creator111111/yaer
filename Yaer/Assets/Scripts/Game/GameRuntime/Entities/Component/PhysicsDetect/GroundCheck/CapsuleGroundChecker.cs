using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.GameRuntime.Entities.Component.PhysicsDetect
{
    public class CapsuleGroundChecker : BaseGroundChecker
    {
        [SerializeField]
        private Vector3 GroundCheckOffset;
        [SerializeField]
        private float CapsuleHeight;
        [SerializeField]
        private float CapsuleRadius;
        [SerializeField]
        private CapsuleDirection2D CapsuleDirection;

        private Vector3 CapsuleCenter
        {
            get
            {
                return Root.position + Root.rotation * GroundCheckOffset;
            }
        }

        private Vector2 CapsuleSize;

        private Vector3 Sphere1Center
        {
            get
            {
                if (CapsuleDirection == CapsuleDirection2D.Horizontal)
                {
                    return CapsuleCenter - 0.5f * new Vector3(CapsuleHeight, 0);
                }
                else
                {
                    return CapsuleCenter + 0.5f * new Vector3(0, CapsuleHeight);
                }
            }
        }

        private Vector3 Sphere2Center
        {
            get
            {
                if (CapsuleDirection == CapsuleDirection2D.Horizontal)
                {
                    return CapsuleCenter + 0.5f * new Vector3(CapsuleHeight, 0);
                }
                else
                {
                    return CapsuleCenter - 0.5f * new Vector3(0, CapsuleHeight);
                }
            }
        }

        private void Awake()
        {
            CapsuleSize = new Vector2(CapsuleHeight, CapsuleRadius);
        }

        public override bool GroundCheck()
        {
            var hit = Physics2D.CapsuleCast(CapsuleCenter, CapsuleSize, CapsuleDirection, 0, Vector2.right, 0, GroundLayerMask);
            return hit.collider != null;
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Sphere1Center, CapsuleRadius);
            Gizmos.DrawSphere(Sphere2Center, CapsuleRadius);
            if (CapsuleDirection == CapsuleDirection2D.Horizontal)
            {
                Gizmos.DrawCube(CapsuleCenter, new Vector3(CapsuleHeight, 2 * CapsuleRadius));
            }
            else
            {
                Gizmos.DrawCube(CapsuleCenter, new Vector3(2 * CapsuleRadius, CapsuleHeight));
            }
        }
    }
}

