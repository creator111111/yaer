using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Base.Control
{
    public class HistoryDialogueBox : MonoBehaviour
    {
        [SerializeField] private Image imgAvatar;
        [SerializeField] private Text txContent;

        public void UpdateDialogue(HistoryDialogueInfo info)
        {
            DialogueAvatarLoader.GetAvatar(info.roleName, info.faceType, RefreshAvatar);
            txContent.text = info.content;
        }

        private void RefreshAvatar(Sprite avatarSprite)
        {
            if (avatarSprite == null)
            {
                imgAvatar.gameObject.SetActive(false);
            }
            else
            {
                imgAvatar.gameObject.SetActive(true);
                imgAvatar.sprite = avatarSprite;
            }
        }
    }
}