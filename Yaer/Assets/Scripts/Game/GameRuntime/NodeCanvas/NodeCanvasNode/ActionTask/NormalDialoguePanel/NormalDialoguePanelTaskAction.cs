using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Path;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// ?????? NormalDialogueNewPanel Form?DialogDebug ??? UIComponentGM ??????? DialogueTMPUGUI?
    /// </summary>
    public abstract class NormalDialoguePanelTaskAction : ActionTask
    {
        protected NormalDialogueFormNewLogic FormLogic { get; private set; }

        /// <summary>DialogDebug ??? GF OpenUIForm ?? true?</summary>
        protected bool SandboxMode { get; private set; }

        protected DialogueTMPUGUI SandboxDialogueUI { get; private set; }

        protected override string OnInit()
        {
            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm == null)
            {
                InitSandboxFallback();
                return base.OnInit();
            }

            string panelPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            var uiForm = uiGm.GetUIForm(panelPath);
            if (uiForm == null)
            {
                Debug.LogWarning("[NormalDialoguePanelTaskAction] NormalDialogueNewPanel ???????? UI ???");
                InitSandboxFallback();
            }
            else
            {
                FormLogic = uiForm.Logic as NormalDialogueFormNewLogic;
                OnGetUILogic(FormLogic);
            }

            return base.OnInit();
        }

        private void InitSandboxFallback()
        {
            SandboxMode = true;
            SandboxDialogueUI = Object.FindObjectOfType<DialogueTMPUGUI>();
            OnGetUILogic(null);
        }

        protected virtual void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
        }

        protected UnityEngine.CanvasGroup GetDialogueUICanvasGroup()
        {
            if (FormLogic != null)
            {
                return FormLogic.dialogueUICanvasGroup;
            }

            return SandboxDialogueUI != null ? SandboxDialogueUI.subtitlesCanvasGroup : null;
        }

        /// <summary>???? Main Camera ???????? CameraComponentGSM??</summary>
        protected void SetDialogueOptionsGroupWorldPosition(Vector3 worldPos)
        {
            if (FormLogic != null)
            {
                FormLogic.SetDialogueOptionsGroupPosition(worldPos);
                return;
            }

            if (SandboxDialogueUI == null || SandboxDialogueUI.DialogueOptionsGroup == null)
            {
                return;
            }

            var canvas = SandboxDialogueUI.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            var screenPos = cam.WorldToScreenPoint(worldPos);
            var canvasRect = canvas.transform as RectTransform;
            var eventCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPos, eventCam, out var localPos))
            {
                SandboxDialogueUI.DialogueOptionsGroup.anchoredPosition = localPos;
            }
        }

        protected UnityEngine.CanvasGroup FindBlackFadeCanvasGroup()
        {
            if (FormLogic != null)
            {
                return FormLogic.BlackFadeCanvasGroup;
            }

            var blackMask = GameObject.Find("BlackMask");
            return blackMask != null ? blackMask.GetComponent<UnityEngine.CanvasGroup>() : null;
        }

        protected void SetFullscreenRaycastMask(bool block)
        {
            if (FormLogic != null)
            {
                FormLogic.BlockOtherInteraction(block);
                return;
            }

            if (SandboxDialogueUI == null)
            {
                return;
            }

            var mask = SandboxDialogueUI.GetComponent<Image>();
            if (mask != null)
            {
                mask.raycastTarget = block;
            }
        }
    }
}
