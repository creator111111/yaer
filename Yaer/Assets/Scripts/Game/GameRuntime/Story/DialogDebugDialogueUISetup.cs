using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using UnityEngine;
namespace Game.GameRuntime.Story
{
    /// <summary>
    /// DialogDebug 沙盒：在 GF Form 逻辑 Awake 之前移除 <see cref="NormalDialogueFormNewLogic"/>，
    /// 并将对话 Canvas 设为 ScreenSpaceOverlay（不依赖 UIComponentGM.UICamera）。
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class DialogDebugDialogueUISetup : MonoBehaviour
    {
        private void Awake()
        {
            StripGfFormFromScene();
            ConfigureDialogueCanvases();
        }

        private static void StripGfFormFromScene()
        {
            foreach (var form in FindObjectsOfType<NormalDialogueFormNewLogic>(true))
            {
                if (form != null)
                {
                    Destroy(form);
                }
            }

            foreach (var systemUi in FindObjectsOfType<ComponentSystemUI>(true))
            {
                if (systemUi != null)
                {
                    Destroy(systemUi);
                }
            }
        }

        private static void ConfigureDialogueCanvases()
        {
            foreach (var canvas in FindObjectsOfType<Canvas>(true))
            {
                if (canvas == null)
                {
                    continue;
                }

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 100);
            }
        }
    }
}
