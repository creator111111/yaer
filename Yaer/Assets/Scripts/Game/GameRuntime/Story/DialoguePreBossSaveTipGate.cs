using System;
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
    /// 备用的 Boss 战前保存提示门控：若要在剧情中间弹出 SystemTips 并阻塞，请改为用 NodeCanvas
    /// 图或 ActionTask 驱动（本类不再从 <c>DialogueTMPUGUI</c> 自动插入，以免破坏「仅由节点图推进」的节奏）。
    /// 需要时可从自定义任务中调用 <see cref="WaitIfNeededBeforeContinueAsync"/>，并在完成提示后由图继续连线。
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
        /// 对话强制结束时调用：若仍有未完成的 s_waitSure 等待，可解除阻塞（例如切场景、停对话）。
        /// </summary>
        public static void CancelPendingAndUnblock()
        {
            if (s_waitSure == null) { return; }
            s_waitSure.TrySetResult();
            s_waitSure = null;
        }

        /// <summary>
        /// 在指定剧情的指定句需弹出提示时，由**剧情侧任务**在合适节点调用，而非字幕 UI 自动插入。
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
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(path, EUIGroup.Top, new OpenFormArgs
            {
                userData = ESystemTipsType.Save,
                callBack = logic =>
                {
                    if (logic is SystemTipsFormLogic form)
                    {
                        form.proxy.onSureEvent += UnblockAfterTipsPanel;
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
