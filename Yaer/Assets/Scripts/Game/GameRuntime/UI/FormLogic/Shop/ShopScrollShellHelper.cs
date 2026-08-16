using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店列表 Scroll 壳交互修正（Fix-S1 / Fix-S2 / Fix-T1 / SF Soft Fade）：
    /// Viewport 去冗余 Image、提高滚轮灵敏度、Scroll 根 Image 全透明且保留射线、
    /// RectMask2D 上下 Softness 虚化过渡（alpha 渐隐，非高斯模糊）。
    /// Editor Bake 与 Play Awake 均可调用，避免场景未跑 Setup 时滚轮仍转不动、灰底残留或刀切硬边。
    /// </summary>
    public static class ShopScrollShellHelper
    {
        public const float DefaultScrollSensitivity = 30f;

        /// <summary>
        /// RectMask2D 左右软边像素宽。列表一般不横溢，默认 0 保持左右贴边硬切。
        /// 若左右也被 Bar_BG 内缘硌到，可调到 4～8。
        /// </summary>
        public const int DefaultMaskSoftnessX = 0;

        /// <summary>
        /// RectMask2D 上下软边像素宽（约 ⅓～½ 行高可感知）。
        /// 可调区间 24～48：偏硬 +8，顶底太虚/有效区显矮 -8。
        /// </summary>
        public const int DefaultMaskSoftnessY = 32;

        private const string ViewportName = "Viewport";

        /// <summary>对 Buy / Sell Scroll 根节点应用滚动与裁剪修正。</summary>
        public static void ApplyInteractionFixes(Transform scrollRoot)
        {
            if (scrollRoot == null)
            {
                return;
            }

            var scrollRect = scrollRoot.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.scrollSensitivity = DefaultScrollSensitivity;
            }

            EnsureScrollRootRaycastTarget(scrollRoot);
            StripRedundantViewportImage(scrollRoot);
            // SF：边界虚化必须走 Helper 常量，禁止在 Bake/Setup 各写魔法数
            EnsureViewportSoftMask(scrollRoot);
        }

        /// <summary>
        /// 给 Viewport 的 RectMask2D 写入默认 Softness，让滚出顶/底的行 alpha 渐隐，而不是直角刀切。
        /// 与 Fix-S1「Viewport 无 Image」并存：软边靠 Mask，不需要半透明 Image。
        /// 无 Mask 时不擅自 Add（建壳由 Bake/Setup 负责）。
        /// 替代方案：仅当 softness==(0,0) 时写入，可保留场景 Fine-tune，但 Buy/Sell 易漂移；本阶段强制写常量。
        /// </summary>
        private static void EnsureViewportSoftMask(Transform scrollRoot)
        {
            var viewport = scrollRoot.Find(ViewportName);
            if (viewport == null)
            {
                return;
            }

            var mask = viewport.GetComponent<RectMask2D>();
            if (mask == null)
            {
                return;
            }

            mask.softness = new Vector2Int(DefaultMaskSoftnessX, DefaultMaskSoftnessY);
        }

        /// <summary>RectMask2D 已负责裁剪时，Viewport 上的 Image 多余且可能干扰射线。</summary>
        private static void StripRedundantViewportImage(Transform scrollRoot)
        {
            var viewport = scrollRoot.Find(ViewportName);
            if (viewport == null)
            {
                return;
            }

            if (viewport.GetComponent<RectMask2D>() == null)
            {
                return;
            }

            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(viewportImage);
            }
            else
            {
                Object.DestroyImmediate(viewportImage);
            }
        }

        /// <summary>
        /// Viewport 无 Image 后，Scroll 根节点仍需 Image 接收拖拽/滚轮命中。
        /// Fix-T1：无论新建或 Unity 默认 ScrollView 自带 Image，统一 sprite=null、alpha=0。
        /// UGUI 在 raycastTarget=true 时 alpha=0 仍参与命中检测。
        /// 替代方案：若极端机型射线异常，可回退 alpha=0.01（肉眼仍视为全透明）。
        /// </summary>
        private static void EnsureScrollRootRaycastTarget(Transform scrollRoot)
        {
            var image = scrollRoot.GetComponent<Image>();
            if (image == null)
            {
                image = scrollRoot.gameObject.AddComponent<Image>();
            }

            image.sprite = null;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
        }
    }
}
