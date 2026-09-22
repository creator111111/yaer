using Game.GameRuntime.UI.FormLogic.Base;
using TMPro;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Version
{
    /// <summary>
    /// 左下角常驻版本号（0922）。文案读 <see cref="Application.version"/>（= ProjectSettings.bundleVersion）。
    /// <para>
    /// 挂 <c>EUIGroup.System</c>；<see cref="UIComponentGM.CloseAllUIForm"/> 永久过滤本 Form，
    /// 换场/读档后由 <see cref="UIComponentGM.EnsureVersionForm"/> 幂等重开。
    /// </para>
    /// <para>替代（否决）：只挂 StartScene——进局消失；塞 FightingPanel——关 HUD/主菜单无面板。</para>
    /// </summary>
    public class VersionFormLogic : BaseUIFormLogic
    {
        [SerializeField] private TextMeshProUGUI versionLabel;

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (versionLabel != null)
            {
                // 无 UI 规范要求 v 前缀：纯 Application.version（施工后为 1.0.5）
                versionLabel.text = Application.version;
                versionLabel.raycastTarget = false;
            }

            // 钉在 System 组底部：勿盖住同组 BlackPanel（换场黑幕期间版本被盖住可接受）
            if (canvas != null && UIForm != null && UIForm.UIGroup != null)
            {
                canvas.sortingOrder = UIForm.UIGroup.Depth;
            }
        }

        /// <summary>水印级常驻窗：禁止打开音效打扰主菜单。</summary>
        public override void PlayerOpenAudio()
        {
        }
    }
}
