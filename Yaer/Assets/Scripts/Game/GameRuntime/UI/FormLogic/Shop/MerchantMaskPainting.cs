using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 对话框 Mask 内商人小表情（UI 版 Body×Face Toggle）。
    /// 挂于 <c>NormalDialogueNewPanel/YaerAvatarRoot/MerchantMaskPainting</c>；
    /// 由 <see cref="Game.GameRuntime.UI.FormLogic.Story.DialogueMaskAvatarPresenter"/> 驱动，不注册 <see cref="ShopkeeperFaceRegistry"/>。
    /// </summary>
    /// <remarks>
    /// 与场景 <see cref="ShopkeeperFaceController"/> 同构，但不继承 <c>StoryFormPainting</c>。
    /// 无 <c>Start</c> 自动 Reset，避免首句与 Presenter 竞态。
    /// </remarks>
    public class MerchantMaskPainting : MonoBehaviour
    {
        public const string LogPrefix = "[MerchantMask]";

        private Transform _bodyRoot;
        private Transform _faceRoot;

        private readonly Dictionary<ShopkeeperBodyType, Transform> _bodyNodes =
            new Dictionary<ShopkeeperBodyType, Transform>();

        private readonly Dictionary<ShopkeeperFaceType, Transform> _faceNodes =
            new Dictionary<ShopkeeperFaceType, Transform>();

        private void Awake()
        {
            CacheChildGroups();
        }

        /// <summary>同时应用身体与脸（Mask Presenter 入口）。</summary>
        public void Apply(ShopkeeperBodyType body, ShopkeeperFaceType face)
        {
            SetBody(body);
            SetFace(face);
        }

        /// <summary>Editor 校正默认 Active；运行时勿在 Start 自动调用。</summary>
        public void ResetDefault()
        {
            Apply(ShopkeeperBodyType.Normal, ShopkeeperFaceType.Face1);
        }

        /// <summary>Editor Setup 后校正子节点 Active。</summary>
        public void EditorResetDefaultActiveState()
        {
            CacheChildGroups();
            ResetDefault();
        }

        private void SetBody(ShopkeeperBodyType body)
        {
            if (!ActivateBodyNode(body))
            {
                Debug.LogWarning(
                    $"{LogPrefix} SetBody={body} 未找到「{ShopkeeperFaceController.BodyTypeToGoName(body)}」。",
                    this);
            }
        }

        private void SetFace(ShopkeeperFaceType face)
        {
            if (!ActivateFaceNode(face))
            {
                Debug.LogWarning(
                    $"{LogPrefix} SetFace={face} 未找到「{ShopkeeperFaceController.FaceTypeToGoName(face)}」。",
                    this);
            }
        }

        private void CacheChildGroups()
        {
            _bodyRoot = ShopkeeperFaceController.FindDescendantByName(transform, ShopkeeperFaceController.BodyGroupName);
            _faceRoot = ShopkeeperFaceController.FindDescendantByName(transform, ShopkeeperFaceController.FaceGroupName);

            _bodyNodes.Clear();
            _faceNodes.Clear();

            RegisterBodyNode(ShopkeeperBodyType.Normal, "Normal");
            RegisterBodyNode(ShopkeeperBodyType.Blush, "Red");
            RegisterBodyNode(ShopkeeperBodyType.Sinister, "YinXian");

            RegisterFaceNode(ShopkeeperFaceType.Face1, "Face1");
            RegisterFaceNode(ShopkeeperFaceType.Face2, "Face2");
            RegisterFaceNode(ShopkeeperFaceType.Face3, "Face3");
            RegisterFaceNode(ShopkeeperFaceType.Face4, "Face4");
            RegisterFaceNode(ShopkeeperFaceType.Face5, "Face5");
        }

        private void RegisterBodyNode(ShopkeeperBodyType type, string childName)
        {
            if (_bodyRoot == null)
            {
                return;
            }

            var child = ShopkeeperFaceController.FindDirectChildByName(_bodyRoot, childName);
            if (child != null)
            {
                _bodyNodes[type] = child;
            }
        }

        private void RegisterFaceNode(ShopkeeperFaceType type, string childName)
        {
            if (_faceRoot == null)
            {
                return;
            }

            var child = ShopkeeperFaceController.FindDirectChildByName(_faceRoot, childName);
            if (child != null)
            {
                _faceNodes[type] = child;
            }
        }

        private bool ActivateBodyNode(ShopkeeperBodyType body)
        {
            var target = _bodyNodes.TryGetValue(body, out var node) ? node : null;
            if (target == null)
            {
                return false;
            }

            SetExclusiveActive(_bodyRoot, target);
            return true;
        }

        private bool ActivateFaceNode(ShopkeeperFaceType face)
        {
            var target = _faceNodes.TryGetValue(face, out var node) ? node : null;
            if (target == null)
            {
                return false;
            }

            SetExclusiveActive(_faceRoot, target);
            return true;
        }

        private static void SetExclusiveActive(Transform parent, Transform target)
        {
            if (parent == null || target == null)
            {
                return;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.SetActive(child == target);
            }
        }
    }
}
