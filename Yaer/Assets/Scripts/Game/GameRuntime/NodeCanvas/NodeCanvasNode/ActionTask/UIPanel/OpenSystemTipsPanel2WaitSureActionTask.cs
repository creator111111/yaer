using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 对话树 Action：打开 SystemTipsPanel2（或同套 SystemTipsFormLogic 的界面），阻塞直到玩家点击确认或取消，再结束节点。
    /// 插入方式：在 NodeCanvas 对话图中添加 Action 节点，选用本任务，接在上一句 Statement 与下一句之间。
    /// </summary>
    [Category("UI")]
    [Name("SystemTipsPanel2-等待确认")]
    public class OpenSystemTipsPanel2WaitSureActionTask : ActionTask
    {
        [Tooltip("UIPrefabPath：Assets/GameRes/Prefabs/UI/{名称}.prefab 中的名称，不含路径与后缀")]
        public BBParameter<string> panelPrefabName;

        [Tooltip("传入 SystemTipsFormLogic.OnOpen 的提示类型，需与图集/界面逻辑一致")]
        public ESystemTipsType tipsType = ESystemTipsType.Save;

        private bool waitFinished;
        private SystemTipsFormLogic boundForm;

        protected override string info
        {
            get
            {
                var n = panelPrefabName != null && !string.IsNullOrEmpty(panelPrefabName.value)
                    ? panelPrefabName.value
                    : "SystemTipsPanel2";
                return $"<i>等待确认: {n}</i>";
            }
        }

        protected override void OnExecute()
        {
            waitFinished = false;
            boundForm = null;

            var name = panelPrefabName == null || string.IsNullOrEmpty(panelPrefabName.value)
                ? "SystemTipsPanel2"
                : panelPrefabName.value;
            var path = UIPrefabPath.GetUIPrefabPath(name);
            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            var existing = uiGm.GetUIForm(path);

            if (existing != null && existing.Logic is SystemTipsFormLogic existingLogic)
            {
                boundForm = existingLogic;
                SubscribeAndWait();
                return;
            }

            uiGm.OpenUIForm(path, EUIGroup.Top, new OpenFormArgs
            {
                userData = tipsType,
                callBack = logic =>
                {
                    if (waitFinished)
                    {
                        return;
                    }

                    if (logic is SystemTipsFormLogic form)
                    {
                        boundForm = form;
                        SubscribeAndWait();
                    }
                    else
                    {
                        Debug.LogError("[OpenSystemTipsPanel2WaitSureActionTask] 打开的界面不是 SystemTipsFormLogic: " +
                                       (logic != null ? logic.GetType().Name : "null"));
                        EndAction(false);
                    }
                }
            });
        }

        private void SubscribeAndWait()
        {
            if (boundForm == null || boundForm.proxy == null)
            {
                EndAction(false);
                return;
            }

            boundForm.proxy.onSureEvent += OnPanelSureOrCancel;
            boundForm.proxy.onCancelEvent += OnPanelSureOrCancel;
        }

        private void OnPanelSureOrCancel()
        {
            if (waitFinished)
            {
                return;
            }

            waitFinished = true;
            UnsubscribeProxy();
            EndAction(true);
        }

        private void UnsubscribeProxy()
        {
            if (boundForm != null && boundForm.proxy != null)
            {
                boundForm.proxy.onSureEvent -= OnPanelSureOrCancel;
                boundForm.proxy.onCancelEvent -= OnPanelSureOrCancel;
            }

            boundForm = null;
        }

        protected override void OnStop(bool interrupted)
        {
            if (!waitFinished)
            {
                waitFinished = true;
                UnsubscribeProxy();
            }

            base.OnStop(interrupted);
        }
    }
}
