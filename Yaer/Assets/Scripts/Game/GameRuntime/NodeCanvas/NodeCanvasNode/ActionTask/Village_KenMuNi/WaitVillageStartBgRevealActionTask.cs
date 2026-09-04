using System;
using Cysharp.Threading.Tasks;
using Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 村开场分层闸门：等到「黑幕已淡完、BG 可见」后再空等 Hold，再进大立绘。
    /// <para>
    /// 重要原因：立绘 Delay 若从树开跑计时，会叠在黑幕淡出上，亮屏后几乎无「只见 BG」空拍。
    /// DialogDebug / 未走进村旁路时闸门默认为已 Ready，只跑 Hold（仍有空拍）。
    /// </para>
    /// </summary>
    [Category("Village_KenMuNi")]
    [Name("等待村开场BG亮屏后空拍")]
    public class WaitVillageStartBgRevealActionTask : ActionTask
    {
        /// <summary>BG 完全可见后再空等的秒数（产品试调：0.5s）。</summary>
        public BBParameter<float> HoldAfterBgVisibleSeconds = 0.5f;

        protected override string info
        {
            get
            {
                return string.Format("<i>' 等 BG 亮屏后空拍 {0}s '</i>", HoldAfterBgVisibleSeconds);
            }
        }

        protected override void OnExecute()
        {
            Do().Forget();
        }

        async UniTaskVoid Do()
        {
            float hold = HoldAfterBgVisibleSeconds != null ? HoldAfterBgVisibleSeconds.value : 1f;

            // 最长兜底：避免闸门未 Signal 时永久卡住（壳失败等）
            float timeout = 8f;
            float elapsed = 0f;
            while (!VillageStartLayerRevealGate.IsBgFullyVisible)
            {
                await UniTask.Yield();
                elapsed += UnityEngine.Time.deltaTime;
                if (elapsed >= timeout)
                {
                    UnityEngine.Debug.LogWarning("[VillageStart] 等 BG 亮屏超时，强制继续分层前奏");
                    VillageStartLayerRevealGate.SignalBgFullyVisible();
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
