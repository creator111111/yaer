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
    /// 肯姆尼村长家室内（<see cref="SceneName.Village_Chief_House"/>）场景管理器。
    /// 行为对齐 <see cref="Village_HomeScene1SceneManager"/>：室内脚步、KenMuNi 地点、正确 nowSceneName。
    /// </summary>
    /// <remarks>
    /// 原因：场景曾误挂 <c>ForestSceneManager</c>（nowSceneName/EnterPos 指向森林与龙宫），
    /// 从村门进屋会落点错误或黑屏。禁止复用 Forest GSM 将就酋长家。
    /// 替代方案：仅改户外门 NextSceneName——室内 EnterPos 仍不匹配，不采用。
    /// </remarks>
    public class Village_Chief_HouseSceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName、EnterPosConfig；必须与场景文件名一致。
            nowSceneName = SceneName.Village_Chief_House;

            // 存档「当前地点」显示肯姆尼。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            var lastScene = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageChiefHouseDebug] lastScene={lastScene} place={PlaceName.KenMuNi}");
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
