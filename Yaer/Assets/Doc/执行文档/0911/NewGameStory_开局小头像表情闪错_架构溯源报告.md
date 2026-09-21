# NewGameStory — 开局小头像表情闪错 — 架构溯源报告

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 台本 / Git  
**Unity**：2020.3.48f1  
**对白**：序章 `NewGameStory`（漫画后龙宫对白）  
**现象**：对话框刚出时左侧小头像先闪一下错脸，再切到第一句正确表情  
**产品期望（钉死）**：框淡入时 **小头像不显示**；**第一句真正出来时**一次性显示该句 FaceType；不闪错脸  
**不是**：改大立绘淡入；换 PNG；关整套 Mask；改进村 KenMuNiStart「框+头像同拍」  
**并行**：与同日「大立绘不出现」案解耦（大立绘已按方案 A 修好）  
**提示词**：`Assets/Doc/提示词/0911/NewGameStory_开局小头像表情闪错_架构侦探提示词.md`

---

## 沟通摘要

### ① 结论一句话

**根因 = H1：`NewGameStory` 框 FadeIn 勾了 `PrepareMaskAvatarOnFadeIn=true`，预亮 Yaer/`Laugh`，首句才是 Yaer/`Unhappy` → Laugh→Unhappy 闪一下。0911 大立绘施工为「对齐 KenMuNi」误带同拍预亮；序章产品应对齐 0902 门口空框（F1 关预亮），不是 F2 改预亮脸。**

### ② 原因（通俗）

对话框淡入任务会干两件事：擦掉残留字；若勾了「预亮小头像」，会在字还没出来时先摆好一张脸再一起淡出。  
进村开场故意要「框和头像同拍」才勾；0911 修大立绘时照抄了这套。  
但序章首句脸是 **Unhappy**，预亮却写了进村常用的 **Laugh** → 你会先看到 Laugh，出字再切 Unhappy。  
产品要的是：**框先空着，等第一句再出脸**（跟村长家门口一样）。

### ③ 用户 30 秒自查

| # | 操作 | 预期 |
|---|------|------|
| 1 | 完整新游戏到对话框刚淡入（字尚未出或刚出） | 左侧 Mask 已亮，脸更像 **Laugh（笑）** |
| 2 | 第一句正文出现 | 切到 **Unhappy（不高兴）** → 体感「闪一下」 |
| 3 | 确认闪的是 **字幕条左侧小头像**，不是场景大立绘 `YaerPainting` | 大立绘案已另修，本案只盯 Mask |
| 4 | （施工后）框淡入全程 Mask 空；首句一次到位 Unhappy | 无 Laugh 预告 |

### ④ 程序补充

见下文 §1～§7。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1 成立**：`PrepareMaskAvatarOnFadeIn=true` + `MaskAvatarRole=Yaer(1)` + `MaskAvatarFace=Laugh(6)` ≠ 首句 `FaceType=Unhappy(1)` |
| **写入点** | `NormalDialogueUIAlphaAnimationTaskAction.PrepareMaskAvatarForFadeIn` → `DialogueMaskAvatarPresenter.Apply`（**早于**首句 Statement） |
| **首句纠正** | `DialogueTMPUGUI.OnGetNewStatement` → Presenter 再 `Apply(Yaer, Unhappy)` |
| **引入原因** | `施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md` 写明 PrepareMask=true「对齐 KenMuNi」；剩余风险表亦写 Face=6 会再被首句盖 |
| **产品对照** | 本期跟 **0902 门口空框**；**不跟** KenMuNiStart 同拍 |
| **方案** | **F1**：仅本 Prefab 关预亮（近零代码）；**禁止** F2 当终局；**禁止**动 KenMuNiStart / 大立绘串行 |

---

## 2. 证据链

### 2.1 Prefab 字段（磁盘现网 · 大立绘 A 施工后）

路径：`Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab`  
首 ActionList（`executionMode=0` 串行）第三拍 UIAlpha：

| 字段 | 序列化值 | 枚举名 |
|------|----------|--------|
| `PrepareMaskAvatarOnFadeIn` | **true** | — |
| `MaskAvatarRole` | **1** | `DialogueRoleName.Yaer` |
| `MaskAvatarFace` | **6** | `DialogueFaceType.Laugh` |
| Delay / Duration / EndOnEnd | 0.5 / 0.5 / true | （大立绘节奏，本案不改） |

紧随其后的第一条 Statement（`$id:"1"`）：

