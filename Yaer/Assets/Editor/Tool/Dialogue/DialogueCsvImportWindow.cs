using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// CSV → DialogueTree .asset 导入窗口。
    /// 菜单：Tools/Dialogue/Import CSV（阶段 1 交付：生成独立 .asset 供 NodeCanvas 校对）。
    /// </summary>
    public class DialogueCsvImportWindow : EditorWindow
    {
        private const string DefaultOutputFolder = "Assets/GameRes/DialogueTrees/Generated";

        private TextAsset csvAsset;
        private DialogueSpeakerMapping speakerMapping;
        private string outputFolder = DefaultOutputFolder;
        private string lastError;
        private Vector2 scrollPosition;

        // 开场前奏（可选），默认全 false，与阶段 1 行为一致
        private bool fadeDialogueUI;
        private bool hideFightingPanelOnStart;
        private bool restoreFightingPanelOnEnd;
        private bool fadePortraitCanvasGroups;
        private GameObject portraitReferencePrefab;
        private bool showPreludeSection = true;

        [MenuItem("Tools/Dialogue/Import CSV")]
        public static void Open()
        {
            var window = GetWindow<DialogueCsvImportWindow>("CSV → DialogueTree");
            window.minSize = new Vector2(420f, 460f);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("CSV → NodeCanvas DialogueTree", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "阶段 1：从 CSV 生成 DialogueTree .asset（StatementNodeEx + 选项分支 + 连线）。\n" +
                "产物需先在 NodeCanvas 编辑器中校对；合并进 Prefab 为阶段 2。\n" +
                "CSV 支持列：…, Extra, FaceType。FaceType 填枚举英文名（如 Smile）；Dialogue/Anim 行有效。" +
                "Type=Anim 时 Extra 填动画键（如 Anim_Gusha），导入器生成 Play UI Animator → Statement。" +
                "旧 6 列仍可用（雅尔默认 Smile，古莎默认 Normal）。" +
                "策划表含 English / Voice 等额外列时，Next 与 FaceType 按表头列名自动识别。",
                MessageType.Info);

            EditorGUILayout.Space(4f);

            csvAsset = (TextAsset)EditorGUILayout.ObjectField("CSV 文件", csvAsset, typeof(TextAsset), false);

            speakerMapping = (DialogueSpeakerMapping)EditorGUILayout.ObjectField(
                "Speaker 映射",
                speakerMapping,
                typeof(DialogueSpeakerMapping),
                false);

            if (speakerMapping == null)
            {
                EditorGUILayout.HelpBox(
                    "未指定映射时将使用内置默认：雅→雅尔、古→古莎、艾米→艾米、艾莉→艾莉、村→村长、埃吉尔→埃吉尔、—→旁白。\n" +
                    "建议在项目中创建 DialogueSpeakerMapping 资产统一管理（可与内置默认内容一致）。",
                    MessageType.None);
            }

            outputFolder = EditorGUILayout.TextField("输出目录", outputFolder);

            EditorGUILayout.Space(6f);
            showPreludeSection = EditorGUILayout.Foldout(showPreludeSection, "开场前奏（可选）", true);
            if (showPreludeSection)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "· 全部不勾选：与阶段 1 相同，仅生成对白/选项节点。\n" +
                    "· 对话框 UI 淡入 / 藏战斗面板：可写入 .asset，合并进 Prefab 后实机有效。\n" +
                    "· 立绘淡入：须指定参考 Prefab（读取 Blackboard 中 CanvasGroup 变量名）；仅生成 .asset 且无 Blackboard 时节点无法绑定，导入时会警告或中止。",
                    MessageType.Info);

                fadeDialogueUI = EditorGUILayout.Toggle("对话框 UI 淡入", fadeDialogueUI);
                hideFightingPanelOnStart = EditorGUILayout.Toggle("开始时隐藏战斗面板", hideFightingPanelOnStart);
                if (!hideFightingPanelOnStart)
                {
                    restoreFightingPanelOnEnd = false;
                }

                using (new EditorGUI.DisabledScope(!hideFightingPanelOnStart))
                {
                    restoreFightingPanelOnEnd = EditorGUILayout.Toggle("结束时恢复战斗面板", restoreFightingPanelOnEnd);
                }

                fadePortraitCanvasGroups = EditorGUILayout.Toggle("立绘 CanvasGroup 淡入", fadePortraitCanvasGroups);
                using (new EditorGUI.DisabledScope(!fadePortraitCanvasGroups))
                {
                    portraitReferencePrefab = (GameObject)EditorGUILayout.ObjectField(
                        "立绘参考 Prefab",
                        portraitReferencePrefab,
                        typeof(GameObject),
                        false);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(csvAsset == null))
            {
                if (GUILayout.Button("生成 DialogueTree .asset", GUILayout.Height(32f)))
                {
                    GenerateAsset();
                }
            }

            if (!string.IsNullOrEmpty(lastError))
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox(lastError, MessageType.Error);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 读取 CSV → 解析校验 → 建图 → 写入 .asset，全程注册 Undo。
        /// </summary>
        private void GenerateAsset()
        {
            lastError = null;

            if (csvAsset == null)
            {
                lastError = "请先选择 CSV TextAsset。";
                return;
            }

            var csvPath = AssetDatabase.GetAssetPath(csvAsset);
            if (string.IsNullOrEmpty(csvPath) || !csvPath.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
            {
                lastError = "所选资源不是 .csv 文件。";
                return;
            }

            string csvText;
            try
            {
                // 与样例 Assets/Dialog/村内第一段对话.csv 一致使用 UTF-8
                csvText = File.ReadAllText(csvPath, Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                lastError = $"读取 CSV 失败：{ex.Message}";
                return;
            }

            if (!DialogueCsvParser.TryParse(csvText, out var rows, out var parseError))
            {
                lastError = parseError;
                return;
            }

            var mapping = speakerMapping != null
                ? speakerMapping
                : DialogueSpeakerMapping.CreateDefaultInstance();

            var assetBaseName = Path.GetFileNameWithoutExtension(csvPath);
            var preludeOptions = BuildPreludeOptions();
            if (!preludeOptions.Validate(out var preludeError))
            {
                lastError = preludeError;
                return;
            }

            var tree = DialogueCsvGraphBuilder.TryBuild(
                rows,
                mapping,
                startRowId: null,
                assetName: assetBaseName,
                preludeOptions);
            if (tree == null)
            {
                lastError = "建图失败，详见 Console。";
                return;
            }

            if (!EnsureOutputFolder(outputFolder))
            {
                lastError = $"无法创建输出目录：{outputFolder}";
                UnityEngine.Object.DestroyImmediate(tree);
                return;
            }

            var outputPath = $"{outputFolder.TrimEnd('/')}/{assetBaseName}.asset";
            outputPath = AssetDatabase.GenerateUniqueAssetPath(outputPath);

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Import CSV Dialogue");
            Undo.RegisterCreatedObjectUndo(tree, "Import CSV Dialogue");

            AssetDatabase.CreateAsset(tree, outputPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(tree);
            Selection.activeObject = tree;

            Debug.Log($"[DialogueCsvImportWindow] 已生成：{outputPath}（{rows.Count} 行 → {tree.allNodes.Count} 节点）");
            lastError = null;
        }

        /// <summary>从窗口勾选组装前奏 DTO。</summary>
        private DialoguePreludeOptions BuildPreludeOptions()
        {
            return new DialoguePreludeOptions
            {
                FadeDialogueUI = fadeDialogueUI,
                HideFightingPanelOnStart = hideFightingPanelOnStart,
                RestoreFightingPanelOnEnd = restoreFightingPanelOnEnd,
                FadePortraitCanvasGroups = fadePortraitCanvasGroups,
                PortraitReferencePrefab = portraitReferencePrefab
            };
        }

        /// <summary>确保输出目录存在（相对 Assets 路径）。</summary>
        private static bool EnsureOutputFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return false;
            }

            if (AssetDatabase.IsValidFolder(folder))
            {
                return true;
            }

            var parts = folder.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                return false;
            }

            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }

            return AssetDatabase.IsValidFolder(folder);
        }
    }
}
