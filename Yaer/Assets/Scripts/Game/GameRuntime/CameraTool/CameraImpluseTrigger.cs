using Cinemachine;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.GameRuntime.Component
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraImpluseTrigger : MonoBehaviour
    {
        private CinemachineImpulseSource impulseSource;
        [SerializeField]
        private Vector3 CameraImpluseVelocity;

        private void Awake()
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void CameraImpulse(Vector3 impluseVelocity = new Vector3())
        {
            if (impluseVelocity ==  Vector3.zero) { impluseVelocity = CameraImpluseVelocity; }
            impulseSource.GenerateImpulse(impluseVelocity);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CameraImpluseTrigger))]
    public class CameraImpluseTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Test Impluse"))
            {
                (target as CameraImpluseTrigger).CameraImpulse();
            }
        }
    }
#endif
}