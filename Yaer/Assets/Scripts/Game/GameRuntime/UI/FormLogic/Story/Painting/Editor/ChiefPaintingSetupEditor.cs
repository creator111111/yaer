#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting.Editor
{
    /// <summary>
    /// 将 SR 版 <c>ChiefPainting</c> 重建为 UI 大立绘（复用 <see cref="ChiefMaskPainting"/> 叠法）。
    /// 菜单：Tools / Dialogue / Setup Chief Painting (UI Big Portrait)
    /// 原因：磁盘旧 Prefab 仍是 SpriteRenderer，无法嵌 DialogueSceneContainer。
    /// 替代方案：抽独立 ChiefPortraitController——P1；本期 Prefab 分离、脚本复用 Mask。
    /// </summary>
    public static class ChiefPaintingSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Chief Painting (UI Big Portrait)";
        public const string PrefabPath = "Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab";
        private const string ArtFolder = "Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘/";
        private const string InstanceName = "ChiefPainting";

        // 与 Mask Setup 同源 SR 偏移；大立绘缩放更大，供对话场景同场三人站位
        private const float PixelsPerUnit = 100f;
        /// <summary>根框 / Face1 满框尺寸（对齐 ChiefMaskPainting）；贴脸坐标按此标定。</summary>
        private static readonly Vector2 FullFrameSize = new Vector2(1128f, 2625f);
        private static readonly Vector2 BodyLocal = new Vector2(5.77f, 13.160001f);
        private static readonly Vector2 Face2Local = new Vector2(9.5f, 23.320002f);
        private static readonly Vector2 Face3Local = new Vector2(9.41f, 22.525f);

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var prefab = CreateOrUpdatePrefab();
            if (prefab == null)
            {
                return;
            }

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"[ChiefPaintingSetup] UI 大立绘已写入：{PrefabPath}");
        }

        /// <summary>供门口对话 Prefab 装配菜单复用；失败返回 null。</summary>
        public static GameObject CreateOrUpdatePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(PrefabPath);
            }

            var root = new GameObject(
                InstanceName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(CanvasGroup));
            root.layer = 5;

            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.sizeDelta = FullFrameSize;
            // 占位站位：偏右（Q2 产品可再调）；alpha 默认 0 由前奏淡入
            rootRt.anchoredPosition = new Vector2(420f, -120f);
            rootRt.localScale = new Vector3(0.32f, 0.32f, 0.32f);

            root.AddComponent<ChiefMaskPainting>();

            // 先加载三脸：任一张缺失则中止，禁止再落盘空 Sprite（0831 H1/H2）
            var face1 = LoadSprite("组 2.png");
            var face2 = LoadSprite("闭眼.png");
            var face3 = LoadSprite("笑颜.png");
            if (face1 == null || face2 == null || face3 == null)
            {
                Debug.LogError(
                    "[ChiefPaintingSetup] 三脸 Sprite 未齐（组 2 / 闭眼 / 笑颜），中止保存，避免空图 Prefab。" +
                    "请确认 ArtFolder 有 png 真文件且保留原 meta guid。");
                Object.DestroyImmediate(root);
                return null;
            }

            // Face1 必须满框：贴脸 AnchoredPos 按满框标定；用 sprite.rect 会写成 880×2048 导致飞脸（0901 H1/H3）
            CreateImageLeaf(root.transform, "Face1", face1, Vector2.zero, true, forceFullFrame: true);
            CreateImageLeaf(
                root.transform,
                "Face2",
                face2,
                (Face2Local - BodyLocal) * PixelsPerUnit,
                false,
                forceFullFrame: false);
            CreateImageLeaf(
                root.transform,
                "Face3",
                face3,
                (Face3Local - BodyLocal) * PixelsPerUnit,
                false,
                forceFullFrame: false);

            root.GetComponent<ChiefMaskPainting>().EditorResetDefaultActiveState();
            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            root.SetActive(true);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <param name="forceFullFrame">
        /// Face1=true：强制 <see cref="FullFrameSize"/>（对齐 Mask）；Face2/3=false：仍用 sprite 像素尺寸。
        /// 原因：仅信 sprite.rect 会把底图缩成 880×2048，贴脸坐标相对可见底图飞偏。
        /// </param>
        private static void CreateImageLeaf(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 anchoredPosition,
            bool active,
            bool forceFullFrame)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = 5;
            go.SetActive(active);

            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.localScale = Vector3.one;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            // 与 Mask 一致：preserveAspect=1；Face1 满框后由 Image 保持比例（Q1）
            image.preserveAspect = true;
            image.raycastTarget = false;

            if (forceFullFrame)
            {
                rt.sizeDelta = FullFrameSize;
            }
            else if (sprite != null)
            {
                rt.sizeDelta = sprite.rect.size;
            }
        }

        private static Sprite LoadSprite(string fileName)
        {
            var path = ArtFolder + fileName;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[ChiefPaintingSetup] 未找到 Sprite：{path}");
            }

            return sprite;
        }
    }
}
#endif
