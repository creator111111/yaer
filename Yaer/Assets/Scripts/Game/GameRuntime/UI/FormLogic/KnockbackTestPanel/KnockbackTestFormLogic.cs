using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Physics;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.KnockbackTestPanel
{
    /// <summary>
    /// 受击 / 击退调试面板：支持三种模式（与 PlayerLogic.OnApplyStatusEffects 分支一致）。
    /// - FullNormal：地面普通受击 + breakHight&gt;0 时走 KnockBackComponent。
    /// - FullBreak：地面 Break 击飞，走 DamageFly，不走 ApplyKnockBack。
    /// - PureKnockback：仅调用 KnockBackComponent，不调 BattleComponent.TakeDamage。
    /// </summary>
    public class KnockbackTestFormLogic : BaseUIFormLogic
    {
        /// <summary>预制体中 cmdItem 实际挂在 root 下（与 ItemArea 同级），与 Scroll View 内 Content 无关。</summary>
        private const string CmdItemPathPrimary = "root/cmdItem";

        private const string CmdItemPathFallback = "root/Scroll View/Viewport/Content/cmdItem";

        private InputField[] _paramInputs;
        private Text _modeHintText;
        private KnockbackTestRunner.TestMode _mode = KnockbackTestRunner.TestMode.FullNormal;
        private Transform _cmdItemTemplate;

        /// <summary>参数顺序：dirX, dirY, breakWidth, breakHight, breakTime, bounceFrequency。</summary>
        private static readonly string[] ParamLabels =
        {
            "dirPos.x（伤害来源方向）",
            "dirPos.y",
            "breakWidth（水平击退距离）",
            "breakHight（与 SetKnockBaseData 第一参数一致）",
            "breakTime（击退时长，对应 knockBackDuration）",
            "bounceFrequency（仅组件字段，TakeDamage 前写入）",
        };

        private static readonly string[] ParamDefaults =
        {
            "1", "0", "2", "0.5", "0.5", "2",
        };

        protected override void LoadAtlas(int targetAtlasCount)
        {
            // 本面板不加载图集；否则 base 的 targetAtlasCount=0 会导致 canStartUpdateUI 一直为 false。
            targetAtlasCount = 0;
            canStartUpdateUI = true;
        }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            _cmdItemTemplate = transform.Find(CmdItemPathPrimary) ?? transform.Find(CmdItemPathFallback);
            if (_cmdItemTemplate == null)
            {
                Debug.LogError("[KnockbackTestPanel] 未找到 cmdItem 模板，请检查路径 root/cmdItem 或 ScrollView/Content/cmdItem。");
            }
            else
            {
                // 先隐藏模板，再克隆；顺序上先放「模式」行，再放参数行，便于先选模式再调数值。
                _cmdItemTemplate.gameObject.SetActive(false);
                BuildModeButtons();
                BuildRowsFromTemplate();
            }

            BindCloseButton();
        }

        /// <summary>
        /// 复用 AA_TestPanel 的 cmdItem 行模板，生成多行输入，避免手写 DefaultControls 资源依赖。
        /// </summary>
        private void BuildRowsFromTemplate()
        {
            if (_cmdItemTemplate == null)
            {
                return;
            }

            var template = _cmdItemTemplate;
            var parent = template.parent;

            _paramInputs = new InputField[ParamLabels.Length];
            for (var i = 0; i < ParamLabels.Length; i++)
            {
                var row = Instantiate(template, parent);
                row.gameObject.SetActive(true);
                var titleGo = UIUtils.findChild(row.gameObject, "textTitle");
                if (titleGo != null)
                {
                    GameTools.setText(titleGo, ParamLabels[i]);
                }

                var inputGo = UIUtils.findChild(row.gameObject, "inputText");
                if (inputGo != null)
                {
                    var inputField = inputGo.GetComponent<InputField>();
                    if (inputField != null)
                    {
                        inputField.text = ParamDefaults[i];
                        _paramInputs[i] = inputField;
                    }
                }
            }

            var applyRow = Instantiate(template, parent);
            applyRow.gameObject.SetActive(true);
            var applyTitle = UIUtils.findChild(applyRow.gameObject, "textTitle");
            if (applyTitle != null)
            {
                GameTools.setText(applyTitle, "操作");
            }

            var enterBtnGo = UIUtils.findChild(applyRow.gameObject, "enterBtn");
            if (enterBtnGo != null)
            {
                GameTools.setObjectClickFunc(enterBtnGo, OnClickApply);
                var enterLabel = UIUtils.findChild(enterBtnGo, "Text");
                if (enterLabel != null)
                {
                    GameTools.setText(enterLabel, "执行受击/击退");
                }
            }

            var hintGo = new GameObject("ModeHint", typeof(RectTransform));
            hintGo.transform.SetParent(parent, false);
            var hintRect = hintGo.GetComponent<RectTransform>();
            hintRect.sizeDelta = new Vector2(900, 120);
            var hintText = hintGo.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            hintText.fontSize = 22;
            hintText.color = Color.yellow;
            hintText.alignment = TextAnchor.UpperLeft;
            hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintText.verticalOverflow = VerticalWrapMode.Overflow;
            _modeHintText = hintText;
            UpdateModeHint();
        }

        /// <summary>在 Scroll 内容区放置三个模式按钮（用 cmdItem 克隆一行）。</summary>
        private void BuildModeButtons()
        {
            if (_cmdItemTemplate == null)
            {
                return;
            }

            var template = _cmdItemTemplate;
            var parent = template.parent;
            var modeRow = Instantiate(template, parent);
            modeRow.gameObject.SetActive(true);
            var titleGo = UIUtils.findChild(modeRow.gameObject, "textTitle");
            if (titleGo != null)
            {
                GameTools.setText(titleGo, "模式（点按钮切换）");
            }

            var inputGo = UIUtils.findChild(modeRow.gameObject, "inputText");
            if (inputGo != null)
            {
                inputGo.SetActive(false);
            }

            var enterBtn = UIUtils.findChild(modeRow.gameObject, "enterBtn");
            if (enterBtn != null)
            {
                enterBtn.SetActive(false);
            }

            var hGo = new GameObject("ModeButtons", typeof(RectTransform));
            hGo.transform.SetParent(modeRow, false);
            var hRect = hGo.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 0.5f);
            hRect.anchorMax = new Vector2(1, 0.5f);
            hRect.sizeDelta = new Vector2(-40, 60);
            hRect.anchoredPosition = new Vector2(0, 0);

            var layout = hGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddModeButton(hRect, "Normal 全流程", KnockbackTestRunner.TestMode.FullNormal);
            AddModeButton(hRect, "Break 击飞", KnockbackTestRunner.TestMode.FullBreak);
            AddModeButton(hRect, "纯击退", KnockbackTestRunner.TestMode.PureKnockback);
        }

        private void AddModeButton(RectTransform parent, string label, KnockbackTestRunner.TestMode mode)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.35f, 0.55f, 0.95f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                _mode = mode;
                UpdateModeHint();
            });
            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 22;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = label;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 48f;
            le.flexibleWidth = 1f;
        }

        private void UpdateModeHint()
        {
            if (_modeHintText == null)
            {
                return;
            }

            switch (_mode)
            {
                case KnockbackTestRunner.TestMode.FullNormal:
                    _modeHintText.text =
                        "当前：Normal 全流程。需 breakHight>0 才会 SetKnockBaseData+ApplyKnockBack；breakTime 映射 knockBackDuration；bounceFrequency 在 TakeDamage 前写入组件。";
                    break;
                case KnockbackTestRunner.TestMode.FullBreak:
                    _modeHintText.text =
                        "当前：Break 击飞。走 DamageFly，不走 KnockBackComponent；下方「击退时长/频率」对击飞轨迹无影响，主要调 breakWidth/breakHight 与状态机内抛物线。";
                    break;
                case KnockbackTestRunner.TestMode.PureKnockback:
                    _modeHintText.text =
                        "当前：纯击退。仅 KnockBackComponent，无受击动画；用于快速调 Sin 位移曲线。";
                    break;
            }
        }

        private void BindCloseButton()
        {
            var closeTr = transform.Find("root/CloseBtn");
            if (closeTr == null)
            {
                return;
            }

            var closeBtn = closeTr.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(CloseForm);
            }
        }

        private void OnClickApply()
        {
            if (!TryParseParams(out var dirX, out var dirY, out var breakWidth, out var breakHight, out var breakTime,
                    out var bounceFrequency))
            {
                Debug.LogWarning("[KnockbackTestPanel] 参数解析失败，请检查数字格式。");
                return;
            }

            var dirSource = new Vector2(dirX, dirY);
            if (dirSource.sqrMagnitude < 1e-4f)
            {
                Debug.LogWarning("[KnockbackTestPanel] dirPos 长度过小，已使用 (1,0)。");
                dirSource = Vector2.right;
            }

            if (!KnockbackTestRunner.TryApply(_mode, dirSource, breakWidth, breakHight, breakTime, bounceFrequency,
                    out var err))
            {
                Debug.LogWarning("[KnockbackTestPanel] " + err);
            }
        }

        private bool TryParseParams(out float dirX, out float dirY, out float breakWidth, out float breakHight,
            out float breakTime, out float bounceFrequency)
        {
            dirX = dirY = breakWidth = breakHight = breakTime = bounceFrequency = 0f;
            if (_paramInputs == null || _paramInputs.Length < ParamLabels.Length)
            {
                return false;
            }

            var ok = true;
            ok &= float.TryParse(_paramInputs[0].text, out dirX);
            ok &= float.TryParse(_paramInputs[1].text, out dirY);
            ok &= float.TryParse(_paramInputs[2].text, out breakWidth);
            ok &= float.TryParse(_paramInputs[3].text, out breakHight);
            ok &= float.TryParse(_paramInputs[4].text, out breakTime);
            ok &= float.TryParse(_paramInputs[5].text, out bounceFrequency);
            return ok;
        }

    }

    /// <summary>
    /// 受击/击退测试的共享执行逻辑，供本 UI 与 Editor 窗口共用（与 FormLogic 同文件，避免工程/csproj 未收录单独文件时 CS0246）。
    /// </summary>
    public static class KnockbackTestRunner
    {
        public enum TestMode
        {
            FullNormal = 0,
            FullBreak = 1,
            PureKnockback = 2,
        }

        /// <summary>
        /// 在玩家已生成的前提下执行一次测试；dirSource 为伤害来源方向（与 DamageData.dirPos 一致）。
        /// </summary>
        public static bool TryApply(TestMode mode, Vector2 dirSource, float breakWidth, float breakHight, float breakTime,
            float bounceFrequency, out string errorMessage)
        {
            errorMessage = null;
            var player = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            if (player == null)
            {
                errorMessage = "无玩家实体：请在 canCreatePlayer 的场景中运行游戏后再试。";
                return false;
            }

            if (dirSource.sqrMagnitude < 1e-4f)
            {
                dirSource = Vector2.right;
            }

            var kb = player.componentSystem.GetComponent<KnockBackComponent>();
            if (kb == null)
            {
                errorMessage = "KnockBackComponent 缺失。";
                return false;
            }

            kb.StopKnockBackEffect();
            kb.bounceFrequency = bounceFrequency;

            switch (mode)
            {
                case TestMode.PureKnockback:
                    kb.SetKnockBaseData(breakHight, breakTime);
                    kb.ApplyKnockBack(-dirSource.normalized, breakWidth);
                    break;

                default:
                    var data = new DamageData
                    {
                        baseDamage = 0,
                        dirPos = dirSource,
                        attackType = mode == TestMode.FullBreak ? AttackType.BreakType : AttackType.NormalType,
                        breakWidth = breakWidth,
                        breakHight = breakHight,
                        breakTime = breakTime,
                    };
                    player.componentSystem.GetComponent<BattleComponent>().TakeDamage(data);
                    break;
            }

            return true;
        }
    }
}
