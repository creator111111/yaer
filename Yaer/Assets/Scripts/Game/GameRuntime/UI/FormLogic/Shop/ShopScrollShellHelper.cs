using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店列表 Scroll 壳交互修正（Fix-S1 / Fix-S2 / Fix-T1）：Viewport 去冗余 Image、提高滚轮灵敏度、
    /// Scroll 根 Image 强制全透明（alpha=0）且保留射线命中。
    /// Editor Bake 与 Play Awake 均可调用，避免场景未跑 Setup 时滚轮仍转不动或灰底残留。
    /// </summary>
    public static class ShopScrollShellHelper
    {
        public const float DefaultScrollSensitivity = 30f;

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
