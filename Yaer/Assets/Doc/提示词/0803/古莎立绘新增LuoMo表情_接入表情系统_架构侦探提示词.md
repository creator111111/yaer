# Cursor Agent Prompt · 古莎立绘新增 LuoMo 表情 · 接入表情系统

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-03  
> **范围**：古莎（Gusha / GuSha）对话表情链路中，把新增的 **LuoMo** 接到可调用状态；雅儿 Happy / Mask 接线仅作对照，**本期不改雅儿、不施工 Mask 启用**  
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

1. 开发者已在 **`GushaPainting`** 的 `Faces` 下**新增**子物体：
   - 节点名：`LuoMo`（英文拼音，未用中文）
   - 位置：Hierarchy → `GushaPainting` → `Faces` → `LuoMo`（与 Angry / Smile / Happy 等同级）
   - Prefab：`Assets/Prefabs/DialougeProtrait/GushaPainting.prefab`
2. **目标**：对话台本 / NodeCanvas 里选表情 **`LuoMo`** 时，古莎大立绘能切到这张脸；小头像若同链路也要能显示（侦探须写清是否同链路、缺口在哪）。
3. **与雅儿 Happy 的关键差异（侦探必须核实）**：
   - 雅儿 `Happy`：**枚举 `DialogueFaceType` 里原本就有**；GoOut 键为 `Armor_NoHeadWear_Happy`。
   - 古莎 `LuoMo`：从现网枚举列表看**很可能尚不存在**（侦探打开 `DialogueFaceType.cs` 确认）。若枚举没有，则 **SayEx / CSV 无法选 LuoMo**，仅加 Prefab 节点不够。
   - 古莎大立绘 Faces 子物体名 = **裸枚举名**（如 `Smile`），基类 `StoryFormPainting.UpdateFace(faceType.ToString())`；**没有** `Armor_NoHeadWear_` 前缀。
4. **古莎特殊服装层**：
   - `GuShaPainting`：`Awkward` / `Cry` / `Daze` / `Sad` 会切 `clothes_other`（为难体）。
   - 侦探须问清并写入开放问题：**LuoMo 是否也要切为难体？**（开发者未说明则标为待拍板，方案里给两种改法。）
5. 对照文档：
   - `Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`（§4.1 古莎）
   - `Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md`（结构可对照，角色不同）
   - `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GuShaPainting.cs`
   - `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs`

---

## 必读 / 优先扫描线索

### A. 枚举与台本入口（本期最可能卡点）
- `DialogueFaceType`：有无 `LuoMo`；若无，新增枚举的序列化/已有存档/对话 Prefab 风险（追加在末尾是否相对安全）
- `StatementNodeEx.FaceType`、`SayEx` 下拉来源
- CSV：`DialogueCsvParser` 非法 FaceType 报错；`DialogueFaceTypeCsvDefaults` 是否与古莎相关
- 台本调用名约定：是否就叫 **`LuoMo`**（与节点名一致）

### B. 大立绘（主对象）
- `StoryFormPainting.UpdateFace`：按子物体 name 字典切换
- `GuShaPainting.UpdateFace`：`spcFaces` 列表与 `clothes_normal` / `clothes_other`
- Prefab：确认 `Faces/LuoMo` 存在、Image 是否已绑图、默认 Active
- 对话场景里嵌套的 `GushaPainting` 实例是否跟母 Prefab；`NormalDialogueNewPanel` 内 Mask 下那份是否另案（**本期只写清：Mask 未接线则跟台本无关**）

### C. 小头像（并列表，勿与大立绘混谈）
- `DialogueAvatarLoader` → 古莎图集路径（预期 `Avatar_Gusha.spriteatlas`）
- 源图：`Assets/ArtRes/UI/Story/DialogueForm/Gusha/Avatar/` 是否已有 / 需要 `LuoMo.png`
- 缺图时行为

### D. 文档缺口
- 0601 §4.1 古莎表情表是否需补 `LuoMo`（记入施工建议）

---

## 侦探任务清单

1. **钉死：仅凭现有 `Faces/LuoMo` 节点，台本选 LuoMo 能否工作？**  
   - 枚举有无 → SayEx 能否选 → 事件能否传到 GuShaPainting → 键名是否为 `LuoMo` → 为难体要不要进 spcFaces。  
   - 给出「零代码 / 只加枚举 / 枚举+spcFaces / 还要补图集」四档结论之一（或组合）。

2. **盘点古莎「LuoMo 全链路」缺口表**（必出表）  

   | 环节 | 现状（有/无/未知） | 期望 | 是否必须改代码 | 备注 |
   |------|-------------------|------|----------------|------|
   | DialogueFaceType.LuoMo | | 可在 SayEx/CSV 选用 | | 重点核实 |
   | GushaPainting Faces 节点 | | LuoMo | | 开发者已加，核实绑图 |
   | GuShaPainting.spcFaces | | 是否纳入为难体 | | 待拍板 |
   | Avatar_Gusha + LuoMo.png | | 小头像同步 | | |
   | 历史头像 | | 同源 Loader | | |
   | Mask 内 GushaPainting | | 是否跟脸 | | 多半未接线，写清 |
   | 0601 手册 | | 补表 | | |

3. **明确「对话要调用 LuoMo」的最小闭环**  
   - 只保证场景大立绘：最小步骤？  
   - 若还要字幕条小头像：额外步骤？  
   - 列出施工员下一轮最小化文件清单（只建议）。

4. **与雅儿 Happy 对照一行**（避免施工员抄错键规则）  
   - 雅儿：`Armor_NoHeadWear_{faceType}`；古莎：`faceType.ToString()`。

5. **验收方法**（检查清单）  
   - DialogDebug / 哪条含古莎对话、SayEx 选 LuoMo、Inspector 应亮 `Faces/LuoMo`、为难体两套衣服谁亮。

6. **开放问题**追加 `Assets/Doc/OPEN_QUESTIONS.md`（仅 LuoMo 相关）：  
   - LuoMo 中文含义/策划名？  
   - 是否切 `clothes_other`？  
   - 小头像 / Mask 是否本期必做？

7. **禁止**：改资产；把 Prefab 节点写成「已可调用」若枚举缺失；建议把节点改成中文名；扩成全角色改造。

---

## 输出要求

写入：`Assets/Doc/执行文档/0803/古莎立绘新增LuoMo表情_接入表情系统_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（枚举缺不缺、大立绘差哪一步才能调 LuoMo）  
② 原因（生活类比 + 脚本/Prefab/图集锚点）  
③ 用户需要做什么（拍板：为难体？小头像？+ 验收清单）  
④ 给程序看的补充：
   - LuoMo 调用链
   - 缺口表
   - 施工员最小改动建议（分：仅大立绘 / 大立绘+小头像；spcFaces 两种分支）
   - 与雅儿 Happy 差异一行
   - 开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认枚举 / spcFaces / 小头像范围后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0803/古莎立绘新增LuoMo表情_接入表情系统_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使古莎对话 FaceType=`LuoMo` 可正确显示。
优先保持裸枚举名 = Faces 子物体名契约；禁止改 Faces 节点为中文；禁止在 Update 堆业务。
若需新增 DialogueFaceType.LuoMo，追加在枚举末尾并说明序列化影响。
每次提交说明：改了哪些文件、实现了什么、如何验证。
```
