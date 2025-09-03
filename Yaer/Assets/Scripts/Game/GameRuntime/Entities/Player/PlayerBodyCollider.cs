using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.Static.Name.Settings;
using UnityEngine;

public enum ColliderDir {
    None, Left, Right, Top, Bottom
}

// 处理玩家身体碰撞体与其他碰撞体之间交互的脚本
public class PlayerBodyCollider : MonoBehaviour
{
    public Rigidbody2D body; // 刚体
    public new Collider2D collider2D; // 碰撞体
    public GameObject playerNode;
    public PlayerLogic playerLogic; // 人物实体逻辑
    public CldInteractiveListener cldInteractiveListener;

    private ColliderDir _lastDirection;

    bool hasSqueezeOut = false; // 是否处于挤出人物阶段
    bool hasTouchSpcGround = false; // 是否碰撞到特殊地形
    float squeezeOutValue = 0; // 每次挤出移动的单位
    Vector2 startSqueezePos = Vector2.zero; // 每次挤出时的开始坐标

    // 获取最后检测到的方向
    public ColliderDir GetLastDirection() => _lastDirection;

    public float pushoutDistance = 0.2f;
    public float safetyOffset = 0.02f;
    // Start is called before the first frame update
    void Start()
    {
        if (playerLogic == null)
        {
            playerLogic = playerNode.GetComponent<PlayerLogic>();
            if (playerLogic == null)
            {
                Debug.LogError("===================请检查PlayerLogic脚本当前是否存在于Player实体上");
            }
        }

        //cldInteractiveListener.onCollisionEnterEvent = OnCollisionShowEventInSpcAction;
        //cldInteractiveListener.onCollisionStayEvent = OnCollisionShowEventInSpcAction;
        cldInteractiveListener.onTriggerEnterEvent = OnCollisionMonster;
        cldInteractiveListener.onTriggerStayEvent = OnCollisionMonster;
    }

    void OnCollisionMonster(Collider2D collider)
    {
        if (playerLogic.isEnableMovePassMonster) { return; }

        var enetityLogic = collider.GetComponent<ColliderResponder>()?.GetEntityLogic() as BaseMonster;
        if (enetityLogic == null) { return; }
        if (enetityLogic.IsDead) { return; }
        var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
        if (!csAnimator.GetSign("IsRunning") && !csAnimator.GetSign("IsNormalAtk")
            && !csAnimator.GetSign("IsClimbMove"))
        {
            return; // 只有玩家处于特定状态时才会执行下面的逻辑
        }
        var playerMoveCpn = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
        if (enetityLogic.gameObject.transform.position.x >= playerLogic.gameObject.transform.position.x)
        {
            // 怪物处于人物右边时,人物如果此时向右移动则阻挡人物移动
            if (playerMoveCpn.IsTurnRight) { 
                playerMoveCpn.StopMove();
                playerLogic.canInStateSetPos = false;
            }
        }
        else
        {
            if (!playerMoveCpn.IsTurnRight) { 
                playerMoveCpn.StopMove();
                playerLogic.canInStateSetPos = false;
            }
        }
    }

