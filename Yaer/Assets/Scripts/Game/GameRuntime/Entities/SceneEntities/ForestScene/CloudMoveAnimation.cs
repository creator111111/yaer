using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.ForestScene
{
    public class CloudMoveAnimation : MonoBehaviour
    {
        [SerializeField] [Range(0f, 20f)] private float CloudSpeed = 1;
        [SerializeField] [Range(0f, 1f)] private float DarkSpeedScale = 1;
        [SerializeField] [Range(0f, 1f)] private float FarSpeedScale = 1;

        [SerializeField] [Range(-1, 1)] private float MoveDirection = -1;

        [SerializeField] private SpriteRenderer[] LightClouds;
        [SerializeField] private SpriteRenderer[] DarkClouds;
        [SerializeField] private SpriteRenderer[] FarClouds;

        [SerializeField] private float LightStartPosition;
        [SerializeField] private float DarkStartPosition;
        [SerializeField] private float FarStartPosition;

        [SerializeField] private float LightEndPosition;
        [SerializeField] private float DarkEndPosition;
        [SerializeField] private float FarEndPosition;
        private Queue<SpriteRenderer> DarkCloudsQueue = new Queue<SpriteRenderer>();
        private Queue<SpriteRenderer> FarCloudsQueue = new Queue<SpriteRenderer>();

        private Queue<SpriteRenderer> LightCloudsQueue = new Queue<SpriteRenderer>();

        private void Start()
        {
            FindClouds();
        }

        private void FixedUpdate()
        {
            CloudMove(LightCloudsQueue, 1);
            CloudMove(DarkCloudsQueue, DarkSpeedScale);
            CloudMove(FarCloudsQueue, FarSpeedScale);

            CheckSwitchCloudPosition(LightCloudsQueue, LightStartPosition, LightEndPosition);
            CheckSwitchCloudPosition(DarkCloudsQueue, DarkStartPosition, DarkEndPosition);
            CheckSwitchCloudPosition(FarCloudsQueue, FarStartPosition, FarEndPosition);
        }

        private void FindClouds()
        {
            var layer = transform.Find("Light");
            DealCloudLayer(layer, ref LightClouds, ref LightCloudsQueue);
            layer = transform.Find("Dark");
            DealCloudLayer(layer, ref DarkClouds, ref DarkCloudsQueue);
            layer = transform.Find("Far");
            DealCloudLayer(layer, ref FarClouds, ref FarCloudsQueue);

            // Debug.Log(FarClouds[0].bounds.size.x);
            var cloud = LightCloudsQueue.First();
            LightStartPosition = GetCloudStartPosition(cloud);
            LightEndPosition = LightCloudsQueue.Last().transform.localPosition.x;
            cloud = DarkCloudsQueue.First();
            DarkStartPosition = GetCloudStartPosition(cloud);
            DarkEndPosition = DarkCloudsQueue.Last().transform.localPosition.x;
            cloud = FarCloudsQueue.First();
            FarStartPosition = GetCloudStartPosition(cloud);
            FarEndPosition = FarCloudsQueue.Last().transform.localPosition.x;
        }

        private float GetCloudStartPosition(SpriteRenderer cloud)
        {
            return cloud.transform.localPosition.x - cloud.bounds.size.x;
        }

        private void DealCloudLayer(Transform layer, ref SpriteRenderer[] clouds, ref Queue<SpriteRenderer> queue)
        {
            Debug.Log(layer.name);
            clouds = layer.GetComponentsInChildren<SpriteRenderer>();
            for (var i = 0; i < clouds.Length; i++) queue.Enqueue(clouds[i]);
        }

        private void CloudMove(Queue<SpriteRenderer> queue, float speedScale)
        {
            foreach (var cloud in queue)
            {
                var posv = cloud.transform.localPosition;
                var pos = posv.x + Time.fixedDeltaTime * CloudSpeed * speedScale * MoveDirection;
                posv.x = pos;
                cloud.transform.localPosition = posv;
            }
        }

        private void CheckSwitchCloudPosition(Queue<SpriteRenderer> queue, float startPosition, float endPosition)
        {
            var first = queue.First().transform;
            if ((first.localPosition.x - startPosition) * MoveDirection >= 0)
            {
                var cloud = queue.Dequeue();
                var posv = cloud.transform.localPosition;
                posv.x = endPosition;
                cloud.transform.localPosition = posv;
                queue.Enqueue(cloud);
            }
        }
    }
}