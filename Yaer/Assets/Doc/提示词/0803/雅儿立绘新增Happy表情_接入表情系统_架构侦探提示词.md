# Cursor Agent Prompt · 雅儿立绘新增 Happy 表情 · 接入表情系统

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-03  
> **范围**：雅儿（Yaer）对话表情链路中，把新增的 **Happy** 接到可调用状态；古莎仅作「Happy 已有先例」对照，**本期不改古莎**  
> **本阶段**：只摸清缺口与改动面，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话，已对齐截图）

1. 开发者已在 **`GoOutStoryYaerPainting`** 的 `Faces` 下**新增**子物体：
   - 节点名：`Armor_NoHeadWear_Happy`（英文，未改中文）
   - 位置：Hierarchy → `GoOutStoryYaerPainting` → `Faces` → `Armor_NoHeadWear_Happy`
2. **目标**：对话台本 / NodeCanvas 里选表情 **`Happy`** 时，雅儿大立绘能切到这张脸；小头像若同链路也要能显示 Happy（侦探须写清是否同链路、缺口在哪）。
3. **契约已知（以代码复核为准，勿当唯一真相）**：
   - 表情枚举：`DialogueFaceType`（含 `Happy`）
   - 村线 GoOut 大立绘键：`Armor_NoHeadWear_{faceType}`（见 `GoOutStoryYaerPainting.ResolveGoOutFaceKey`）
   - 小头像：图集子图名 = 枚举英文名（见 `DialogueAvatarLoader`）
   - 历史手册：`Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`
   - 雅儿换图手册：`Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md`
   - 表情总链路溯源：`Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
4. **已知风险（侦探必须核实）**：
   - 0601 文档里雅儿每套小头像图集原先只有 **9 表情，不含 Happy**；Happy 多用于古莎/夏尔。
   - 枚举里已有 `Happy`，不等于雅儿资源/Prefab 已齐。
   - `YaerPainting`（连衣裙）Faces 键可能是 `Dress_Crown_*`；仅加 GoOut 节点**不会**覆盖新游戏线立绘。
   - `NormalDialogueNewPanel` 内 Mask 嵌立绘变脸接线可能仍是「下一轮」——侦探须写清本期「对话调用 Happy」到底覆盖大立绘 / 小头像 / Mask 头像哪几条。

---

## 必读 / 优先扫描线索

### A. 枚举与台本入口
- `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs` — `Happy` 是否已存在、序号是否影响序列化
- `StatementNodeEx.FaceType`、`SayEx`、CSV 第 7 列 / `DialogueFaceTypeCsvDefaults`
- CSV / 导入校验：`DialogueCsvParser` 对未知 FaceType 的行为（Happy 已在枚举则应可过）

### B. 大立绘（本次主对象之一）
- `StoryFormPainting.UpdateFace`：按子物体 **name** 字典切换
- `GoOutStoryYaerPainting`：`ResolveGoOutFaceKey` → 期望键是否为 `Armor_NoHeadWear_Happy`
- Prefab：`Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` — 确认 `Faces` 下是否已有 `Armor_NoHeadWear_Happy`、Image 是否绑图
- 对照：`YaerPainting` / `NewGameYaerPainting` 是否也需要 `Dress_Crown_Happy` 才算「全线可调」
- 古莎对照（只读）：`GuShaPainting` 用裸枚举名 `Happy`，**不改**

### C. 小头像（必须并列表，勿与大立绘混谈）
- `DialogueAvatarLoader` / `DialogueAvatarPathHelper`
- 四套雅儿图集：`Assets/GameRes/Atlas/Avatar/Avatar_Yaer_*.spriteatlas`
- 源图：`Assets/ArtRes/UI/Story/DialogueForm/Yaer/Avatar/{Dress|ArmorNone|ArmorCrown|Armor}/` 是否已有 `Happy.png`
- 缺图时行为：字幕条 Portrait 隐藏还是空白

### D. UI / Mask 头像（写清是否本期范围）
- `NormalDialogueNewPanel` Mask + `YaerAvatarRoot` 内嵌立绘是否已接 FaceType
- 技术说明：`Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`

### E. 文档与策划手册缺口
- 0601 / 0614 中雅儿表情表是否需补 Happy（只记入报告「施工建议」，本阶段不改文档也可，但要列出）

---

## 侦探任务清单

1. **钉死：台本选 `Happy` 后，GoOut 大立绘是否已能工作**  
   - 仅凭现有 `Armor_NoHeadWear_Happy` 节点 + `ResolveGoOutFaceKey`，是否**零代码**即可切换？  
   - 若不能，卡在哪一层（事件未订、键名不一致、Awake 字典未收录、默认脸覆盖等）？

2. **盘点雅儿「Happy 全链路」缺口表**（必出表）  

   | 环节 | 现状（有/无/未知） | 期望 | 是否必须改代码 | 备注 |
   |------|-------------------|------|----------------|------|
   | DialogueFaceType.Happy | | 可在 SayEx/CSV 选用 | | |
   | GoOut Faces 节点 | | Armor_NoHeadWear_Happy | | 开发者已加，核实 |
   | YaerPainting Faces | | Dress_Crown_Happy？ | | |
   | 四套 Avatar PNG + 图集 | | Happy 子图 | | |
   | 历史头像 | | 同源 Loader | | |
   | Mask 内嵌立绘 | | 是否跟脸 | | |
   | 0601/0614 手册 | | 补表 | | |

3. **明确「对话要调用 Happy」的最小闭环**  
   - 只保证村线大立绘：最小步骤是什么？  
   - 若还要字幕条小头像同步：额外步骤是什么？  
   - 列出**施工员下一轮**的最小化文件清单（只建议，不施工）。

4. **验收方法**（给用户检查清单）  
   - DialogDebug 或哪条村线对话、Inspector 看哪几个节点、SayEx 选 Happy 后应亮哪张脸、小头像如何对照。

5. **开放问题写入** `Assets/Doc/OPEN_QUESTIONS.md`（仅追加与本次 Happy 相关条目；不要擅自改核心设计）。  
   例如：连衣裙线是否本期必做；小头像四套是否必须同步上 Happy；Mask 头像是否另开轮。

6. **禁止事项**  
   - 不改代码 / Prefab / 图集 / 台本；  
   - 不扩成全角色表情改造；  
   - 不把「大立绘已加节点」写成「小头像也自动好了」；  
   - 不建议为了中文可读性改 Faces 节点名为中文。

---

## 输出要求

写入：`Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（GoOut 是否已可零代码调用 / 还缺什么）  
② 原因（生活类比 + 技术锚点：脚本 / Prefab 节点 / 图集路径）  
③ 用户需要做什么（检查清单）  
④ 给程序看的补充：
   - Happy 调用链（台本 → Actor 事件 → UpdateFace 键）
   - 缺口表（任务 2）
   - 施工员最小改动建议清单（分：仅大立绘 / 大立绘+小头像）
   - 开放问题

完成后用 MASTER 固定四段式口头汇报结论；详细内容以报告文件为准。
```

---

## 施工员续跑（侦探报告通过后再开）

> 本阶段先不要贴。等溯源确认「零代码即可 / 还需改哪些」后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使雅儿对话 FaceType=`Happy` 可正确显示。
优先保持现有 FaceType 契约与 GoOut `Armor_NoHeadWear_{faceType}` 键规则；禁止改 Faces 节点名为中文；禁止在 Update 堆业务。
每次提交说明：改了哪些文件、实现了什么、如何验证。
```
