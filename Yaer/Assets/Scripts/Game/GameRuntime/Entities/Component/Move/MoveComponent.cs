using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Move
{
    public enum EDirectionType
    {
        Left,
        Right
    }

    public class MoveComponent : BaseGFComponentMono
    {
        [SerializeField] private Transform root; // 根节点
        [SerializeField] private Rigidbody2D rg;
        [SerializeField] private EDirectionType direction;
        /// <summary>
        /// 重力，向下为负
        /// </summary>
        [SerializeField] protected Vector2 Gravity = new Vector2(0, -9.8f);

        public Vector2 m_Gravity => Gravity;

        public bool canGravity = true; // 是否需要受到重力影响

        public Vector2 Velocity;

        [SerializeField] private Vector2 AnimatedVelocity;

        #region 地面检测
        public bool IsGrounded;
        public BaseGroundChecker groundChecker;

        public event Action GroundedEvent;
        public event Action UnGroundedEvent;
        #endregion
        public List<EDirectionType> moveDirs = new List<EDirectionType>();// 玩家当前的移动方向

        public float moveSpeedX
        {
            get => Velocity.x;
            set => Velocity.x = value;
        }

        public float moveSpeedY
        {
            get => Velocity.y;
            set => Velocity.y = value;
        }

        public Transform Root => root;

        public bool IsMoveRight => moveSpeedX > 0;
        public bool IsMoveLeft => moveSpeedX < 0;
        public bool IsMoveUp => moveSpeedY > 0;
        public bool IsMoveDown => moveSpeedY < 0;

        public bool IsTurnRight => direction == EDirectionType.Right;

        private bool isMoveCurve;
        private CurveMoveInfo curveMoveInfo;
        private Dictionary<string, MoveCurveData> moveCurveDataDic = new Dictionary<string, MoveCurveData>();
        
        public EDirectionType Direction => direction;
        public Vector2 DirV2 => direction == EDirectionType.Right ? Vector2.right : Vector2.left;
        public int DirectionSign => Direction == EDirectionType.Right ? 1 : -1;

        public Action<Vector2> onTurnAction;
        public Action OnJumpUp;
        public Action OnJumpDown;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (root == null) Debug.LogError("MoveComponent的root没有引用-" + transform.root.name);
            if (rg == null) Debug.LogError("MoveComponent的rg没有引用-" + transform.root.name);
        }
#endif

        protected override void OnInit()
        {
            // 默认右向
            direction = EDirectionType.Right;
            groundChecker = gameObject.GetComponent<BaseGroundChecker>();
            if (groundChecker != null)
            {
                groundChecker.Init(this.root);
            }
        }

        public override void Check()
        {
            base.Check();

            if (rg == null)
            {
                Debug.LogWarning(transform.root + "的rg引用为空");
            }
        }

        protected void GroundCheck()
        {
            if (groundChecker != null)
            {
                bool oldGrounded = IsGrounded;
                IsGrounded = groundChecker.GroundCheck();
                if (IsGrounded && !oldGrounded)
                {
                    GroundedEvent?.Invoke();
                    if (moveSpeedY < 0)
                    {
                        moveSpeedY = 0;
                    }
                }
                else if (oldGrounded && !IsGrounded)
                {
                    UnGroundedEvent?.Invoke();
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            //rg.move

            // rg.constraints = RigidbodyConstraints2D.FreezeAll;
/*            rg.velocity = Vector2.zero;
            if (isMoveLeft)
            {
                rg.constraints = RigidbodyConstraints2D.FreezeRotation;
                MovePosition(rg.position + Vector2.left * (moveSpeedX * Time.deltaTime * 10));
            }

            if (isMoveRight)
            {
                rg.constraints = RigidbodyConstraints2D.FreezeRotation;
                MovePosition(rg.position + Vector2.right * (moveSpeedX * Time.deltaTime * 10));
            }

            if (isMoveUp)
            {
                rg.constraints = RigidbodyConstraints2D.FreezeRotation;
                MovePosition(rg.position + Vector2.up * (moveSpeedY));
            }

            if (isMoveDown)
            {
                rg.constraints = RigidbodyConstraints2D.FreezeRotation;
                MovePosition(rg.position + Vector2.down * (moveSpeedY));
            }

            if (isMoveCurve)
            {
                rg.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            isMoveCurve = false;*/
        }

        public override void OnFixedUpdate()
        {
            GroundCheck();
            if (!IsGrounded && canGravity)
            {
                Velocity += Time.fixedDeltaTime * Gravity;
            }
            
/*            if (IsGrounded)
            {
                moveSpeedY = 0;
            }*/

            MoveVelocity(Velocity);
            //if (Velocity.y == 0)
            //{
            //    rg.constraints = RigidbodyConstraints2D.FreezeRotation;
            //    rg.velocity = Vector2.zero;
            //    var movePos = rg.position;
            //    var canMove = false;
            //    if (moveDirs.Contains(EDirectionType.Left) || moveDirs.Contains(EDirectionType.Right))
            //    {
            //        canMove = true;
            //        movePos += Vector2.right * (moveSpeedX * Time.deltaTime);
            //    }
            //    if (canMove) MovePosition(movePos);
            //}
            
        }

        public Vector2 GetPos() => rg.position;

        public void MoveVelocity(Vector2 velocity)
        {
            rg.velocity = velocity;
        }

        public virtual void MovePosition(Vector2 position)
        {
            rg.MovePosition(position);
        }

        public void MoveForce(Vector2 force)
        {
            rg.AddForce(force);
        }

        public void ApplyAnimatedMoveSpeed()
        {
            Velocity = AnimatedVelocity;
            moveSpeedX *= DirectionSign;
        }
        /// <summary>
        /// 设置抛物线运动的初速度
        /// </summary>
        /// <param name="height"></param>
        /// <param name="distance"></param>
        protected void SetParabolaSpeed(float height, float distance)
        {
            if (Gravity.y >= 0) 
            {
                Debug.LogWarning("抛物线运动时，重力需为负数");
                return;
            }
            float time = Mathf.Sqrt(2 * height / Mathf.Abs(Gravity.y));

            moveSpeedY = -time * Gravity.y;
            moveSpeedX = Mathf.Abs(distance) / (2 * time);
        }


        // --------------------------------------------------------------------------------
        // 转向
        public void TurnLeft(bool isCheckDir = true)
        {
            if (root.rotation == Quaternion.Euler(0, 0, 0)) {
                root.rotation = Quaternion.Euler(0, 180, 0);
            }
            if (isCheckDir && direction != EDirectionType.Right) { return; }
            root.rotation = Quaternion.Euler(0, 180, 0);
            direction = EDirectionType.Left;
            moveSpeedX *= -1;
            onTurnAction?.Invoke(DirV2);
        }

        public void TurnRight(bool isCheckDir = true)
        {
            if (root.rotation == Quaternion.Euler(0, 180, 0))
            {
                root.rotation = Quaternion.Euler(0, 0, 0);
            }
            if (isCheckDir && direction != EDirectionType.Left) { return; }
            root.rotation = Quaternion.Euler(0, 0, 0);
            direction = EDirectionType.Right;
            moveSpeedX *= -1;
            onTurnAction?.Invoke(DirV2);
        }

        // --------------------------------------------------------------------------------
        // 移动
        public void MoveRight(bool isCheckDir=true)
        {
            moveDirs.Clear();
            moveDirs.Add(EDirectionType.Right);
            TurnRight(isCheckDir);
        }

        public void MoveLeft(bool isCheckDir = true)
        {
            moveDirs.Clear();
            moveDirs.Add(EDirectionType.Left);
            TurnLeft(isCheckDir);
        }

        public void MoveUp()
        {
        }

        public void MoveDown()
        {
        }

        public void StopMove()
        {
            Velocity = Vector2.zero;
            rg.velocity = Vector2.zero;
            moveDirs.Clear();
        }

        // 在X方向上停止移动
        public void StopMoveInX()
        {
            Velocity.x = 0;
            rg.velocity = new Vector2(0, rg.velocity.y);
            moveDirs.Clear();
        }

        // --------------------------------------------------------------------------------
        // 曲线运动
        public void CurveMove(string curveName, float xMultiples, float yMultiples, Vector2 dir, Vector2 startPos, float normalized)
        {
            if (curveMoveInfo == null)
            {
                if (moveCurveDataDic.TryGetValue(curveName, out var data))
                {
                    curveMoveInfo = new CurveMoveInfo()
                    {
                        data = data,
                        dir = dir,
                        xMultiples = xMultiples,
                        yMultiples = yMultiples,
                        startPos = startPos
                    };
                }
                else
                {
                    Debug.LogError("没有找到曲线数据，curveName: " + curveName);
                    return;
                }
            }

            isMoveCurve = true;
            MovePosition(CalCurve(normalized));
        }

        protected Vector2 CalCurve(float normalized)
        {
            // 通过曲线计算出一个插值因子（0～1之间）
            float curveValue = curveMoveInfo.data.movementCurve.Evaluate(normalized);

            // 根据方向和倍数计算位移偏移量
            Vector2 offset = new Vector2(curveMoveInfo.xMultiples * curveValue, curveMoveInfo.yMultiples * curveValue);

            // 计算目标位置
            Vector2 newPosition = curveMoveInfo.startPos + offset;

            return newPosition;
        }

        protected void LoadMoveCurveData(string assetPath)
        {
            if (moveCurveDataDic.ContainsKey(assetPath))
            {
                return;
            }

            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<MoveCurveData>(assetPath, data => { moveCurveDataDic.Add(assetPath, data); });
        }
    }

    public class CurveMoveInfo
    {
        public MoveCurveData data;
        public Vector2 startPos;
        public Vector2 dir;
        public float xMultiples;
        public float yMultiples;
    }
}