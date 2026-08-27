using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Static.Enum.Dialogue;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 将 UTF-8 CSV 文本解析为 <see cref="DialogueRow"/> 列表，并做 ID / Next 引用校验。
    /// 使用引号感知的字段拆分，避免中文逗号或对白内逗号被误切。
    /// 列位置优先按表头列名解析（支持 English / Voice 等额外列）；无表头命中时回退旧版固定列序。
    /// </summary>
    public static class DialogueCsvParser
    {
        /// <summary>
        /// Choice 的 Next 列中「END」占位符，表示该选项无后续节点（对话 Success 结束）。
        /// 与整数 ID 区分，供 <see cref="DialogueCsvGraphBuilder"/> 按 sourceIndex 跳过连线或挂结束 Action。
        /// </summary>
        public const int EndBranchSentinel = -1;

        /// <summary>旧版固定列序：至少 6 列（无 FaceType）。</summary>
        private const int LegacyMinColumnCount = 6;

        /// <summary>旧版 FaceType 列索引（0-based，第 7 列）。</summary>
        private const int LegacyFaceTypeColumnIndex = 6;

        /// <summary>
        /// 解析 CSV 并完成结构校验。失败时 error 含原因，rows 可能为部分结果（调用方应忽略）。
        /// </summary>
        /// <param name="mapping">可选；用于店行 Face/Body 分流校验。</param>
        public static bool TryParse(
            string csvText,
            out List<DialogueRow> rows,
            out string error,
            out bool hasBodyTypeColumn,
            DialogueSpeakerMapping mapping = null)
        {
            rows = new List<DialogueRow>();
            error = null;
            hasBodyTypeColumn = false;

            if (string.IsNullOrWhiteSpace(csvText))
            {
                error = "CSV 内容为空。";
                return false;
            }

            var lines = SplitLines(csvText);
            if (lines.Count < 2)
            {
                error = "CSV 至少需要表头与一行数据。";
                return false;
            }

            // 第一行表头：按列名定位 Next / FaceType 等（兼容 English、Voice 等额外列）
            var headerFields = ParseCsvLine(lines[0]);
            if (!DialogueCsvColumnMap.TryFromHeader(headerFields, out var columnMap, out var headerError))
            {
                error = headerError;
                return false;
            }

            hasBodyTypeColumn = columnMap.HasBodyTypeColumn;

            // 跳过表头（第一行）
            for (var lineIndex = 1; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var fields = ParseCsvLine(line);
                if (fields.Count <= columnMap.MinRequiredFieldCount)
                {
                    Debug.LogWarning(
                        $"[DialogueCsvParser] 第 {lineIndex + 1} 行列数不足（至少 {columnMap.MinRequiredFieldCount + 1} 列），已跳过：{line}");
                    continue;
                }

                if (!int.TryParse(columnMap.GetField(fields, columnMap.IdIndex), out var id))
                {
                    Debug.LogWarning(
                        $"[DialogueCsvParser] 第 {lineIndex + 1} 行 ID 非法（「{fields[0]}」），已跳过。");
                    continue;
                }

                rows.Add(new DialogueRow
                {
                    id = id,
                    type = columnMap.GetField(fields, columnMap.TypeIndex),
                    speaker = columnMap.GetField(fields, columnMap.SpeakerIndex),
                    text = columnMap.GetField(fields, columnMap.TextIndex),
                    next = columnMap.GetField(fields, columnMap.NextIndex),
                    extra = columnMap.GetField(fields, columnMap.ExtraIndex),
                    faceType = columnMap.GetField(fields, columnMap.FaceTypeIndex),
                    bodyType = columnMap.GetField(fields, columnMap.BodyTypeIndex),
                });
            }

            if (rows.Count == 0)
            {
                error = "未解析到任何有效数据行。";
                return false;
            }

            return Validate(rows, columnMap.HasBodyTypeColumn, mapping, out error);
        }

        /// <summary>
        /// 校验 ID 唯一性、Next 引用存在性、Choice 行 Extra/Next 数量一致。
        /// </summary>
        public static bool Validate(
            IReadOnlyList<DialogueRow> rows,
            bool hasBodyTypeColumn,
            DialogueSpeakerMapping mapping,
            out string error)
        {
            error = null;
            var idSet = new HashSet<int>();
            var idLookup = new HashSet<int>();

            foreach (var row in rows)
            {
                idLookup.Add(row.id);
            }

            foreach (var row in rows)
            {
                if (!idSet.Add(row.id))
                {
                    error = $"ID 重复：{row.id}";
                    return false;
                }

                if (!TryParseNodeType(row.type, out _))
                {
                    error = $"ID {row.id} 的 Type 非法（「{row.type}」），仅支持 Dialogue / Choice / Anim。";
                    return false;
                }

                if ((IsDialogueType(row.type) || IsAnimType(row.type)) && string.IsNullOrWhiteSpace(row.speaker))
                {
                    error = $"ID {row.id} 为 {row.type} 但 Speaker 为空。";
                    return false;
                }

                // FaceType：店行 Face1～5；其它角色 DialogueFaceType
                if ((IsDialogueType(row.type) || IsAnimType(row.type)) && !string.IsNullOrWhiteSpace(row.faceType))
                {
                    if (ShopkeeperCsvDefaults.IsShopkeeperRow(row, mapping))
                    {
                        if (ShopkeeperCsvDefaults.IsLikelyDialogueFaceType(row.faceType))
                        {
                            error =
                                $"ID {row.id} 店行 FaceType「{row.faceType}」非法，须用 Face1～Face5，勿填 Laugh/Angry 等对话表情。";
                            return false;
                        }

                        if (!ShopkeeperCsvDefaults.TryParseFace(row.faceType, out _))
                        {
                            error = $"ID {row.id} 店行 FaceType 非法（「{row.faceType}」），须为 Face1～Face5。";
                            return false;
                        }
                    }
                    else if (!Enum.TryParse<DialogueFaceType>(row.faceType, ignoreCase: true, out _))
                    {
                        error = $"ID {row.id} 的 FaceType 非法（「{row.faceType}」），须为 DialogueFaceType 枚举名。";
                        return false;
                    }
                }

                // 可选 BodyType 列：仅店行可填；无列时跳过
                if (hasBodyTypeColumn
                    && (IsDialogueType(row.type) || IsAnimType(row.type))
                    && !string.IsNullOrWhiteSpace(row.bodyType))
                {
                    if (ShopkeeperCsvDefaults.IsShopkeeperRow(row, mapping))
                    {
                        if (!ShopkeeperCsvDefaults.TryParseBody(row.bodyType, out _))
                        {
                            error =
                                $"ID {row.id} 店行 BodyType 非法（「{row.bodyType}」），须为 Normal / Red / YinXian。";
                            return false;
                        }
                    }
                    else
                    {
                        error =
                            $"ID {row.id} 非店行 Speaker「{row.speaker}」的 BodyType 必须为空。";
                        return false;
                    }
                }

                if (IsAnimType(row.type))
                {
                    if (string.IsNullOrWhiteSpace(row.extra))
                    {
                        error = $"ID {row.id} 为 Anim 但 Extra（动画键，如 Anim_Gusha）为空。";
                        return false;
                    }
                }

                if (IsChoiceType(row.type))
                {
                    if (string.IsNullOrWhiteSpace(row.extra))
                    {
                        error = $"ID {row.id} 为 Choice 但 Extra（选项文案）为空。";
                        return false;
                    }

                    var choiceTexts = SplitPipeList(row.extra);
                    var nextTargets = SplitNextTargets(row.next);
                    if (nextTargets.Count > 0 && choiceTexts.Count != nextTargets.Count)
                    {
                        error =
                            $"ID {row.id} Choice 的 Extra 选项数（{choiceTexts.Count}）与 Next 分支数（{nextTargets.Count}）不一致。";
                        return false;
                    }
                }

                var nextIds = SplitNextTargets(row.next);
                foreach (var nextId in nextIds)
                {
                    // END 占位不计入 ID 存在性校验
                    if (nextId == EndBranchSentinel)
                    {
                        continue;
                    }

                    if (!idLookup.Contains(nextId))
                    {
                        error = $"ID {row.id} 的 Next 引用了不存在的 ID：{nextId}";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>判断是否为对白行（大小写不敏感）。</summary>
        public static bool IsDialogueType(string type)
        {
            return TryParseNodeType(type, out var parsed) && parsed == DialogueNodeKind.Dialogue;
        }

        /// <summary>判断是否为选项分支行。</summary>
        public static bool IsChoiceType(string type)
        {
            return TryParseNodeType(type, out var parsed) && parsed == DialogueNodeKind.Choice;
        }

        /// <summary>判断是否为动作戏行（播 UI Animator + 字幕）。</summary>
        public static bool IsAnimType(string type)
        {
            return TryParseNodeType(type, out var parsed) && parsed == DialogueNodeKind.Anim;
        }

        /// <summary>
        /// 解析 Next 列：空 / 单独 END 返回空列表；
        /// 多分支时 END 记为 <see cref="EndBranchSentinel"/>（如「END|17」与两个 Extra 选项对齐）。
        /// </summary>
        public static List<int> SplitNextTargets(string next)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(next))
            {
                return result;
            }

            var trimmedWhole = next.Trim();
            // 单独 END：无出边（线性结束或 Choice 仅一支 END）
            if (string.Equals(trimmedWhole, "END", StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            foreach (var part in next.Split('|'))
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                // 多分支中的 END 占位，与 Extra 选项下标一一对应
                if (string.Equals(trimmed, "END", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(EndBranchSentinel);
                    continue;
                }

                if (int.TryParse(trimmed, out var targetId))
                {
                    result.Add(targetId);
                }
                else
                {
                    Debug.LogWarning($"[DialogueCsvParser] Next 列含非法 ID 片段：「{trimmed}」，已忽略。");
                }
            }

            return result;
        }

        /// <summary>按竖线拆分 Extra 等列，保留空段以便与 Next 数量对齐校验。</summary>
        public static List<string> SplitPipeList(string value)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            foreach (var part in value.Split('|'))
            {
                result.Add(part.Trim());
            }

            return result;
        }

        private enum DialogueNodeKind
        {
            Dialogue,
            Choice,
            /// <summary>动作戏：先播 UI Animator，再出 Statement 字幕（Extra=动画键）。</summary>
            Anim,
        }

        private static bool TryParseNodeType(string type, out DialogueNodeKind kind)
        {
            kind = DialogueNodeKind.Dialogue;
            if (string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            if (string.Equals(type.Trim(), "Dialogue", StringComparison.OrdinalIgnoreCase))
            {
                kind = DialogueNodeKind.Dialogue;
                return true;
            }

            if (string.Equals(type.Trim(), "Choice", StringComparison.OrdinalIgnoreCase))
            {
                kind = DialogueNodeKind.Choice;
                return true;
            }

            // 角/翅膀等帧动画行：Type=Anim，Extra=Anim_Gusha / Anim_Yaer
            if (string.Equals(type.Trim(), "Anim", StringComparison.OrdinalIgnoreCase))
            {
                kind = DialogueNodeKind.Anim;
                return true;
            }

            return false;
        }

        /// <summary>兼容 \n / \r\n 换行。</summary>
        private static List<string> SplitLines(string text)
        {
            var lines = new List<string>();
            using (var reader = new System.IO.StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// RFC 4180 风格单行解析：双引号包裹字段、"" 转义引号。
        /// 不使用 string.Split(',')，避免对白内逗号误切。
        /// </summary>
        internal static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 连续两个引号 → 字面量引号
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Length = 0;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString());
            return fields;
        }

        /// <summary>
        /// 由 CSV 表头解析列索引。策划表可含 English、Voice 等额外列，Next / FaceType 按列名定位。
        /// 表头未命中 Next 时回退旧版固定列序（6 或 7 列）。
        /// </summary>
        private sealed class DialogueCsvColumnMap
        {
            public int IdIndex { get; private set; }
            public int TypeIndex { get; private set; }
            public int SpeakerIndex { get; private set; }
            public int TextIndex { get; private set; }
            public int NextIndex { get; private set; }
            public int ExtraIndex { get; private set; }
            public int FaceTypeIndex { get; private set; } = -1;

            /// <summary>可选列；-1 表示表头无 BodyType。</summary>
            public int BodyTypeIndex { get; private set; } = -1;

            public bool HasBodyTypeColumn => BodyTypeIndex >= 0;

            /// <summary>数据行至少需要的最大列索引（0-based）。</summary>
            public int MinRequiredFieldCount =>
                new[] { IdIndex, TypeIndex, SpeakerIndex, TextIndex, NextIndex, ExtraIndex, FaceTypeIndex, BodyTypeIndex }
                    .Where(i => i >= 0)
                    .Max();

            public string GetField(IReadOnlyList<string> fields, int index)
            {
                if (index < 0 || index >= fields.Count)
                {
                    return string.Empty;
                }

                return fields[index].Trim();
            }

            public static bool TryFromHeader(
                IReadOnlyList<string> headerFields,
                out DialogueCsvColumnMap map,
                out string error)
            {
                map = null;
                error = null;

                if (headerFields == null || headerFields.Count == 0)
                {
                    error = "CSV 表头为空。";
                    return false;
                }

                var nextIndex = FindColumnIndex(headerFields, "Next");
                if (nextIndex >= 0)
                {
                    return TryBuildFromNamedHeader(headerFields, nextIndex, out map, out error);
                }

                return TryBuildLegacyPositional(headerFields, out map, out error);
            }

            /// <summary>表头含 Next 列名：按 ID / Type / … 列名定位，忽略 English、Voice 等。</summary>
            private static bool TryBuildFromNamedHeader(
                IReadOnlyList<string> headerFields,
                int nextIndex,
                out DialogueCsvColumnMap map,
                out string error)
            {
                map = new DialogueCsvColumnMap();
                error = null;

                // out 不能传给带 private set 的属性，须先写入局部变量再赋回 map
                if (!TryRequireColumn(headerFields, "ID", out var idIndex, out error)
                    || !TryRequireColumn(headerFields, "Type", out var typeIndex, out error)
                    || !TryRequireColumn(headerFields, "Speaker", out var speakerIndex, out error)
                    || !TryRequireColumn(headerFields, "Text", out var textIndex, out error))
                {
                    map = null;
                    return false;
                }

                if (!TryRequireColumn(headerFields, "Extra", out var extraIndex, out error))
                {
                    map = null;
                    return false;
                }

                map.IdIndex = idIndex;
                map.TypeIndex = typeIndex;
                map.SpeakerIndex = speakerIndex;
                map.TextIndex = textIndex;
                map.NextIndex = nextIndex;
                map.ExtraIndex = extraIndex;

                // FaceType 或策划表别名 Face；无此列时建图走说话人默认
                map.FaceTypeIndex = FindColumnIndex(headerFields, "FaceType");
                if (map.FaceTypeIndex < 0)
                {
                    map.FaceTypeIndex = FindColumnIndex(headerFields, "Face");
                }

                // 可选：商店老板娘身体变体
                map.BodyTypeIndex = FindColumnIndex(headerFields, "BodyType");

                return true;
            }

            /// <summary>旧版 6/7 列固定列序，兼容无 Next 列名的历史 CSV。</summary>
            private static bool TryBuildLegacyPositional(
                IReadOnlyList<string> headerFields,
                out DialogueCsvColumnMap map,
                out string error)
            {
                map = new DialogueCsvColumnMap
                {
                    IdIndex = 0,
                    TypeIndex = 1,
                    SpeakerIndex = 2,
                    TextIndex = 3,
                    NextIndex = 4,
                    ExtraIndex = 5,
                    FaceTypeIndex = headerFields.Count > LegacyFaceTypeColumnIndex
                        ? LegacyFaceTypeColumnIndex
                        : -1,
                };
                error = null;

                if (headerFields.Count < LegacyMinColumnCount)
                {
                    error = $"CSV 表头列数不足（至少 {LegacyMinColumnCount} 列），且未找到 Next 列名。";
                    map = null;
                    return false;
                }

                return true;
            }

            private static bool TryRequireColumn(
                IReadOnlyList<string> headerFields,
                string columnName,
                out int index,
                out string error)
            {
                index = FindColumnIndex(headerFields, columnName);
                if (index >= 0)
                {
                    error = null;
                    return true;
                }

                error = $"CSV 表头缺少必需列：{columnName}。";
                return false;
            }

            private static int FindColumnIndex(IReadOnlyList<string> headerFields, string columnName)
            {
                for (var i = 0; i < headerFields.Count; i++)
                {
                    if (string.Equals(headerFields[i].Trim(), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}
