using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic.Tips;
using Game.Static.Path;
using GameFramework.UnityRuntime.UI;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    /// <summary>
    /// 专门处理提示面板的组件。
    /// <para>
    /// 0923：同帧连发 <see cref="OpenTipsForm"/> 时，首屏 <c>OpenUIForm</c> 回调异步未到前
    /// <c>tipsFormLogic</c> 仍为 null，旧逻辑会再开第二个 TipsPanel → 横幅叠屏。
    /// 现网：opening 期间后续 key 入 <see cref="_pendingTips"/>，首屏回调后逐条
    /// <see cref="TipsFormLogic.AddTipsInfo"/>，复用面板内队列串行（方案 A）。
    /// </para>
    /// </summary>
    public class TipsComponentGSM : BaseComponentGSM
    {
        private UIComponentGM uiComponentGM;

        private TipsFormLogic tipsFormLogic;

        /// <summary>TipsPanel 正在 Open、callBack 尚未赋 Logic。</summary>
        private bool _openingTipsPanel;

        /// <summary>
        /// 首屏 Open 进行中时后续 Tips 暂存；callBack 后逐条 Add，禁止再 OpenUIForm。
        /// 元素：(info key, tipsType)。
        /// </summary>
        private readonly Queue<(string info, ETipsType type)> _pendingTips =
            new Queue<(string info, ETipsType type)>();

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            uiComponentGM = GameManager.GetGMComponent<UIComponentGM>();
            _openingTipsPanel = false;
            _pendingTips.Clear();
            tipsFormLogic = null;
        }

        public override void OnShutdown()
        {
            _openingTipsPanel = false;
            _pendingTips.Clear();
            tipsFormLogic = null;
            base.OnShutdown();
        }

        public void OpenTipsForm(string info, ETipsType tipsType = ETipsType.Item)
        {
            var proxy = GameManager.GetGMComponent<MVCComponentGM>().GetProxy<TipsFormProxy>();
            // 缺图静默（现网契约，勿改成 Tips/弹窗）
            if (proxy.GetTipsSprite(info) == null)
            {
                return;
            }

            // 面板已开且可用：直接入队串行（0901 理想路径）
            if (tipsFormLogic != null && tipsFormLogic.isActiveAndEnabled)
            {
                tipsFormLogic.AddTipsInfo(info, tipsType);
                return;
            }

            // 正在异步 Open：禁止再 OpenUIForm，后续 key 进待开队列
            if (_openingTipsPanel)
            {
                _pendingTips.Enqueue((info, tipsType));
                return;
            }

            // 冷启动：本条走 userData 首屏；其余等 callBack 再 Add（勿在回调里再 Add 首条）
            _openingTipsPanel = true;
            _pendingTips.Clear();

            var serialId = uiComponentGM.OpenUIForm(
                UIPrefabPath.GetUIPrefabPath("TipsPanel"),
                EUIGroup.Middle,
                new OpenFormArgs()
                {
                    userData = new TipsFormArgs()
                    {
                        info = info,
                        type = tipsType
                    },
                    callBack = OnTipsPanelOpened
                });

            // Open 同步失败：清状态，避免永久卡在 opening
            if (serialId < 0)
            {
                Debug.LogWarning("[TipsGSM] OpenUIForm(TipsPanel) 返回无效 SerialId，取消 opening。");
                ResetOpeningState();
            }
        }

        /// <summary>
        /// 首屏打开成功：赋 Logic，再把 opening 期间积压的 key 全部 AddTipsInfo。
        /// 首条已在 OnOpen(userData) 入队，此处只刷 pending，避免双播。
        /// </summary>
        private void OnTipsPanelOpened(UIFormLogic formLogic)
        {
            tipsFormLogic = formLogic as TipsFormLogic;
            _openingTipsPanel = false;

            if (tipsFormLogic == null)
            {
                Debug.LogWarning("[TipsGSM] TipsPanel callBack Logic 非 TipsFormLogic，丢弃 pending。");
                _pendingTips.Clear();
                return;
            }

            // 面板内 tipsQueue + ShowCharCoroutine 负责一条接一条
            while (_pendingTips.Count > 0)
            {
                var pending = _pendingTips.Dequeue();
                tipsFormLogic.AddTipsInfo(pending.info, pending.type);
            }
        }

        /// <summary>Open 失败或关场景时复位，防止 pending/_opening 脏留。</summary>
        private void ResetOpeningState()
        {
            _openingTipsPanel = false;
            _pendingTips.Clear();
        }

        public void OpenTipsFormDaysLater(int days)
        {
            OpenTipsForm($"DaysLater_{days}", ETipsType.Info);
        }

        public void OpenTipsArriveScene(string sceneName)
        {
            OpenTipsForm($"Arrive_{sceneName}", ETipsType.Info);
        }
    }
}
