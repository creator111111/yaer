using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Monster.WormEgg;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.Static.Name.Settings;
using UnityEngine;

public enum ColliderDir {
    None, Left, Right, Top, Bottom
}

// �������������ײ����������ײ��֮�佻���Ľű�
public class PlayerBodyCollider : MonoBehaviour
{
    public Rigidbody2D body; // ����
    public new Collider2D collider2D; // ��ײ��
    public GameObject playerNode;
    public PlayerLogic playerLogic; // ����ʵ���߼�
    public CldInteractiveListener cldInteractiveListener;

    private ColliderDir _lastDirection;

    bool hasSqueezeOut = false; // �Ƿ��ڼ�������׶�
    bool hasTouchSpcGround = false; // �Ƿ���ײ���������
    float squeezeOutValue = 0; // ÿ�μ����ƶ��ĵ�λ
    Vector2 startSqueezePos = Vector2.zero; // ÿ�μ���ʱ�Ŀ�ʼ����

    // ��ȡ����⵽�ķ���
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
                Debug.LogError("===================����PlayerLogic�ű���ǰ�Ƿ������Playerʵ����");
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

        // TreeHole: WormEgg Body is Trigger; ClimbMove+StopMove blocks squat-atk approach.
        // Living egg blocks via GroundCld; BaseMonster.OnDead disables GroundCld. Skip WormEgg here.
        // Alt: allow atk in ClimbMove (OPEN_QUESTIONS Q3) or HUD tip.
        if (enetityLogic is WormEggLogic) { return; }

        var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
        if (!csAnimator.GetSign("IsRunning") && !csAnimator.GetSign("IsNormalAtk")
            && !csAnimator.GetSign("IsClimbMove"))
        {
            return; // ֻ����Ҵ����ض�״̬ʱ�Ż�ִ��������߼�
        }
        var playerMoveCpn = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
        if (enetityLogic.gameObject.transform.position.x >= playerLogic.gameObject.transform.position.x)
        {
            // ���ﴦ�������ұ�ʱ,���������ʱ�����ƶ����赲�����ƶ�
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

    // �����ĳ�����⶯����������ײ����ײ��������ײ��
    private void OnCollisionShowEventInSpcAction(Collision2D collision)
    {
        var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
        // ����׶����ﲻ�ɱ�����
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
            // �����ײ���Ĳ��ǹ�����������������Ҫ��������
            if (checkHasCollisonGrounp(collision))
            {
                // �Ѿ���ʼ����ʱ���������������ʱ���򼷳�
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
        // ������ײ���ߣ��ӵ�ǰ����ָ����ײ�㣩
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 collisionNormal = contact.normal;
        var collsionSize = collision.collider.bounds.size;
        // ����ʹ����ײ�����жϷ���
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
            // �����ײ���ھ��ν��䣬ʹ�����ĵ�������Ϊ��ѡ
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
            // ���������⴦�������Ƿ���
            if (capsule.direction == CapsuleDirection2D.Vertical)
            {
                // ��ֱ���������ȿ���y��
                _lastDirection = collisionNormal.y > 0 ? ColliderDir.Right : ColliderDir.Left;
            }
            else
            {
                // ˮƽ���������ȿ���x��
                _lastDirection = collisionNormal.x > 0 ? ColliderDir.Top : ColliderDir.Bottom;
            }
        }
        
        // ������ײ��ȣ���͸���룩
        float penetrationDepth = contact.separation; // ����ֵΪ����ʾ��͸���
        Vector2 safePosition = body.position - collisionNormal * penetrationDepth;
        Vector2 newPosition = hasSqueezeOut ? startSqueezePos : Vector2.Lerp(body.position, safePosition, 0.8f);
        if (startSqueezePos == Vector2.zero) { startSqueezePos = newPosition; }
        bool isRight = true;
        if (enetityLogic != null)
        {
            isRight = playerLogic.gameObject.transform.position.x >= enetityLogic.gameObject.transform.position.x;
        }
        
        // ������ײ������������ﵱǰ����Ĵ�����ʽ
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
        // ��ʼ��������
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
            // ����������������������ͻ���ĵ��λ���ǽ������Ҫ���򼷳�����
            if (layerName == "Map")
            {
                return true;
            }else if (layerName == "GroundCenter")
            {
                // ����Ƿ������ײ������
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
