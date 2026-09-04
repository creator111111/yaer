# Village_村长家门口初次对话 — 框出时空头像 — 架构溯源报告

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【架构侦探】只读定根因（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**现象**：对话框已出现、正文仍空的第一帧，左侧已露出雅儿小头像（用户描述为默认/闭眼）  
**产品期望**：框刚出时左槽**空**；首句真正显示时再出该句小头像  
**对白**：`Village_村长家门口初次对话`  
**提示词**：`提示词/0902/Village_村长家门口初次对话_框出时空头像_架构侦探提示词.md`  
**对照**：`Village_KenMuNiStart` 故意「框+头像同拍」；本期门口**不要**照搬  

**关联文档**：
- `技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- `技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`
- `执行文档/0806/Village_KenMuNiStart_对话框渐入渐出对齐_架构溯源报告.md`（预亮机制）
- `提示词/0804/对话框小表情_首句未跟FaceType_架构侦探提示词.md`（默认脸竞态参考）

---

## 沟通摘要

### ① 结论一句话

**H1 成立：门口 Prefab 框 FadeIn 勾了 `PrepareMaskAvatarOnFadeIn=true`（Yaer / Smug），淡入前就 `Presenter.Apply` 亮 Mask；同时清字 → 正中「有框无字却有雅儿头像」。施工默认 F1：仅门口关掉预亮；勿动 KenMuNiStart。**

### ② 原因（通俗）

对话框淡入任务有两件事：① 把字擦掉（所以你看到空框）；② 若勾了「预亮小头像」，会在擦字的同时先把雅儿脸摆好再一起淡出。  
进村开场是故意要「框和头像同拍」才勾的；门口 Prefab 误用了同一套配置，但产品要的是空框 → 首句再出头像。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 门口三人戏：框出现瞬间（无字）左槽 **无** 小头像 | |
| 2 | 第一句「奶奶。」出现时，左槽出现 **古莎** 对应表情（Happy） | |
| 3 | 后续雅/村/古换脸正常；大立绘三人戏不受影响 | |
| 4 | `Village_KenMuNiStart`：框出仍可预亮 Mask（框+头像同拍） | |
| 5 | 商店等其它已勾预亮的对话无回归 | |

### ④ 程序补充

见下文 §①～§⑧。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 左槽身份 | **Mask 立绘**（`useMaskAvatar=1`；旧 Portrait/`Yaer` Active=false） |
| 根因 | **H1** · 门口 Prefab `PrepareMaskAvatarOnFadeIn=true` + Role=`Yaer`(1) + Face=`Smug`(3) |
| 空字原因 | FadeIn → `ClearSubtitleTextsForEmptyFrame()`（符合现网；非 bug） |
| 默认脸写入点 | FadeIn 内 `PrepareMaskAvatarForFadeIn` → `DialogueMaskAvatarPresenter.Apply(Yaer, Smug)` |
| 首句真源 | Statement → `OnGetNewStatement` → `Apply`；首句为 **古莎「奶奶。」FaceType=Happy(10)** |
| 方案 | **F1** · 门口 Prefab 关预亮（零代码）；H2/H4 非主因，F2 仅作兜底 |
| 禁止 | 关 KenMuNiStart 预亮；关 `useMaskAvatar`；改 CSV/大立绘；全局无差别关预亮 |

---

## ② 左槽控件身份（任务 1）

| 控件 | 路径 / 状态 | 是否截图真源 |
|------|-------------|--------------|
| **Mask 立绘** | `Bottom/Mask/YaerAvatarRoot` + `DialogueMaskAvatarPresenter`；`useMaskAvatar: 1` | ✅ **是** |
| 旧 Portrait | 节点名 `Yaer`（`actorPortrait`）；`m_IsActive: 0`；`OnGetAvatar` 在 Mask 模式下保持关 | ❌ 非主影 |
| 场景大立绘 | Prefab 壳内 `GoOutStoryYaerPainting` 等（DialogueSceneContainer） | ❌ 非字幕条左槽 |

磁盘核实（`NormalDialogueNewPanel.prefab`）：
- `DialogueTMPUGUI.useMaskAvatar: 1`
- 旧头像 GO `Yaer`：`m_IsActive: 0`
- Mask 内 `GoOutStoryYaerPainting` 实例 Override：`m_IsActive: 0`（默认关，靠 Presenter.Apply 再开）

**结论**：用户截图左槽是 **Mask 下的雅儿 Painting**，不是旧 Portrait 双影。

---

## ③ 时序图（任务 2）— 门口对白现网

```
Npc_Chief Enter
  → BlackPanel Show → TriggerStory("Village_村长家门口初次对话")
  → Open NormalDialogueNewPanel 壳 + Instantiate 门口 Prefab
  → Prefab 前奏：
       FightingPanel 藏
       → 三人场景大立绘并行 CanvasGroup Fade（约 1s）
       → NormalDialogueUIAlphaAnimationTaskAction（框 FadeIn）
            │
            ├─ 必要时 Active(subtitlesGroup)
            ├─ ClearSubtitleTextsForEmptyFrame()     ← 名字/正文清空 【空字】
            ├─ PrepareMaskAvatarOnFadeIn == true     ← ★ 默认脸写入点
            │     → Presenter.Apply(Yaer, Smug)      ← Mask 雅儿亮起 【空头像 bug】
            └─ DOFade(0→1) 框透明度
  → Statement 首句：古莎「奶奶。」FaceType=Happy
       → OnGetNewStatement → Apply(Gusha, Happy)     ← 才是产品期望的「首句出头像」
  → …后续句…
