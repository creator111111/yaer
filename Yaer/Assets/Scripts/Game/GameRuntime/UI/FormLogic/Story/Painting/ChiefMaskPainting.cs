using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting
{
    /// <summary>
    /// 村长 Mask 局部表情（仅 Face1/2/3）。勿写入全局 <see cref="Game.Static.Enum.Dialogue.DialogueFaceType"/>。
    /// </summary>
    public enum ChiefFaceType
    {
        /// <summary>默认底图（组 2）。</summary>
        Face1 = 0,
        /// <summary>闭眼贴脸。</summary>
        Face2 = 1,
        /// <summary>笑颜贴脸。</summary>
        Face3 = 2
    }

    /// <summary>
    /// 对话框 Mask 内精灵村长小表情（UI 版：Face1 底图常亮 + Face2/Face3 互斥贴脸）。
    /// 挂于 <c>NormalDialogueNewPanel/YaerAvatarRoot/ChiefMaskPainting</c>；
    /// 由 <see cref="DialogueMaskAvatarPresenter"/> 驱动。
    /// </summary>
    /// <remarks>
    /// 不继承 <c>StoryFormPainting</c>；无 Start 自动 Reset，避免首句竞态盖脸。
    /// 叠法：Face1 常开；Face2/Face3 不同时开；二者都关 → 仍显示 Face1（禁止空白）。
    /// </remarks>
    public class ChiefMaskPainting : MonoBehaviour
    {
        public const string LogPrefix = "[ChiefMask]";

        private Transform _face1;
        private Transform _face2;
        private Transform _face3;
        private bool _cached;

        private void Awake()
        {
            CacheFaces();
        }

        /// <summary>Mask Presenter 入口：按叠法开关三脸。</summary>
        public void Apply(ChiefFaceType face)
        {
            CacheFaces();
            if (_face1 == null)
            {
                Debug.LogWarning($"{LogPrefix} 缺少 Face1，无法显示村长立绘。", this);
                return;
            }

            // Face1 底图始终开，禁止空白
            _face1.gameObject.SetActive(true);

            bool showFace2 = face == ChiefFaceType.Face2;
            bool showFace3 = face == ChiefFaceType.Face3;

            if (_face2 != null)
            {
                _face2.gameObject.SetActive(showFace2);
            }
            else if (showFace2)
            {
                Debug.LogWarning($"{LogPrefix} 请求 Face2 但节点缺失。", this);
            }

            if (_face3 != null)
            {
                _face3.gameObject.SetActive(showFace3);
            }
            else if (showFace3)
            {
                Debug.LogWarning($"{LogPrefix} 请求 Face3 但节点缺失。", this);
            }
        }

        /// <summary>Editor 校正默认 Active；运行时勿在 Start 自动调用。</summary>
        public void ResetDefault()
        {
            Apply(ChiefFaceType.Face1);
        }

        /// <summary>Editor Setup 后校正子节点 Active。</summary>
        public void EditorResetDefaultActiveState()
        {
            _cached = false;
            CacheFaces();
            ResetDefault();
        }

        private void CacheFaces()
        {
            if (_cached)
            {
                return;
            }

            _face1 = FindDirectChild(transform, "Face1");
            _face2 = FindDirectChild(transform, "Face2");
            _face3 = FindDirectChild(transform, "Face3");
            _cached = true;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
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
    }
}
