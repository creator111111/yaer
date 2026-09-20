# Cursor Agent Prompt · NewGameStory 开局对话框小头像表情闪一下（预亮错脸）

> **角色**：【架构侦探】只读溯源；**禁止改代码 / Prefab / 台本 / Git 提交**  
> **日期**：2026-09-11  
> **对白**：序章 **`NewGameStory`**（新游戏漫画结束后的龙宫对白）  
> **现象（用户实测）**：  
> 1. 开局对话框刚出来时，左侧小头像会先闪一下 **默认/不对的表情**  
> 2. 随后才切到第一句话的正确表情  
> **产品期望（钉死）**：  
> 1. **对话框刚出现 / 淡入时**：小头像区 **不显示**（空，不预亮任何脸）  
> 2. **第一句话真正出来时**：一次性显示该句对应表情（跟台本 FaceType）  
> 3. 体感：**不闪一下错脸**  
> **不是**：改场景大立绘 `YaerPainting` 淡入；不是换 PNG；不是关掉整套 Mask 改回旧 Portrait；不是改进村 `Village_KenMuNiStart`「框+头像同拍」产品  
> **并行**：与同日「大立绘不出现」案解耦——大立绘已按方案 A 修好；本案是 **字幕条 Mask 小头像时序**  
> **报告落盘**：`Assets/Doc/执行文档/0911/NewGameStory_开局小头像表情闪错_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
大立绘淡入完成
  → 对话框淡入 …… 此时：有框、【无小头像】
  → 首句 OnGetNewStatement …… 此时：出字 + 出该句 Mask 表情（一次到位）
  → 后续句按现网换脸
```

| 阶段 | 框 | 正文 | 小头像（Mask） |
|------|----|------|----------------|
| 框刚出 / 淡入中 | ✅ | 空或未出首句 | ❌ **须空** |
| 首句真正显示 | ✅ | ✅ 第一句 | ✅ **仅该句 FaceType** |
| 用户现状 | ✅ | 随后出字 | ⚠ **先错脸，再切对** |

### 与进村开场对照（勿照搬）

| 对白 | `PrepareMaskAvatarOnFadeIn` | 产品 |
|------|------------------------------|------|
| `Village_KenMuNiStart` | **故意 true**（框+头像同拍，如 Yaer/Laugh） | 进村分层要同拍，**本期禁止改** |
| `Village_村长家门口初次对话` 等 | **false**（0902 空框定稿） | 空框 → 首句再出头像 |
| **`NewGameStory`（本案）** | 助手预扫现为 **true**（0911 大立绘施工「对齐 KenMuNi」写入） | **产品要空框再出首句脸**（对齐门口，不对齐进村同拍） |

### 现网机制（助手预扫 · 高度可疑）

```
NormalDialogueUIAlphaAnimationTaskAction（框 FadeIn）
  → ClearSubtitleTextsForEmptyFrame()
  → 若 PrepareMaskAvatarOnFadeIn=true
       → Presenter.Apply(MaskAvatarRole, MaskAvatarFace)   // ⚠ 预亮
  → DOFade 框透明度

首句：
DialogueTMPUGUI → OnGetNewStatement → Presenter.Apply(台本 role, 台本 faceType)
```

| 环节 | 预扫结果 | 若失败的体感 |
|------|----------|--------------|
| **NewGameStory UIAlpha BB** | `PrepareMaskAvatarOnFadeIn=true`；`MaskAvatarRole=1`（**Yaer**）；`MaskAvatarFace=6`（枚举 **`Laugh`**） | 框淡入时先亮 Laugh |
| **首句 Statement** | 同 Prefab 下一节点 `FaceType={"_value":1}` → 枚举 **`Unhappy`** | 首句再切 Unhappy → **Laugh→Unhappy 闪一下** |
| **写入来源** | `施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md` 明确把 PrepareMask=true「对齐 KenMuNi」 | 大立绘修复时误带进「同拍预亮」，与序章产品冲突 |
| **同构先例** | 0902 门口：`PrepareMask=true` + 预亮脸 ≠ 首句脸 → 空框却有默认头像；施工 **关预亮** | 本案更像「有预亮错脸」变体，根因同类 |
| **Presenter** | `Awake` 先 `HideAllPaintings`；首句再 Apply | 若未预亮，框出应保持空，直到首句 |

