using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using GameFramework.UnityRuntime.Utility;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components
{
    public class PlayerAnimaCameraTrackComponent : BaseGFComponentMono, IPlayerComponent
    {
        public PlayerLogic PlayerLogic { get; set; }
        private Transform cameraTrackTsf;

        protected override void OnInit()
        {
            cameraTrackTsf = PlayerLogic.transform.Find("CameraTrack");
            if (cameraTrackTsf is null)
            {
                Log.Error("找不到CameraTrack");
            }
        }

        public void SetMainCameraFollowTrack()
        {
            PlayerLogic.sceneManager.GetModule<CameraComponentGSM>().SetFollow(cameraTrackTsf);
        }

        public void SetMainCameraFollowRoot()
        {
            PlayerLogic.sceneManager.GetModule<CameraComponentGSM>().SetFollow(PlayerLogic.transform);
        }
    }
}