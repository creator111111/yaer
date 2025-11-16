using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.PureMVC;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.SceneEntities.ForestScene;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Scene.Forest;
using Game.Static.Path.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Story.ForestSceneFirstEnter
{
    public class ForestSceneLinEnStory : BaseSceneEntityLogic
    {
        [SerializeField] private GameObject linEn;
        [SerializeField] private Transform cameraTrack;

        public void PrepaerPlay()
        {
            linEn.gameObject.SetActive(true);
        }

        public void LinEnStoryLinEnMove()
        {
            linEn.GetComponent<Animator>().Play("LinEnStoryLinEnMove");
        }

        public void OnDialogueEnd()
        {
            SceneManager.GetModule<CameraComponentGSM>().SetLock(false);
            SceneManager.GetModule<CameraComponentGSM>().SetFollow(cameraTrack);
            SceneManager.GetModule<CameraComponentGSM>().SetLock(true);

            GetComponent<Animator>().Play("CameraTrack");
            SceneManager.GetArchiveData<ForestSceneData>().homeDoorStoryComplete = true;

            //控制背景音乐
            var sounds = FindObjectOfType<ForestSceneManager>().gameObject.GetComponentsInChildren<SoundToggleComponent>();
            foreach (var sound in sounds)
            {
                if (sound.GetSoundType == SoundType.BGM)
                    sound.gameObject.SetActive(false);
                else if (sound.GetSoundType == SoundType.SFX)
                    sound.gameObject.SetActive(true);
            }
        }

        public void OnCameraMoveEnd()
        {
            SceneManager.GetModule<SceneEntityComponentGSM>().GetAllSceneEntities().
               Find(X => X.name == "Soldier1").
               transform.GetComponent<ForestSceneSoldierHeadTurn>().SetNormalHead();
            
            Invoke("SwitchCameraToPlayer", 2.0f);
        }

        private void SwitchCameraToPlayer()
        {
            SceneManager.GetModule<CameraComponentGSM>().SetLock(false);
            // 摄像机移动回雅尔
            SceneManager.GetModule<CameraComponentGSM>().SetFollow(GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>().transform);
        }
    }
}