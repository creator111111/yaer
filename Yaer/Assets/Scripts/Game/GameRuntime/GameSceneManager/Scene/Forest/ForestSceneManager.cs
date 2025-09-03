using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
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
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.GameSceneManager.Scene.Forest
{
    public class ForestSceneManager : BaseGameSceneManager
    {
        private ForestSceneData sceneData;

        SoundToggleComponent bgmSoundCpn;
        SoundToggleComponent soundSfxCpn_2; // 随机播放的风吹树叶音效组件
        SoundToggleComponent soundSfxCpn_3; // 随机播放的鸟叫声组件
        float timeCount_2 = 0;
        float timeCount_3 = 0;
        float timeDistance_2 = 10; // 风吹树叶音效播放时间间隔
        float timeDistance_3 = 20; // 鸟叫声时间间隔
        public override void OnInit()
        {
            base.OnInit();
            timeCount_2 = timeDistance_2 - 1;
            timeCount_3 = timeDistance_3 - 1;

            nowSceneName = SceneName.ForestScene;

            // 记录位置
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.HomeToJingLingVillage);
            
            sceneData = GetArchiveData<ForestSceneData>();
            
            GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<ForestSceneKingLogic>().SetObjActive(false);
            GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<ForestSceneLaiLogic>().SetObjActive(!sceneData.homeDoorStoryComplete);
            GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<ForestSceneLinEnLogic>().SetObjActive(false);

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
                timeDistance_2 = GameTools.getRandomIntNum(10, 15);// 下次播放时间间隔不确定
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

            // 门口剧情没有完成，取消摄像机跟随
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

        // 播放鸟叫
        void PlayBirdAudio()
        {
            var baseName = "鸟叫{0}.mp3";
            var randomIndex = GameTools.getRandomIntNum(1, 3);
            var realName = string.Format(baseName, randomIndex);
            soundSfxCpn_3.ChangeSoundRes(realName);
            soundSfxCpn_3.PlaySound();
        }

        // 播放风声
        void PlayWindAudio()
        {
            soundSfxCpn_2.PlaySound();
        }

        public override void initAllSceneMonster()
        {
            
        }
    }
}