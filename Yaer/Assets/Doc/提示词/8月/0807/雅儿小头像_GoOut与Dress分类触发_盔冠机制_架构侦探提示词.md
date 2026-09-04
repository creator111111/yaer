# Cursor Agent Prompt · 雅儿小头像机制溯源（GoOut↔Dress / 盔冠头饰 / 原图集→Mask）

> **角色**：【架构侦探】只溯源、不改代码 / Prefab / 图集 / CSV / 台本  
> **日期**：2026-08-07  
> **范围**：对话框左侧雅儿「小头像」——**原来怎么分类、怎么触发**；`GoOutStoryYaerPainting`（外出/铠甲线）vs `YaerPainting`（室内 Dress）何时切；头盔 / 王冠等头饰如何跟；与现网 Mask 是否真有切换机制  
> **本阶段**：追本溯源写清「原机制 → 现网 Mask」对照表；**不施工**  
> **非目标**：不改开场分层间隔；不扩新表情；不重做 Prefab 摆位

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话 · 2026-08-07）

1. Hierarchy 里 `Bottom/Mask/YaerAvatarRoot` 下**同时有**：  
   - `GoOutStoryYaerPainting`（外出/铠甲线，红箭头）  
   - `YaerPainting`（室内 Dress 线，红箭头）  
   以及 Amy / Aliy / Gusha。  
2. 开发者感觉：**现在对话框小头像好像没有机制**说「什么时候切 GoOut、什么时候用 YaerPainting」。  
3. 记得**原来的雅儿小头像机制是有的**：按室内/室外（或服装）分类触发，还有**头盔、王冠**等不同效果。  
4. 要求：**追本溯源**——原来怎么分类、怎么触发；现网 Mask 接了多少、缺了多少。

截图真源（Hierarchy）：`NormalDialogueNewPanel` → `…/Bottom/Mask/YaerAvatarRoot` → 两套雅儿 Painting 并列。

### 两套「小头像」历史（勿混）

| 世代 | 显示物 | 分类真源（助手线索） | 状态 |
|------|--------|----------------------|------|
| **旧图集小头像** | `Bottom/Portrait` + `Avatar_Yaer_*.spriteatlas` | `DialogueAvatarLoader` 读存档 **Clothes + Headwear** 选图集，再按 FaceType 取 Sprite | 现网 `useMaskAvatar=true` 时 Portrait 关；Loader 仍可能服务**历史列表** |
| **Mask 嵌立绘小头像** | Mask 窗内嵌 Painting Prefab | `DialogueMaskAvatarPresenter` 显隐 `GoOutStoryYaerPainting` ↔ `YaerPainting`，再 `UpdateFace` | 0803 接线；0806 称已按存档切 Dress↔GoOut |

生活类比：旧的是「贴纸册按衣服+头饰抽一张贴」；新的是「窗后摆两个全身人偶，只亮一个，再拨脸」。分类维度应同源（衣服骨 + 头饰骨），实现不同。

### 分类维度（原机制应具备 · 须侦探钉死）

| 维度 | 典型取值 | 外出线（GoOut）表现 | 室内线（Dress）表现 |
|------|----------|---------------------|---------------------|
| **衣服 Clothes** | Dress /（非 Dress≈外出铠甲或白裙线） | 启用 `GoOutStoryYaerPainting` | 启用 `YaerPainting` |
| **头饰 Headwear** | None / Crown / ArmorHead（盔） | GoOut：`armorCrown` / `armorHead` 显隐；Face 键现网多为 `Armor_NoHeadWear_*`（须核实是否还有 Crown/盔脸键） | Dress：Faces 多为 `Dress_Crown_*`（王冠是否写死进脸键、有无无冠变体） |
| **表情 FaceType** | 台本枚举 | `Armor_NoHeadWear_{Face}`（Normal→Smile） | `Dress_Crown_{Face}`（Normal→Smile） |

旧图集四套路径线索（技术说明 / 0727）：  
`Avatar/Yaer/{Dress|ArmorNone|ArmorCrown|Armor}/` 或 `Avatar_Yaer_Dress_Crown` 等 atlas——侦探须对照 `DialogueAvatarLoader` **实际选表规则**写成表。

### 现网 Mask 线索（0806 施工后 · 须核实是否真生效）

`DialogueMaskAvatarPresenter` 注释/代码线索：

- `yaerUseGoOutOnly`：**默认 false**；true 时调试强制 GoOut（曾 MVP 写死 true，0806 称已改）。  
- `IsYaerUsingGoOut()`：非强制时读 `PlayerClothesData` Clothes；`Clothes==Dress` → Dress，否则 GoOut。  
- 切 GoOut 时补 `SyncHeadwearFromArchive()`（盔/冠）。  
- Dress Face：Presenter 拼 `Dress_Crown_*`（`YaerPainting.cs` 几乎空壳，键不在子类）。  

