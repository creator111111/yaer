using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House
{
    /// <summary>
    /// 肯姆尼村民居 <see cref="SceneName.Village_House4"/> 室内场景管理器。
    /// 行为对齐 <see cref="Game.GameRuntime.GameSceneManager.Scene.Home1.HomeScene1Manager"/> 的「室内」最小集：
    /// Home 动画（Config.isFightingScene=false）、室内脚步、正确 nowSceneName。
    /// </summary>
    /// <remarks>
    /// 替代方案：继续挂 <c>ForestSceneManager</c> 仅改 Config 可临时试玩，但 nowSceneName 仍为 ForestScene，存档/任务易错，故不采用。
    /// </remarks>
    public class Village_House4SceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName 匹配、全局查询；勿使用 ForestScene。
            nowSceneName = SceneName.Village_House4;

            // 存档「当前地点」仍显示肯姆尼；若将来要「某某的家」单独地名，再增 PlaceName 常量。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 与 Village_KenMuNiSceneManager 类似，加载完成后再写一次 SetNowPlace，避免切场顺序覆盖。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            // 验收用：核对上一场景与地点键是否被其它逻辑改写。
            var lastScene = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageHouse4Debug] lastScene={lastScene} place={PlaceName.KenMuNi}");
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
