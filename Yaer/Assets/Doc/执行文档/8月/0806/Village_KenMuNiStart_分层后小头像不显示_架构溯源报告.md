# Village_KenMuNiStart 分层后小头像不显示 — 架构溯源报告

**文档版本**：v1.1（2026-08-06）  
**文档性质**：【架构侦探】只读溯源 + **拍板记录**（本文件仍不施工；施工另开）  
**范围**：分层显现节奏**已正确**；回归 bug——字幕条左侧 **Mask 小头像**黑窗不显示。不扩台本、不重做分层产品节奏。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0806/Village_KenMuNiStart_分层后小头像不显示_架构侦探提示词.md`
- 分层施工：`0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md`（已落地 `PrepareVillageStartLayeredReveal`）
- Mask 接线：`0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`；`DialogueMaskAvatarPresenter.cs`

**Unity**：2020.3.48f1  

**拍板（2026-08-06）**：修法采用 **方案 B 白名单**（不用排除子树）。理由：后续还有其它开场要做同类「分层显现准备」，白名单可复用、不易再误伤 Mask/壳内同名节点。

---

## ① 结论一句话

**根因是进村旁路 `PrepareVillageStartLayeredReveal` 用名字模糊匹配（`Painting`/`GoOut`/`Gusha`/`Bottom`…）把 `NormalDialogueNewPanel` 整棵子树里的 CanvasGroup 打成 alpha=0，误伤了 Mask 内同名 `GoOutStoryYaerPainting` 等；拍2 只把字幕条 `subtitlesCanvasGroup` 淡回 1，拍3 只淡回 Prefab BB 场景大立绘，Presenter.Apply 只 `SetActive` 不改 alpha → Mask 窗一直黑。已拍板方案 B：Prepare 改为白名单（只动字幕条 + 明确场景大立绘），禁止扫整棵 Panel；保住分层与零漏缝，并给后续其它开场复用。**

---

## ② 原因（生活类比）

### 生活类比

舞台分层时，关灯员按名册把所有叫「演员立绘」的灯关掉——台上大演员和话筒旁小显示器**同名**，一起关了。后来有人专门给大演员开灯（Prefab 前奏 Fade），字幕条总开关也打开了（对话框 Alpha），但**小显示器没人开**，相框里就一直黑屏。人（Active）其实已经摆进去了，只是灯（CanvasGroup.alpha）没亮。

### 黑窗是哪块 UI（钉死）

| 项 | 现网事实 |
|----|----------|
| 期望路径 | `NormalDialogueNewPanel/Bottom/Mask/YaerAvatarRoot/GoOutStoryYaerPainting`（雅儿首句；`yaerUseGoOutOnly=1`） |
| `useMaskAvatar` | Prefab 序列化 **`1`** → Mask 为真源，旧 Portrait 不参与 |
| 正文 / 大立绘 | 正常 → 不是整段对话挂了 |
| 黑的是什么 | **Mask 内 Painting 的 CanvasGroup.alpha 仍为 0**（Active 可被 Presenter 打开，仍看不见） |

### 为何不是其它假说（并列排查）

| 假说 | 结论 |
|------|------|
| Presenter 首句未 Apply / FaceType 竞态 | **次要/否**：Apply 会 `SetActive(true)+UpdateFace`；alpha=0 时仍黑。错脸类 bug 会「有脸但错」，不是空黑窗 |
| `useMaskAvatar` 关掉 | **否**：现网 =1 |
| Prefab 分层 Action 误关 Mask Active | **否**：前奏只动 UI Alpha + BB 场景立绘，不碰 `YaerAvatarRoot` |
| HideAll 后未再开 | **否**：首句雅儿应 Resolve→GoOut 再 Active；且若 Active 失败更像整槽空，与「Prepare 误伤」叠加时仍以 alpha 为主因 |
| 仅进村复现 | **高度吻合**：Prepare 只在村 `FinalizeVillageStartCoverAndCloseBlack`；DialogDebug 不走旁路 → 小头像应正常（对照实验） |

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板

1. **修法**：**✅ 已决议 — 方案 B 白名单**（只动字幕条 CanvasGroup + 明确路径/引用的场景大立绘；禁止 `GetComponentsInChildren` 按名字广扫整棵 Panel）。  
   - 不用方案 A（排除子树）：后续其它开场还要做同类 Prepare，广扫+禁区容易再漏、难复用。  
2. 是否顺手给 Presenter「Activate 时 alpha=1」防再误伤？（建议：**可选加厚**，非必须；主修仍在 Prepare 白名单）

### 验收清单

1. 新档进村：分层节奏不变（BG → 框 → 大立绘）；首句起 Mask 小头像**可见**（雅儿 Laugh/台本脸）。  
2. 大立绘仍正常；无裸村景。  
3. DialogDebug 同 Prefab 不回归。  
4. 后续句换古莎等，小头像仍切换。

---

## ④ 给程序看的补充

### 4.1 现网时序图（拍2 → 首句 → 小头像应亮未亮）

```
【进村旁路】
TriggerStory → 开壳 + Instantiate Prefab（含全屏 BG）
  → Finalize… → PrepareVillageStartLayeredReveal()
       · dialogueUICanvasGroup (subtitles/Bottom) alpha = 0
       · 凡名含 Painting|GoOut|Gusha|Bottom|subtitles|Subtitle 的 CanvasGroup → 0
            ├─ Bottom                         ✓ 故意
            ├─ DialogueScene…/GoOutStoryYaerPainting（场景大立绘）✓ 故意
            ├─ DialogueScene…/GushaPainting     ✓ 故意
            └─ Bottom/Mask/YaerAvatarRoot/GoOutStoryYaerPainting  ✗ 误伤
            └─ …/GushaPainting、YaerPainting、Amy…               ✗ 误伤
  → CloseFormFade（拍1：只见 BG）

