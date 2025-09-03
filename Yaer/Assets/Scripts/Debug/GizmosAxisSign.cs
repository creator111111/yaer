using System;
using UnityEngine;

namespace GameDebug
{
    public class GizmosAxisSign: MonoBehaviour
    {
        [SerializeField] private bool open = true;
        [SerializeField] private float axisXLength = 1;
        [SerializeField] private float axisYLength = 1;

        private void OnDrawGizmos()
        {
            if (open)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + Vector3.left * axisXLength / 2, transform.position + Vector3.right * axisXLength / 2);
                Gizmos.DrawLine(transform.position + Vector3.up * axisYLength / 2, transform.position + Vector3.down * axisYLength / 2);
            }
        }
    }
}