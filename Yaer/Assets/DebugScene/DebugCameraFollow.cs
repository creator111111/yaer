using UnityEngine;

namespace DebugScene
{
    public class DebugCameraFollow : MonoBehaviour
    {
        public bool isFollow;
        public float followSpeed;

        public Transform player;
        private Camera mainCamera;

        public void Start()
        {
            mainCamera = GetComponent<Camera>();
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (isFollow)
            {
                // 平滑移动摄像机
                var posX = Mathf.Lerp(transform.position.x, player.position.x, Time.deltaTime * followSpeed);
                transform.position = new Vector3(posX, transform.position.y, transform.position.z);
            }
        }
    }
}