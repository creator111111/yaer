using Cinemachine;
using Game.GameMgr;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_Shop
{
    /// <summary>
    /// 肯姆尼村商店 <see cref="SceneName.Village_Shop"/> 纯 UI 场景管理器。
    /// 无玩家、无走路；ESC 菜单靠基类默认挂载的 <c>InputComponentGSM</c>。
    /// </summary>
    /// <remarks>
    /// 相机策略：场景挂齐 GSM 相机链避免未绑定 Error；进场后锁死并强制对准「商店界面合层」。
    /// 原因：无 Follow 时 FramingTransposer 可能把机位带偏，只 CancelFollow 仍看不见海报。
    /// 替代方案：把合层整页搬进 Overlay Canvas（0704 长线）——本阶段不采用。
    /// </remarks>
    public class Village_ShopSceneManager : BaseGameSceneManager
    {
        /// <summary>世界空间美术合层根节点名（与 Hierarchy 一致）。</summary>
        private const string ShopCompositeRootName = "商店界面合层";

        /// <summary>合层背景中心附近的默认机位（与执行说明一致）；找不到合层时兜底。</summary>
        private static readonly Vector3 FallbackCameraWorldPos = new Vector3(0.65f, -0.14f, -10f);

        /// <summary>对齐 HomeScene2 的正交尺寸，刚好框住 19.2×10.8 背景。</summary>
        private const float ShopOrthoSize = 5.4f;

        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName 匹配、全局查询。
            nowSceneName = SceneName.Village_Shop;

            // 存档「当前地点」仍显示肯姆尼；商店不单独占 PlaceName。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            // 纯 UI 不跑 InitPlayer，基类不会自动关 FightingPanel；进店显式关掉血条 HUD。
            // 时机必须在 OnInit（黑幕仍全黑）：OnEnterScene 要等黑幕渐出结束才触发，关晚了会闪一下。
            // 原因：上一场景（如村里）打开的 FightingPanel 会跨场景残留。
            // 替代方案：①仍在 OnEnterScene 关（会闪）；②改基类 canCreatePlayer==false 也调 OpenFightingPanel——影响面大，不采用。
            CloseFightingPanelIfOpen();
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 再写一次，避免切场顺序覆盖地点键。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            // 纯 UI 无玩家：不会走 PlayerLogic.LoadingSceneEndHandle 放行菜单；显式放行 ESC。
            // 若看不到下方「subscribed ESC」日志，说明 InitModules 曾在 Input 前中断（已修 Map 空引用）。
            var input = GetModule<InputComponentGSM>();
            if (input != null)
            {
                input.SetAllowOpenMenu(true);
                Debug.Log("[VillageShopDebug] SetAllowOpenMenu(true)");
            }
            else
            {
                Debug.LogError("[VillageShopDebug] InputComponentGSM 缺失，ESC 无法开菜单。", this);
            }

            // 无玩家：取消跟拍并锁死，再强制对准合层（Brain 关掉，避免 CM 每帧改机位）。
            LockShopCameraPipeline();
            FocusMainCameraOnShopComposite();

            // 验收：确认从 Door_Shop 进来时 LastSceneName 为 Village_KenMuNi1
            var last = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageShopDebug] enter Village_Shop lastScene={last}");
        }

        /// <summary>
        /// 若 FightingPanel（血条 HUD）仍开着则关掉；供 OnInit 在全黑阶段调用，避免渐出后闪一下。
        /// </summary>
        private static void CloseFightingPanelIfOpen()
        {
            var fightingPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            var ui = GameManager.GetGMComponent<UIComponentGM>();
            if (ui == null)
            {
                return;
            }

            if (ui.GetUIForm(fightingPath) != null)
            {
                ui.CloseUIForm(fightingPath);
                Debug.Log("[VillageShopDebug] CloseUIForm FightingPanel (OnInit, while black)");
            }
        }

        /// <summary>
        /// 取消跟拍 + 加锁；CameraComponent 未绑定时只打日志，不抛空引用中断进场。
        /// </summary>
        private void LockShopCameraPipeline()
        {
            var cameraGsm = GetModule<CameraComponentGSM>();
            if (cameraGsm == null)
            {
                Debug.LogWarning("[VillageShopDebug] CameraComponentGSM 缺失，跳过 CancelFollow/SetLock。", this);
                return;
            }

            if (cameraGsm.CameraComponent == null)
            {
                Debug.LogWarning(
                    "[VillageShopDebug] CameraComponentGSM.cameraComponent 未绑定：请检查 SceneManager/Camera 引用。",
                    this);
                cameraGsm.SetLock(true);
                return;
            }

            cameraGsm.CancelFollow();
            cameraGsm.SetLock(true);
        }

        /// <summary>
        /// 强制主相机对准合层精灵包围盒中心，并关闭 CinemachineBrain，避免无 Follow 时被带偏。
        /// </summary>
        private void FocusMainCameraOnShopComposite()
        {
            var composite = GameObject.Find(ShopCompositeRootName);
            if (composite != null)
            {
                composite.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[VillageShopDebug] 未找到「{ShopCompositeRootName}」。", this);
            }

            var cam = ResolveShopMainCamera();
            if (cam == null)
            {
                Debug.LogError("[VillageShopDebug] 找不到主相机，合层无法显示。", this);
                return;
            }

            // 商店无跟拍：关掉 Brain，否则 FramingTransposer 在 Follow=null 时可能把机位拽飞/留在村里坐标。
            var brain = cam.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                brain.enabled = false;
            }

            cam.enabled = true;
            if (!cam.CompareTag("MainCamera"))
            {
                cam.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.depth = -1;
            // 保证 Default 层（合层 Sprite）在裁剪掩码内；~0 即 Everything。
            cam.cullingMask = ~0;

            // 正交尺寸固定 5.4（对齐 HomeScene2 / 19.2×10.8 背景）。
            // 原因：曾用「全合层包围盒 × 1.08」自动放大，5.4→约 5.83，进店会被刷成 5.8 并出黑边。
            // 替代方案：只按「背景」一张图算 need —— 仍可能因分辨率比例漂移，商店锁死固定值更稳。
            var ortho = ShopOrthoSize;
            var focusXy = FallbackCameraWorldPos;
            var rendererCount = 0;

            if (composite != null)
            {
                // 优先对准名为「背景」的精灵中心；找不到再退回文档兜底坐标。
                var bg = composite.transform.Find("背景");
                if (bg != null)
                {
                    focusXy = bg.position;
                    var bgRenderer = bg.GetComponent<SpriteRenderer>();
                    if (bgRenderer != null)
                    {
                        focusXy = bgRenderer.bounds.center;
                    }
                }

                var renderers = composite.GetComponentsInChildren<SpriteRenderer>(true);
                rendererCount = renderers != null ? renderers.Length : 0;
            }

            var camPos = new Vector3(focusXy.x, focusXy.y, -10f);
            cam.transform.position = camPos;
            cam.orthographicSize = ortho;

            // 若 vcam 仍在，同步机位，避免以后重新启用 Brain 时跳回旧坐标。
            var cameraGsm = GetModule<CameraComponentGSM>();
            var vcam = cameraGsm != null && cameraGsm.CameraComponent != null
                ? cameraGsm.CameraComponent.VirtualCamera
                : null;
            if (vcam != null)
            {
                vcam.transform.position = camPos;
                vcam.m_Lens.OrthographicSize = ortho;
                vcam.Follow = null;
                vcam.PreviousStateIsValid = false;
            }

            Debug.Log(
                $"[VillageShopDebug] focus camPos={camPos} ortho={ortho:F2} " +
                $"camEnabled={cam.enabled} brain={(brain != null && brain.enabled)} " +
                $"composite={(composite != null)} spriteRenderers={rendererCount} " +
                $"vcam={(vcam != null)}",
                this);
        }

        /// <summary>优先 Camera.main；否则找 Tag=MainCamera；再否则场景里第一台非 UI 相机。</summary>
        private static Camera ResolveShopMainCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

            var tagged = GameObject.FindWithTag("MainCamera");
            if (tagged != null)
            {
                var taggedCam = tagged.GetComponent<Camera>();
                if (taggedCam != null)
                {
                    return taggedCam;
                }
            }

            var all = Object.FindObjectsOfType<Camera>();
            for (var i = 0; i < all.Length; i++)
            {
                // UICamera 只渲 UI 层（mask 常为 1<<5），跳过它。
                if (all[i] != null && all[i].cullingMask != (1 << 5))
                {
                    return all[i];
                }
            }

            return all != null && all.Length > 0 ? all[0] : null;
        }

        /// <summary>室内语义：虽无玩家脚步，与民居室内保持一致。</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
