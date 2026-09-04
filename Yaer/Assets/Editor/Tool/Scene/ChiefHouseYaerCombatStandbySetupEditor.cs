#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Game.GameRuntime.GameSceneManager.Scene.Village_House;
using Game.Static.Name.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 预置「雅儿战斗待机」单帧涂层：写 Prefab 合层资产 + 场景拆包合层（0901 H2 教训双写）。
    /// 菜单：Tools / Scene / Setup Chief House 雅儿战斗待机预置
    /// </summary>
    /// <remarks>
    /// 产品（0902）：续聊用场景贴纸站古莎旁，勿切真玩家 Combat。
    /// 施工默认：铠甲基本无 第 1 帧单 SR；脚位 = 古莎待机 + 左侧偏移；默认 Active=false；SortingOrder≈10。
    /// 替代：美术交付完整 Prefab 后改本菜单只 Instantiate；真 Animator 循环另案。
    /// </remarks>
    public static class ChiefHouseYaerCombatStandbySetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup Chief House 雅儿战斗待机预置";
        private const string HouseCompositePrefabPath =
            "Assets/ArtRes/Scene/Village/Prefab/村长家合层.prefab";
        private const string ChiefScenePath = "Assets/GameRes/Scenes/Village_Chief_House.unity";
        private const string HouseCompositeName = "村长家合层";
        private const string GushaStandbyName = "古莎待机";
        private const string YaerStandbyName = "雅儿战斗待机";

        /// <summary>施工默认帧：铠甲基本无 / 1.png（Q2 可改口跟存档头饰）。</summary>
        private const string DefaultSpritePath =
            "Assets/ArtRes/Animation/Yaer/Combat/Idle/铠甲基本无/1.png";

        private const string AutoRequestFileName = "ChiefHouseYaerCombatStandbySetup.request";

        /// <summary>相对古莎待机 local X 偏移（左侧旁站）。</summary>
        private const float LocalOffsetXFromGusha = -4.5f;

        /// <summary>对齐待机量级；须 &lt; 村长 12。</summary>
        private const int SortingOrder = 10;

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
                Debug.LogWarning("[YaerCombatStandbySetup] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[YaerCombatStandbySetup] 检测到请求文件，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var sprite = LoadDefaultSprite();
            if (sprite == null)
            {
                Debug.LogError("[YaerCombatStandbySetup] 缺默认 Sprite：" + DefaultSpritePath);
                return;
            }

            // 1) Prefab 资产侧
            SetupIntoHouseCompositePrefab(sprite);

            // 2) 场景拆包合层才是玩时真源
            SetupIntoChiefHouseScene(sprite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                "[YaerCombatStandbySetup] 完成：Prefab + 场景 Design/村长家合层 均已预置「"
                + YaerStandbyName
                + "」（默认关）。");
        }

        private static UnityEngine.Sprite LoadDefaultSprite()
        {
            // SpriteMode=Single 时 LoadAllAssets 取 Sprite 子资源
            // 须写 UnityEngine.Sprite：父命名空间 EditorC.Tool.Sprite 会抢裸名 Sprite（CS0118）
            var assets = AssetDatabase.LoadAllAssetsAtPath(DefaultSpritePath);
            if (assets == null)
            {
                return null;
            }

            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is UnityEngine.Sprite s)
                {
                    return s;
                }
            }

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(DefaultSpritePath);
        }

        private static void SetupIntoHouseCompositePrefab(UnityEngine.Sprite sprite)
        {
            var housePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HouseCompositePrefabPath);
            if (housePrefab == null)
            {
                Debug.LogError("[YaerCombatStandbySetup][Prefab] 缺合层：" + HouseCompositePrefabPath);
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(HouseCompositePrefabPath);
            try
            {
                if (!TryPlaceYaerStandby(root.transform, sprite, out var msg))
                {
                    Debug.LogError("[YaerCombatStandbySetup][Prefab] " + msg);
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, HouseCompositePrefabPath);
                Debug.Log("[YaerCombatStandbySetup][Prefab] " + msg, root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetupIntoChiefHouseScene(UnityEngine.Sprite sprite)
        {
            var scene = EditorSceneManager.OpenScene(ChiefScenePath, OpenSceneMode.Single);
            var composite = FindNamed(scene, HouseCompositeName);
            if (composite == null)
            {
                Debug.LogError("[YaerCombatStandbySetup][Scene] 未找到「" + HouseCompositeName + "」。");
                return;
            }

            if (!TryPlaceYaerStandby(composite, sprite, out var msg))
            {
                Debug.LogError("[YaerCombatStandbySetup][Scene] " + msg);
                return;
            }

            BindGsmRef(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[YaerCombatStandbySetup][Scene] " + msg + "（已保存场景）", composite);
        }

        /// <summary>
        /// 在合层下放置/重置「雅儿战斗待机」：旁古莎、默认关、单 SR、SortingOrder=10。
        /// </summary>
        private static bool TryPlaceYaerStandby(Transform compositeRoot, UnityEngine.Sprite sprite, out string message)
        {
            message = null;
            if (compositeRoot == null)
            {
                message = "合层 Transform 为空。";
                return false;
            }

            var gusha = FindDeepChild(compositeRoot, GushaStandbyName);
            if (gusha == null)
            {
                message = "合层下未找到「" + GushaStandbyName + "」。";
                return false;
            }

            // 幂等：已有则删再建，避免叠两份
            var existing = FindDeepChild(compositeRoot, YaerStandbyName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var go = new GameObject(YaerStandbyName);
            go.transform.SetParent(compositeRoot, false);
            go.transform.localPosition = gusha.localPosition + new Vector3(LocalOffsetXFromGusha, 0f, 0f);
            go.transform.localRotation = gusha.localRotation;
            go.transform.localScale = gusha.localScale;
            go.SetActive(false);
            go.layer = gusha.gameObject.layer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            // 合层内其它物件多用 Default；侧面对齐 SceneObject 亦可，此处跟古莎同层防穿帮
            var gushaSr = gusha.GetComponent<SpriteRenderer>();
            if (gushaSr != null)
            {
                sr.sortingLayerID = gushaSr.sortingLayerID;
                sr.sortingLayerName = gushaSr.sortingLayerName;
            }
            else
            {
                sr.sortingLayerName = SortingLayerName.SceneObject;
            }

            sr.sortingOrder = SortingOrder;
            EditorUtility.SetDirty(go);

            message = "已预置 "
                + YaerStandbyName
                + " @ local "
                + go.transform.localPosition
                + " sortingOrder="
                + SortingOrder
                + "（默认关）";
            return true;
        }

        private static void BindGsmRef(UnityEngine.SceneManagement.Scene scene)
        {
            var gsm = Object.FindObjectOfType<Village_Chief_HouseSceneManager>();
            if (gsm == null)
            {
                Debug.LogWarning("[YaerCombatStandbySetup] 未找到 Village_Chief_HouseSceneManager，跳过引用绑定。");
                return;
            }

            var yaer = FindNamed(scene, YaerStandbyName);
            var so = new SerializedObject(gsm);
            var prop = so.FindProperty("yaerCombatStandby");
            if (prop != null && yaer != null)
            {
                prop.objectReferenceValue = yaer.gameObject;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gsm);
                Debug.Log("[YaerCombatStandbySetup] 已绑 GSM yaerCombatStandby。");
            }
            else
            {
                Debug.LogWarning(
                    "[YaerCombatStandbySetup] 未绑 GSM：prop="
                    + (prop != null)
                    + " yaer="
                    + (yaer != null)
                    + "（须先编译含 yaerCombatStandby 字段的 GSM）。");
            }
        }

        private static Transform FindNamed(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var t = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(x => x.name == name);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

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
