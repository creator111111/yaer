using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.ForestScene
{
    public class ParallelBackground : MonoBehaviour
    {
        [SerializeField] private Transform Far;
        [SerializeField] private Transform Mid;
        [SerializeField] private Transform Near;

        [SerializeField] private float FarOffset;
        [SerializeField] private float MidOffset;
        [SerializeField] private float NearOffset;

        private new Camera camera;

        private void Start()
        {
            camera = Camera.main;
        }

        private void Update()
        {
            Near.SetX(camera.transform.position.x * NearOffset);
            Mid.SetX(camera.transform.position.x * MidOffset);
            Far.SetX(camera.transform.position.x * FarOffset);
        }
    }

    public static class TransformExtensions
    {
        public static void SetX(this Transform transform, float x)
        {
            var position = transform.position;
            position.x = x;
            transform.position = position;
        }
    }
}