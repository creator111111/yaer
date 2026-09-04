using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 对话内：系统 BlackPanel 全黑 → 玩家 SetPos 到场景锚点 → 相机强制跟拍对齐 → 淡出黑幕 → EndAction。
    /// </summary>
    /// <remarks>
    /// 原因（0901 出村长家送树屋）：现网无「黑幕传送玩家」Action；不可无遮罩硬闪；不可 LoadScene。
    /// 替代方案：两段 Story + GSM 串（T3）——多一次 Trigger/记次风险，报告否决。
    /// Destination 默认按场景物体名查找（Prefab 不宜硬绑场景 Transform）。
    /// 转场时保持对白壳打开，仅黑幕盖屏（Q8）。
    /// </remarks>
    [Category("Player")]
    [Name("黑幕传送玩家")]
    public class BlackFadeTeleportPlayerActionTask : ActionTask
    {
        /// <summary>场景内落点物体名；默认报告钉死名。</summary>
        [Tooltip("在当前场景按名查找 Transform（建议 Objects/TeleportTo_YaerTreeHouseDoor）")]
        public BBParameter<string> DestinationObjectName = "TeleportTo_YaerTreeHouseDoor";

        /// <summary>可选：直接拖场景 Transform（运行时实例化对白时通常为空，回退按名找）。</summary>
        [Tooltip("优先于按名查找；对话 Prefab 实例通常无法序列化场景引用")]
        public BBParameter<Transform> Destination;

        [Tooltip("全黑后是否强制相机跟随玩家并 Snap（推荐开）")]
        public BBParameter<bool> SnapCameraToPlayer = true;

        [Tooltip("SetPos 后是否 Flush 村 WalkArea 权威坐标（防脚出区）")]
        public BBParameter<bool> FlushVillageWalkArea = true;

        protected override string info
        {
            get
            {
                var named = DestinationObjectName != null ? DestinationObjectName.value : null;
                if (Destination != null && Destination.value != null)
                {
                    return "黑幕传送 → " + Destination.value.name;
                }

                return string.IsNullOrEmpty(named)
                    ? "黑幕传送玩家（未配置落点）"
                    : "黑幕传送 → " + named;
            }
        }

        protected override void OnExecute()
        {
            var dest = ResolveDestination();
            if (dest == null)
            {
                Debug.LogError(
                    "[BlackFadeTeleport] 未找到落点 Transform。请摆 "
                    + (DestinationObjectName != null ? DestinationObjectName.value : "(null)"),
                    agent != null ? agent.transform : null);
                EndAction(false);
                return;
            }

            OpenSystemBlackFade(black =>
            {
                try
                {
                    TeleportPlayerAndCamera(dest);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[BlackFadeTeleport] 传送异常：" + ex.Message);
                }

                if (black == null)
                {
                    EndAction(true);
                    return;
                }

                // 亮幕后继续下一条 Statement；对白壳保持打开
                black.CloseFormFade(() => EndAction(true));
            });
        }

        private Transform ResolveDestination()
        {
            if (Destination != null && Destination.value != null)
            {
                return Destination.value;
            }

            var name = DestinationObjectName != null ? DestinationObjectName.value : null;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindNamedRecursive(root.transform, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindNamedRecursive(Transform tr, string objectName)
        {
            if (tr.name == objectName)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                var child = FindNamedRecursive(tr.GetChild(i), objectName);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private static void OpenSystemBlackFade(Action<BlackFormLogic> onBlackReady)
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(
                uiPath,
                EUIGroup.System,
                new OpenFormArgs
                {
                    userData = new ShowBlackFormArgs
                    {
                        showType = BlackFadeType.FadeShow,
                        onShowEnd = onBlackReady
                    }
                });
        }

        private void TeleportPlayerAndCamera(Transform dest)
        {
            var player = GameManager.GetGMComponent<EntityComponentGM>()
                .GetEntityLogic<PlayerLogic>();
            if (player == null)
            {
                Debug.LogError("[BlackFadeTeleport] 无 PlayerLogic");
                return;
            }

            // 只改 X/Y，保留村纵深 Z
            player.SetPos(new Vector2(dest.position.x, dest.position.y));

            if (FlushVillageWalkArea == null || FlushVillageWalkArea.value)
            {
                var town = player.componentSystem != null
                    ? player.componentSystem.TryGetComponent<TownPlayerLocomotion>()
                    : null;
                town?.FlushAuthoritativeVillageTransformAfterSceneDepthInject();
            }

            if (SnapCameraToPlayer != null && !SnapCameraToPlayer.value)
            {
                return;
            }

            var cameraMgr = GameManager.GetGameSceneManager()?.GetModule<CameraComponentGSM>();
            if (cameraMgr == null)
            {
                return;
            }

            // SetFollow 在 IsLock 时直接 return：短暂解锁 → Snap → 恢复原锁态
            bool wasLock = cameraMgr.IsLock;
            if (wasLock)
            {
                cameraMgr.SetLock(false);
            }

            cameraMgr.SetFollow(player.transform, null, forceSnapToTarget: true);

            if (wasLock)
            {
                cameraMgr.SetLock(true);
            }
        }
    }
}
