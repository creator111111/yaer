using System;
using Cysharp.Threading.Tasks;
using Game.GameRuntime.GameSceneManager.Scene.Village_Shop;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 商店首次进店分层闸门：等到「换场黑幕已淡完、店背景可见」后再空等 Hold，再进雅/古大立绘淡入。
    /// <para>
    /// 重要原因：立绘 Delay 若从树开跑计时，会叠在黑幕淡出上，亮屏后几乎无可见 0→1 过程（0827 P1）。
    /// </para>
    /// </summary>
    [Category("Village_Shop")]
    [Name("等待商店开场亮屏后空拍")]
    public class WaitShopStartBgRevealActionTask : ActionTask
    {
        /// <summary>店背景完全可见后再空等的秒数（产品试调：0.4s）。</summary>
        public BBParameter<float> HoldAfterBgVisibleSeconds = 0.4f;

        protected override string info
        {
            get => string.Format("<i>' 等店亮屏后空拍 {0}s '</i>", HoldAfterBgVisibleSeconds);
        }

        protected override void OnExecute()
        {
            Do().Forget();
        }

        async UniTaskVoid Do()
        {
            float hold = HoldAfterBgVisibleSeconds != null ? HoldAfterBgVisibleSeconds.value : 0.4f;

            // 最长兜底：避免闸门未 Signal 时永久卡住
            float timeout = 8f;
            float elapsed = 0f;
            while (!ShopStartLayerRevealGate.IsBgFullyVisible)
            {
                await UniTask.Yield();
                elapsed += UnityEngine.Time.deltaTime;
                if (elapsed >= timeout)
                {
                    UnityEngine.Debug.LogWarning("[ShopStart] 等店亮屏超时，强制继续前奏");
                    ShopStartLayerRevealGate.SignalBgFullyVisible();
                    break;
                }
            }

            if (hold > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(hold));
            }

            EndAction();
        }
    }
}
