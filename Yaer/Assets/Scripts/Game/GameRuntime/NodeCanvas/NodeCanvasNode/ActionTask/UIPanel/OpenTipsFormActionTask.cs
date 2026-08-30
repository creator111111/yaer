using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.FormLogic.Tips;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 对话树 Action：弹出「获得道具」花边横幅（TipsPanel），与卧室宝箱
    /// <c>TipsComponentGSM.OpenTipsForm("GetAiLinSword")</c> 同路。
    /// <para>
    /// 重要原因：现网 <see cref="AddTipsInfoActionTask"/> 强制 <see cref="ETipsType.Info"/>，
    /// 不会播「获得物品音效」；剑样板必须用默认 <see cref="ETipsType.Item"/>。
    /// 本 Task 默认 Item，并允许显式改类型，避免破坏 Info 用法。
    /// </para>
    /// <para>
    /// 使用约定（老农复用等）：
    /// 1) TipKey 必须等于图集 Sprite 名（如 GetAiLinSword），且三语 TipInfoAtlas 已含该图；
    /// 2) 入包与横幅是两步——道具请另挂 <c>GetItemActionTask</c>（或 C# AddMainItem），本节点只弹窗；
    /// 3) 缺图时 OpenTipsForm 静默不弹（Proxy 会打 Error「未找到Tips图片」）。
    /// </para>
    /// 替代方案：扩展 AddTipsInfoActionTask 增加 TipsType 参数（报告 A2）——效果等价，
    /// 但会改变旧节点语义面，故本期新建本类（报告 A1）。
    /// </summary>
    [Category("UIPanel")]
    [Name("打开Tips横幅(可Item)")]
    public class OpenTipsFormActionTask : ActionTask
    {
        /// <summary>
        /// 图集 Sprite 名 / png 文件名（无扩展名）。例：GetAiLinSword、GetHpBall。
        /// </summary>
        [Tooltip("TipKey = tipsInfo*.spriteatlas 内 Sprite 名；文案印在图上，非动态 TMP")]
        public BBParameter<string> TipKey;

        /// <summary>
        /// Item：花边横幅 +「获得物品音效」；Info/Boss：其它提示，不走该音效。
        /// 默认 Item，对齐艾琳之剑样板。
        /// </summary>
        [Tooltip("默认 Item（获得道具音效）；仅当产品要 Info/Boss 时再改")]
        public ETipsType TipsType = ETipsType.Item;

        protected override string info
        {
            get
            {
                var key = TipKey != null ? TipKey.ToString() : "(null)";
                return string.Format("<i>' OpenTipsForm: {0} ({1}) '</i>", key, TipsType);
            }
        }

        protected override void OnExecute()
        {
            // 空 Key 直接失败结束，避免对 Proxy 传 null 造成难查的静默
            var key = TipKey != null ? TipKey.value : null;
            if (string.IsNullOrEmpty(key))
            {
                UnityEngine.Debug.LogError("[OpenTipsFormActionTask] TipKey 为空，跳过 OpenTipsForm。");
                EndAction(false);
                return;
            }

            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr == null)
            {
                UnityEngine.Debug.LogError("[OpenTipsFormActionTask] GameSceneManager 为空。");
                EndAction(false);
                return;
            }

            // 与 HomeScene2Box.OnHomeScene2Box_GetSword 同入口；缺图时内部 return 不弹窗
            sceneMgr.GetModule<TipsComponentGSM>().OpenTipsForm(key, TipsType);
            EndAction(true);
        }
    }
}
