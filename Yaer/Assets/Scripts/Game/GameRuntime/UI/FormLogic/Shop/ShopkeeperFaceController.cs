using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店老板娘专用切脸/切身控制器（Body/Face 子物体互斥 Toggle）。
    /// 挂于场景 <c>商店界面合层</c> 根；对 <c>Body/Normal|Red|YinXian</c> 与 <c>Face/Face1～5</c> 做 <c>SetActive</c>。
    /// </summary>
    /// <remarks>
    /// 不走雅儿 <c>StoryFormPainting</c> / <c>DialogueFaceType</c> 链。
    /// 替代方案：SR 单槽换图（v1）——用户 Hierarchy 改版后已 supersede。
    /// </remarks>
    public class ShopkeeperFaceController : MonoBehaviour
    {
        public const string LogPrefix = "[ShopkeeperFace]";

        /// <summary>身体变体父节点（与 Hierarchy 一致）。</summary>
        public const string BodyGroupName = "Body";

        /// <summary>脸部变体父节点。</summary>
        public const string FaceGroupName = "Face";

        [Header("调试")]
        [SerializeField] private bool logFaceChanges = true;

        private Transform _bodyRoot;
        private Transform _faceRoot;

        private readonly Dictionary<ShopkeeperBodyType, Transform> _bodyNodes =
            new Dictionary<ShopkeeperBodyType, Transform>();

        private readonly Dictionary<ShopkeeperFaceType, Transform> _faceNodes =
            new Dictionary<ShopkeeperFaceType, Transform>();

        private ShopkeeperFaceType _currentFace = ShopkeeperFaceType.Face1;
        private ShopkeeperBodyType _currentBody = ShopkeeperBodyType.Normal;

        /// <summary>当前脸 ID。</summary>
        public ShopkeeperFaceType CurrentFace => _currentFace;

        /// <summary>当前身 ID。</summary>
        public ShopkeeperBodyType CurrentBody => _currentBody;

        private void Awake()
        {
            CacheChildGroups();
            ShopkeeperFaceRegistry.Register(this);
        }

        private void OnDestroy()
        {
            ShopkeeperFaceRegistry.Unregister(this);
        }

        /// <summary>换身：Body 下互斥 Active。</summary>
        public void SetBody(ShopkeeperBodyType body)
        {
            if (!ActivateBodyNode(body))
            {
                Debug.LogWarning($"{LogPrefix} SetBody={body} 未找到子 GO「{BodyTypeToGoName(body)}」。", this);
                return;
            }

            _currentBody = body;

            if (logFaceChanges)
            {
                Debug.Log($"{LogPrefix} SetBody={body}", this);
            }
        }

        /// <summary>换脸：Face 下互斥 Active。</summary>
        public void SetFace(ShopkeeperFaceType face)
        {
            if (!ActivateFaceNode(face))
            {
                Debug.LogWarning($"{LogPrefix} SetFace={face} 未找到子 GO「{FaceTypeToGoName(face)}」。", this);
                return;
            }

            _currentFace = face;

            if (logFaceChanges)
            {
                Debug.Log($"{LogPrefix} SetFace={face}", this);
            }
        }

        /// <summary>同时应用身体与脸（对白桥接入口）。</summary>
        public void Apply(ShopkeeperBodyType body, ShopkeeperFaceType face)
        {
            SetBody(body);
            SetFace(face);
        }

        /// <summary>恢复默认 Normal + Face1。</summary>
        public void ResetDefault()
        {
            Apply(ShopkeeperBodyType.Normal, ShopkeeperFaceType.Face1);
        }

        /// <summary>Editor Setup 后校正默认 Active 状态。</summary>
        public void EditorResetDefaultActiveState()
        {
            CacheChildGroups();
            ResetDefault();
        }

        private void CacheChildGroups()
        {
            // Body/Face 可能在「 MerchantPainting」子树下；递归按名查找组根。
            _bodyRoot = FindDescendantByName(transform, BodyGroupName);
            _faceRoot = FindDescendantByName(transform, FaceGroupName);

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

        /// <summary>
        /// 在 parent 下按名找直接子节点（含 inactive；不用 Transform.Find，避免漏 inactive）。
        /// </summary>
        public static Transform FindDirectChildByName(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>自 root 向下递归按名查找（用于 Body/Face 组不在根直接子级时）。</summary>
        public static Transform FindDescendantByName(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var nested = FindDescendantByName(root.GetChild(i), targetName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private void RegisterBodyNode(ShopkeeperBodyType type, string childName)
        {
            if (_bodyRoot == null)
            {
                return;
            }

            var child = FindDirectChildByName(_bodyRoot, childName);
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

            var child = FindDirectChildByName(_faceRoot, childName);
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

        /// <summary>同一父节点下仅保留 target 为 Active。</summary>
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

        /// <summary>内部枚举 → Hierarchy GO 名（CSV 对外 Red/YinXian 与此一致）。</summary>
        public static string BodyTypeToGoName(ShopkeeperBodyType body)
        {
            switch (body)
            {
                case ShopkeeperBodyType.Normal:
                    return "Normal";
                case ShopkeeperBodyType.Blush:
                    return "Red";
                case ShopkeeperBodyType.Sinister:
                    return "YinXian";
                default:
                    return "Normal";
            }
        }

        public static string FaceTypeToGoName(ShopkeeperFaceType face)
        {
            return $"Face{(int)face + 1}";
        }
    }
}
