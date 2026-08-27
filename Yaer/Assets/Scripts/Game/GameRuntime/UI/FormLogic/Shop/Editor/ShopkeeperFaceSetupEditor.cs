#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop.Editor
{
    /// <summary>
    /// 一键在 Village_Shop 场景「商店界面合层」上挂切脸组件并校正 Body/Face 默认 Active。
    /// 菜单：Tools / Shop / Setup Shopkeeper Face Controller
    /// </summary>
    public static class ShopkeeperFaceSetupEditor
    {
        private const string MenuPath = "Tools/Shop/Setup Shopkeeper Face Controller";
        private const string VillageShopScenePath = "Assets/GameRes/Scenes/Village_Shop.unity";
        private const string CompositeRootName = "商店界面合层";

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var scene = EditorSceneManager.OpenScene(VillageShopScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[ShopkeeperFaceSetup] 无法打开场景 {VillageShopScenePath}");
                return;
            }

            var composite = GameObject.Find(CompositeRootName);
            if (composite == null)
            {
                Debug.LogError($"[ShopkeeperFaceSetup] 场景中未找到「{CompositeRootName}」。");
                return;
            }

            var controller = composite.GetComponent<ShopkeeperFaceController>();
            if (controller == null)
            {
                controller = composite.AddComponent<ShopkeeperFaceController>();
            }

            var debugInput = composite.GetComponent<ShopkeeperFaceDebugInput>();
            if (debugInput == null)
            {
                debugInput = composite.AddComponent<ShopkeeperFaceDebugInput>();
            }

            controller.EditorResetDefaultActiveState();

            var soDebug = new SerializedObject(debugInput);
            soDebug.FindProperty("controller").objectReferenceValue = controller;
            soDebug.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(debugInput);
            EditorUtility.SetDirty(composite);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log(
                $"[ShopkeeperFaceSetup] 已挂载并校正「{CompositeRootName}」Body/Face Toggle 默认（Normal + Face1）。");
        }
    }
}
#endif
