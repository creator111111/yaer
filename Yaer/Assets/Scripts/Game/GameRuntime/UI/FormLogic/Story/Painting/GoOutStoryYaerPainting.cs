using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using Game.Static.Enum.Dialogue;
using Game.Static.Name.Clothes;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting
{
    public class GoOutStoryYaerPainting : StoryFormPainting
    {
        public GameObject armorNone;
        public GameObject armorHead;
        public GameObject armorCrown;

        /// <summary>
        /// 场景大立绘：有 DialogueActorEx，开场默认 Smile，再听 Actor 事件换脸。
        /// Mask 壳嵌套实例：无 Actor；Presenter 已在 SetActive 后 UpdateFace(台本脸)。
        /// 若此处仍强制 Smile，会在同帧 Start 里盖掉首句 FaceType（见 0804 首句未跟脸报告）。
        /// 替代方案：Presenter 延后到 EndOfFrame 再 Apply（可能闪 Smile）；本方案在根因处跳过。
        /// </summary>
        protected override void SetDefaultPainting()
        {
            // 头饰跟存档；表情由 RefreshAvatar → OnRefreshAvatarEvent 驱动，禁止 Start 强制 Smile 盖首句（0827 R2-雅）。
            SyncHeadwearFromArchive();
        }

        /// <summary>
        /// 按存档 Headwear 显隐盔/冠。供 Start 与 Mask Presenter 每次启用时调用：
        /// Start 只跑一次，面板复用再切回 GoOut 时需补同步（0806 Dress 启用验收点）。
        /// 替代方案：Presenter 不调、依赖首次 Start（面板池化/二次启用时头饰可能过期）。
        /// </summary>
        public void SyncHeadwearFromArchive()
        {
            var gsm = GameManager.GetGameSceneManager();
            if (gsm == null)
            {
                // DialogDebug 等沙盒：无存档时关头饰
                SetArmorHeadwearActive(false, false);
                return;
            }

            var playerClothesData = gsm.GetArchiveData<PlayerClothesData>();
            if (playerClothesData == null)
            {
                SetArmorHeadwearActive(false, false);
                return;
            }

            var headWear = playerClothesData.GetClothesName(BoneName.Headwear);
            var hasCrown = headWear == ClothesName.HeadWear.Crown;
            var hasArmorHead = headWear == ClothesName.HeadWear.ArmorHead;
            SetArmorHeadwearActive(hasArmorHead, hasCrown);
        }

        /// <summary>
        /// 与基类 Start 订阅 Actor 的查找范围保持一致，避免 Mask/场景判定漂移。
        /// </summary>
        private DialogueActorEx FindDialogueActorEx()
        {
            var dialogueActor = GetComponent<DialogueActorEx>();
            if (dialogueActor == null && transform.parent != null)
            {
                dialogueActor = transform.parent.GetComponent<DialogueActorEx>();
            }
            return dialogueActor;
        }

        private void SetArmorHeadwearActive(bool hasArmorHead, bool hasCrown)
        {
            if (armorHead != null)
            {
                armorHead.SetActive(hasArmorHead);
            }
            if (armorCrown != null)
            {
                armorCrown.SetActive(hasCrown);
            }
        }

        protected override void RegisterRefreshAvatarEvent(DialogueActorEx dialogueActor)
        {
            dialogueActor.OnRefreshAvatarEvent += (roleName, faceType, sprite) =>
            {
                UpdateFace(ResolveGoOutFaceKey(faceType));
            };
        }

        /// <summary>
        /// GoOut 立绘集文件名形如 Armor_NoHeadWear_Smile；CSV/图里常用 Normal，但集内无 Normal 键，回退 Smile 避免说话时全隐藏。
        /// 公开供 Mask Presenter 等 UI 壳复用同一键规则，避免复制字符串。
        /// </summary>
        public static string ResolveGoOutFaceKey(DialogueFaceType faceType)
        {
            if (faceType == DialogueFaceType.Normal)
            {
                return "Armor_NoHeadWear_Smile";
            }

            return $"Armor_NoHeadWear_{faceType}";
        }
    }
}
