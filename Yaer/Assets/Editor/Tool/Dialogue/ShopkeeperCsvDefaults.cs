using System;
using Game.GameRuntime.UI.FormLogic.Shop;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 商店老板娘 CSV 列解析：FaceType（Face1～5）与可选 BodyType（Normal/Red/YinXian）。
    /// 与雅/古 <see cref="DialogueFaceType"/> 分流；空列在 GraphBuilder 继承上一句。
    /// </summary>
    public static class ShopkeeperCsvDefaults
    {
        /// <summary>Speaker 映射后的 Actor 参数名。</summary>
        public const string ShopkeeperActorName = "老板娘";

        /// <summary>CSV Speaker 列简称（与映射表一致）。</summary>
        public const string ShopkeeperCsvSpeaker = "店";

        /// <summary>判断映射后的 Actor 是否为老板娘。</summary>
        public static bool IsShopkeeperActor(string actorParameterName)
        {
            return string.Equals(actorParameterName, ShopkeeperActorName, StringComparison.Ordinal);
        }

        /// <summary>无映射时按 CSV 简称判断。</summary>
        public static bool IsShopkeeperCsvSpeaker(string csvSpeaker)
        {
            return string.Equals(csvSpeaker?.Trim(), ShopkeeperCsvSpeaker, StringComparison.Ordinal);
        }

        /// <summary>映射优先；未命中映射时回退 CSV 简称。</summary>
        public static bool IsShopkeeperRow(DialogueRow row, DialogueSpeakerMapping mapping)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.speaker))
            {
                return false;
            }

            if (mapping != null && mapping.TryResolve(row.speaker, out var actorName))
            {
                return IsShopkeeperActor(actorName);
            }

            return IsShopkeeperCsvSpeaker(row.speaker);
        }

        /// <summary>解析店行 FaceType 字符串（Face1～Face5）。</summary>
        public static bool TryParseFace(string raw, out ShopkeeperFaceType face)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                face = ShopkeeperFaceType.Face1;
                return true;
            }

            return Enum.TryParse(raw.Trim(), true, out face);
        }

        /// <summary>解析 BodyType 列：Normal / Red / YinXian。</summary>
        public static bool TryParseBody(string raw, out ShopkeeperBodyType body)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                body = ShopkeeperBodyType.Normal;
                return true;
            }

            var trimmed = raw.Trim();
            if (string.Equals(trimmed, "Normal", StringComparison.OrdinalIgnoreCase))
            {
                body = ShopkeeperBodyType.Normal;
                return true;
            }

            if (string.Equals(trimmed, "Red", StringComparison.OrdinalIgnoreCase))
            {
                body = ShopkeeperBodyType.Blush;
                return true;
            }

            if (string.Equals(trimmed, "YinXian", StringComparison.OrdinalIgnoreCase))
            {
                body = ShopkeeperBodyType.Sinister;
                return true;
            }

            body = ShopkeeperBodyType.Normal;
            return false;
        }

        /// <summary>空串保持 current；非空则解析并写入。</summary>
        public static bool ApplyFaceInheritance(string raw, ref ShopkeeperFaceType current)
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

        /// <summary>空串保持 current；非空则解析并写入。</summary>
        public static bool ApplyBodyInheritance(string raw, ref ShopkeeperBodyType current)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            if (!TryParseBody(raw, out var parsed))
            {
                return false;
            }

            current = parsed;
            return true;
        }

        /// <summary>店行误填 Laugh/Angry 等 DialogueFaceType 时拒导入。</summary>
        public static bool IsLikelyDialogueFaceType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            return Enum.TryParse(raw.Trim(), true, out Game.Static.Enum.Dialogue.DialogueFaceType _);
        }
    }
}
