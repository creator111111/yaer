using System;
using DebugScene;
using Game.GameRuntime.Entities.Component.Move;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Component.Move
{
    public class DebugMoveComponent : BaseGFComponentMono
    {
        [SerializeField] private Transform root; // 根节点
        [SerializeField] private Rigidbody2D rg;
        [SerializeField] private EDirectionType direction = EDirectionType.Right; // 默认右向

        [SerializeField] private float moveSpeedX;
        [SerializeField] private float moveSpeedY;

        private bool inCollision;

        private bool isMoveRight;
        private bool isMoveLeft;
        private bool isMoveUp;
        private bool isMoveDown;
        private bool isMoveCurve;
        private bool canMoveRight;
        private bool canMoveLeft;
        private bool canMoveUp;
        private bool canMoveDown;

        private CldEventListener cldEventListener;
        private Vector2 lastCollisionNormal;
        private Vector2 desiredMove;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (root == null) Debug.LogError("MoveComponent的root没有引用-" + transform.root.name);
            if (rg == null) Debug.LogError("MoveComponent的rg没有引用-" + transform.root.name);

            if (rg != null)
            {
                // 添加监听脚本
                cldEventListener = rg.gameObject.AddComponent<CldEventListener>();
            }
        }
#endif

        protected override void OnInit()
        {
            cldEventListener = rg.gameObject.GetComponent<CldEventListener>();
            cldEventListener.onCollisionEnter2DEvent += OnCollisionEnter2D;
            cldEventListener.onCollisionExit2DEvent += OnCollisionExit2D;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            rg.constraints = RigidbodyConstraints2D.FreezeAll;
            inCollision = true;

            // 计算所有接触点法线的平均值
            if (collision.contacts.Length > 0)
            {
                Vector2 averageNormal = Vector2.zero;
                foreach (var contact in collision.contacts)
                {
                    averageNormal += contact.normal;
                }

                averageNormal.Normalize();
                lastCollisionNormal = averageNormal;
                Debug.Log("平均碰撞法线: " + lastCollisionNormal);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            rg.constraints = RigidbodyConstraints2D.FreezeRotation;
            inCollision = false;

            // 使用存储的碰撞法线计算离开方向（取反即为离开方向）
            Vector2 leaveDirection = -lastCollisionNormal;
            Debug.Log("离开方向: " + leaveDirection);
        }

        public override void Check()
        {
            base.Check();

            if (rg == null)
            {
                Debug.LogWarning(transform.root + "的rg引用为空");
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            // 每帧先冻结所有移动并清零速度
            rg.constraints = RigidbodyConstraints2D.FreezeAll;
            rg.velocity = Vector2.zero;

            // 如果处于碰撞状态，不直接返回，而是限制移动方向
            // 这里先计算输入的移动向量
            Vector2 desiredMove = Vector2.zero;
            if (isMoveLeft)
            {
                desiredMove += Vector2.left * moveSpeedX;
            }

            if (isMoveRight)
            {
                desiredMove += Vector2.right * moveSpeedX;
            }

            if (isMoveUp)
            {
                desiredMove += Vector2.up * moveSpeedY;
            }

            if (isMoveDown)
            {
                desiredMove += Vector2.down * moveSpeedY;
            }

            // 如果处于碰撞状态并且记录了碰撞法线，则将 desiredMove 中指向碰撞方向的分量移除
            if (inCollision && lastCollisionNormal != Vector2.zero)
            {
                // 计算 desiredMove 在碰撞法线方向上的分量
                float dot = Vector2.Dot(desiredMove, -lastCollisionNormal);
                if (dot > 0) // 表示期望移动方向中有分量指向碰撞法线的方向
                {
                    // 去除该分量
                    desiredMove = Vector2.zero;
                }
            }

            // 如果处理后仍有移动，则解除冻结旋转以允许移动
            if (desiredMove != Vector2.zero)
            {
                rg.constraints = RigidbodyConstraints2D.FreezeRotation;
                MovePosition(rg.position + desiredMove);
            }

            // 重置移动输入标记
            isMoveLeft = false;
            isMoveRight = false;
            isMoveUp = false;
            isMoveDown = false;
            isMoveCurve = false;
        }

        public void SetMoveSpeed(float speed) => moveSpeedX = speed;
        public Vector2 GetPos() => rg.position;

        public void MoveVelocity(Vector2 velocity)
        {
            rg.velocity = velocity;
        }

        public void MovePosition(Vector2 position)
        {
            rg.MovePosition(position);
        }

        public void MoveForce(Vector2 force)
        {
            rg.AddForce(force);
        }

        // --------------------------------------------------------------------------------
        // 转向
        public void TurnLeft()
        {
            if (direction == EDirectionType.Right)
            {
                root.rotation = Quaternion.Euler(0, 180, 0);
            }

            direction = EDirectionType.Left;
        }

        public void TurnRight()
        {
            if (direction == EDirectionType.Left)
            {
                root.rotation = Quaternion.Euler(0, 0, 0);
            }

            direction = EDirectionType.Right;
        }

        // --------------------------------------------------------------------------------
        // 移动
        public void MoveRight()
        {
            isMoveRight = true;
            TurnRight();
        }

        public void MoveLeft()
        {
            isMoveLeft = true;
            TurnLeft();
        }

        public void MoveUp()
        {
            isMoveUp = true;
        }

        public void MoveDown()
        {
            isMoveDown = true;
        }

        public void StopMove()
        {
            isMoveRight = false;
            isMoveLeft = false;
            rg.velocity = Vector2.zero;
        }

        // --------------------------------------------------------------------------------
    }
}