**用户怀疑「没有切换机制」的可能含义（并列假说）**：

| ID | 假说 | 若成立则用户体感 |
|----|------|------------------|
| H1 | Prefab 仍序列化 `yaerUseGoOutOnly=true`，代码默认 false 未覆盖 | 永远 GoOut，像「没机制」 |
| H2 | 有机制但真源是**存档 Clothes**，不是「室内场景/对话 Prefab 摆了哪套大立绘」 | 大立绘 Dress、小头像仍 GoOut（或相反）→ 像没按室内外切 |
| H3 | 机制只切衣服套，头饰/盔冠不同步或 Dress 无无冠态 | 「原来有盔冠，现在丢了」 |
| H4 | 旧 Loader 图集规则更细（四套），Mask 只做了两套 Painting | 分类变粗，像机制残了 |
| H5 | 文档写已施工，运行时引用未绑 / Find 失败 / 日志未打 | 资产有、逻辑空转 |

### 本期要回答的核心题（侦探报告必须答）

1. **原来的**雅儿小头像（图集世代）如何按衣服/头饰/表情分类与触发？完整决策表。  
2. **GoOut vs Dress（室外 vs 室内）**在旧系统与 Mask 系统里各自的判定条件是什么？是否等于「场景室内外」还是「存档 Clothes」？  
3. **头盔 / 王冠**在旧系统与 `GoOutStoryYaerPainting.SyncHeadwearFromArchive` / Dress `Dress_Crown_*` 里如何工作？有无缺口。  
4. 现网 Mask **到底有没有**切换机制？若有，触发点在哪一帧（`OnGetNewStatement` / Apply / 存档）？若无或半残，缺哪环。  
5. 大立绘服装与小头像服装是否同真源？不同时以谁为准（只记录 OPEN，不擅自改设计）。

### 范围冻结

- **要**：机制溯源表、调用链、旧→新对照、缺口清单、可选修复方案比选（只建议）  
- **不要**：改代码；改 Prefab；扩表情；改台本；借题改开场分层

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/演出相关/对话立绘与表情系统_技术说明.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md
@Assets/Doc/执行文档/0727/NormalDialogueNewPanel_遮罩立绘搭对话头像_Prefab修改执行说明.md
@Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md
@Assets/Doc/执行文档/0806/雅儿Mask小立绘_室内Dress未启用_架构溯源报告.md
@Assets/Doc/提示词/0806/雅儿Mask小立绘_室内Dress未启用_架构侦探提示词.md
@Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md
@Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueAvatarLoader.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GoOutStoryYaerPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/YaerPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/StoryFormPainting.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab
@Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab
@Assets/Prefabs/DialougeProtrait/YaerPainting.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 对话框 Mask 下同时放了 `GoOutStoryYaerPainting` 和 `YaerPainting`，一个偏室外/外出，一个偏室内 Dress。
2. 开发者怀疑：**现在没有（或看不清）「何时切哪一套」的机制**。
3. 记得原来小头像会按服装分类，还有头盔、王冠等效果——请把**原来的分类与触发**追清楚，再对照现网 Mask 还剩什么。
4. 本期只出溯源报告与缺口；不施工。

---

## 必读 / 优先扫描线索

### A. 旧图集世代（原小头像 · 必先画清）

扫描 `DialogueAvatarLoader`（及调用它的 `DialogueActorEx` / `DialogueTMPUGUI.OnGetAvatar`）：

- [ ] 读哪些存档字段（`PlayerClothesData` / Bone：Clothes、Headwear）  
- [ ] 如何映射到 atlas / 路径（Dress / ArmorNone / ArmorCrown / Armor 等）  
- [ ] FaceType → Sprite 名规则  
- [ ] 沙盒默认（`SandboxDefaultClothes/Headwear`）  
- [ ] 与「室内外场景」有无直接关系（若无，写明：原机制跟**衣服存档**不跟地图名）

输出：**旧系统决策表**（Clothes × Headwear × FaceType → 用哪套图）。

### B. GoOut Painting 世代（外出线 · 盔冠）

扫描 `GoOutStoryYaerPainting`：

- [ ] `SetDefaultPainting` / `SyncHeadwearFromArchive`  
- [ ] `armorHead` / `armorCrown` 显隐条件（ArmorHead / Crown / None）  
- [ ] Face 键是否永远 `Armor_NoHeadWear_*`，戴冠/盔时脸键变不变  
- [ ] 场景大立绘 vs Mask 内嵌实例是否同一脚本

### C. Dress Painting 世代（室内线）

