#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Game.GameRuntime.Story.Node;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Tips;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 村长家续聊：锚句「针线包」后插入 GetItem → OpenTips → SaveBag；并 Pack tipsInfo 三语图集。
    /// 菜单：Tools / Dialogue / Setup Village 续聊针线包奖励
    /// </summary>
    /// <remarks>
    /// 原因（0901）：入包与 Tips 横幅是两步；TipKey 必须是 GetSewingKit（中文文件名取不到）。
    /// 对齐老农三连；改边 36→37，串入三 Action，勿冲掉 Import 以外的手工节点时用本菜单重跑。
    /// 替代方案：手改 Prefab YAML——易坏 $ref / 节点 ID，不采用。
    /// </remarks>
    public static class VillageChiefContinueSewingKitSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Village 续聊针线包奖励";
        private const string PrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab";
        private const string AnchorSubstring = "针线包";
        private const string ItemName = "SewingKit";
        private const string TipKey = "GetSewingKit";
        private const string AutoRequestFileName = "ChiefContinueSewingKitSetup.request";

        private static readonly string[] TipsAtlasPaths =
        {
            "Assets/GameRes/Atlas/TipsPanel/tipsInfo.spriteatlas",
            "Assets/GameRes/Atlas/TipsPanel/tipsInfo_en.spriteatlas",
            "Assets/GameRes/Atlas/TipsPanel/tipsInfo_jp.spriteatlas",
        };

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
                Debug.LogWarning("[SewingKitSetup] 无法删除自动请求文件：" + ex.Message);
                return;
            }

            Debug.Log("[SewingKitSetup] 检测到 Library/ChiefContinueSewingKitSetup.request，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            PackTipsAtlases();

            if (!InsertRewardNodesIntoContinuePrefab())
            {
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SewingKitSetup] 完成：续聊 Prefab 已挂三连 + tipsInfo 已 Pack。");
        }

        /// <summary>把 TipInfoAtlas 文件夹内新图打进 tipsInfo*（否则 GetSprite 找不到 GetSewingKit）。</summary>
        private static void PackTipsAtlases()
        {
            var atlases = TipsAtlasPaths
                .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                .Where(a => a != null)
                .ToArray();
            if (atlases.Length == 0)
            {
                Debug.LogError("[SewingKitSetup] 未找到 tipsInfo*.spriteatlas");
                return;
            }

            SpriteAtlasUtility.PackAtlases(atlases, EditorUserBuildSettings.activeBuildTarget);
            foreach (var a in atlases)
            {
                EditorUtility.SetDirty(a);
            }

            Debug.Log($"[SewingKitSetup] Pack 图集 ×{atlases.Length}");
        }

        /// <summary>
        /// 在「针线包」Statement 与下一句之间插入：
        /// GetItem(SewingKit,1) → OpenTipsForm(GetSewingKit, Item) → SavePlayerBag。
        /// 已存在同 TipKey 的 OpenTips 则跳过（可重复点菜单）。
        /// </summary>
        private static bool InsertRewardNodesIntoContinuePrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[SewingKitSetup] Prefab 不存在：" + PrefabPath);
                return false;
            }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var controller = root.GetComponent<DialogueTreeController>();
                if (controller == null || controller.behaviour == null)
                {
                    Debug.LogError("[SewingKitSetup] Prefab 无 DialogueTreeController / behaviour。");
                    return false;
                }

                var tree = controller.behaviour as DialogueTree;
                if (tree == null)
                {
                    Debug.LogError("[SewingKitSetup] behaviour 不是 DialogueTree。");
                    return false;
                }

                // 幂等：已挂过 GetSewingKit Tips 则只 Pack，不重复插节点
                if (TreeAlreadyHasSewingKitReward(tree))
                {
                    Debug.Log("[SewingKitSetup] 图内已有 GetSewingKit Tips，跳过插节点。");
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                    return true;
                }

                var anchor = FindAnchorStatement(tree);
                if (anchor == null)
                {
                    Debug.LogError("[SewingKitSetup] 未找到含「针线包」的 Statement 节点。");
                    return false;
                }

                if (anchor.outConnections == null || anchor.outConnections.Count == 0)
                {
                    Debug.LogError("[SewingKitSetup] 锚句无出边，无法串奖励。");
                    return false;
                }

                // 现网单出边：36 → 37；拆掉后串三连再接回原下一句
                var oldOut = anchor.outConnections[0];
                var nextNode = oldOut.targetNode;
                tree.RemoveConnection(oldOut);

                var basePos = anchor.position;
                var getNode = CreateGetItemNode(tree, basePos + new Vector2(160f, 80f));
                var tipsNode = CreateOpenTipsNode(tree, basePos + new Vector2(160f, 160f));
                var saveNode = CreateSaveBagNode(tree, basePos + new Vector2(160f, 240f));

                tree.ConnectNodes(anchor, getNode);
                tree.ConnectNodes(getNode, tipsNode);
                tree.ConnectNodes(tipsNode, saveNode);
                tree.ConnectNodes(saveNode, nextNode);

                controller.SetBoundGraphReference(tree);
                EditorUtility.SetDirty(controller);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[SewingKitSetup] 已插入 GetItem→OpenTips→SaveBag → 下一句。");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool TreeAlreadyHasSewingKitReward(DialogueTree tree)
        {
            foreach (var node in tree.allNodes.OfType<ActionNode>())
            {
                var tips = node.action as OpenTipsFormActionTask;
                if (tips == null || tips.TipKey == null)
                {
                    continue;
                }

                if (tips.TipKey.value == TipKey)
                {
                    return true;
                }
            }

            return false;
        }

        private static StatementNodeEx FindAnchorStatement(DialogueTree tree)
        {
            foreach (var node in tree.allNodes.OfType<StatementNodeEx>())
            {
                var text = node.statement != null ? node.statement.text : null;
                if (!string.IsNullOrEmpty(text) && text.Contains(AnchorSubstring))
                {
                    return node;
                }
            }

            return null;
        }

        private static ActionNode CreateGetItemNode(DialogueTree tree, Vector2 position)
        {
            var node = tree.AddNode<ActionNode>(position);
            var task = (GetItemActionTask)Task.Create(typeof(GetItemActionTask), tree);
            task.ItemName = ItemName;
            task.Num = 1;
            node.action = task;
            return node;
        }

        private static ActionNode CreateOpenTipsNode(DialogueTree tree, Vector2 position)
        {
            var node = tree.AddNode<ActionNode>(position);
            var task = (OpenTipsFormActionTask)Task.Create(typeof(OpenTipsFormActionTask), tree);
            if (task.TipKey == null)
            {
                task.TipKey = new BBParameter<string>();
            }

            task.TipKey.value = TipKey;
            task.TipsType = ETipsType.Item;
            node.action = task;
            return node;
        }

        private static ActionNode CreateSaveBagNode(DialogueTree tree, Vector2 position)
        {
            var node = tree.AddNode<ActionNode>(position);
            var task = (SavePlayerBagActionTask)Task.Create(typeof(SavePlayerBagActionTask), tree);
            node.action = task;
            return node;
        }
    }
}
#endif
