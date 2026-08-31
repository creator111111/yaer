#if UNITY_EDITOR
using Game.GameRuntime.UI.FormLogic.Story;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting.Editor
{
    /// <summary>
    /// 从 SR「精灵村长游戏中立绘」生成 UI 版 ChiefMaskPainting，并嵌套进 NormalDialogueNewPanel。
    /// 菜单：Tools / Dialogue / Setup Chief Mask Painting
    /// </summary>
    public static class ChiefMaskPaintingSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Chief Mask Painting";
        private const string MaskPrefabPath = "Assets/Prefabs/DialougeProtrait/ChiefMaskPainting.prefab";
        private const string DialoguePanelPath = "Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab";
        private const string ArtFolder = "Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘/";
        private const string InstanceName = "ChiefMaskPainting";
        private const string YaerAvatarRootName = "YaerAvatarRoot";

        // SR 源：组2=(5.77,13.16)，Face2=(9.5,23.32)，Face3=(9.41,22.525)；PPU=100
        private const float PixelsPerUnit = 100f;
        private static readonly Vector2 BodyLocal = new Vector2(5.77f, 13.160001f);
        private static readonly Vector2 Face2Local = new Vector2(9.5f, 23.320002f);
        private static readonly Vector2 Face3Local = new Vector2(9.41f, 22.525f);

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var maskPrefab = CreateOrUpdateMaskPrefab();
            if (maskPrefab == null)
            {
                return;
            }

            EmbedInDialoguePanel(maskPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[ChiefMaskSetup] ChiefMaskPainting Prefab + NormalDialogueNewPanel 接线完成。");
        }

        private static GameObject CreateOrUpdateMaskPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(MaskPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(MaskPrefabPath);
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
            // 底图约 1128×2625；根框用底图尺寸，整体再缩放进 Mask
            rootRt.sizeDelta = new Vector2(1128f, 2625f);
            rootRt.anchoredPosition = new Vector2(40f, -280f);
            rootRt.localScale = new Vector3(0.18f, 0.18f, 0.18f);

            root.AddComponent<ChiefMaskPainting>();

            // Face1 底图居中；Face2/3 相对底图 SR 偏移贴头
            CreateImageLeaf(root.transform, "Face1", LoadSprite("组 2.png"), Vector2.zero, true);
            CreateImageLeaf(
                root.transform,
                "Face2",
                LoadSprite("闭眼.png"),
                (Face2Local - BodyLocal) * PixelsPerUnit,
                false);
            CreateImageLeaf(
                root.transform,
                "Face3",
                LoadSprite("笑颜.png"),
                (Face3Local - BodyLocal) * PixelsPerUnit,
                false);

            root.GetComponent<ChiefMaskPainting>().EditorResetDefaultActiveState();
            root.SetActive(false);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, MaskPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EmbedInDialoguePanel(GameObject maskPrefab)
        {
            var panelRoot = PrefabUtility.LoadPrefabContents(DialoguePanelPath);
            try
            {
                var avatarRoot = FindDeepChild(panelRoot.transform, YaerAvatarRootName);
                if (avatarRoot == null)
                {
                    Debug.LogError($"[ChiefMaskSetup] 未找到 {YaerAvatarRootName}。");
                    return;
                }

                var existing = avatarRoot.Find(InstanceName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(maskPrefab, avatarRoot);
                instance.name = InstanceName;
                instance.SetActive(false);

                var presenter = avatarRoot.GetComponent<DialogueMaskAvatarPresenter>();
                if (presenter == null)
                {
                    Debug.LogError("[ChiefMaskSetup] YaerAvatarRoot 上无 DialogueMaskAvatarPresenter。");
                    return;
                }

                var so = new SerializedObject(presenter);
                so.FindProperty("chiefMaskPainting").objectReferenceValue =
                    instance.GetComponent<ChiefMaskPainting>();
                so.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(presenter);
                PrefabUtility.SaveAsPrefabAsset(panelRoot, DialoguePanelPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(panelRoot);
            }
        }

        private static void CreateImageLeaf(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 anchoredPosition,
            bool active)
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
            image.preserveAspect = true;
            image.raycastTarget = false;

            if (sprite != null)
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
                Debug.LogWarning($"[ChiefMaskSetup] 未找到 Sprite：{path}");
            }

            return sprite;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
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
