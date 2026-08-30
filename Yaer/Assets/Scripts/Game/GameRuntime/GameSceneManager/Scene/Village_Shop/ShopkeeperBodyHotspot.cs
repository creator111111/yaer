using Game.GameMgr;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_Shop
{
    /// <summary>
    /// 商店老板娘立绘 Head / Chest 热区：接收 Physics2D 射线点击，转交
    /// <see cref="Village_ShopSceneManager.TryTriggerShopkeeperSpecial"/>。
    /// </summary>
    /// <remarks>
    /// 原因：合层是世界空间 SpriteRenderer，无 Canvas；须用 Collider2D + Physics2DRaycaster，
    /// 不能挂裸 UGUI Image（方案 A 不可行，见 0828 溯源报告方案 B）。
    /// 替代方案：World Space Canvas 透明 Image；或 Overlay 分区 Button——本期不采用。
    /// 禁止：在 Update 里用 mousePosition 轮询判点。
    /// </remarks>
    [RequireComponent(typeof(Collider2D))]
    public class ShopkeeperBodyHotspot : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>热区部位：决定触发哪条特殊对白 Prefab。</summary>
        public enum HotspotKind
        {
            /// <summary>点头 → <c>Village_ShopHead</c>（与 Prefab 文件名对齐，0829 方案 A）</summary>
            Head = 0,

            /// <summary>点胸（店内 C1～C5）→ <c>Village_ShopChest</c>（与 Prefab 文件名对齐，0830 方案 A）</summary>
            Chest = 1,
        }

        [SerializeField]
        [Tooltip("Head=点头对白；Chest=点胸店内段（止于 C5，不含树屋）。")]
        private HotspotKind hotspotKind = HotspotKind.Head;

        /// <summary>当前热区类型（供 GSM / 调试读取）。</summary>
        public HotspotKind Kind => hotspotKind;

        /// <summary>
        /// EventSystem + Physics2DRaycaster 命中本 Collider 时回调。
        /// 不直接调 StoryGSM，统一走场景管理器封装（Hide UI / 互斥 / 热区开关）。
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            var gsm = GameManager.GetGameSceneManager() as Village_ShopSceneManager;
            if (gsm == null)
            {
                Debug.LogWarning(
                    "[ShopHotspot] 当前场景不是 Village_ShopSceneManager，忽略点击。",
                    this);
                return;
            }

            var storyName = hotspotKind == HotspotKind.Head
                ? Village_ShopSceneManager.ShopkeeperHeadClickStoryName
                : Village_ShopSceneManager.ShopkeeperChestClickStoryName;

            bool started = gsm.TryTriggerShopkeeperSpecial(storyName);
            Debug.Log(
                $"[ShopHotspot] click kind={hotspotKind} story={storyName} started={started}",
                this);
        }
    }
}
