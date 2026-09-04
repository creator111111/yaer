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
    /// 肯姆尼民居 <see cref="SceneName.Village_HomeScene1"/> 室内场景管理器。
    /// 行为对齐 <see cref="Village_HomeScene2SceneManager"/> / <see cref="Village_HomeScene23SceneManager"/>：
    /// 室内脚步、KenMuNi 地点、正确 nowSceneName。
    /// </summary>
    /// <remarks>
    /// 原因：场景曾误挂龙宫 <c>HomeScene1Manager</c>（nowSceneName=HomeScene1、PlaceName.Home，
    /// 且可能 NRE 取 HomeScene1Xiaer）。禁止改龙宫 Manager 将就村屋。
    /// 替代方案：村门改指龙宫 HomeScene1——会混存档/地点，不采用。
    /// </remarks>
    public class Village_HomeScene1SceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName、EnterPosConfig；必须与场景文件名一致。
            nowSceneName = SceneName.Village_HomeScene1;

            // 存档「当前地点」显示肯姆尼；勿写 PlaceName.Home（龙宫身份）。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            var lastScene = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageHomeScene1Debug] lastScene={lastScene} place={PlaceName.KenMuNi}");
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
