using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店列表 Scroll 壳交互修正（Fix-S1 / Fix-S2）：Viewport 去冗余 Image、提高滚轮灵敏度、保证 Scroll 根可接收射线。
    /// Editor 菜单与 Play 时 Awake 均可调用，避免场景未跑 Setup 时滚轮仍转不动。
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

        /// <summary>Viewport 无 Image 后，Scroll 根节点需透明 Image 接收拖拽/滚轮命中。</summary>
        private static void EnsureScrollRootRaycastTarget(Transform scrollRoot)
        {
            var image = scrollRoot.GetComponent<Image>();
            if (image == null)
            {
                image = scrollRoot.gameObject.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0.02f);
            }

            image.raycastTarget = true;
        }
    }
}