| 字段 | 值 | 说明 |
|------|-----|------|
| `FaceType` | **1** | `DialogueFaceType.Unhappy` |
| `_actorName` | 雅尔（编码显示为雅尔） | 首句说话人为雅儿 |
| 后续 FaceType | 2, 3, … | 后续句另议；本案只钉开局闪 |

**预亮脸 vs 首句脸：Laugh ≠ Unhappy → 必闪（若预亮生效）。**

枚举锚点：`Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs`  
`None=0, Unhappy=1, …, Smile=4, Surprised=5, Laugh=6`  
`DialogueRoleName`：`None=0, Yaer=1`（`RoleName.cs`）

### 2.2 调用链（两帧之间玩家看见什么）

```
串行：Wait → YaerPainting CanvasGroup Fade → UIAlpha FadeIn
                                              │
NormalDialogueUIAlphaAnimationTaskAction.OnExecute
  ├─ ClearSubtitleTextsForEmptyFrame()     → 名/正文空
  ├─ PrepareMaskAvatarOnFadeIn == true
  │     └─ PrepareMaskAvatarForFadeIn()
  │           └─ Presenter.Apply(Yaer, Laugh)   ← ★ 错脸写入点（框仍可透明/正在淡入）
  └─ DOFade 字幕条 CanvasGroup 0→1            ← 玩家看到：框渐出 + Laugh 小头像同现

首句 StatementNodeEx
  └─ DialogueTMPUGUI → OnGetNewStatement(Yaer, Unhappy, text)
        └─ Presenter.Apply(Yaer, Unhappy)       ← 切到正确脸（闪的第二拍）
```

代码：

- `NormalDialogueUIAlphaAnimationTaskAction.cs` ≈63–73、119–134  
- `DialogueMaskAvatarPresenter.cs`：`Awake`→`HideAllPaintings`；`OnGetNewStatement`→`Apply`

**体感时间线**

| 阶段 | 框 | 正文 | Mask 小头像 |
|------|----|------|-------------|
| 框淡入中（现网） | ✅ 渐出 | 空（已 Clear） | ✅ **Laugh（预亮）** |
| 首句真正显示 | ✅ | 第一句 | ✅ **Unhappy**（再 Apply） |
| **产品期望** | ✅ | 空→首句 | ❌ 空 → ✅ 仅 Unhappy |

### 2.3 与 0911 大立绘施工的捆绑关系

`Assets/Doc/施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`：

- 前奏表第 3 行：UIAlpha … **`PrepareMaskAvatarOnFadeIn=true`（对齐 KenMuNi）**  
- 剩余风险：`MaskAvatarFace=6`「对齐 KenMuNi 默认；首句 Statement 会再 Apply 正式脸」

→ 大立绘修复本身（Wait / CanvasGroupAlpha / 去 YaerShow）**正确**；  
误把 **KenMuNi 同拍预亮**绑进序章 → 造成本案回归。  
**修本案不得回退大立绘串行淡入。**

### 2.4 产品对照（门口 vs 进村）

| 对白 | PrepareMask | 产品 | 本期 |
|------|--------------|------|------|
| `Village_KenMuNiStart` | **true**（故意同拍，如 Yaer/Laugh） | 框+头像同淡 | **禁止改** |
| 村长家门口初次（0902） | 曾误 true → **F1 关预亮** | 空框 → 首句出头像 | 样板 |
| **`NewGameStory`** | 现 **true**（0911 误对齐） | 用户钉死：**空框再出首句脸** | **跟门口，F1** |

先例报告：`执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md`（同构：预亮脸 ≠ 首句 / 或空框不该有头像）。

---

## 3. 假说表（H1～H6）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | Prefab 预亮 Laugh，首句 Unhappy → 闪错脸 | **✅ 主因** | Prefab true/Role1/Face6；首句 FaceType=1；代码路径明确 |
| **H2** | 未预亮时 SetDefault 先亮 Smile 再被盖 | **非主因**；F1 后若仍闪再验 | Laugh 专属来自 Face=6，非 Smile 默认；Awake 已 HideAll |
| **H3** | 旧 actorPortrait / 图集 Loader 闪 | **排除倾向** | 壳 `useMaskAvatar: 1`；闪脸与 Mask 预亮枚举一致 |
| **H4** | Prefab Mask 子树默认 Active 某脸 | **非主因** | Presenter Awake HideAll；若无 PrepareMask 应保持空至首句 |
| **H5** | 误认场景大立绘表情闪 | **排除（产品钉 Mask）** | 用户/提示词钉字幕条左侧；大立绘另案已修 |
| **H6** | 首句 FaceType 写错 | **排除作主因** | 首句 Unhappy 与台词「啊啊…碍事」匹配；问题是**多了预亮拍** |