【Prefab 前奏 · 已施工】
Node1  NormalDialogueUIAlpha：Delay1 + Fade→1（只动 subtitlesCanvasGroup）
       → 正文条可见；Mask 子 Painting 的独立 CanvasGroup 仍为 0
Node2  BB 场景 GoOut/Gusha 并行 Fade→1（只绑场景实例，不绑 Mask）
Node3  Statement 首句 → OnGetNewStatement
       → Presenter.Apply：HideAll → SetActive(GoOut)=true → UpdateFace
       → ★ Active 开了，CanvasGroup.alpha 仍 0 → Mask 黑窗
```

### 4.2 误伤节点表（必出）

| 节点路径 | 被 Prepare 命中？ | 谁应负责恢复 alpha/Active | 现网是否恢复 |
|----------|-------------------|---------------------------|--------------|
| `Bottom`（字幕条 CanvasGroup = 通常即 `subtitlesCanvasGroup`） | **是**（名含 Bottom） | Prefab Node1 `NormalDialogueUIAlpha` →1 | **是**（故正文可见） |
| `Bottom/Mask/YaerAvatarRoot/GoOutStoryYaerPainting` 等 Mask 内 Painting | **是**（名含 Painting/GoOut/Gusha） | 无人；Presenter 只 SetActive | **否** → 黑窗 |
| Prefab 场景大立绘 `GoOutStoryYaerPainting` / `GushaPainting`（BB） | **是** | Prefab Node2 `CanvasGroupAlpha` →1 | **是**（大立绘正常） |
| `BlackMask` | 跳过 | — | 保持 0（正确） |
| `BG` | 无 CanvasGroup，不扫 | 兜底 SetActive(true) | 拍1 可见（正确） |

**代码锚点**（现网）：

```265:306:Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
        void PrepareVillageStartLayeredReveal()
        {
            // ...
            var groups = logicRoot.GetComponentsInChildren<CanvasGroup>(true);
            // name Contains Painting|GoOut|Gusha|Bottom|subtitles|Subtitle → alpha=0
            // 未排除 Mask / YaerAvatarRoot 子树
        }
```

```67:84:Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
        public void Apply(DialogueRoleName role, DialogueFaceType faceType)
        {
            HideAllPaintings();
            // ...
            painting.gameObject.SetActive(true);
            painting.UpdateFace(ResolveFaceKey(role, faceType));
            // 不改 CanvasGroup.alpha → 无法自愈 Prepare 误伤
        }
