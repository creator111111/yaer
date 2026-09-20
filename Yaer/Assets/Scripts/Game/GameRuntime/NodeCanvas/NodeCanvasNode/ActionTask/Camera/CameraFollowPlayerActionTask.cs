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

        /// <summary>
        /// 是否用手推（forceSnap）对齐到玩家。
        /// 默认 false：推镜拉回后相机已在落点附近，再手推会二次屏闪（0913 走廊吃羊等）。
        /// 需要「远处瞬切/手推收束」的图在 NodeCanvas 勾 true。
        /// </summary>
        public BBParameter<bool> forceSnapToTarget = false;

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
                // 0913：默认不 forceSnap，避免拉回后二次手推闪一下；已由 CameraMove 软交接到 EndPos/玩家。
                // 替代：本节点勾 forceSnapToTarget；或 Forest 式短黑幕掩护手推。
                cameraMgr.SetFollow(player.transform, onComplete: null, forceSnapToTarget: forceSnapToTarget.value);
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
