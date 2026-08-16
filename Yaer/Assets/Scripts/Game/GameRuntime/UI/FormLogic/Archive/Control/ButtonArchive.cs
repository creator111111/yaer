using Game.GameMgr;
using Game.Static.Enum.Map;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Archive.Control
{
    /// <summary>
    ///     实现存档按钮控件
    /// </summary>
    public class ButtonArchive : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string guid;

        [SerializeField] private Image imgBg; // 按钮背景
        [SerializeField] private Image imgFg; // 前景
        [SerializeField] private Image imgUsing; // 正在使用的存档
        [SerializeField] private Image HasDataImgPointerEnterMark;
        [SerializeField] private Image ImgPointerEnterMark;
        // [SerializeField] private TMP_Text txTitle;
        // [SerializeField] private TMP_Text txID;
        // [SerializeField] private TMP_Text txCreateDate;
        // [SerializeField] private TMP_Text txGameDuration;
        [SerializeField] private Text txTitle;
        [SerializeField] private Text txID;
        [SerializeField] private Text txCreateDate;
        [SerializeField] private Text txGameDuration;
        [SerializeField] private Button btnDelete;
        [SerializeField] private Transform playTimeNode;
        [SerializeField] private Transform createDateNode;

        [SerializeField] private Text txCreateTimeTitle;
        [SerializeField] private Text txPlayTimeTitle;

        [SerializeField] private Color NormalTextColor;
        [SerializeField] private Color PointerEnterTextColor;

        private int clickTimes; // 点击次数
        private bool isSelect; // 被选中标识
        public Action<ButtonArchive> onClickTwice; // 第二次点击处理的事件
        public Action<ButtonArchive> onClickOnce; // 单机处理的事件
        public Action<string> onClickDelete;

        public string Guid => guid;

        Dictionary<LanguageEnumType, string> textConfig_1 = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "存档" }, { LanguageEnumType.English, "Archive"}, { LanguageEnumType.Japanese, "セーブデータ"},
        };
        /// <summary>存档标题章节前缀：除肯姆尼外一律使用「序章」。</summary>
        Dictionary<LanguageEnumType, string> textConfig_2 = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "序章" }, { LanguageEnumType.English, "Prologue"}, { LanguageEnumType.Japanese, "序章"},
        };

        /// <summary>
        /// 仅当存档地点内部键为 <see cref="PlaceName.KenMuNi"/>（肯姆尼）时使用此前缀，与序章表 <see cref="textConfig_2"/> 互斥。
        /// 若未来还有其它「第一章」地点，可改为按表驱动（地点键 → 前缀字典）。
        /// </summary>
        Dictionary<LanguageEnumType, string> textConfig_2_Chapter1OnlyKenMuNi = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "第一章" }, { LanguageEnumType.English, "Chapter 1"}, { LanguageEnumType.Japanese, "第1章"},
        };
        Dictionary<LanguageEnumType, string> textConfig_3 = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "保存日期:" }, { LanguageEnumType.English, "Save Data:"}, { LanguageEnumType.Japanese, "ほぞんび"},
        };
        Dictionary<LanguageEnumType, string> textConfig_4 = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "游戏时长:" }, { LanguageEnumType.English, "Play Time"}, { LanguageEnumType.Japanese, "プレイ時間"},
        };

        public bool IsSelect
        {
            get => isSelect;
            set
            {
                if (value)
                {
                    clickTimes++;
                    // 选中切换为前景图片
                    imgFg.gameObject.SetActive(true);
                    imgBg.gameObject.SetActive(false);
                    imgUsing.gameObject.SetActive(false);
                }
                else
                {
                    clickTimes = 0;
                    imgFg.gameObject.SetActive(false);
                    imgBg.gameObject.SetActive(true);
                    imgUsing.gameObject.SetActive(false);
                }

                isSelect = value;
            }
        }

        public void OnInit()
        {
            clickTimes = 0;
            // 默认显示背景
            imgFg.gameObject.SetActive(false);
            imgUsing.gameObject.SetActive(false);
            imgBg.gameObject.SetActive(true);

            // 动态 TTF（如 Alibaba）的 Font Material 常带非 UI Shader → 背景裁得住、字漏出遮罩
            EnsureAllTextsUseMaskableFontMaterial();

            btnDelete.onClick.AddListener(() => onClickDelete?.Invoke(guid));
            btnDelete.gameObject.SetActive(false);
        }

        /// <summary>
        /// 将本行所有 <see cref="Text"/> 的字体材质切到 <c>UI/Default Font</c>，使其响应 Mask / RectMask2D。
        /// <para>
        /// 重要原因：Inspector 里 Material 显示为 Font Material、Maskable 已勾时仍漏字，是因为 Shader 不写 Stencil/不接 RectClip。
        /// 替代方案：整页改 TMP（改动面大）；或手搓独立 .mat——动态字体贴图会变，运行时改 Shader 更稳。
        /// </para>
        /// </summary>
        void EnsureAllTextsUseMaskableFontMaterial()
        {
            var texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                EnsureMaskableFontMaterial(texts[i]);
            }
        }

        /// <summary>按 Font 缓存一份可遮罩材质，避免每行 new Material 泄漏。</summary>
        static readonly Dictionary<int, Material> s_maskableFontMaterials = new Dictionary<int, Material>();

        static void EnsureMaskableFontMaterial(Text text)
        {
            if (text == null)
            {
                return;
            }

            text.maskable = true;

            var font = text.font;
            if (font == null || font.material == null)
            {
                return;
            }

            var uiFontShader = Shader.Find("UI/Default Font");
            if (uiFontShader == null)
            {
                Debug.LogWarning("[ButtonArchive] 找不到 Shader「UI/Default Font」，列表文字可能仍逃出遮罩");
                return;
            }

            // 已是可遮罩材质则只清掉错误覆写，走字体默认即可
            var assigned = text.material;
            if (assigned != null && assigned.shader == uiFontShader)
            {
                return;
            }

            if (assigned == null && font.material.shader == uiFontShader)
            {
                return;
            }

            int key = font.GetInstanceID();
            if (!s_maskableFontMaterials.TryGetValue(key, out var mat) || mat == null)
            {
                // 从字体材质拷贝贴图/属性，再强制 UI 遮罩 Shader
                mat = new Material(font.material);
                mat.shader = uiFontShader;
                mat.name = font.name + " (UI Maskable)";
                s_maskableFontMaterials[key] = mat;
            }

            text.material = mat;
        }


        /// <summary>
        ///     单击选中
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 右击无效
            if (eventData.button == PointerEventData.InputButton.Right) return;

            IsSelect = true;

            // 第二次点击
            if (clickTimes >= 2)
            {
                clickTimes = 0;
                // 触发第二次点击事件
                onClickTwice?.Invoke(this);
                return;
            }

            onClickOnce?.Invoke(this);
        }

        /// <summary>
        ///     更新按钮上的信息
        /// </summary>
        /// <param name="id">存档按钮序号</param>
        /// <param name="guid">存档唯一标识guid</param>
        /// <param name="createDate">创建日期</param>
        /// <param name="gameDuration">游玩时间</param>
        public void UpdateInfo(int id, string guid, string sceneName=default, DateTime createDate = default, float gameDuration = default)
        {
            var curLanagueType = GameManager.Instance.language;
            var baseText_1 = textConfig_1.ContainsKey(curLanagueType) ? textConfig_1[curLanagueType] : textConfig_1[LanguageEnumType.English];
            txID.text = $"{baseText_1}{id}";

            // 创建的是空按钮
            if (string.IsNullOrEmpty(guid))
            {
                // 隐藏地名和日期
                txTitle.gameObject.SetActive(false);
                txGameDuration.text = "";
                txCreateDate.text = "";
                createDateNode.gameObject.SetActive(false);
                playTimeNode.gameObject.SetActive(false);
            }
            else
            {
                this.guid = guid;

                // 显示地名和日期（章节前缀：仅肯姆尼用「第一章」，其余用「序章」）
                txTitle.gameObject.SetActive(true);
                var chapterPrefixTable = sceneName == PlaceName.KenMuNi ? textConfig_2_Chapter1OnlyKenMuNi : textConfig_2;
                var baseText_2 = chapterPrefixTable.ContainsKey(curLanagueType)
                    ? chapterPrefixTable[curLanagueType]
                    : chapterPrefixTable[LanguageEnumType.English];
                txTitle.text = $"{baseText_2}：{PlaceName.GetPlaceChsName(sceneName)}";
                txCreateDate.text = createDate.ToString("yyyy-M-d");
                TimeSpan playTimeSpan = new TimeSpan(0, 0, (int)gameDuration);
                txGameDuration.text = playTimeSpan.ToString(@"hh\:mm\:ss");

                btnDelete.gameObject.SetActive(true);

                createDateNode.gameObject.SetActive(true);
                playTimeNode.gameObject.SetActive(true);
                // 设置创建日期和游玩时间多语言文本
                var dateStr = textConfig_3.ContainsKey(curLanagueType) ? textConfig_3[curLanagueType] : textConfig_3[LanguageEnumType.English];
                var palyTimeStr = textConfig_3.ContainsKey(curLanagueType) ? textConfig_4[curLanagueType] : textConfig_4[LanguageEnumType.English];
                txCreateTimeTitle.text = dateStr;
                txPlayTimeTitle.text = palyTimeStr;
            }
        }

        public void SetUsing(bool value)
        {
            if (isSelect)
            {
                return;
            }

            imgUsing.gameObject.SetActive(value);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                HasDataImgPointerEnterMark.gameObject.SetActive(true);
            }

            ImgPointerEnterMark.gameObject.SetActive(true);

            txCreateDate.color = PointerEnterTextColor;
            txCreateTimeTitle.color = PointerEnterTextColor;
            txGameDuration.color = PointerEnterTextColor;
            txID.color = PointerEnterTextColor;
            txPlayTimeTitle.color = PointerEnterTextColor;
            txTitle.color = PointerEnterTextColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                HasDataImgPointerEnterMark.gameObject.SetActive(false);
            }

            ImgPointerEnterMark.gameObject.SetActive(false);

            txCreateDate.color = NormalTextColor;
            txCreateTimeTitle.color = NormalTextColor;
            txGameDuration.color = NormalTextColor;
            txID.color = NormalTextColor;
            txPlayTimeTitle.color = NormalTextColor;
            txTitle.color = NormalTextColor;
        }
    }
}