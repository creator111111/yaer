using System;
using UnityEngine;

namespace DebugScene
{
    public class CldEventListener: MonoBehaviour
    {
        public event Action<Collision2D> onCollisionEnter2DEvent;
        public event Action<Collision2D> onCollisionExit2DEvent;

        private void OnCollisionEnter2D(Collision2D other)
        {
            onCollisionEnter2DEvent?.Invoke(other);
        }
        
        private void OnCollisionExit2D(Collision2D other)
        {
            onCollisionExit2DEvent?.Invoke(other);
        }
    }
}