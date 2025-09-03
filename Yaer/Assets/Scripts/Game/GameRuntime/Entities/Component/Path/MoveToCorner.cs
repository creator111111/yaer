using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Path
{
    public class MoveToCorner : MonoBehaviour
    {
        [SerializeField] private bool start;

        public Rigidbody2D rg;
        public Collider2D targetCollider; // 目标碰撞盒
        public Collider2D selfCld;
        public EMoveToPosType moveToPosType = EMoveToPosType.RightTop; // 设置要移动到的角落 (例如左下，右上等)
        public Vector2 moveToPos;
        public Vector2 offset;
        public Vector2 reCalArea;
        private float x1;

        private void Update()
        {
            if (start) CalPos();
        }

        private void OnDrawGizmos()
        {
            if (start)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(moveToPos, selfCld.bounds.size);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(moveToPos, selfCld.bounds.size + new Vector3(reCalArea.x, reCalArea.y, 0));
            }
        }

        private void CalPos()
        {
            if (targetCollider is null)
            {
                Debug.LogError("Target collider not assigned.");
                return;
            }

            // 获取目标碰撞盒的四个角
            var targetBounds = targetCollider.bounds;
            var xMax = targetBounds.max.x;
            var xMin = targetBounds.min.x;
            var yMax = targetBounds.max.y;
            var yMin = targetBounds.min.y;
            var leftBottom = new Vector2(xMin, yMin);
            var leftTop = new Vector2(xMin, yMax);
            var rightBottom = new Vector2(xMax, yMin);
            var rightTop = new Vector2(xMax, yMax);


            // 在正下方或正上方现移动到角落
            if (selfCld.bounds.max.y < targetBounds.min.y)
            {
                if (selfCld.bounds.min.x < targetBounds.min.x)
                    // leftBottom
                    moveToPos = new Vector2(leftBottom.x - selfCld.bounds.size.x / 2 - offset.x, leftBottom.y - selfCld.bounds.size.y / 2 - offset.y);
                else
                    // rightBottom
                    moveToPos = new Vector2(rightBottom.x + selfCld.bounds.size.x / 2 + offset.x, rightBottom.y - selfCld.bounds.size.y / 2 - offset.y);
            }
            else
            {
                if (selfCld.bounds.min.x < targetBounds.min.x)
                    // leftTop
                    moveToPos = new Vector2(leftTop.x - selfCld.bounds.size.x / 2 - offset.x, leftTop.y + selfCld.bounds.size.y / 2 + offset.y);
                else
                    // rightTop
                    moveToPos = new Vector2(rightTop.x + selfCld.bounds.size.x / 2 + offset.x, rightTop.y + selfCld.bounds.size.y / 2 + offset.y);
            }

            // 不在
            if (selfCld.bounds.min.x > rightTop.x + offset.x - reCalArea.x / 2)
                // 移动到正右
                moveToPos = new Vector2(rightTop.x + selfCld.bounds.size.x / 2 + offset.x, targetBounds.center.y);
            else if (selfCld.bounds.max.x < leftTop.x - offset.x + reCalArea.x / 2)
                // 左
                moveToPos = new Vector2(leftTop.x - selfCld.bounds.size.x / 2 - offset.x, targetBounds.center.y);
        }
    }
}