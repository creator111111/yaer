using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

// 场景故事触发基类
public class BaseSceneStoryObj : MonoBehaviour
{
    public BaseGameSceneManager sceneMgr;
    public BaseSceneStoryMgr storyMgr; // 故事管理器
    public bool hasStartStory = false; // 是否开始故事
    public bool hasEndStory = false; // 是否结束故事
    //======================事件触发时需要将玩家限制在一个区域
    public GameObject leftColliderNode; // 左侧碰撞体
    public GameObject rightColliderNode; // 右侧碰撞体

    // Start is called before the first frame update
    public virtual void Start()
    {
        
        InitStoryMgr();
        if (sceneMgr == null)
        {
            sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
        }
        // 故事对象初始化时检查是否已经不需要再触发该事件了
        if (storyMgr != null)
        {
            storyMgr.sceneStoryObj = this;
            if (storyMgr.hasPassEvent){ HideSceneObj(); }
        }
    }

    public virtual void InitStoryMgr()
    {
        // 子类需要在这里设置属于自己的管理类
    }

    // Update is called once per frame
    public virtual void Update()
    {
    }

    public virtual void BattleStoryStartOrEnd(bool isStart)
    {
        // 是否激活碰撞体限制玩家行动
        if (leftColliderNode != null) { leftColliderNode.SetActive(isStart); }
        if (rightColliderNode != null) {  rightColliderNode.SetActive(isStart); }
        hasStartStory = isStart;
        hasEndStory = !isStart;
        // 子类在此基础上实现自己的逻辑
    }

    // 隐藏事件相关的场景对象
    public virtual void HideSceneObj()
    {
        if (leftColliderNode != null) { leftColliderNode.SetActive(false); }
        if (rightColliderNode != null) { rightColliderNode.SetActive(false); }
    }

    public virtual void OnDestroy()
    {
        // 故事节点被销毁时设置管理器退出当前故事
        if (storyMgr != null) { storyMgr.ExitCurStory(); }
    }
}
