#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 村长家门口 / 继续对话：三人立绘 + Actor「村长」定稿摆位（真理源=门口 Prefab）。
    /// Door / Continue Setup 的 <c>NudgePortraitLayout</c> 必须同源调用，禁止再写死雅 X=-380。
    /// </summary>
    /// <remarks>
    /// 原因（0901）：Continue 雅仍 -380、Actor 村长停 (0,0)；门口已定稿雅 (348,52)、村长 (1156,-232)+Y180。
    /// 替代方案：两 Setup 各复制一组常量——易再次漂移，故集中于此。
    /// </remarks>
    public static class VillageChiefDialoguePortraitLayout
    {
        /// <summary>GoOutStoryYaerPainting 门口定稿。</summary>
        public static readonly Vector2 YaerPaintingPos = new Vector2(348f, 52f);

        /// <summary>GushaPainting 门口定稿。</summary>
        public static readonly Vector2 GushaPaintingPos = new Vector2(0f, -330f);

        /// <summary>ChiefPainting 对话内脚位 X；Y 保持 -120。</summary>
        public const float ChiefPaintingPosX = 420f;

        public const float ChiefPaintingPosY = -120f;

        /// <summary>对话内村长立绘 Scale（母体默认 0.32 过小）。</summary>
        public const float ChiefPaintingScale = 0.65f;

        /// <summary>Actor「村长」门口定稿位置。</summary>
        public static readonly Vector2 ActorChiefPos = new Vector2(1156f, -232f);

        /// <summary>Actor「村长」门口定稿：绕 Y 翻转（面向左）。</summary>
        public static readonly Vector3 ActorChiefEuler = new Vector3(0f, 180f, 0f);

        /// <summary>
        /// 按门口定稿写三人 Painting + Actor「村长」。
        /// 调用方：Door / Continue Setup 的 Nudge；可单独菜单重钉。
        /// </summary>
        public static void ApplyToDialogueRoot(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            TrySetAnchoredPosition(FindDeepChild(root.transform, "GoOutStoryYaerPainting"), YaerPaintingPos);
            TrySetAnchoredPosition(FindDeepChild(root.transform, "GushaPainting"), GushaPaintingPos);

            var chief = FindDeepChild(root.transform, "ChiefPainting");
            TrySetAnchoredPosition(chief, new Vector2(ChiefPaintingPosX, ChiefPaintingPosY));
            TrySetLocalScale(chief, ChiefPaintingScale);

            // Actor 父节点：只改名「村长」，勿误伤立绘子物体
            TrySetActorChiefLayout(FindDeepChild(root.transform, "村长"));
        }

        private static void TrySetActorChiefLayout(Transform actor)
        {
            if (actor == null)
            {
                return;
            }

            var rt = actor as RectTransform;
            if (rt == null)
            {
                return;
            }

            if (rt.anchoredPosition != ActorChiefPos)
            {
                rt.anchoredPosition = ActorChiefPos;
                EditorUtility.SetDirty(rt);
            }

            var targetRot = Quaternion.Euler(ActorChiefEuler);
            if (rt.localRotation != targetRot)
            {
                rt.localRotation = targetRot;
                EditorUtility.SetDirty(rt);
            }
        }

        private static void TrySetAnchoredPosition(Transform t, Vector2 pos)
        {
            if (t == null)
            {
                return;
            }

            var rt = t as RectTransform;
            if (rt == null)
            {
                return;
            }

            if (rt.anchoredPosition == pos)
            {
                return;
            }

            rt.anchoredPosition = pos;
            EditorUtility.SetDirty(rt);
        }

        private static void TrySetLocalScale(Transform t, float uniform)
        {
            if (t == null)
            {
                return;
            }

            var s = new Vector3(uniform, uniform, uniform);
            if (t.localScale == s)
            {
                return;
            }

            t.localScale = s;
            EditorUtility.SetDirty(t);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (parent.name == name)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindDeepChild(parent.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