    // 当玩家某种特殊动作中身体碰撞体碰撞到其他碰撞体
    private void OnCollisionShowEventInSpcAction(Collision2D collision)
    {
        var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
        // 特殊阶段人物不可被挤出
        if (csAnimator.GetSign("IsClimb") || csAnimator.GetSign("IsRunning") ||
            csAnimator.GetSign("IsJumpUp"))
        {
            hasSqueezeOut = false;
            startSqueezePos = Vector2.zero;
            return;
        }
        var enetityLogic = collision.collider.GetComponent<ColliderResponder>()?.GetEntityLogic() as BaseEntityLogic;
        if (!(enetityLogic is BaseMonster))
        {
            // 如果碰撞到的不是怪物或者特殊地形则不需要挤出人物
            if (checkHasCollisonGrounp(collision))
            {
                // 已经开始挤出时则在碰到特殊地形时反向挤出
                if (!hasTouchSpcGround)
                {
                    squeezeOutValue = hasSqueezeOut ? -squeezeOutValue : squeezeOutValue;
                    hasTouchSpcGround = true;
                }
            }
            else
            {
                hasSqueezeOut = false;
                hasTouchSpcGround = false;
                startSqueezePos = Vector2.zero;
                return;
            }
        }
        // 计算碰撞法线（从当前物体指向碰撞点）
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 collisionNormal = contact.normal;
        var collsionSize = collision.collider.bounds.size;
        // 优先使用碰撞法线判断方向
        if (Mathf.Abs(collisionNormal.x) > Mathf.Abs(collisionNormal.y))
        {
            _lastDirection = collisionNormal.x > 0 ? ColliderDir.Right : ColliderDir.Left;
        }
        else
        {
            _lastDirection = collisionNormal.y > 0 ? ColliderDir.Top : ColliderDir.Bottom;
        }
        if (collision.collider is BoxCollider2D)
        {
            // 如果碰撞点在矩形角落，使用中心点向量作为备选
            if (Mathf.Abs(collisionNormal.x) == Mathf.Abs(collisionNormal.y))
            {
                Vector2 centerDir = collision.transform.position - transform.position;
                if (Mathf.Abs(centerDir.x) > Mathf.Abs(centerDir.y))
                {
                    _lastDirection = centerDir.x > 0 ? ColliderDir.Right : ColliderDir.Left;
                }
                else
                {
                    _lastDirection = centerDir.y > 0 ? ColliderDir.Top : ColliderDir.Bottom;
                }
            }
        }
        if (collision.collider is CapsuleCollider2D capsule)
        {
            // 胶囊体特殊处理：考虑方向
            if (capsule.direction == CapsuleDirection2D.Vertical)
            {
                // 垂直胶囊体优先考虑y轴
                _lastDirection = collisionNormal.y > 0 ? ColliderDir.Right : ColliderDir.Left;
            }
            else
            {
                // 水平胶囊体优先考虑x轴
                _lastDirection = collisionNormal.x > 0 ? ColliderDir.Top : ColliderDir.Bottom;
            }
        }
        
        // 计算碰撞深度（穿透距离）
        float penetrationDepth = contact.separation; // 分离值为负表示穿透深度
        Vector2 safePosition = body.position - collisionNormal * penetrationDepth;
        Vector2 newPosition = hasSqueezeOut ? startSqueezePos : Vector2.Lerp(body.position, safePosition, 0.8f);
        if (startSqueezePos == Vector2.zero) { startSqueezePos = newPosition; }
        bool isRight = true;
        if (enetityLogic != null)
        {
            isRight = playerLogic.gameObject.transform.position.x >= enetityLogic.gameObject.transform.position.x;
        }
        
        // 根据碰撞方向的设置人物当前坐标的处理方式
        switch (_lastDirection)
        {
            case ColliderDir.Left:
                if (!hasSqueezeOut) {
                    hasSqueezeOut = true;
                    squeezeOutValue = -0.5f; 
                }
                break;
            case ColliderDir.Right:
                if (!hasSqueezeOut) {
                    hasSqueezeOut = true;
                    squeezeOutValue = 0.5f;
                }
                break;
            case ColliderDir.Top:
            case ColliderDir.Bottom:
                if (!hasSqueezeOut)
                {
                    squeezeOutValue = isRight ? 0.5f : -0.5f;
                    hasSqueezeOut = true;
                }
                break;
            default:
                break;
        }

        startSqueezePos.x += squeezeOutValue;
        playerLogic.canInStateSetPos = false;
        // 开始挤出过程
        var layerName = LayerMask.LayerToName(collision.gameObject.layer);
        if (layerName.StartsWith("Monster"))
        {
            playerLogic.SetPos(startSqueezePos);
        }
        else if (layerName == "Map" || layerName == "GroundCenter")
        {
            body.MovePosition(startSqueezePos);
        }
        //
    }

    bool checkHasCollisonGrounp(Collision2D collision)
    {
        var layerName = LayerMask.LayerToName(collision.gameObject.layer);
        if (hasSqueezeOut)
        {
            // 挤出人物过程中如果碰到了突出的地形或者墙，则需要反向挤出人物
            if (layerName == "Map")
            {
                return true;
            }else if (layerName == "GroundCenter")
            {
                // 检测是否侧面碰撞到地形
                ContactPoint2D contact = collision.GetContact(0);
                Vector2 collisionNormal = contact.normal;
                Debug.Log("===================collisionNormal.x" + collisionNormal.x);
                return collisionNormal.x != 0;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
