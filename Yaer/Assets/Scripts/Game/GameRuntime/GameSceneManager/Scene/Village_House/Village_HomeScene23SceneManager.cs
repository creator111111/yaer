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
    /// 肯姆尼民居 <see cref="SceneName.Village_HomeScene23"/> 室内场景管理器。
    /// 行为对齐 <see cref="Village_HomeScene2SceneManager"/> 的「室内」最小集：
    /// Home 动画（Config.isFightingScene=false）、室内脚步、正确 nowSceneName。
    /// </summary>
    /// <remarks>
    /// 原因：场景曾误挂 <see cref="Village_House4SceneManager"/> 且 nowSceneName 写成 House4，
    /// 与文件名/村门目标不一致，回村落点与存档身份漂移。
    /// 2026-08-04：由 Village_HomeScene4 全量改名为 Village_HomeScene23（保留本脚本 .meta GUID）。
    /// 替代方案：强行改 House4Manager.nowSceneName——会破坏「真·House4」身份，不采用。
    /// </remarks>
    public class Village_HomeScene23SceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName、EnterPosConfig 双向表；必须与场景文件名一致。
            nowSceneName = SceneName.Village_HomeScene23;

            // 存档「当前地点」仍显示肯姆尼；与其它村内民居一致。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            var lastScene = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageHomeScene23Debug] lastScene={lastScene} place={PlaceName.KenMuNi}");
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
