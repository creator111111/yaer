using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 从参考对话 Prefab 的 Blackboard 解析 <see cref="CanvasGroup"/> 变量名，
    /// 供 <see cref="DialoguePreludeBuilder"/> 写入 Action 节点（方案 A：仅写 _name 字符串）。
    /// </summary>
    public static class DialoguePortraitReferenceResolver
    {
        /// <summary>
        /// 读取参考 Prefab 上 CanvasGroup 类型 Blackboard 变量名（保持 Blackboard 内顺序）。
        /// </summary>
        /// <param name="referencePrefab">如 Village_KenMuNiStart.prefab。</param>
        /// <param name="variableNames">解析到的变量名列表。</param>
        /// <param name="error">失败原因。</param>
        public static bool TryResolveCanvasGroupVariableNames(
            GameObject referencePrefab,
            out List<string> variableNames,
            out string error)
        {
            variableNames = new List<string>();
            error = null;

            if (referencePrefab == null)
            {
                error = "参考 Prefab 为空。";
                return false;
            }

            var prefabPath = AssetDatabase.GetAssetPath(referencePrefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                error = "所选对象不是项目内的 Prefab 资源。";
                return false;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var controller = prefabRoot.GetComponentInChildren<DialogueTreeController>(true);
                if (controller == null)
                {
                    error = $"Prefab「{referencePrefab.name}」上未找到 DialogueTreeController。";
                    return false;
                }

                var blackboard = controller.blackboard;
                if (blackboard == null || blackboard.variables == null || blackboard.variables.Count == 0)
                {
                    error = $"Prefab「{referencePrefab.name}」的 Blackboard 无变量。";
                    return false;
                }

                foreach (var pair in blackboard.variables)
                {
                    var variable = pair.Value;
                    if (variable?.varType == typeof(UnityEngine.CanvasGroup))
                    {
                        variableNames.Add(variable.name);
                    }
                }

                if (variableNames.Count == 0)
                {
                    error = $"Prefab「{referencePrefab.name}」的 Blackboard 中未找到 CanvasGroup 类型变量。";
                    return false;
                }

                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
