using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DisableAcitonType { 
    None,
    SquatUp, // 蹲下到起立
    Jump, // 跳跃
}

// 绑定此脚本的碰撞体区域将在玩家进入后禁用玩家某些动作
public class CanNotSomeActionArea : MonoBehaviour
{
    public Collider2D colliderObj;
    public DisableAcitonType curDisableType;
    // Start is called before the first frame update
    void Start()
    {
        if (colliderObj == null)
        {
            colliderObj = GetComponent<Collider2D>();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        var enetityLogic = collider.GetComponent<ColliderResponder>()?.GetEntityLogic() as BaseEntityLogic;
        if (enetityLogic is PlayerLogic playerLogic)
        {
            checkActionState(playerLogic, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        var enetityLogic = collider.GetComponent<ColliderResponder>()?.GetEntityLogic() as BaseEntityLogic;
        if (enetityLogic is PlayerLogic playerLogic)
        {
            checkActionState(playerLogic, false);
        }
    }


    void checkActionState(PlayerLogic playerLogic, bool isEnterArea = true)
    {
        switch (curDisableType)
        {
            case DisableAcitonType.None:
            case DisableAcitonType.SquatUp:
                playerLogic.isEnableSquatUp = !isEnterArea; // 进入当前区域则禁用

                break;
            case DisableAcitonType.Jump:
                playerLogic.isEnableJump = !isEnterArea;
                break;
                break;
            default:
                break;
        }
    }
}
