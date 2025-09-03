using UnityEngine;

namespace GameFramework.CoreExtend.Generic
{
    public static class GameObjectExtension
    {
        public static Vector3 GetWorldPosition(this GameObject gameObject)
        {
            return gameObject.transform.position;
        }
        
        public static Vector3 GetLocalPosition(this GameObject gameObject)
        {
            return gameObject.transform.localPosition;
        }
    }
}