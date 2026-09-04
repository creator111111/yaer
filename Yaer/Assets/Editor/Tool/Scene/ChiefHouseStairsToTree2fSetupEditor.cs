#if UNITY_EDITOR
using System.IO;
using Game.GameRuntime.Entities.Component.Map;
using Game.Static.Name.Res;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Scene
{
    /// <summary>
    /// 村长家楼梯顶换场门 + KenMuNi1 1 楼门前落点（E3′）+ LeftDoor EnterPosKey。
    /// 菜单：Tools / Scene / Setup Chief House 楼梯上楼换场巨树2楼
    /// </summary>
    /// <remarks>
    /// 原因（0901）：缺楼梯顶门；LeftDoor 与楼梯抢 EnterPos；须绑 WalkArea2（代码侧）但不改其形状。
    /// 合层「楼梯」仅美术；本菜单旁挂 Stairs 样板。
    /// </remarks>
    public static class ChiefHouseStairsToTree2fSetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup Chief House 楼梯上楼换场巨树2楼";
        private const string ChiefScenePath = "Assets/GameRes/Scenes/Village_Chief_House.unity";
        private const string VillageScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity";
        private const string StairsPrefabPath = "Assets/Prefabs/Stairs.prefab";
        private const string AutoRequestFileName = "ChiefHouseStairs2fSetup.request";

        private const string StairsDoorName = "StairsDoor_ToTree2f";
        private const string ExitFrom1fName = "ExitFrom_HomeSceneChief";

        /// <summary>合层楼梯世界锚（室内划区报告）附近上层平台。</summary>
        private static readonly Vector3 StairsDoorWorldPos = new Vector3(-10.5f, 2.2f, 0f);

        /// <summary>户外 House_Chief 附近 1 楼门前（地面 Y≈-6）。</summary>
        private static readonly Vector3 ExitFrom1fLocalPos = new Vector3(-156.5f, -5.5f, 0f);

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
                Debug.LogWarning("[Stairs2f] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[Stairs2f] 检测到请求文件，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            SetupChiefHouseStairsDoor();
            SetupVillage1fExitAndEnterPos();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Stairs2f] 完成：楼梯门 + ExitFrom_HomeSceneChief + LeftDoor EnterPosKey。");
        }

        private static void SetupChiefHouseStairsDoor()
        {
            var scene = EditorSceneManager.OpenScene(ChiefScenePath, OpenSceneMode.Single);
            var stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StairsPrefabPath);
            if (stairsPrefab == null)
            {
                Debug.LogError("[Stairs2f] 缺少 Stairs.prefab：" + StairsPrefabPath);
                return;
            }

            // Objects 须 Active，否则 Trigger 不碰；现网曾误关
            var objects = FindNamed(scene, "Objects");
            if (objects == null)
            {
                Debug.LogError("[Stairs2f] 未找到 Objects");
                return;
            }

            if (!objects.gameObject.activeSelf)
            {
                objects.gameObject.SetActive(true);
                EditorUtility.SetDirty(objects.gameObject);
            }

            // 幂等：删旧门再摆
            var existing = FindNamed(scene, StairsDoorName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(stairsPrefab, objects);
            instance.name = StairsDoorName;
            instance.transform.position = StairsDoorWorldPos;
            instance.SetActive(true);

            // 合层已有楼梯美术：关样板 SR，避免双影
            var sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }

            var door = instance.GetComponent<SceneChangeDoor>();
            if (door == null)
            {
                Debug.LogError("[Stairs2f] Stairs 无 SceneChangeDoor");
                return;
            }

            var so = new SerializedObject(door);
            so.FindProperty("NextSceneName").stringValue = SceneName.Village_KenMuNi1;
            so.FindProperty("TriggerWhenMoveIn").boolValue = true;
            so.FindProperty("ShowLoadingUI").boolValue = false;
            // 楼梯留空 → lastScene=Village_Chief_House → 2f EnterPos
            so.FindProperty("EnterPosKey").stringValue = string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();

            // LeftDoor：E3′ 填大门键
            var leftDoor = FindNamed(scene, "LeftDoor")?.GetComponent<SceneChangeDoor>();
            if (leftDoor != null)
            {
                var leftSo = new SerializedObject(leftDoor);
                leftSo.FindProperty("EnterPosKey").stringValue = SceneName.Village_Chief_House_Door;
                leftSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(leftDoor);
            }
            else
            {
                Debug.LogWarning("[Stairs2f] 未找到 LeftDoor，请手填 EnterPosKey=" + SceneName.Village_Chief_House_Door);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Stairs2f] Chief：已摆 " + StairsDoorName + " @ " + StairsDoorWorldPos, instance);
        }

        private static void SetupVillage1fExitAndEnterPos()
        {
            var scene = EditorSceneManager.OpenScene(VillageScenePath, OpenSceneMode.Single);
            var map = FindNamed(scene, "Map");
            if (map == null)
            {
                Debug.LogError("[Stairs2f] KenMuNi1 无 Map");
                return;
            }

            Transform exit1f = FindNamed(scene, ExitFrom1fName);
            if (exit1f == null)
            {
                var go = new GameObject(ExitFrom1fName);
                go.layer = LayerMask.NameToLayer("Map") >= 0 ? LayerMask.NameToLayer("Map") : 8;
                exit1f = go.transform;
                exit1f.SetParent(map, false);
            }

            exit1f.localPosition = ExitFrom1fLocalPos;
            EditorUtility.SetDirty(exit1f);

            // EnterPosConfig：追加 Village_Chief_House_Door → ExitFrom_HomeSceneChief
            // 保留 Village_Chief_House → ExitFrom_HomeSceneChief2f 不动
            var gsm = Object.FindObjectOfType<Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi.Village_KenMuNiSceneManager>();
            if (gsm == null)
            {
                Debug.LogError("[Stairs2f] 未找到 Village_KenMuNiSceneManager");
                return;
            }

            var gsmSo = new SerializedObject(gsm);
            var listProp = gsmSo.FindProperty("EnterPosConfig");
            if (listProp == null || !listProp.isArray)
            {
                Debug.LogError("[Stairs2f] EnterPosConfig 缺失");
                return;
            }

            bool found = false;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                if (elem.FindPropertyRelative("lastScene").stringValue == SceneName.Village_Chief_House_Door)
                {
                    elem.FindPropertyRelative("pos").objectReferenceValue = exit1f;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                listProp.arraySize++;
                var elem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                elem.FindPropertyRelative("lastScene").stringValue = SceneName.Village_Chief_House_Door;
                elem.FindPropertyRelative("pos").objectReferenceValue = exit1f;
                elem.FindPropertyRelative("DatePass").vector3IntValue = Vector3Int.zero;
            }

            // 确认楼梯键仍指向 2f
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                if (elem.FindPropertyRelative("lastScene").stringValue == SceneName.Village_Chief_House)
                {
                    var pos2f = FindNamed(scene, "ExitFrom_HomeSceneChief2f");
                    if (pos2f != null)
                    {
                        elem.FindPropertyRelative("pos").objectReferenceValue = pos2f;
                    }
                }
            }

            gsmSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gsm);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Stairs2f] KenMuNi1：EnterPos " + SceneName.Village_Chief_House_Door + " → " + ExitFrom1fName);
        }

        // 须写全名：本文件命名空间为 EditorC.Tool.Scene，裸写 Scene 会 CS0118
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
