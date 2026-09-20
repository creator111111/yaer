#if UNITY_EDITOR
using System.IO;
using Game.GameRuntime.Entities.Component.Map;
using Game.GameRuntime.GameSceneManager.Scene.Village_House;
using Game.Static.Name.Res;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorC.Tool.Scene
{
    /// <summary>
    /// 巨树 2 楼回程进村长家：KenMuNi1 摆 StairsDoor_BackToChief + Chief 楼梯顶 EnterFrom_Tree2f。
    /// 菜单：Tools / Scene / Setup 巨树2楼回程进村长家
    /// </summary>
    /// <remarks>
    /// 原因（0920）：回程门从未做过；美术树洞无 SceneChangeDoor。对齐上楼 Stairs 样板。
    /// 替代：手改 YAML PrefabInstance——易漏 sceneObjs / Objects 子节点，故用菜单幂等摆放。
    /// </remarks>
    public static class Tree2fBackToChiefSetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup 巨树2楼回程进村长家";
        private const string ChiefScenePath = "Assets/GameRes/Scenes/Village_Chief_House.unity";
        private const string VillageScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity";
        private const string StairsPrefabPath = "Assets/Prefabs/Stairs.prefab";
        private const string AutoRequestFileName = "Tree2fBackToChiefSetup.request";

        private const string BackDoorName = "StairsDoor_BackToChief";
        private const string EnterFromTree2fName = "EnterFrom_Tree2f";

        /// <summary>
        /// 2 楼落点旁、WalkArea2 内；相对 ExitFrom_HomeSceneChief2f(-157.65,41.66) 右偏半身，
        /// 避免一落地立刻再触发。
        /// </summary>
        private static readonly Vector3 BackDoorWorldPos = new Vector3(-156.0f, 41.5f, 0f);

        /// <summary>楼梯顶附近、略偏室内，避开 StairsDoor_ToTree2f(-4.51,4.8)。</summary>
        private static readonly Vector3 EnterFromTree2fLocalPos = new Vector3(-2.8f, 4.0f, 0f);

        [InitializeOnLoadMethod]
        private static void AutoSetupFromRequestFile()
        {
            EditorApplication.delayCall += TryConsumeAutoSetupRequest;
        }

        private static void TryConsumeAutoSetupRequest()
        {
            var abs = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "Library", AutoRequestFileName));
            if (!File.Exists(abs))
            {
                return;
            }

            try
            {
                File.Delete(abs);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Tree2fBack] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[Tree2fBack] 检测到请求文件，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            SetupVillageBackDoor();
            SetupChiefEnterFromTree2f();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tree2fBack] 完成：StairsDoor_BackToChief + EnterFrom_Tree2f + EnterPos 键。");
        }

        private static void SetupVillageBackDoor()
        {
            var scene = EditorSceneManager.OpenScene(VillageScenePath, OpenSceneMode.Single);
            var stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StairsPrefabPath);
            if (stairsPrefab == null)
            {
                Debug.LogError("[Tree2fBack] 缺少 Stairs.prefab：" + StairsPrefabPath);
                return;
            }

            var objects = FindNamed(scene, "Objects");
            if (objects == null)
            {
                Debug.LogError("[Tree2fBack] 未找到 Objects");
                return;
            }

            if (!objects.gameObject.activeSelf)
            {
                objects.gameObject.SetActive(true);
                EditorUtility.SetDirty(objects.gameObject);
            }

            var existing = FindNamed(scene, BackDoorName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(stairsPrefab, objects);
            instance.name = BackDoorName;
            instance.transform.position = BackDoorWorldPos;
            instance.SetActive(true);

            // 无楼梯美术双影：关 SR（树上只需触发盒）
            var sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }

            // 缩小触发盒，避免盖住 2f 落点
            var box = instance.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = new Vector2(1.8f, 2.5f);
                box.offset = Vector2.zero;
                EditorUtility.SetDirty(box);
            }

            var door = instance.GetComponent<SceneChangeDoor>();
            if (door == null)
            {
                Debug.LogError("[Tree2fBack] Stairs 无 SceneChangeDoor");
                return;
            }

            var so = new SerializedObject(door);
            so.FindProperty("NextSceneName").stringValue = SceneName.Village_Chief_House;
            so.FindProperty("TriggerWhenMoveIn").boolValue = true;
            so.FindProperty("ShowLoadingUI").boolValue = false;
            // 必填键：空会落到大门 EnterFrom_Village
            so.FindProperty("EnterPosKey").stringValue = SceneName.Village_KenMuNi1_Tree2f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(door);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Tree2fBack] KenMuNi1：已摆 " + BackDoorName + " @ " + BackDoorWorldPos, instance);
        }

        private static void SetupChiefEnterFromTree2f()
        {
            var scene = EditorSceneManager.OpenScene(ChiefScenePath, OpenSceneMode.Single);
            var map = FindNamed(scene, "Map");
            if (map == null)
            {
                Debug.LogError("[Tree2fBack] Chief 无 Map");
                return;
            }

            Transform enter = FindNamed(scene, EnterFromTree2fName);
            if (enter == null)
            {
                var go = new GameObject(EnterFromTree2fName);
                go.layer = LayerMask.NameToLayer("Map") >= 0 ? LayerMask.NameToLayer("Map") : 8;
                enter = go.transform;
                enter.SetParent(map, false);
            }

            enter.localPosition = EnterFromTree2fLocalPos;
            EditorUtility.SetDirty(enter);

            var gsm = Object.FindObjectOfType<Village_Chief_HouseSceneManager>();
            if (gsm == null)
            {
                Debug.LogError("[Tree2fBack] 未找到 Village_Chief_HouseSceneManager");
                return;
            }

            var gsmSo = new SerializedObject(gsm);
            var listProp = gsmSo.FindProperty("EnterPosConfig");
            if (listProp == null || !listProp.isArray)
            {
                Debug.LogError("[Tree2fBack] EnterPosConfig 缺失");
                return;
            }

            // 大门键 Village_KenMuNi1 → EnterFrom_Village 保持不动
            bool found = false;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                if (elem.FindPropertyRelative("lastScene").stringValue == SceneName.Village_KenMuNi1_Tree2f)
                {
                    elem.FindPropertyRelative("pos").objectReferenceValue = enter;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                listProp.arraySize++;
                var elem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                elem.FindPropertyRelative("lastScene").stringValue = SceneName.Village_KenMuNi1_Tree2f;
                elem.FindPropertyRelative("pos").objectReferenceValue = enter;
                elem.FindPropertyRelative("DatePass").vector3IntValue = Vector3Int.zero;
            }

            gsmSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gsm);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log(
                "[Tree2fBack] Chief：EnterPos " + SceneName.Village_KenMuNi1_Tree2f +
                " → " + EnterFromTree2fName + " @ " + EnterFromTree2fLocalPos);
        }

        private static Transform FindNamed(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindRecursive(root.transform, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindRecursive(Transform tr, string name)
        {
            if (tr.name == name)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                var c = FindRecursive(tr.GetChild(i), name);
                if (c != null)
                {
                    return c;
                }
            }

            return null;
        }
    }
}
#endif