```

**「默认脸」写入点钉死**：框 FadeIn 的 `PrepareMaskAvatarForFadeIn`，**早于**首句 Statement。  
清字只清 TMP，**不** Hide Mask → 故截图呈现「有框、无字、有雅儿头像」。

---

## ④ Prefab BB 实测（任务 3）

### 4.1 门口 · `Village_村长家门口初次对话.prefab`

序列化片段（磁盘只读核实）：

```
PrepareMaskAvatarOnFadeIn: {"_value": true}
MaskAvatarRole: {"_value": 1}   → DialogueRoleName.Yaer
MaskAvatarFace: {"_value": 3}   → DialogueFaceType.Smug
```

### 4.2 进村对照 · `Village_KenMuNiStart.prefab`

```
PrepareMaskAvatarOnFadeIn: {"_value": true}
MaskAvatarRole: {"_value": 1}   → Yaer
MaskAvatarFace: {"_value": 6}   → Laugh
```

与技术说明「对话框淡入 + 预亮 Mask（Yaer / Laugh）」一致。

### 4.3 首句 Statement（门口）

| 序 | 演员 | 文案 | Face / Chief |
|----|------|------|--------------|
| 0 | 古莎 | 奶奶。 | FaceType=`Happy`(10) |
| 1 | 村长 | 古莎，路上还平安吗。 | UseChiefPortrait + ChiefFace=2 |

→ 预亮用的是 **雅儿 Smug**，与首句 **古莎 Happy** 完全不是同一人/脸；空框阶段露雅儿 = 产品错误预告。

---

## ⑤ 假说证伪（H1～H5）

| ID | 假说 | 结果 | 证据 |
|----|------|------|------|
| **H1** | 门口勾了 `PrepareMaskAvatarOnFadeIn` 提前 Apply | ✅ **成立（主因）** | Prefab BB true + Yaer/Smug；代码路径 `PrepareMaskAvatarForFadeIn` |
| **H2** | Painting `Start`→`SetDefaultPainting` 亮默认脸 | ❌ 非主因 | `GoOutStoryYaerPainting.SetDefaultPainting` 仅 `SyncHeadwearFromArchive`，**不再**强制 Smile；Presenter `Awake` 已 `HideAllPaintings`；GoOut 默认 Active=0 |
| **H3** | 旧 Portrait 残留 | ❌ | Portrait Active=0；`useMaskAvatar` 下 `OnGetAvatar` 保持关 |
| **H4** | Prefab 默认亮着某 Face | ❌ 非主因 | GoOut Override Active=0；需 Apply 才开 |
| **H5** | 首句早到又被清字盖掉，只剩头像 | ❌ 次要/不成立为现网主路径 | 清字在 FadeIn；首句在 Fade 节点之后；预亮已足够解释 |

用户观感「默认/闭眼」：预亮 Face 实为 **Smug**（非 CloseEyes=11）。可能是 Smug 视觉偏「眯/默认脸」，或口语统称；根因仍是 **FadeIn 预亮雅儿**，不必改 Face 表。

---

## ⑥ 为何进村可预亮、门口不能照搬（任务 4）

| | `Village_KenMuNiStart` | `Village_村长家门口初次对话` |
|--|------------------------|------------------------------|
| 产品 | 分层：框与 Mask **同拍淡入**，再出首句 | **空框** → 首句再出头像 |
| Prefab | 故意 `PrepareMaskAvatarOnFadeIn=true`（Yaer/Laugh） | **误同构** true（Yaer/Smug） |
| 首句角色 | 常与预亮雅儿一致或紧随 | 首句是 **古莎**，预亮雅儿更突兀 |
| 修法 | **保留**预亮 | **关**预亮（F1） |

**禁止**：为修门口全局关掉 `PrepareMaskAvatarOnFadeIn` 字段默认值，或改公共逻辑使 KenMuNiStart 无法预亮。

---

## ⑦ 方案与最小修复清单（任务 5）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1 · 门口关预亮** | 门口 Prefab：`PrepareMaskAvatarOnFadeIn=false`（Role/Face 可留空）；保持 FadeIn 清字 | ✅ **推荐**（H1 成立，零代码） |
| F2 · 框出 HideAll | FadeIn 清字时若未勾预亮则 `HideAll` | 可选兜底；本期非必须 |
| F3 · 预亮 Role=None | `PrepareMaskAvatarForFadeIn` 对 None **直接 return，不 HideAll** | ❌ 无效/易误解 |
| F4 / F5 | 延后出框 / 关 Mask 体系 | ❌ |

### 施工最小清单（F1）

1. 打开 `Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`
2. 找到框 FadeIn 节点 `NormalDialogueUIAlphaAnimationTaskAction`
3. 将 **`PrepareMaskAvatarOnFadeIn` 改为 false**（或清空 `_value`）
4. **不要**改：`Village_KenMuNiStart`、`ClearSubtitleTextsForEmptyFrame`、Presenter、CSV、大立绘
5. 验收见沟通摘要清单；必回归 KenMuNiStart 框+头像同拍

### 回归范围

| 必测 | 说明 |
|------|------|
| 门口初次对话 | 空框无头像 → 首句古莎头像 |
| KenMuNiStart | 预亮仍生效 |
| （若只改门口 Prefab）商店/其它预亮对话 | 一般无影响；若误改公共 C# 则需测 |

---

## ⑧ OPEN / 残留风险

| ID | 项 | 说明 | 状态 |
|----|----|------|------|
| Q1 | 门口是否允许「框出预亮首句角色」？ | 产品钉死：**否**；空框 → 首句 | ✅ 已决议 |
| Q2 | 预亮 Face=Smug 是否历史拷贝 KenMuNi 时误填？ | 不影响 F1；可事后核对 Setup 工具默认值 | ⏳ 可选 |
| Q3 | Setup 菜单再建门口 Prefab 是否会再次写回预亮=true？ | 若有 Door Setup，施工时核对默认 BB，防回潮 | ⏳ 施工核对 |

残留风险：仅改门口 Prefab 后，若工具一键 Setup 按 KenMuNi 模板重写 FadeIn BB，可能回潮 → 施工说明须写「Setup 默认勿勾预亮」。

---

## ⑨ 代码锚点速查

| 主题 | 路径 |
|------|------|
| FadeIn 清字 + 预亮 | `NormalDialogueUIAlphaAnimationTaskAction.cs` |
| Apply / HideAll / None 旗 | `DialogueMaskAvatarPresenter.cs` |
| 首句事件 / useMaskAvatar | `DialogueTMPUGUI.cs` |
| GoOut 不再强制 Smile | `GoOutStoryYaerPainting.SetDefaultPainting` |
| 门口 Prefab | `GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab` |
| 进村对照 | `GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` |

---

## ⑩ 给施工员的一句话

**只改门口对话 Prefab：关掉 `PrepareMaskAvatarOnFadeIn`。代码与 KenMuNiStart 预亮一律不动。**
