using Cysharp.Threading.Tasks;
using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Path;
using GameFramework.UnityRuntime.UI;
using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// 在指定剧情的指定句弹出 SystemTipsPanel2，阻塞对话树 <c>Continue</c>，
    /// 直到玩家在提示面板上点击确认或取消（通过 <see cref="SystemTipsFormProxy"/> 事件解除阻塞）。
    /// </summary>
    public static class DialoguePreBossSaveTipGate
    {
        private static int s_subtitleLineInSession;
        private static UniTaskCompletionSource s_waitSure;

        public static void ResetSubtitleLineCounter()
        {
            s_subtitleLineInSession = 0;
        }

        /// <summary>
        /// 一句字幕展示完毕（含打字/配音）后、等待玩家推进前调用；若触发提示则返回 true，且内部已处理后续一次推进等待。
        /// </summary>
        public static async UniTask<bool> WaitIfNeededBeforeContinueAsync(string lineText)
        {
            var settings = DialoguePreBossSaveTipSettings.Instance;
            if (settings == null)
            {
                return false;
            }

            var gsm = GameManager.GetGameSceneManager()?.GetModule<StoryComponentGSM>();
            var storyName = gsm != null ? gsm.CurrentRunningStoryName : null;
            if (string.IsNullOrEmpty(storyName) || storyName != settings.targetStoryName)
            {
                return false;
            }

            if (settings.pauseAtSubtitleLineIndex <= 0)
            {
                return false;
            }

            s_subtitleLineInSession++;
            if (s_subtitleLineInSession != settings.pauseAtSubtitleLineIndex)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(settings.alsoRequireLineContains) &&
                (lineText == null || !lineText.Contains(settings.alsoRequireLineContains)))
            {
                Debug.LogWarning(
                    $"[DialoguePreBossSaveTipGate] 句号已到但文本不包含子串，跳过提示。text={lineText}",
                    settings);
                return false;
            }

            s_waitSure = new UniTaskCompletionSource();
            var path = UIPrefabPath.GetUIPrefabPath(settings.tipsPanelPrefabName);
            // SystemTipsFormLogic.OnOpen 需要 ESystemTipsType；Save 与「请注意保存」类文案一致。
            // 在 UI 打开回调里订阅 proxy.onSureEvent，保证先于 CloseForm 触发 Continue（与 btnSure 点击顺序一致）。
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(path, EUIGroup.Top, new OpenFormArgs
            {
                userData = ESystemTipsType.Save,
                callBack = logic =>
                {
                    if (logic is SystemTipsFormLogic form)
                    {
                        form.proxy.onSureEvent += UnblockAfterTipsPanel;
                        // 避免点「再想想」关闭面板后对话永远卡在 await
                        form.proxy.onCancelEvent += UnblockAfterTipsPanel;
                    }
                }
            });

            await s_waitSure.Task;
            s_waitSure = null;
            return true;
        }

        private static void UnblockAfterTipsPanel()
        {
            s_waitSure?.TrySetResult();
        }
    }
}
