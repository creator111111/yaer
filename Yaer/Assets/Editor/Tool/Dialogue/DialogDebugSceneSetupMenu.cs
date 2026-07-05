using System.Collections.Generic;
using Game.GameRuntime.Story;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 编辑模式下搭建 DialogDebug 解耦沙盒层级（Bootstrap + DialogUI + Playground），不创建 SceneManager / GSM。
    /// </summary>
    public static class DialogDebugSceneSetupMenu
    {
        private const string ScenePath = "Assets/GameRes/Scenes/DialogDebug.unity";
        private const string DialogueUiPrefabPath = "Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab";
        private const string DefaultDialoguePrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStar_Test.prefab";

        private static readonly string[] ToolbarObjectNames =
        {
            "ButtonSave", "ButtonLoad", "ButtonClose", "ButtonHistory", "ButtonSettings", "HistoryPanel"
        };

        [MenuItem("Tools/Dialogue/Setup DialogDebug Scene")]
        public static void SetupDialogDebugScene()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("DialogDebug", "请在非 Play 模式下运行本菜单。", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            RemoveLegacyGfObjects();

            var bootstrap = EnsureChild("_Bootstrap");
            if (bootstrap.GetComponent<DialogDebugRuntimeBootstrap>() == null)
            {
                bootstrap.AddComponent<DialogDebugRuntimeBootstrap>();
            }

            if (bootstrap.GetComponent<DialogDebugDialogueUISetup>() == null)
            {
                bootstrap.AddComponent<DialogDebugDialogueUISetup>();
            }

            EnsureEventSystem();

            var dialogUiRoot = EnsureChild("DialogUI");
            var dialogueUi = EnsureDialogueUi(dialogUiRoot.transform);

            var instanceRoot = EnsureChild("DialogueInstanceRoot");
            var playgroundGo = EnsureChild("DialogDebugPlayground");
            var playground = playgroundGo.GetComponent<DialogDebugPlayground>();
            if (playground == null)
            {
                playground = playgroundGo.AddComponent<DialogDebugPlayground>();
            }

            var defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultDialoguePrefabPath);
            var so = new SerializedObject(playground);
            so.FindProperty("dialoguePrefab").objectReferenceValue = defaultPrefab;
            so.FindProperty("dialogueContainer").objectReferenceValue = instanceRoot.transform;
            so.FindProperty("dialogueUI").objectReferenceValue = dialogueUi;
            so.FindProperty("playOnStart").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "DialogDebug",
                "沙盒层级已就绪：_Bootstrap、DialogUI、DialogueInstanceRoot、DialogDebugPlayground。\n\n" +
                "使用方式：Open 本场景 → Play（无需 InitScene）。\n" +
                "可在 Inspector 拖换 dialoguePrefab 测不同对话。",
                "OK");
        }

        /// <summary>清理旧 GF 方案遗留：SceneManager、StoryTestTrigger 等。</summary>
        private static void RemoveLegacyGfObjects()
        {
            var sceneManager = GameObject.Find("SceneManager");
            if (sceneManager != null)
            {
                Object.DestroyImmediate(sceneManager);
            }

            var storyTrigger = GameObject.Find("StoryTestTrigger");
            if (storyTrigger != null)
            {
                Object.DestroyImmediate(storyTrigger);
            }
        }

        private static GameObject EnsureChild(string name)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
            }

            return go;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// 从 NormalDialogueNewPanel 实例化 UI，移除 GF Form 脚本并隐藏存读档等工具栏。
        /// </summary>
        private static DialogueTMPUGUI EnsureDialogueUi(Transform dialogUiParent)
        {
            var existing = dialogUiParent.GetComponentInChildren<DialogueTMPUGUI>(true);
            if (existing != null)
            {
                return existing;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DialogueUiPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[DialogDebug Setup] 未找到 UI prefab: {DialogueUiPrefabPath}");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, dialogUiParent);
            instance.name = "NormalDialogueUI";

            var rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            StripGfFormComponents(instance);
            HideToolbarButtons(instance.transform);

            var canvas = instance.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
            }

            return instance.GetComponentInChildren<DialogueTMPUGUI>(true);
        }

        private static void StripGfFormComponents(GameObject uiRoot)
        {
            foreach (var form in uiRoot.GetComponentsInChildren<NormalDialogueFormNewLogic>(true))
            {
                if (form != null)
                {
                    Object.DestroyImmediate(form, true);
                }
            }

            foreach (var systemUi in uiRoot.GetComponentsInChildren<ComponentSystemUI>(true))
            {
                if (systemUi != null)
                {
                    Object.DestroyImmediate(systemUi, true);
                }
            }

            var removeList = new List<MonoBehaviour>();
            foreach (var mb in uiRoot.GetComponents<MonoBehaviour>())
            {
                if (mb is CanvasScaler || mb is GraphicRaycaster || mb is Image)
                {
                    continue;
                }

                removeList.Add(mb);
            }

            foreach (var mb in removeList)
            {
                Object.DestroyImmediate(mb, true);
            }
        }

        private static void HideToolbarButtons(Transform root)
        {
            foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (var name in ToolbarObjectNames)
                {
                    if (tr.name == name)
                    {
                        tr.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