### 假说表（须并列证伪）

| ID | 假说 | 证伪手段 |
|----|------|----------|
| **H1（首选）** | Prefab 勾了 **`PrepareMaskAvatarOnFadeIn`**，预亮 **Laugh**，首句才是 **Unhappy** → 闪错脸 | 读 Prefab 序列化；Play 时 FadeIn 前后 Mask Active/脸；对照首句 FaceType |
| **H2** | 未预亮，但 Mask Painting 首次 `Start`→`SetDefaultPainting` 亮默认 Smile 再被首句盖 | 关预亮后是否仍闪；查 Dress/`YaerPainting` Mask 实例 SetDefault |
| **H3** | 旧 `actorPortrait` / 图集 Loader 闪一下 | `useMaskAvatar`；Hierarchy 左槽是 Mask 还是 Portrait |
| **H4** | Prefab Mask 子树默认 Active 某脸 | 离线看 `NormalDialogueNewPanel` / 实例默认 Active |
| **H5** | 大立绘 `YaerPainting` 表情被误认成「小头像闪」（用户口误） | 确认闪的是字幕条左侧 Mask，不是场景大立绘 |
| **H6** | 首句 FaceType 台本/节点写错，第二拍才「看起来对」 | 读首句节点 FaceType 与产品期望是否一致 |

### 方案倾向（仅倾向，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1 · 关预亮（对齐门口）** | `NewGameStory`：`PrepareMaskAvatarOnFadeIn=false`；框淡入保持清字；首句 `OnGetNewStatement` 再 Apply | **首选**（若 H1；零/近零代码） |
| **F2 · 预亮改成首句同脸** | 保持 true，但 `MaskAvatarFace` 改成与首句相同（Unhappy） | 能消「错脸」，但仍是「框出就有头像」，**不符合**「先不显示小头像」 |
| **F3 · FadeIn 强制 HideAll** | 未勾预亮时 FadeIn 再 HideAll 兜底 | 辅修 H2/H4；勿做成全局破坏 KenMuNiStart |
| **F4** | 改通用 Presenter / 改进村 Prefab | **禁止**本期范围 |

**产品钉死**：要的是 **F1 空框**，不是 F2「预亮对脸」。

### 侦探禁止事项

- 禁止改代码 / Prefab「先修再查」。  
- 禁止改 `Village_KenMuNiStart` 的 PrepareMask（进村同拍）。  
- 禁止动大立绘 CanvasGroup 淡入、旁路 Prepare、换素材。  
- 禁止把「对齐 KenMuNi」当作序章小头像的产品真理——须以用户本期期望为准。

---

## 【架构侦探】执行段（复制给 Agent）

你是【架构侦探】。只读溯源，**禁止改代码**。

### 目标

查清：`NewGameStory` 开局对话框出现时小头像为何先闪错脸，再切到首句表情；确认是否与 `PrepareMaskAvatarOnFadeIn` 预亮脸 ≠ 首句 FaceType 成对；给出最小修复建议（优先 Prefab 关预亮，对齐 0902 门口空框产品）。

### 必查清单

1. **Prefab 前奏 UIAlpha**  
   - 精确读出：`PrepareMaskAvatarOnFadeIn` / `MaskAvatarRole` / `MaskAvatarFace` 的值与枚举名。  
   - 精确读出：**第一条 Statement** 的 Role（若有）与 `FaceType` 枚举名。  
   - 表格对比：预亮脸 vs 首句脸是否不一致。

