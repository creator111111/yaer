using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.MainNPC;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path.Sound;
using GameFramework.UnityRuntime.UI;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi
{
    /// <summary>
    /// <c>Village_KenMuNi1</c> 专用场景管理器：业务与 <see cref="Game.GameRuntime.GameSceneManager.Scene.Forest.ForestSceneManager"/> 对齐（同套 <see cref="ForestSceneData"/>、
    /// 莱/林恩实体、BGM/SFX、战斗面板门控等），避免进村后剧情与音效逻辑回退。
    /// <para>
    /// 与森林管理器的<strong>必须差异</strong>：<see cref="SceneName.Village_KenMuNi1"/> 作为当前场景名；
    /// <see cref="PlaceName.KenMuNi"/> 作为存档「当前地点」内部键，使读档 UI 显示「肯尼姆」三语字典项（不动存档管线代码）。
    /// </para>
    /// <para>
    /// <strong>替代方案</strong>：若将来要以 Unity 场景名字符串作地点键，需与 <see cref="SceneName"/> 职责分离，避免表维护混淆；当前采用独立常量 <see cref="PlaceName.KenMuNi"/>。
    /// </para>
    /// </summary>
    public class Village_KenMuNiSceneManager : BaseGameSceneManager
    {
        /// <summary>本村复用森林场景存档结构（门口剧情、莱/林恩显隐等）。</summary>
        private ForestSceneData sceneData;

        SoundToggleComponent bgmSoundCpn;
        /// <summary>随机风吹树叶类环境音。</summary>
        SoundToggleComponent soundSfxCpn_2;
        /// <summary>随机鸟叫环境音。</summary>
        SoundToggleComponent soundSfxCpn_3;
        float timeCount_2 = 0;
        float timeCount_3 = 0;
        /// <summary>风吹音效最小间隔基数，与随机区间配合（与森林逻辑一致）。</summary>
        float timeDistance_2 = 10;
        /// <summary>鸟叫音效间隔基数（与森林逻辑一致）。</summary>
        float timeDistance_3 = 20;

        public override void OnInit()
        {
            base.OnInit();
            timeCount_2 = timeDistance_2 - 1;
            timeCount_3 = timeDistance_3 - 1;

            // 逻辑自报场景名：供全局查询、切场景等；不可再写 ForestScene，否则与真实场景不一致。
            nowSceneName = SceneName.Village_KenMuNi1;

            // 写入 PlayerMapData，存档标题链走 PlaceName 字典（见执行说明 §2.3）。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            sceneData = GetArchiveData<ForestSceneData>();

            // 村庄场景未必摆放森林同款 NPC（莱/王/林恩）；缺失时跳过显隐，避免 Awake 中断导致 SceneManager 被禁用。
            // 替代方案：从 ForestScene 复制对应实体到 objRoot 并刷新 sceneObjs，则可恢复与森林一致的剧情显隐。
            TrySetSceneEntityActive<ForestSceneKingLogic>(false);
            TrySetSceneEntityActive<ForestSceneLaiLogic>(!sceneData.homeDoorStoryComplete);
            TrySetSceneEntityActive<ForestSceneLinEnLogic>(false);

            var bgmNode = UIUtils.findChild(gameObject, "BGM");
            bgmSoundCpn = bgmNode.GetComponent<SoundToggleComponent>();
            bgmSoundCpn.gameObject.SetActive(sceneData.homeDoorStoryComplete);
            var sfxNode_2 = UIUtils.findChild(gameObject, "SFX_2");
            soundSfxCpn_2 = sfxNode_2.GetComponent<SoundToggleComponent>();
            var sfxNode_3 = UIUtils.findChild(gameObject, "SFX_3");
            soundSfxCpn_3 = sfxNode_3.GetComponent<SoundToggleComponent>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            timeCount_2 += Time.deltaTime;
            if (timeCount_2 > timeDistance_2)
            {
                timeCount_2 = 0;
                timeDistance_2 = GameTools.getRandomIntNum(10, 15);
                PlayWindAudio();
            }
            timeCount_3 += Time.deltaTime;
            if (timeCount_3 > timeDistance_3)
            {
                timeCount_3 = 0;
                PlayBirdAudio();
            }
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 场景加载完成后再写一次当前地点：避免与切场景顺序相关的逻辑在 Awake/OnInit 之后又改回其它地图键；
            // 存档标题读取的是 PlayerMapData.GetNowPlace()，此处与 OnInit 双写保证验收稳定。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            if (sceneData.homeDoorStoryComplete == false)
            {
                GetModule<CameraComponentGSM>().CancelFollow();
                GetModule<CameraComponentGSM>().SetLock(true);
            }
        }

        protected override void OnOpenFightingPanel(UIFormLogic uIFormLogic)
        {
            var FightingFormLogic = uIFormLogic as FightingFormLogic;
            if (sceneData.homeDoorStoryComplete == false)
            {
                FightingFormLogic.UpdateBattleImageVisiable(false);
            }
        }

        public override TerrainType GetCurSceneTerrainType()
        {
            return TerrainType.LandType;
        }

        /// <summary>播放随机鸟叫资源（与森林相同命名约定）。</summary>
        void PlayBirdAudio()
        {
            var baseName = "鸟叫{0}.mp3";
            var randomIndex = GameTools.getRandomIntNum(1, 3);
            var realName = string.Format(baseName, randomIndex);
            soundSfxCpn_3.ChangeSoundRes(realName);
            soundSfxCpn_3.PlaySound();
        }

        /// <summary>播放风声（SFX_2）。</summary>
        void PlayWindAudio()
        {
            soundSfxCpn_2.PlaySound();
        }

        public override void initAllSceneMonster()
        {
        }

        /// <summary>
        /// 按类型设置场景实体显隐；本村未配置该逻辑组件时静默跳过。
        /// </summary>
        void TrySetSceneEntityActive<T>(bool active) where T : BaseSceneEntityLogic
        {
            var logic = GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<T>();
            if (logic != null)
            {
                logic.SetObjActive(active);
            }
        }
    }
}