---

## 4. 方案裁定

| 方案 | 做法 | 裁定 |
|------|------|------|
| **F1 · 关预亮** | `PrepareMaskAvatarOnFadeIn=false`；清字保留；首句再 Apply | **✅ 首选**（对齐门口；满足「先不显示」） |
| **F2 · 预亮改 Unhappy** | true 但 Face=1 | ❌ 能消错脸，仍「框出就有头像」，**违产品** |
| **F3 · FadeIn 再 HideAll** | 辅修 H2/H4 | 仅 F1 后仍闪时评估；**勿全局伤 KenMuNi** |
| **F4** | 改 Presenter / 改 KenMuNiStart | **禁止**本期 |

---

## 5. 建议施工步骤（只建议不施工）

1. **仅改** `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab`  
   - UIAlpha：`PrepareMaskAvatarOnFadeIn = **false**`  
   - `MaskAvatarRole` / `MaskAvatarFace` 可保留（不再生效）或清空；**勿改**首句 `FaceType`  
2. **禁止**改 `Village_KenMuNiStart` 的 PrepareMask。  
3. **禁止**用 F2（预亮改成 Unhappy 仍 true）冒充结案。  
4. **禁止**回退 0911 大立绘 Wait/CanvasGroupAlpha；**禁止**改 Presenter 全局默认。  
5. 施工说明：`Assets/Doc/施工说明/0911/NewGameStory_开局小头像空框取消预亮_施工说明.md`  
6. 验收：  
   - 框淡入：**无**小头像  
   - 第一句：**直接** Unhappy（或台本该脸），**无** Laugh→Unhappy  
   - 大立绘仍正常；KenMuNiStart 框+头像同拍无回归  

---

## 6. 剩余风险

| 风险 | 说明 |
|------|------|
| 其它「对齐 KenMuNi」Prefab | 若也误开 PrepareMask 且产品要空框，会同类闪/空框有头像；逐条按产品改，勿全局关字段默认 |
| F1 后仍闪 Smile | 再查 H2（首次 Activate→SetDefault）；必要时限定 NewGame 的 F3，勿动村线 |
| CSV 重导 | 可能冲掉 PrepareMask=false；验收前核对 |
| 与大立绘说明文档 | 施工说明仍写 PrepareMask=true；本案修后应在新施工说明写明「序章小头像改空框，不再对齐 KenMuNi 同拍」 |

---

## 7. 可复制给施工员（5～8 条）

> 根因已拍板：**H1**（预亮 Laugh ≠ 首句 Unhappy；产品要空框）。走 **F1**。

1. **目标**：`NewGameStory` 框淡入时 Mask 小头像不显示；第一句出现时一次到位正确脸；无错脸闪一下。  
2. **最小改动**：仅 `NewGameStory.prefab` 的 `NormalDialogueUIAlphaAnimationTaskAction`：`PrepareMaskAvatarOnFadeIn=false`。  
3. **禁止**把 `MaskAvatarFace` 改成 Unhappy 却保持 true（F2，违「先不显示」）。  
4. **禁止**改 `Village_KenMuNiStart`；**禁止**回退大立绘串行淡入；**禁止**改 Presenter/换 PNG/关 useMaskAvatar。  
5. 勿改首句 Statement 的 `FaceType`（保持 Unhappy=1，除非台本另改）。  
6. 施工说明落盘：`Assets/Doc/施工说明/0911/NewGameStory_开局小头像空框取消预亮_施工说明.md`（写明与 0911 大立绘「对齐 KenMuNi」解绑原因）。  
7. 验收：完整新游戏；框出无头像 → 首句 Unhappy；大立绘仍在；进村同拍无回归。  
8. 若关预亮后仍闪默认 Smile：转验收查 H2，勿先动村线。

---

## 附录：关键锚点

| 主题 | 路径 |
|------|------|
| Prefab | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` |
| FadeIn + 预亮 | `.../NormalDialogueUIAlphaAnimationTaskAction.cs` |
| Mask 驱动 | `.../DialogueMaskAvatarPresenter.cs` |
| 首句事件 | `.../DialogueTMPUGUI.cs` → `OnGetNewStatement` |
| 表情枚举 | `.../DialogueFaceType.cs` / `DialogueRoleName` |
| 引入说明 | `施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md` |
| 空框先例 | `执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md` |
| 进村同拍（勿改） | `技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md` |
