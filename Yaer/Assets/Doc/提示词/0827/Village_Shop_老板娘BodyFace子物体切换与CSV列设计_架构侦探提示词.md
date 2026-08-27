# Cursor Agent Prompt · Village_Shop：老板娘 Body×Face 子物体切换 + CSV 列设计（Extra vs BodyType）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-27（第二轮 · 用户已改 Hierarchy + 已编 CSV 草案）  
> **场景**：`Village_Shop.unity` → `商店界面合层`  
> **用户 Hierarchy（截图 · 已施工）**：
> ```
> 商店界面合层
>   ├── 背景
>   ├── Body          ← 3 身互斥 Active
>   │     ├── Normal
>   │     ├── Red
>   │     └── YinXian
>   └── Face          ← 5 脸互斥 Active
>         ├── Face1 … Face5
> ```
> **用户 CSV 草案（截图）**：列含 `Extra` / `FaceType`；**店** 行 `Extra=Red`，`FaceType=Face1|Face2`；**雅/古** 行仍用 `Laugh`/`Cry` 等通用脸  
> **核心问题**：Body 写 **Extra** 还是新建 **BodyType** 列？表情系统改为 **子物体 SetActive 切换**（≠ 雅儿 Painting / ≠ 上轮 Sprite 换图）  
> **本阶段**：只读；禁止改代码 / CSV / Prefab / 场景

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话 · 本轮聚焦）

1. 首次进店对白（及后续商店对白）播放时，**老板娘**要随句切换 **3 种身体 × 5 种表情** 的组合。  
2. 实现方式已拍板（用户侧）：**不是**换 Sprite、**不是** `StoryFormPainting.UpdateFace`，而是 **`Body` / `Face` 下子物体互斥 `SetActive(true/false)`**。  
3. 策划已在 CSV 里试填：部分 **店** 行 `Extra` 填 `Red` 表示脸红身；`FaceType` 填 `Face1`/`Face2`。  
4. 侦探须裁定：**CSV 列怎么设计才不乱**、**导入器怎么改最小**、**运行时谁订阅对白事件去切子物体**。

### 与上轮报告的差异（必写进新报告）

| 项 | 上轮报告 v1（0827） | 用户现网（本轮） |
|----|---------------------|------------------|
| 换脸方式 | 单槽 `SpriteRenderer` 换 `表情1` 图 | **`Face/Face1～5` 子 GO 互斥 Active** |
| 换身方式 | 单槽 `正常体` SR 换图 | **`Body/Normal|Red|YinXian` 互斥 Active** |
| 已有代码 | `ShopkeeperFaceController`（SR 换图） | **须评估重构为 Toggle 模式** |
| CSV | 未接对话 | 用户已填 `Extra=Red` + `FaceType=Face1` |

### CSV 列语义 · 现网导入器（磁盘预扫）

`DialogueCsvParser` / `DialogueCsvGraphBuilder` 当前契约：

| 列 | Dialogue 行 | Choice 行 | Anim 行 |
|----|-------------|-----------|---------|
| **Extra** | **导入时忽略**（不写入节点） | **必填**：选项文案 `\|` 分隔 | **必填**：动画键 `Anim_*` |
| **FaceType** | 写入 `StatementNodeEx.FaceType`（`DialogueFaceType` 枚举） | 忽略 | 忽略 |

**FaceType 校验**：非空时必须能 `Enum.TryParse<DialogueFaceType>` —— **`Face1` / `Face2` 不在枚举内，导入会失败**（用户 CSV 已踩雷）。

**Extra 放 Body 的风险**：

| 方案 | 说明 | 预判 |
|------|------|------|
| **A · Dialogue 行复用 Extra 存 Body** | 如 `Red` / `Normal` / `YinXian` | 与 Choice/Anim 的 Extra **同名不同义**；Dialogue 行目前**不落盘**，须改 GraphBuilder + 运行时读取；策划易混 |
| **B · 新建 `BodyType` 列（推荐倾向）** | 与 `FaceType` 并列；仅 **店** 行填写 | 语义清晰；`DialogueCsvColumnMap` 已支持按表头加可选列（同 `English`/`Voice`）；空=保持上一句/默认 Normal |
| **C · 合并到 FaceType 一列** | 如 `Red|Face2` | ❌ 与雅/古 FaceType 混用，解析脆 |

**Face 列设计（与 Body 分开裁定）**：

| 方案 | 说明 | 预判 |
|------|------|------|
| **F1 · FaceType 按 Speaker 分流校验** | `店`→`ShopkeeperFaceType`（Face1～5）；其它→`DialogueFaceType` | 用户已在用 `FaceType=Face1`；改校验即可，**不必**扩 `DialogueFaceType` |
| **F2 · 新建 `ShopFace` 列** | 店专用；FaceType 仍给雅/古 | 表更宽，但零歧义 |
| **F3 · 把 Face1～5 追加进 `DialogueFaceType`** | 枚举污染 | 上轮已否决 |

### Hierarchy ↔ 枚举映射（用户命名 · 侦探核对场景 YAML）

