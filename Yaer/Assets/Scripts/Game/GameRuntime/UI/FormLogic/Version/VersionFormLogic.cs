using Game.GameRuntime.UI.FormLogic.Base;
using TMPro;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Version
{
    /// <summary>
    /// 左下角版本号（0924：仅主界面）。文案读 <see cref="Application.version"/>（= ProjectSettings.bundleVersion）。
    /// <para>
    /// 挂 <c>EUIGroup.System</c>；由 <see cref="UIComponentGM.EnsureVersionForm"/> 在
    /// <c>OpenMainMenu</c> 幂等打开。进局 / 换场 <see cref="UIComponentGM.CloseAllUIForm"/> 会关掉，不再保活。
    /// </para>
    /// <para>替代（否决）：0922 常驻抗 CloseAll；塞 FightingPanel；改 bundleVersion 数值。</para>
    /// </summary>
    public class VersionFormLogic : BaseUIFormLogic
    {
        [SerializeField] private TextMeshProUGUI versionLabel;

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (versionLabel != null)
            {
                // 无 UI 规范要求 v 前缀：纯 Application.version（现网 1.0.5）
                versionLabel.text = Application.version;
                versionLabel.raycastTarget = false;
            }

            // 钉在 System 组底部：勿盖住同组 BlackPanel
            if (canvas != null && UIForm != null && UIForm.UIGroup != null)
            {
                canvas.sortingOrder = UIForm.UIGroup.Depth;
            }
        }

        /// <summary>水印级窗：禁止打开音效打扰主菜单。</summary>
        public override void PlayerOpenAudio()
        {
        }
    }
}
