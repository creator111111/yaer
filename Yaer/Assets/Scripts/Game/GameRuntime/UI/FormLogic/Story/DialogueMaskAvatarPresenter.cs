using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
using Game.Static.Enum.Dialogue;
using Game.Static.Name.Clothes;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Story
{
    /// <summary>
    /// 字幕条 Mask 立绘真源驱动：挂在 NormalDialogueNewPanel/Bottom/Mask/YaerAvatarRoot。
    /// 订阅 <see cref="DialogueTMPUGUI.OnGetNewStatement"/>，按说话角色互斥显隐已嵌套的 Painting，
    /// 并调用各 Painting 的 <see cref="StoryFormPainting.UpdateFace"/> 切脸。
    /// 不订场景 Actor 事件（UI 壳下 Painting 父级无 DialogueActorEx，见 0803 Mask 接线报告）。
    /// 雅儿服装：默认跟存档 <see cref="PlayerClothesData"/> Clothes（Dress↔GoOut）；
    /// <see cref="yaerUseGoOutOnly"/> 仅作调试强制 GoOut（0806 Dress 启用 / 0803 Q2 第二小步）。
    /// </summary>
    public class DialogueMaskAvatarPresenter : MonoBehaviour
    {
        [Header("可选手动拖引用；留空则按子物体名自动绑定")]
        [SerializeField] private DialogueTMPUGUI dialogueTmp;
        [SerializeField] private StoryFormPainting goOutYaerPainting;
        [SerializeField] private StoryFormPainting dressYaerPainting;
        [SerializeField] private StoryFormPainting gushaPainting;
        [SerializeField] private StoryFormPainting amyPainting;
        [SerializeField] private StoryFormPainting aliyPainting;
        [SerializeField] private MerchantMaskPainting merchantMaskPainting;
        [SerializeField] private ChiefMaskPainting chiefMaskPainting;

        /// <summary>
        /// 店句 Mask 已由 <see cref="ApplyShopkeeperPortrait"/> 点亮时置 true。
        /// 同帧 <see cref="OnGetNewStatement"/> 会传 None 写历史，须跳过 HideAll 避免 Merchant 黑块（0827 F3）。
        /// </summary>
        private bool shopkeeperMaskActive;

        /// <summary>
        /// 村长门口句：TMP 在 <see cref="ApplyChiefPortrait"/> 后 Invoke(None)；须跳过 HideAll（对齐店旗）。
        /// </summary>
        private bool chiefMaskActive;

        /// <summary>
        /// 调试强制雅儿走 GoOut（忽略存档 Clothes）。默认 false：按存档 Dress↔GoOut 切换。
        /// 替代方案：删除本字段，仅保留存档分支（少调试手段）；或方案 C 镜像场景大立绘类型。
        /// </summary>
        [SerializeField] private bool yaerUseGoOutOnly = false;

        private void Awake()
        {
            BindReferencesIfNeeded();
            // 接线前 Prefab 可能默认亮古莎；启动时先全关，等首句 OnGetNewStatement 再驱动
            HideAllPaintings();
        }

        private void OnEnable()
        {
            BindReferencesIfNeeded();
            if (dialogueTmp != null)
            {
                dialogueTmp.OnGetNewStatement += OnGetNewStatement;
            }
        }

        private void OnDisable()
        {
            if (dialogueTmp != null)
            {
                dialogueTmp.OnGetNewStatement -= OnGetNewStatement;
            }
        }

        /// <summary>
        /// 与历史页同源事件：role + faceType + 文本。旁白会传 None。
        /// </summary>
        private void OnGetNewStatement(DialogueRoleName role, DialogueFaceType faceType, string text)
        {
            Apply(role, faceType);
        }

        /// <summary>
        /// 对外入口：全关 → 开当前角色 Painting → UpdateFace。
        /// 未支持角色（King/Lai…）保持 Mask 空，不残留上一句立绘。
        /// </summary>
        public void Apply(DialogueRoleName role, DialogueFaceType faceType)
        {
            if (role == DialogueRoleName.None)
            {
                // 店句 / 村长门口句：TMP 在专用 Apply 后会 Invoke(None) 写历史；勿关掉刚亮的 Mask。
                if (shopkeeperMaskActive || chiefMaskActive)
                {
                    return;
                }

                HideAllPaintings();
                return;
            }

            shopkeeperMaskActive = false;
            chiefMaskActive = false;
            HideAllPaintings();

            // 村长晚宴：RoleName.Chief + Smile/CloseEyes → MapToChiefFace（F2）
            if (role == DialogueRoleName.Chief)
            {
                ApplyChiefPortrait(MapToChiefFace(faceType));
                return;
            }

            var painting = ResolvePainting(role);
            if (painting == null)
            {
                return;
            }

            painting.gameObject.SetActive(true);
            // 加厚：若曾被旁路误伤 CanvasGroup.alpha=0，仅 SetActive 仍黑窗；Activate 时拉回不透明
            var cg = painting.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha < 1f)
            {
                cg.alpha = 1f;
            }

            // GoOut：Start 若已跑过不会再 SetDefaultPainting；每次启用时补一次头饰同步（0806 验收点）
            if (painting is GoOutStoryYaerPainting goOutPainting)
            {
                goOutPainting.SyncHeadwearFromArchive();
            }

            var faceKey = ResolveFaceKey(role, faceType);
            // 验收用：确认雅儿走的是哪套 Painting + Face 键（室内 Dress / 村线 GoOut）
            if (role == DialogueRoleName.Yaer)
            {
                var suit = IsYaerUsingGoOut() ? "GoOut" : "Dress";
                Debug.Log($"[MaskAvatar] Yaer → {suit} face={faceKey}");
            }

            painting.UpdateFace(faceKey);
        }

        /// <summary>
        /// 店句专用：亮 Merchant Mask 小表情，与场景合层读同一 ShopBody/ShopFace。
        /// 不扩 <see cref="DialogueRoleName"/>；不走 <see cref="StoryFormPainting.UpdateFace"/>。
        /// </summary>
        public void ApplyShopkeeperPortrait(ShopkeeperBodyType body, ShopkeeperFaceType face)
        {
            shopkeeperMaskActive = true;
            HideAllPaintings();

            if (merchantMaskPainting == null)
            {
                shopkeeperMaskActive = false;
                Debug.LogWarning("[MaskAvatar] 店句但 MerchantMaskPainting 未绑定。", this);
                return;
            }

            merchantMaskPainting.gameObject.SetActive(true);
            var cg = merchantMaskPainting.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha < 1f)
            {
                cg.alpha = 1f;
            }

            merchantMaskPainting.Apply(body, face);
        }

        /// <summary>
        /// 村长句：亮 Chief Mask。门口直通 Face1～3 或晚宴 Map 后调用。
        /// 会置 <see cref="chiefMaskActive"/>，配合 TMP Invoke(None) 防黑块。
        /// </summary>
        public void ApplyChiefPortrait(ChiefFaceType face)
        {
            chiefMaskActive = true;
            HideAllPaintings();

            if (chiefMaskPainting == null)
            {
                chiefMaskActive = false;
                Debug.LogWarning("[MaskAvatar] 村长句但 ChiefMaskPainting 未绑定。", this);
                return;
            }

            chiefMaskPainting.gameObject.SetActive(true);
            var cg = chiefMaskPainting.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha < 1f)
            {
                cg.alpha = 1f;
            }

            chiefMaskPainting.Apply(face);
        }

        /// <summary>
        /// F2：晚宴 CSV 旧 FaceType → ChiefFace（施工默认草稿；Sad/Laugh 暂 Face1）。
        /// </summary>
        public static ChiefFaceType MapToChiefFace(DialogueFaceType faceType)
        {
            switch (faceType)
            {
                case DialogueFaceType.CloseEyes:
                    return ChiefFaceType.Face2;
                case DialogueFaceType.Smile:
                    return ChiefFaceType.Face3;
                case DialogueFaceType.Normal:
                case DialogueFaceType.None:
                default:
                    // Sad / Laugh / 其它：占位 Face1（OPEN Q4）
                    return ChiefFaceType.Face1;
            }
        }

        private void HideAllPaintings()
        {
            SetPaintingActive(goOutYaerPainting, false);
            SetPaintingActive(dressYaerPainting, false);
            SetPaintingActive(gushaPainting, false);
            SetPaintingActive(amyPainting, false);
            SetPaintingActive(aliyPainting, false);
            if (merchantMaskPainting != null)
            {
                merchantMaskPainting.gameObject.SetActive(false);
            }

            if (chiefMaskPainting != null)
            {
                chiefMaskPainting.gameObject.SetActive(false);
            }
        }

        private static void SetPaintingActive(StoryFormPainting painting, bool active)
        {
            if (painting != null)
            {
                painting.gameObject.SetActive(active);
            }
        }

        private StoryFormPainting ResolvePainting(DialogueRoleName role)
        {
            switch (role)
            {
                case DialogueRoleName.Yaer:
                    // 调试强制 / 非 Dress 存档 → GoOut；Clothes==Dress → dress（缺引用时回退 GoOut）
                    if (IsYaerUsingGoOut())
                    {
                        return goOutYaerPainting;
                    }
                    return dressYaerPainting != null ? dressYaerPainting : goOutYaerPainting;
                case DialogueRoleName.Gusha:
                    return gushaPainting;
                case DialogueRoleName.Amy:
                    return amyPainting;
                case DialogueRoleName.Aliy:
                    return aliyPainting;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Face 键必须与各 Prefab Faces 子物体 name 一致。
        /// Yaer GoOut：Armor_NoHeadWear_{face}；Dress：Presenter 直接拼 Dress_Crown_*（绕过基类裸枚举订阅）。
        /// Gusha/Amy/Aliy：裸枚举名。
        /// </summary>
        private string ResolveFaceKey(DialogueRoleName role, DialogueFaceType faceType)
        {
            if (role != DialogueRoleName.Yaer)
            {
                return faceType.ToString();
            }

            if (IsYaerUsingGoOut())
            {
                return GoOutStoryYaerPainting.ResolveGoOutFaceKey(faceType);
            }

            // Dress 路径：与 Prefab Faces 子物体名对齐；Normal 无独立键 → Smile
            if (faceType == DialogueFaceType.Normal)
            {
                return "Dress_Crown_Smile";
            }
            return $"Dress_Crown_{faceType}";
        }

        /// <summary>
        /// 雅儿是否走 GoOut 线：调试开关优先；否则读存档 Clothes（非 Dress → GoOut）。
        /// 无 PlayerData（DialogDebug 沙盒）时与 <see cref="DialogueAvatarLoader"/> 一致默认 Dress。
        /// </summary>
        private bool IsYaerUsingGoOut()
        {
            if (yaerUseGoOutOnly)
            {
                return true;
            }
            return !IsYaerDressFromArchive();
        }

        /// <summary>
        /// 存档 Clothes 是否为室内 Dress。无存档组件时默认 true（沙盒跟 Loader 默认装）。
        /// </summary>
        private static bool IsYaerDressFromArchive()
        {
            var playerData = GameManager.GetGMComponent<PlayerDataComponentGM>();
            if (playerData == null)
            {
                // 沙盒无存档：与 DialogueAvatarLoader.SandboxDefaultClothes=Dress 对齐，便于室内 DialogDebug
                return true;
            }

            var clothesData = playerData.GetClothesData();
            if (clothesData == null)
            {
                return true;
            }

            var clothes = clothesData.GetClothesName(BoneName.Clothes);
            return clothes == ClothesName.Clothes.Dress;
        }

        private void BindReferencesIfNeeded()
        {
            if (dialogueTmp == null)
            {
                dialogueTmp = GetComponentInParent<DialogueTMPUGUI>();
            }

            // 子物体名与 Prefab 实例 m_Name override 对齐（见 NormalDialogueNewPanel）
            if (goOutYaerPainting == null)
            {
                goOutYaerPainting = FindChildPainting("GoOutStoryYaerPainting");
            }
            if (dressYaerPainting == null)
            {
                dressYaerPainting = FindChildPainting("YaerPainting");
            }
            if (gushaPainting == null)
            {
                gushaPainting = FindChildPainting("GushaPainting");
            }
            if (amyPainting == null)
            {
                amyPainting = FindChildPainting("AmyPainting");
            }
            if (aliyPainting == null)
            {
                aliyPainting = FindChildPainting("AliyPainting");
            }

            if (merchantMaskPainting == null)
            {
                var merchantTr = transform.Find("MerchantMaskPainting");
                if (merchantTr != null)
                {
                    merchantMaskPainting = merchantTr.GetComponent<MerchantMaskPainting>();
                }
            }

            if (chiefMaskPainting == null)
            {
                var chiefTr = transform.Find("ChiefMaskPainting");
                if (chiefTr != null)
                {
                    chiefMaskPainting = chiefTr.GetComponent<ChiefMaskPainting>();
                }
            }
        }

        private StoryFormPainting FindChildPainting(string childName)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                return null;
            }
            return child.GetComponent<StoryFormPainting>();
        }
    }
}