```

### 4.3 对照实验（如何一锤定音）

| 实验 | 若结果 | 含义 |
|------|--------|------|
| DialogDebug 拖 `Village_KenMuNiStart`（不走进村旁路） | 小头像正常 | 坐实 Prepare / 进村专用 |
| 进村复现时 Hierarchy 看 `Mask/.../GoOutStoryYaerPainting` 的 CanvasGroup.alpha | =0 | 坐实误伤未恢复 |
| 临时跳过 Prepare 对 Painting 的名字匹配（脑内/下轮施工） | 小头像应恢复 | 施工方向 |

### 4.4 方案比选表

| 方案 | 做法摘要 | 保分层 | 改动面 | 风险 | 推荐？ |
|------|----------|--------|--------|------|--------|
| A | Prepare 排除位于 `Mask` / `YaerAvatarRoot` 下的 CanvasGroup；场景大立绘仍按名/路径藏 | **是** | 仅数行 | 广扫仍在；其它开场复用时易再漏禁区 | ❌ 不采用 |
| **B** | **白名单**：只动 `dialogueUICanvasGroup` + 明确 DialogueScene/BB 场景立绘；**禁止**按名字扫整棵 Panel | **是** | Prepare 重写匹配 | 略多代码；名单要随开场立绘补全 | ✅ **已拍板** |
| C | Presenter.Apply / 框 Fade 完成时强制当前 Mask Painting `alpha=1` | 是 | Presenter 或 UIAlpha | 治标；不替代白名单 | 可作加厚 |
| D | 框 Fade 时广扫把 Mask 下 Painting 拉回 1 | 是 | Prefab/任务或旁路 | 易误拉场景立绘 | 不推荐 |

### 4.4.1 白名单落地约定（施工必遵 · 便于其它开场复用）

**原则**：Prepare 只写「允许藏」的引用/路径；名单外 CanvasGroup **一律不碰**（含 Mask 小头像、无关 UI）。

| 白名单项（本期 KenMuNiStart） | 怎么找 | 淡出前 alpha | 谁负责再亮 |
|------------------------------|--------|--------------|------------|
| 字幕条 | `NormalDialogueFormNewLogic.dialogueUICanvasGroup`（=`subtitlesCanvasGroup`） | 0 | Prefab `NormalDialogueUIAlpha` |
| 场景雅儿大立绘 | 对话实例下 BB/`DialogueScene` 的 `GoOutStoryYaerPainting`（**非** `Bottom/Mask/...`） | 0 | Prefab Node `CanvasGroupAlpha` |
| 场景古莎大立绘 | 同上 `GushaPainting`（场景实例） | 0 | 同上 |
| 全屏 BG | 仅兜底 `SetActive(true)`；无 CanvasGroup 则不改 alpha | 保持可见 | — |

**禁止再出现**：`GetComponentsInChildren<CanvasGroup>` + `name.Contains("Painting"|"GoOut"|…)` 广扫。

**其它开场复用**：新开场若也要「仅 BG → 框 → 立绘」，复制同一白名单模式，把「场景立绘」换成该 Prefab BB 实际节点；**不要**再发明一套名字模糊匹配。若多场景重复，可后续抽到薄工具/基类方法（非本期必做）。

### 4.5 施工员最小改动清单（只建议）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `Village_KenMuNiSceneManager.cs` → `PrepareVillageStartLayeredReveal` | **改成白名单**：只对上表项写 alpha=0；删掉名字 Contains 广扫；注释写明「其它开场照抄白名单，勿广扫」 |
| 2 | （可选）`DialogueMaskAvatarPresenter.Apply` | Activate 后若有 CanvasGroup 则 `alpha=1`（加厚，非主修） |
| 3 | （可选调试）白名单命中时 `Debug.Log("[VillageStart][Prepare] hide "+path)`，验收后可删 | |
| 不改 | Prefab 分层节奏；台本；Mask 接线主链；其它对话 Prefab（除非抽公共方法） | |

**禁止**：关掉分层；Update 轮询补头像；重开 0803 Mask 大改造；退回「排除子树但仍广扫」。

### 4.6 开放问题

| ID | 问题 | 决议 / 建议 | 状态 |
|----|------|-------------|------|
| Q1 | Prepare 用排除子树（A）还是白名单（B）？ | **B 白名单**（后续其它开场要复用） | ✅ 已拍板 |
| Q2 | 是否顺手 Presenter「Activate 时 alpha=1」？ | **可选**；主修仍在 Prepare 白名单 | 待施工取舍 |

---

## 施工员下一轮最小化清单（建议 · 已拍板 B）

1. 将 `PrepareVillageStartLayeredReveal` 改为 **白名单**（字幕条 + 场景大立绘引用），禁止名字广扫。  
2. 进村验收：分层不变 + 首句 Mask 小头像可见 + 换人切换。  
3. DialogDebug 对照无回归。  
4. 注释/文档约定：后续其它开场 Prepare 复用白名单模式。  