2. **调用链**  
   - `NormalDialogueUIAlphaAnimationTaskAction.PrepareMaskAvatarForFadeIn` → `DialogueMaskAvatarPresenter.Apply`。  
   - 首句：`OnGetNewStatement` → `Apply`。  
   - 写清两帧之间玩家会看到什么。

3. **与 0911 施工关系**  
   - 核对 `施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md` 是否写入 PrepareMask=true。  
   - 说明：大立绘修复与小头像预亮是否被捆绑「对齐 KenMuNi」导致回归。

4. **排除**  
   - H2/H3/H4/H5：一句话证伪或标待 Play。  
   - 确认闪的是 **Mask 小头像**，不是场景 `YaerPainting`。

5. **产品对照**  
   - 引用 0902 门口空框决议；明确 NewGame 本期跟门口还是跟 KenMuNiStart。  
   - 用户已钉死：跟「先不显示 → 首句再出」。

### 报告结构

1. 结论一句话（根因 + 是否 H1）  
2. 证据链（Prefab 字段、枚举名、方法、施工说明引用）  
3. 假说表 H1～H6  
4. 用户 30 秒自查（看 FadeIn 时 Mask 是否已亮 Laugh）  
5. 建议施工（F1 优先；写清禁止改 KenMuNiStart）  
6. 剩余风险（其它也「对齐 KenMuNi」误开 PrepareMask 的序章外对白）  
7. 文末「可复制给施工员」5～8 条 bullet  

### 输出要求

- 中文；大白话；结论 → 原因 → 检查清单 →（可选）程序补充。  
- 报告：`Assets/Doc/执行文档/0911/NewGameStory_开局小头像表情闪错_架构溯源报告.md`  
- 可引用：  
  - `Assets/Doc/执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md`  
  - `Assets/Doc/施工说明/0902/Village_村长家门口初次对话_框出时空头像_施工说明.md`  
  - `Assets/Doc/施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`  
  - `Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`（对照「故意同拍」）

---

## 【施工员】Prompt（根因拍板后再用 · 预稿）

> 仅当侦探确认 **H1（PrepareMask 预亮脸 ≠ 首句脸 / 产品要空框）** 或等价后使用。

1. **最小改动（F1）**：仅改 `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab`  
   - `NormalDialogueUIAlphaAnimationTaskAction.PrepareMaskAvatarOnFadeIn = **false**`  
   - `MaskAvatarRole` / `MaskAvatarFace` 可保留或清空（以不再生效为准；勿借机改首句 FaceType）  
2. **禁止**改 `Village_KenMuNiStart`；**禁止**为消闪把预亮脸改成 Unhappy 却仍 true（违反「先不显示」）。  
3. **禁止**改 `DialogueMaskAvatarPresenter` 全局逻辑，除非侦探证明必须 F3 且限定 NewGame。  
4. **禁止**回退 0911 大立绘串行淡入。  
5. 施工说明：`Assets/Doc/施工说明/0911/NewGameStory_开局小头像空框取消预亮_施工说明.md`  
6. 验收：  
   - 框淡入过程：**无**小头像  
   - 第一句出现：**直接**正确表情（预扫首句为 Unhappy，以侦探核实为准）  
   - **无** Laugh→Unhappy（或其它错脸）闪一下  
   - 大立绘仍正常；进村 KenMuNiStart 框+头像同拍无回归  

---

## 【验收员】Prompt（施工后 · 预稿）

1. 可加 `[NewGameMaskDebug]`：FadeIn 时是否调用 `PrepareMaskAvatarForFadeIn`；首句 Apply 的 face。  
2. 录屏或逐帧看：框出瞬间 Mask 子 Painting 是否 Active。  
3. 输出验证结果 + 是否误伤村线。

---

## 给用户的一句话

> 很大概率是对话框淡入时 **提前亮了 Laugh 小头像**，第一句才是 Unhappy——所以会闪一下；产品要的是框先空着，等第一句再出脸（跟村长家门口那套一样，不要跟进村「框头同拍」）。
