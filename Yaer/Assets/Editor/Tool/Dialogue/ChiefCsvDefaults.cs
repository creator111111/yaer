using System;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
using Game.Static.Enum.Dialogue;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 村长 CSV 列解析：FaceType Face1～Face3（门口台本直写）；与雅/古 DialogueFaceType、店 Face1～5 分流。
    /// 晚宴 Smile/CloseEyes 仍走 DialogueFaceType + RoleName.Chief（F2），不强制本分流。
    /// </summary>
    public static class ChiefCsvDefaults
    {
        /// <summary>Speaker 映射后的 Actor 参数名。</summary>
        public const string ChiefActorName = "村长";

        /// <summary>CSV Speaker 列简称。</summary>
        public const string ChiefCsvSpeaker = "村";

        public static bool IsChiefActor(string actorParameterName)
        {
            return string.Equals(actorParameterName, ChiefActorName, StringComparison.Ordinal);
        }

        public static bool IsChiefCsvSpeaker(string csvSpeaker)
        {
            return string.Equals(csvSpeaker?.Trim(), ChiefCsvSpeaker, StringComparison.Ordinal);
        }

        public static bool IsChiefRow(DialogueRow row, DialogueSpeakerMapping mapping)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.speaker))
            {
                return false;
            }

            if (mapping != null && mapping.TryResolve(row.speaker, out var actorName))
            {
                return IsChiefActor(actorName);
            }

            return IsChiefCsvSpeaker(row.speaker);
        }

        /// <summary>
        /// 是否为门口直写的 Face1～3 token（排除误把 DialogueFaceType 当 ChiefFace）。
        /// </summary>
        public static bool IsChiefFaceToken(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            var t = raw.Trim();
            return string.Equals(t, "Face1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(t, "Face2", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(t, "Face3", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>解析村长 Face1～3；空串视为 Face1（继承层另处理）。</summary>
        public static bool TryParseFace(string raw, out ChiefFaceType face)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                face = ChiefFaceType.Face1;
                return true;
            }

            if (!IsChiefFaceToken(raw))
            {
                face = ChiefFaceType.Face1;
                return false;
            }

            return Enum.TryParse(raw.Trim(), true, out face);
        }

        /// <summary>空串保持 current；非空则须为 Face1～3。</summary>
        public static bool ApplyFaceInheritance(string raw, ref ChiefFaceType current)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            if (!TryParseFace(raw, out var parsed))
            {
                return false;
            }

            current = parsed;
            return true;
        }

        /// <summary>村长行误填且既非 Face1～3 也非 DialogueFaceType 时由 Parser 报错。</summary>
        public static bool IsValidChiefFaceTypeColumn(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            if (IsChiefFaceToken(raw))
            {
                return true;
            }

            // 晚宴兼容：Smile / CloseEyes 等
            return Enum.TryParse(raw.Trim(), true, out DialogueFaceType _);
        }
    }
}
