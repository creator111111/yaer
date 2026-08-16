using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// Village_KenMuNiStart 角/翅膀动画 Prefab 装配：单帧子物体 + Animator + 默认隐藏 + Blackboard。
    /// </summary>
    /// <remarks>
    /// 原因：手改 Prefab YAML 易丢引用；用菜单一次装配可验收阶段 1～2。
    /// 菜单：Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim
    /// </remarks>
    public static class KenMuNiStartAnimSetup
    {
        private const string PrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab";
        private const string GushaControllerPath = "Assets/GameRes/Animation/Dialogue/Anim_Gusha_Horn.controller";
        private const string YaerControllerPath = "Assets/GameRes/Animation/Dialogue/Anim_Yaer_Wing.controller";

        [MenuItem("Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim")]
        public static void Setup()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                SetupContainer(prefabRoot, "Anim_Gusha", "G1", GushaControllerPath, out var gushaAnimator);
                SetupContainer(prefabRoot, "Anim_Yaer", "Y1", YaerControllerPath, out var yaerAnimator);

                var blackboard = prefabRoot.GetComponent<Blackboard>();
                if (blackboard == null)
                {
                    Debug.LogError("[KenMuNiStartAnimSetup] Prefab 无 Blackboard。");
                }
                else
                {
                    EnsureAnimatorVariable(blackboard, "Anim_Gusha", gushaAnimator);
                    EnsureAnimatorVariable(blackboard, "Anim_Yaer", yaerAnimator);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                Debug.Log("[KenMuNiStartAnimSetup] 已装配 Anim_Gusha / Anim_Yaer（Animator + 单子物体 + 默认隐藏 + BB）。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        static void SetupContainer(
            GameObject root,
            string containerName,
            string keepChildName,
            string controllerPath,
            out Animator animator)
        {
            animator = null;
            var container = FindDeep(root.transform, containerName);
            if (container == null)
            {
                Debug.LogError($"[KenMuNiStartAnimSetup] 未找到 {containerName}");
                return;
            }

            // 只留一帧显示 Image；其余子物体删除（Sprite 已录进 Clip）。
            var toDelete = new List<GameObject>();
            for (var i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i).gameObject;
                if (child.name != keepChildName)
                {
                    toDelete.Add(child);
                }
            }

            foreach (var go in toDelete)
            {
                Object.DestroyImmediate(go);
            }

            animator = container.GetComponent<Animator>();
            if (animator == null)
            {
                animator = container.gameObject.AddComponent<Animator>();
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogError($"[KenMuNiStartAnimSetup] 缺少 Controller：{controllerPath}");
            }
            else
            {
                animator.runtimeAnimatorController = controller;
            }

            // 入场前隐藏，避免五帧叠显；播放时由 PlayUiAnimator 打开。
            container.gameObject.SetActive(false);
        }

        static void EnsureAnimatorVariable(Blackboard blackboard, string varName, Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            // Blackboard.variables 是 IBlackboard 显式实现，不能直接点；用 GetVariable / SetVariableValue。
            if (blackboard.GetVariable(varName) != null)
            {
                blackboard.SetVariableValue(varName, animator);
                return;
            }

            blackboard.AddVariable(varName, animator);
        }

        static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindDeep(parent.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