| 用户 GO 名 | 建议枚举 | 旧 v1 名 / 源图 |
|------------|----------|-----------------|
| `Body/Normal` | `ShopkeeperBodyType.Normal` | 正常体 |
| `Body/Red` | `ShopkeeperBodyType.Blush` 或 alias `Red` | 脸红体 |
| `Body/YinXian` | `ShopkeeperBodyType.Sinister` | 阴险体 |
| `Face/Face1`～`Face5` | `ShopkeeperFaceType.Face1`～`Face5` | 表情1～5 |

CSV 填法建议（报告须拍板）：

```
BodyType: Normal | Red | YinXian     （空 = Normal 或继承上一句）
FaceType: Face1 | Face2 | …         （仅 Speaker=店 时）
FaceType: Laugh | Cry | …           （Speaker=雅/古 时，走原链）
```

### 运行时桥接（预设计 · 本期侦探只出方案）

```
CSV Import
  → StatementNodeEx（+ 是否需扩展 BB：ShopBody / ShopFace？）
Play SayEx
  → SubtitlesRequestInfoEx（现仅有 DialogueFaceType）
  → 若 Actor=老板娘：
       ShopkeeperFaceController.SetBody + SetFace（Toggle 子 GO）
     否则：
       DialogueMaskAvatarPresenter（原链，不动）
```

**Speaker**：默认映射表 **无 `店`**（`DialogueSpeakerMapping.CreateDefaultInstance`）；0601 台本写 `店→老板娘`，侦探须查是否已有 SO / Prefab Actor。

### 与通用立绘系统的边界（钉死）

- **雅 / 古 / NPC**：仍走 `DialogueFaceType` + Mask / Painting；**BodyType 列留空**。  
- **店（老板娘）**：**只**切 `商店界面合层` 下 Body/Face 子物体；**不走**小头像图集、**不走** `StoryFormPainting`。  
- 同屏：首次进店可能是「左侧雅/古 Mask 立绘 + 右侧合层老板娘 Body/Face」——侦探须写清 **是否同句双轨并行**、会不会互相 `SetActive` 干扰。

### 须比较的施工方向

| ID | 内容 | 说明 |
|----|------|------|
| **T1** | 重构 `ShopkeeperFaceController` → Toggle 模式 | 缓存 `Body`/`Face` 下子 Transform；`SetBody/SetFace` 互斥 Active |
| **T2** | CSV：`BodyType` 新列 + Speaker 分流 FaceType 校验 | 最小改 Parser/GraphBuilder/Row |
| **T3** | 运行时：`ShopkeeperDialogueBridge` 订 `OnGetNewStatement` | 判 Actor 名 / Speaker 映射 |
| **T4** | 扩展 `StatementNodeEx` / `SubtitlesRequestInfoEx` 携带 Body | 若不用 BB，Bridge 只能读 CSV 重建的节点字段 |
| **T5** | 废弃 SR 换图路径 | v1 已写代码若存在，报告写迁移步骤 |

### 严禁（本阶段）

- 直接改用户 CSV 或强行 Import 失败表  
- 把 `Face1` 塞进 `DialogueFaceType` 枚举（除非报告论证无替代且用户拍板）  
- 用 Choice 行的 Extra 语义去解释 Dialogue 行的 Body  
- 把 Body/Face Toggle 逻辑写进 `Update` 轮询  
- 本期做首次进店存档旗标 / 藏 UI / 黑屏（仍属下期）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘表情变化系统_架构溯源报告.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Doc/提示词/0827/Village_Shop_老板娘表情变化系统_架构侦探提示词.md
@Assets/Editor/Tool/Dialogue/DialogueCsvParser.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Editor/Tool/Dialogue/DialogueRow.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/StatementNodeEx.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/SubtitlesRequestInfoEx.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceType.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperBodyType.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceRegistry.cs
@Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/ArtRes/Scene/Village/商店界面合层.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、CSV、Prefab、场景。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

用户已把 `商店界面合层` 改成 **Body(3) + Face(5)** 子物体结构，打算用 **开关子物体** 做表情组合。  
编 CSV 时发现：**身体变体放 Extra 还是新建 BodyType？** `FaceType` 里填 `Face1` 会不会和雅儿 `Laugh` 冲突？  
请给出 **CSV 规范 + 导入器改动面 + Toggle 控制器重构 + 对白桥接** 的最小方案。

---

## 侦探任务清单

### A. 核实现网 Hierarchy（场景已保存）

| 项 | 填 |
|----|-----|
| `Body` / `Face` 父节点路径 | |
| 子 GO 名是否与 Normal/Red/YinXian、Face1～5 一致 | |
| 各子节点组件（SR / 空 GO / SortingGroup） | |
| `ShopkeeperFaceController` 是否仍绑旧 `正常体`/`表情1` | |
| 默认 Play 哪几个 Active | |

### B. CSV 列设计裁定（核心 · 必出对比表）

对 **Extra vs BodyType** 与 **FaceType 分流 vs ShopFace 新列** 做决策表：