扫描 `YaerPainting` + Prefab `Faces`：

- [ ] 子类是否几乎空壳；变脸键谁拼（Presenter？）  
- [ ] 是否只有 `Dress_Crown_*`；有无无冠 / 其它头饰变体  
- [ ] 与旧 atlas「Dress+Crown」是否一一对应

### D. Mask Presenter 现网（新小头像）

扫描 `DialogueMaskAvatarPresenter` + `NormalDialogueNewPanel.prefab` 序列化：

- [ ] `yaerUseGoOutOnly` **Prefab 实际值**（勿只看代码默认）  
- [ ] `ResolvePainting` / `IsYaerUsingGoOut` / `IsYaerDressFromArchive`  
- [ ] 触发点：`OnGetNewStatement` → `Apply` 时序  
- [ ] 切套装时另一套是否 Active=false；引用拖拽 vs Find 名  
- [ ] 切 GoOut 是否每次 `SyncHeadwearFromArchive`  
- [ ] PrepareMask（UIAlpha 前奏）是否走同一套 Resolve

对照用户假说 H1～H5，逐条 **成立 / 不成立 / 部分成立**。

### E. 大立绘 vs 小头像真源

| 层 | 谁决定穿哪套 | 现网 |
|----|--------------|------|
| 场景大立绘 | 对话 Prefab 嵌哪套 Painting | 须举例 NewGame / 村开场 / 进屋 |
| Mask 小头像 | Presenter / 存档 / 写死？ | |
| 旧 Portrait | Loader+存档 | |

回答：开发者说的「室内/室外」在技术上应映射成 **Clothes==Dress?** 还是 **场景 Prefab 摆了哪套?** 二者冲突时现网听谁。

### F. 旧→新对照总表（报告核心交付）

| 能力 | 旧图集 | Mask 现网 | 缺口 |
|------|--------|-----------|------|
| Dress↔外出切换 | | | |
| 王冠 | | | |
| 头盔 ArmorHead | | | |
| 无头饰 | | | |
| FaceType 变脸 | | | |
| 与大立绘服装一致 | | | |
| 历史列表头像 | | | |

### G. 范围与禁止

- 可建议方案（只比选）：跟存档 / 镜像大立绘 / 对话线写死 / 恢复更细四态等。  
- **禁止**：施工；删旧 Portrait；广扫 Panel；把「室内外」未经拍板改成新真源。

---

## 侦探任务清单

1. **结论一句话**：原来小头像如何分类触发；现网 Mask 有无完整切换；最大缺口是什么。

2. **旧图集决策表**（Clothes × Headwear × Face → 资源）。

3. **Mask 现网决策表**（含 `yaerUseGoOutOnly` Prefab 实值）。

4. **盔 / 冠机制**专节（GoOut 显隐 vs Dress 脸键写死 Crown）。

5. **假说 H1～H5** 判定表。

6. **旧→新对照总表** + 调用链（两代并列）。

7. **方案比选**（只建议，推荐 1 个）— 若用户要「跟室内外大立绘一致」与「跟存档」冲突，明确需拍板。

8. **开放问题**追加 OPEN（「雅儿小头像分类触发机制溯源 · 2026-08-07」）：  
   - 小头像服装真源：存档 vs 镜像场景大立绘？  
   - Dress 是否长期只有 Crown 脸键？  
   - GoOut 戴冠时是否应改 Face 前缀，还是只显隐 crown 物体？  
   - 旧四套 atlas 是否还需与 Mask 一一对应？

9. **禁止**：改代码；写「机制很简单」却不出决策表；把 0806 报告当唯一真相不复核 Prefab 序列化。

---

## 输出要求

写入：`Assets/Doc/执行文档/0807/雅儿小头像_GoOut与Dress分类触发_盔冠机制_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：贴纸册 vs 窗后人偶）  
③ 用户需要做什么（拍板真源 + 验收时看哪套亮）  
④ 给程序看的补充：旧决策表、Mask 决策表、盔冠专节、H1～H5、对照总表、方案、OPEN

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认：① 服装真源（存档 vs 镜像大立绘）② 盔冠在 Dress/GoOut 的规则 ③ 是否要补回旧图集级细态 后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/技术文档/演出相关/对话立绘与表情系统_技术说明.md
@Assets/Doc/执行文档/0807/雅儿小头像_GoOut与Dress分类触发_盔冠机制_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做最小化修改，补齐雅儿 Mask 小头像在 GoOut↔Dress 与头饰（盔/冠）上的分类触发，使其行为对齐报告认定的原机制真源。
禁止在 Update 堆补丁；禁止名字广扫 Panel。每次说明：改了哪些文件、切换条件、如何验证室内/外出与盔冠。
```
