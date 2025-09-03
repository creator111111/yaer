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
    PLAYER, // 人物
    MONSTER, // 怪物
    NPC, // NPC
    SCENE_SPC_ITEM, // 特殊场景可互动实体
}

// 场景实体控制器,用来处理实体相关的一些额外逻辑
public class BaseEntityControll : MonoBehaviour
{
    public int entityType = (int)ENTITY_TYPE.None; // 场景实体类型
    public bool canTouchWithPlayer = true; // 是否能够与玩家产生交互

    public SceneEntity sceneEntity = null; // 当前控制器控制的场景中的实体

    public InteractiveComponent interactiveComponent = null; // 当前实体中的可交互组件

    public float keyTipsPosX = 0;// 按键提示节点X坐标的位置
    public float keyTipsPosY = 0;// 按键提示节点Y坐标的位置

    // Start is called before the first frame update
    void Start()
    {
        // 绑定实体控制器
        if (interactiveComponent != null)
        {
            interactiveComponent.entityControll = this;
            interactiveComponent.onClickInteractiveEvent += (interactiveComponent) =>OnInteractiveComponentTriggerWithPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 添加按键提示到某个场景实体中
    public void AddKeyTipsNode(GameObject keyTipsNode, ControlInputType inputType= ControlInputType.Interact)
    {
        if (!canTouchWithPlayer) { return; }
        if (keyTipsNode != null)
        {
            keyTipsNode.SetActive(true);
            // 添加提示节点到当前对象上
            keyTipsNode.transform.SetParent(gameObject.transform, false);
            keyTipsNode.transform.rotation = new Quaternion(0, 0, 0, 0); // 重置旋转
            // 调整显示节点位置
            keyTipsNode.transform.localPosition = new Vector3(keyTipsPosX, keyTipsPosY, 0);
            //var textKeyTips = UIUtils.findChild(keyTipsNode, "textKey");
            //textKeyTips.SetActive(!isMouseTips);

            keyTipsNode.GetComponent<KeyTipsNodeSrc>().ShowStoryTriggerEffect(false, inputType);
            //if (!isMouseTips) {
            //    var realKeyStr = string.Format("<color=white>{0}</color>", keyStr);
            //    GameTools.setTMPUGUIText(textKeyTips, realKeyStr);
            //}
        }
    }

    // 去除提示按钮节点
    public void RemoveKeyTipsNode(GameObject keyTipsNode, bool isRealRemove=false)
    {
        keyTipsNode.SetActive(false);
        if (isRealRemove)
        {
            keyTipsNode = null;
            Destroy(keyTipsNode);
        }
    }

    // 当任意实体组件和玩家开始交互时
    public void OnInteractiveComponentTriggerWithPlayer(ControlInputType inputType = ControlInputType.Interact)
    {
        // 开始交互时，按键提示需要变色处理
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
