#if UNITY_EDITOR
using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.UI.FormLogic.Story;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop.Editor
{
    /// <summary>
    /// 从 SR 版 MerchantPainting 资源生成 UI 版 MerchantMaskPainting.prefab，
    /// 并嵌套进 NormalDialogueNewPanel → YaerAvatarRoot。
    /// 菜单：Tools / Shop / Setup Merchant Mask Painting
    /// </summary>
    public static class MerchantMaskPaintingSetupEditor
    {
        private const string MenuPath = "Tools/Shop/Setup Merchant Mask Painting";
        private const string MaskPrefabPath = "Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab";
        private const string DialoguePanelPath = "Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab";
        private const string ArtFolder = "Assets/ArtRes/Scene/Village/商店界面合层/";
        private const string MerchantInstanceName = "MerchantMaskPainting";
        private const string YaerAvatarRootName = "YaerAvatarRoot";

        // MerchantPainting.prefab（SR）中 Body/Normal 与 Face/Face1 的局部坐标差，按 100px/世界单位换算为 UI 偏移。
        // Normal: (13.54, 5.235)，Face1: (13.23, 8.425) → Face 相对居中 Body 偏移 (-0.31, 3.19) 世界单位。
        private const float MerchantPaintingPixelsPerUnit = 100f;
        private static readonly Vector2 MerchantPaintingBodySpriteLocalPos = new Vector2(13.539999f, 5.235f);
        private static readonly Vector2 MerchantPaintingFaceSpriteLocalPos = new Vector2(13.23f, 8.424999f);

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
            Debug.Log("[MerchantMaskSetup] MerchantMaskPainting Prefab + NormalDialogueNewPanel 接线完成。");
        }

        private static GameObject CreateOrUpdateMaskPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MaskPrefabPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(MaskPrefabPath);
            }

            var root = new GameObject(
                MerchantInstanceName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(CanvasGroup));

            root.layer = 5;
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.sizeDelta = new Vector2(836, 1047);
            rootRt.anchoredPosition = new Vector2(40f, -320f);
            rootRt.localScale = new Vector3(0.22f, 0.22f, 0.22f);

            root.AddComponent<MerchantMaskPainting>();

            var bodyRoot = CreateGroup(root.transform, ShopkeeperFaceController.BodyGroupName);
            CreateImageLeaf(bodyRoot, "Normal", LoadSprite("正常体.png"), true);
            CreateImageLeaf(bodyRoot, "Red", LoadSprite("脸红体.png"), false);
            CreateImageLeaf(bodyRoot, "YinXian", LoadSprite("阴险体.png"), false);

            var faceRoot = CreateGroup(
                root.transform,
                ShopkeeperFaceController.FaceGroupName,
                GetFaceOffsetFromMerchantPainting());
            CreateImageLeaf(faceRoot, "Face1", LoadSprite("表情1.png"), true);
            CreateImageLeaf(faceRoot, "Face2", LoadSprite("表情2.png"), false);
            CreateImageLeaf(faceRoot, "Face3", LoadSprite("表情3.png"), false);
            CreateImageLeaf(faceRoot, "Face4", LoadSprite("表情4.png"), false);
            CreateImageLeaf(faceRoot, "Face5", LoadSprite("表情5.png"), false);

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
                    Debug.LogError($"[MerchantMaskSetup] 未找到 {YaerAvatarRootName}。");
                    return;
                }

                var existing = avatarRoot.Find(MerchantInstanceName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(maskPrefab, avatarRoot);
                instance.name = MerchantInstanceName;
                instance.SetActive(false);

                var presenter = avatarRoot.GetComponent<DialogueMaskAvatarPresenter>();
                if (presenter == null)
                {
                    Debug.LogError("[MerchantMaskSetup] YaerAvatarRoot 上无 DialogueMaskAvatarPresenter。");
                    return;
                }

                var merchantPainting = instance.GetComponent<MerchantMaskPainting>();
                var so = new SerializedObject(presenter);
                so.FindProperty("merchantMaskPainting").objectReferenceValue = merchantPainting;
                so.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(presenter);
                PrefabUtility.SaveAsPrefabAsset(panelRoot, DialoguePanelPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(panelRoot);
            }
        }

        /// <summary>
        /// Face 组相对 Body 的 UI 偏移，与 MerchantPainting.prefab 中 Face 相对 Normal 的偏移一致。
        /// </summary>
        private static Vector2 GetFaceOffsetFromMerchantPainting()
        {
            var delta = MerchantPaintingFaceSpriteLocalPos - MerchantPaintingBodySpriteLocalPos;
            return delta * MerchantPaintingPixelsPerUnit;
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            return CreateGroup(parent, name, Vector2.zero);
        }

        private static Transform CreateGroup(Transform parent, string name, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = anchoredPosition;
            rt.localScale = Vector3.one;
            return rt;
        }

        private static void CreateImageLeaf(Transform parent, string name, Sprite sprite, bool active)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = 5;
            go.SetActive(active);

            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
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
                Debug.LogWarning($"[MerchantMaskSetup] 未找到 Sprite：{path}");
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
