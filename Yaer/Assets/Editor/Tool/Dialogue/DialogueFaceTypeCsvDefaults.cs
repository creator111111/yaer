using System;
using Game.Static.Enum.Dialogue;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// CSV FaceType 列 → DialogueFaceType；空值时按 Actor 参数名给默认表情。
    /// 默认规则：雅尔图集无 Normal，故空列时默认 Smile；古莎/艾米/艾莉等有 Normal 的角色默认 Normal；村长占位 Normal 并 Warning。
    /// </summary>
    public static class DialogueFaceTypeCsvDefaults
    {
        /// <summary>
        /// 解析 FaceType 原始字符串；空串时按 actorParameterName 查默认表。
        /// 非空但无法解析时返回 false（Validate 阶段应已拦截，此处为建图兜底）。
        /// </summary>
        public static bool TryResolve(
            string faceTypeRaw,
            string actorParameterName,
            out DialogueFaceType faceType)
        {
            if (!string.IsNullOrWhiteSpace(faceTypeRaw))
            {
                if (Enum.TryParse(faceTypeRaw.Trim(), true, out faceType))
                {
                    return true;
                }

                faceType = DialogueFaceType.Normal;
                return false;
            }

            faceType = GetDefaultForActor(actorParameterName);
            return true;
        }

        /// <summary>
        /// 空 FaceType 列时，按映射后的 Actor 参数名返回安全默认表情。
        /// 雅尔 → Smile（Avatar_Yaer 无 Normal）；古莎/艾米/艾莉 → Normal；
        /// 村长/埃吉尔 → Normal 并 Warning（图集未就绪）；其它 → Normal 并 Warning。
        /// </summary>
        private static DialogueFaceType GetDefaultForActor(string actorParameterName)
        {
            if (string.Equals(actorParameterName, "雅尔", StringComparison.Ordinal))
            {
                return DialogueFaceType.Smile;
            }

            // Avatar_Gusha / Avatar_Amy / Avatar_Aliy 均有 Normal
            if (string.Equals(actorParameterName, "古莎", StringComparison.Ordinal)
                || string.Equals(actorParameterName, "艾米", StringComparison.Ordinal)
                || string.Equals(actorParameterName, "艾莉", StringComparison.Ordinal))
            {
                return DialogueFaceType.Normal;
            }

            // 村长：立绘资源未接入前占位 Normal，避免因空 FaceType 列导致建图失败
            if (string.Equals(actorParameterName, "村长", StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    "[DialogueFaceTypeCsvDefaults] Actor「村长」FaceType 列为空，使用 Normal；立绘图集未就绪，请检查映射与 CSV。");
                return DialogueFaceType.Normal;
            }

            // 埃吉尔：Avatar_Aegir 未入库前占位 Normal，与村长同理，空 FaceType 列不阻塞导入
            if (string.Equals(actorParameterName, "埃吉尔", StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    "[DialogueFaceTypeCsvDefaults] Actor「埃吉尔」FaceType 列为空，使用 Normal；立绘图集未就绪，请检查映射与 CSV。");
                return DialogueFaceType.Normal;
            }

            Debug.LogWarning(
                $"[DialogueFaceTypeCsvDefaults] Actor「{actorParameterName}」FaceType 列为空，使用 Normal；请检查映射与 CSV。");
            return DialogueFaceType.Normal;
        }

        // 可选 v1.1：WarnIfFaceNotInAtlas(actorParameterName, faceType) 读 SpriteAtlas 校验，非 MVP
    }
}