| 维度 | 方案 A（Extra 存 Body） | 方案 B（BodyType 列） | 方案 C（其它） |
|------|-------------------------|----------------------|----------------|
| 策划可读性 | | | |
| 与 Choice/Anim Extra 冲突 | | | |
| Parser/GraphBuilder 改动量 | | | |
| 运行时取 Body 难度 | | | |
| 推荐 | | | **拍板** |

**输出「策划填表示例」**（3～5 行）：含 **店**（Red+Face2）、**雅**（空 Body + Laugh）、**古**（空 Body + Cry）。

### C. FaceType 导入冲突（必验证）

1. 用户 CSV 中 `Face1`/`Face2` —— 现网 `DialogueCsvParser.Validate` 是否 **必失败**？  
2. 若 Speaker=店 时允许 `ShopkeeperFaceType` 字符串，校验规则怎么写？  
3. `StatementNodeEx` 只有 `DialogueFaceType` BB —— Body/Face 店专用数据 **存哪**（新 BB / 自定义节点字段 / 并行 Dictionary）？  
4. 是否扩展 `SubtitlesRequestInfoEx` 携带 `ShopkeeperBodyType` + `ShopkeeperFaceType`？

### D. Toggle 控制器设计（替代 v1 SR 换图）

1. API：`SetBody` / `SetFace` / `Apply( body, face )` / `ResetDefault`  
2. 内部：遍历 `Body`、`Face` 子节点互斥 Active；**SortingOrder / 层级** 是否需额外处理  
3. 与 v1 `ShopkeeperFaceController` 差异：迁移清单（删 SR 引用 / 改 Editor Setup）  
4. 空列继承：Body 或 Face CSV 为空时 **保持上一句** 还是 **回默认**？

### E. 对白桥接链路

1. `DialogueSpeakerMapping` 是否含 **`店`→?**；DialogueTree Actor 名是什么  
2. 订阅点：`DialogueTMPUGUI.OnGetNewStatement` 还是 SayEx 内扩展  
3. 判定老板娘句的条件：Actor 名 / Role 枚举 / Speaker 映射  
4. 同句雅/古 + 店 是否可能同时发言（一般不会）—— 仅店句时切 Body/Face  
5. 首次 `Start` / 默认脸竞态：会否像 GoOut `SetDefaultPainting` 盖掉 CSV 脸（写风险）

### F. 最小施工清单（给施工员 · 本阶段不执行）

| # | 模块 | 动作 | 依赖 |
|---|------|------|------|
| 1 | CSV 规范 + 样例表 | 拍板 BodyType 列名与合法值 | B |
| 2 | Parser + GraphBuilder + Row | 读 BodyType；店 FaceType 分流校验 | B,C |
| 3 | Statement / Subtitles 扩展 | 携带 Shop Body+Face | C |
| 4 | ShopkeeperFaceController | Toggle 重构 | D |
| 5 | ShopkeeperDialogueBridge | 运行时订阅 | E |
| 6 | Speaker 映射 + 用户 CSV 复验 Import | 店/雅/古 | E |

**排除**：首次进店存档、藏 UI、黑屏。

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | CSV Import 用户表（或侦探用样例） | 无 Face1 校验错误；BodyType 落盘 |
| 2 | Play 首次进店对白（或 DialogDebug 绑树） | 店句切换 Body+Face；雅/古仍走 Mask |
| 3 | 仅改 Face、Body 空 | 身保持 / 回默认（按拍板） |
| 4 | `Extra=Red` 行（若改 BodyType 后） | 脸红身 Active |
| 5 | Debug 键 / 原 Debug 脚本 | Toggle 模式仍可调 3×5 |
| 6 | Console | 无 Missing GO / 重复 Active |

### H. 开放问题

- `Red` vs 枚举名 `Blush` CSV 用哪个对外？  
- `FaceType` 店行是否 **禁止** 填 Laugh（防误用）？  
- 用户 CSV 文件路径与 Import 窗口用的 Speaker SO 是否已配 `店`？  
- Body+Face 组合是否有策划禁止项（如 YinXian+Face1）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md`

结构（MASTER 四段式）：

① **结论一句话**（Extra 还是 BodyType + Toggle 怎么接对白）  
② **原因**（Extra 在 Choice/Anim 已有主职；Face1 过不了现网校验）  
③ **用户检查清单**（CSV 怎么填、Import 前配什么）  
④ **给程序**：列设计表 + Toggle API + 导入/运行时改动文件清单 + 与 v1 报告 diff

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Editor/Tool/Dialogue/
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【施工员】。按报告实现 Body×Face Toggle + CSV 列 + 对白桥接。

必须遵守：
- Body/Face 用子物体互斥 SetActive，禁止回退 SR 单槽换图（除非报告保留双模式）；
- CSV 列设计按报告拍板（倾向 BodyType 新列，Extra 不承载 Dialogue Body）；
- FaceType=Face1～5 仅 Speaker=店 时合法；雅/古仍走 DialogueFaceType；
- 扩展导入与运行时最小 diff；代码含详细注释；
- 改完帮用户 CSV 走一遍 Import 冒烟；不做首次进店存档/藏 UI。

提交说明：CSV 填法示例、Import 结果、Play 切脸截图描述、与 v1 Controller 差异。
```
