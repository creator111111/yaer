using UnityEngine;

namespace Game.GameRuntime.Entities.Component.CldController
{
    public class CldControllerNode : MonoBehaviour
    {
        [SerializeField] public string groupName;
        [SerializeField] private string nodeName;
        [SerializeField] private Collider2D cld;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(groupName))
            {
                groupName = "group1";
            }
            
            if (string.IsNullOrEmpty(nodeName))
            {
                nodeName = gameObject.name;
            }
            
            if (cld == null)
            {
                cld = GetComponent<Collider2D>();
            }
        }

        public void SetIsTrigger(bool isTrigger)
        {
            cld.isTrigger = isTrigger;
        }
        
        public void SetCldActive(bool isActive)
        {
            cld.enabled = isActive;
        }
    }
}