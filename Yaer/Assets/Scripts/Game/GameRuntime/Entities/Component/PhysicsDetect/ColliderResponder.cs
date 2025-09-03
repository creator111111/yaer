using GameFramework.UnityRuntime.Entity;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect
{
    public class ColliderResponder : MonoBehaviour
    {
        [SerializeField] private bool isChild;
        [SerializeField] private ColliderResponder parent;
        public EntityLogic entityLogic;

        public EntityLogic GetEntityLogic()
        {
            if (isChild)
            {
                if (parent == null) Debug.LogError($"{gameObject.name}的ColliderResponder设置了isChild,但parent为空--");
                return parent.GetEntityLogic();
            }

            return entityLogic;
        }
    }
}