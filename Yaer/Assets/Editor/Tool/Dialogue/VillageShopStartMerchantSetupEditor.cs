#if UNITY_EDITOR
using Game.GameRuntime.Story.NodeCanvasExtend;
using NodeCanvas.DialogueTrees;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 在 Village_ShopStart 下新建轻量 Merchant GO，并绑定 Actor 参数「老板娘」。
    /// 菜单：Tools / Dialogue / Setup Village_ShopStart Merchant Actor
    /// </summary>
    public static class VillageShopStartMerchantSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Village_ShopStart Merchant Actor";
        private const string PrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab";
        private const string MerchantGoName = "Merchant";

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (!TrySetupMerchant(prefabRoot, out var message))
                {
                    Debug.LogError($"[VillageShopStartMerchantSetup] {message}");
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                Debug.Log($"[VillageShopStartMerchantSetup] {message}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        /// <summary>供其它 Editor 复用。</summary>
        public static bool TrySetupMerchant(GameObject prefabRoot, out string message)
        {
            message = null;
            if (prefabRoot == null)
            {
                message = "Prefab 根为空。";
                return false;
            }

            var controller = prefabRoot.GetComponent<DialogueTreeController>();
            if (controller == null)
            {
                message = "未找到 DialogueTreeController。";
                return false;
            }

            var merchantTransform = FindOrCreateMerchant(prefabRoot.transform);
            var merchantActor = merchantTransform.GetComponent<DialogueActorEx>();
            if (merchantActor == null)
            {
                merchantActor = merchantTransform.gameObject.AddComponent<DialogueActorEx>();
            }

            ApplyMerchantActorFields(merchantActor);

            controller.Validate();
            var tree = controller.behaviour as DialogueTree;
            if (tree == null)
            {
                message = "DialogueTree behaviour 未就绪，请检查 bound graph。";
                return false;
            }

            tree.SetActorReference(ShopkeeperCsvDefaults.ShopkeeperActorName, merchantActor);
            controller.SetBoundGraphReference(tree);

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(merchantActor.gameObject);

            message =
                $"已创建/更新「{MerchantGoName}」并绑定 Actor「{ShopkeeperCsvDefaults.ShopkeeperActorName}」。";
            return true;
        }

        private static Transform FindOrCreateMerchant(Transform root)
        {
            var existing = root.Find(MerchantGoName);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(MerchantGoName, typeof(RectTransform));
            go.layer = root.gameObject.layer;
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = Vector2.zero;
            rt.SetSiblingIndex(root.childCount - 1);
            return rt;
        }

        private static void ApplyMerchantActorFields(DialogueActorEx merchantActor)
        {
            var so = new SerializedObject(merchantActor);
            so.FindProperty("_name").stringValue = ShopkeeperCsvDefaults.ShopkeeperActorName;
            so.FindProperty("_roleName").enumValueIndex = 0;
            so.FindProperty("_portrait").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
