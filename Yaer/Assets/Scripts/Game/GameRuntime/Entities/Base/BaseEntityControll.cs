using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.Static.Enum;
using UnityEngine;

enum ENTITY_TYPE
{
    None, // 未知实体
    PLAYER, // 玩家
    MONSTER, // 怪物
    NPC, // NPC
    SCENE_SPC_ITEM, // 特殊场景可互动实体
}

// 交互门禁 + 按键提示的通用控制器
public class BaseEntityControll : MonoBehaviour
{
    public int entityType = (int)ENTITY_TYPE.None; // 实体类型
    public bool canTouchWithPlayer = true; // 是否允许与玩家交互（开关门禁）

    public SceneEntity sceneEntity = null; // 当前控制器控制的场景实体
    public InteractiveComponent interactiveComponent = null; // 当前实体中的可交互组件

    public float keyTipsPosX = 0; // 按键提示节点在本地 X 的位置
    public float keyTipsPosY = 0; // 按键提示节点在本地 Y 的位置

    // Start is called before the first frame update
    void Start()
    {
        // 为可交互组件补齐反向引用 + 订阅点击事件
        if (interactiveComponent != null)
        {
            interactiveComponent.entityControll = this;
            interactiveComponent.onClickInteractiveEvent += (interactiveComponent) => OnInteractiveComponentTriggerWithPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 将按键提示节点挂到当前实体上（前提：canTouchWithPlayer=true）
    public void AddKeyTipsNode(GameObject keyTipsNode, ControlInputType inputType = ControlInputType.Interact)
    {
        if (!canTouchWithPlayer) { return; }
        if (keyTipsNode != null)
        {
            keyTipsNode.SetActive(true);

            // 把提示节点作为当前实体的子物体
            keyTipsNode.transform.SetParent(gameObject.transform, false);

            // 重置旋转，避免出现奇怪的朝向
            keyTipsNode.transform.rotation = new Quaternion(0, 0, 0, 0);

            // 设置提示节点的本地坐标
            keyTipsNode.transform.localPosition = new Vector3(keyTipsPosX, keyTipsPosY, 0);

            // 初始化提示节点显示状态（此处不开“触发变色”，交给交互触发事件控制）
            keyTipsNode.GetComponent<KeyTipsNodeSrc>().ShowStoryTriggerEffect(false, inputType);

            // var textKeyTips = UIUtils.findChild(keyTipsNode, "textKey");
            // textKeyTips.SetActive(!isMouseTips);
        }
    }

    // 隐藏按键提示节点（可选：是否销毁）
    public void RemoveKeyTipsNode(GameObject keyTipsNode, bool isRealRemove = false)
    {
        keyTipsNode.SetActive(false);

        // 若需要真实销毁（一般不建议频繁销毁/重建）
        if (isRealRemove)
        {
            keyTipsNode = null;
            Destroy(keyTipsNode);
        }
    }

    // 当任意可交互实体开始被玩家点击/交互时触发
    // 作用：如果玩家身上的 keyTipsNode 处于激活状态，则触发 KeyTipsNodeSrc 的触发态特效/变色
    public void OnInteractiveComponentTriggerWithPlayer(ControlInputType inputType = ControlInputType.Interact)
    {
        var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
        if (playerLogic)
        {
            if (playerLogic.keyTipsNode && playerLogic.keyTipsNode.activeSelf)
            {
                playerLogic.keyTipsNode.GetComponent<KeyTipsNodeSrc>().ShowStoryTriggerEffect(true, inputType);
            }
        }
    }
}