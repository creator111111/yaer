using Cysharp.Threading.Tasks;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Camera")]
    [Name("相机跟随主角")]
    public class CameraFollowPlayerActionTask : ActionTask
    {
        private CameraComponentGSM cameraMgr;
        private PlayerLogic player;

        public BBParameter<bool> isFollowPlayer = true; // 是否跟随玩家

        protected override string OnInit()
        {
            cameraMgr = GameManager.GetGameSceneManager().GetModule<CameraComponentGSM>();
            player = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            return base.OnInit();
        }

        protected override string info
        {
            get
            {
                if (isFollowPlayer.value)
                {
                    return "相机跟随主角";
                }
                else
                {
                    return "相机锁定在原地";
                }
            }
        }

        protected override void OnExecute() 
        {
            Follow().Forget();
        }

        private async UniTask Follow()
        {
            await UniTask.WaitUntil(() => !cameraMgr.IsLock);
            if (isFollowPlayer.value)
            {
                cameraMgr.SetFollow(player.transform);
            }
            else
            {
                //cameraMgr.SetFollow(null);
                cameraMgr.SetLock(true);
            }
            EndAction();
        }
    }
}