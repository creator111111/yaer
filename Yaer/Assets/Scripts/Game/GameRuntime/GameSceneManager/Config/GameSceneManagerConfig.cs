using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Config
{
    [CreateAssetMenu(fileName = "GameSceneManagerConfig", menuName = "Config/GameSceneManagerConfig")]
    public class GameSceneManagerConfig: ScriptableObject
    {
        // 场景设置
        public bool canMove; // 当前场景是否允许移动
        public bool canCreatePlayer; // 当前场景允许创建玩家
        public bool isPlayingScene; // 是否是游戏场景
        public bool isFightingScene; // 是否是战斗场景
        public bool canRaycast; // 是否允许射线检测
        public bool canSave; // 是否允许保存非默认位置
    }
}