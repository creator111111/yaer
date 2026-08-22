# Cursor Agent Prompt · Village_HomeScene45：隔断墙靠近触发半透明

> **角色**：先【架构侦探】对拍现网与可复用组件，报告拍板后【施工员】实现  
> **日期**：2026-08-22  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
> **目标物体**：`Object/隔断墙`（开发者 Hierarchy 选中；**非** `Map/Design/村民家3合层` 内同名装饰）  
> **需求（开发者）**：隔断墙**本身不做物理阻挡**（无实心碰撞）；另加 **Trigger 碰撞体**；玩家**靠近**后墙变 **半透明**；离开后恢复不透明。  
> **本阶段侦探**：只读、不改场景 / 代码 / Prefab

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 隔断墙加一个碰撞体触发器，墙本身没有碰撞。玩家靠近了之后变成半透明视觉效果。

拆解：

| # | 要求 | 说明 |
|---|------|------|
| 1 | **无物理挡人** | 不加非 Trigger 的 Collider；玩家可穿过墙所在区域（靠 Sorting 挡视线，不靠碰撞） |
| 2 | **有感应区** | 单独 `BoxCollider2D`（或 Polygon2D）**Is Trigger = true** |
| 3 | **靠近 → 半透明** | 改 `SpriteRenderer.color.a`（或子 Renderer 列表） |
| 4 | **离开 → 恢复** | `OnTriggerExit2D` 还原 alpha |
| 5 | **仅 45 号屋本期** | 先服务 `Village_HomeScene45`；若做通用脚本可复用到其它室内前景 |

**本期不涉及**：`SceneEntity` / GSM 登记 / 对话 / 换场（**不用改** `Village_HomeScene45SceneManager`）。

### 现场 Hierarchy（开发者截图）

```
Object/
  NPC45
  面包 / 饼干        ← Item 预制体
  隔断墙             ← 本期目标（交互半透明）
Map/Design/村民家3合层/
  … 隔断墙 …         ← 美术合层里的同名装饰，勿绑 Trigger（避免双份逻辑）
```

### 现网磁盘预扫（`Village_HomeScene45.unity`）

| 项 | `Object/隔断墙` 现网 |
|----|---------------------|
| 组件 | 仅 **Transform + SpriteRenderer** |
| Collider | **无** |
| Layer | `0`（Default） |
| SortingOrder | `5` |
| Color alpha | `1` |
| LocalPosition | 约 `(-9.49, 1.11, 3.75)`（Z 用于前后排序） |

结论：符合「纯贴图挡视线、无碰撞」；缺 Trigger + 半透明逻辑。

### 工程内可复用线索（侦探须对拍是否够用）

| 现网组件 | 路径 | 与本期关系 |
|----------|------|------------|
| `ActivateChildOnPlayerFootTrigger` | `Entities/SceneEntities/CommonEntity/` | **可参考**：`OnTriggerEnter2D/Exit2D` + 识别 **`PlayerFoot`** 物体名；现网是 `SetActive`，需改为 **改 alpha** |
| `VillageSceneObjectDepthSort` | `Entities/Component/Physics/` | **不是同一需求**：切 Sorting Layer 做 Y 遮挡，**不改透明度** |
| `VillagePlayerDepthZone` | 同上 | 村庄纵深区，无关室内前景 fade |
| UI `CanvasGroup` / 对话 Fade | Story/UI | 不适用 Sprite 场景物件 |

**预扫结论**：**无现成「靠近半透明」脚本**；最近似 `ActivateChildOnPlayerFootTrigger`，但需新脚本或泛化。

### 玩家检测约定（须与现网一致）

- 玩家脚底碰撞体物体名：**`PlayerFoot`**（Layer 通常为 PlayerFoot）
- Trigger 回调里过滤：`other.gameObject.name == "PlayerFoot"` 或 `CompareTag`（侦探查 Player 预制体实际配置）
- 玩家根上有 **Rigidbody2D** → Trigger 可工作；隔断墙侧 **不需** Rigidbody（Kinematic 规则按 Unity 2D 文档核实）

### 生活类比

隔断墙是「毛玻璃屏风」——人走得过去，但挡视线。现在在屏风前划一块感应区：人一脚迈进感应区，玻璃变透；退出去又变实。

### 侦探须比较的方案

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A（推荐候选）** | 新建 `SpriteFadeOnPlayerFootTrigger`（或放 `SceneEntities/CommonEntity`）：序列化 `SpriteRenderer[]`、正常/靠近 alpha、可选 lerp 时长；子物体 `ProximityTrigger` 挂 BoxCollider2D Trigger | 与 `ActivateChildOnPlayerFootTrigger` 同风格；可复用；场景只挂组件 | 需定 alpha 默认值、Trigger 尺寸 |
| B | 仅场景挂空脚本 + 内联逻辑 | 最快 | 不可复用；违架构 |
| C | 用 `VillageSceneObjectDepthSort` 凑合 | 已有 | **不满足半透明**；否决 |
| D | 改 Shader/Material 专用透明 | 效果可控 | 改动面大；本期过度 |

Trigger 放哪：

| 子方案 | 说明 |
|--------|------|
| D1 | **子物体** `ProximityTrigger`（推荐）：Collider 可略大于墙图，不挡射线误触 |
| D2 | 与 Sprite 同 GO 挂 Trigger | 简单但 Collider 与图绑死 |

### 美术层去重

`村民家3合层` 内若仍有 **隔断墙** 贴图：

- 交互半透明只驱动 **`Object/隔断墙`** 的 Renderer  
- 合层内同名建议 **隐藏 SpriteRenderer** 或删子节点，避免 **叠两张墙**（侦探现场对拍）

### 严禁

- 给隔断墙加 **非 Trigger** 碰撞挡人（除非产品改口）  
- 把 Trigger 挂在 `Map/Design` 合层上导致预制体改动面过大  
- 用 `Update` 每帧 `Find("Player")`（应用 Enter/Exit 或缓存 PlayerFoot）  
- 误用 `SceneEntity` / 登记 GSM（纯视觉）  
- 与 `VillageSceneObjectDepthSort` 同物体双开导致排序打架（若墙需保留高 SortingOrder，本脚本 **只改 alpha 不改 Layer**）

### 开放参数（侦探写入报告，施工用默认值）

| 参数 | 建议默认 | 说明 |
|------|----------|------|
| `normalAlpha` | `1` | 远离时 |
| `nearAlpha` | `0.35～0.5` | 靠近时半透明（策划可调） |
| `fadeDuration` | `0.15～0.25s` | 0=瞬切；>0 用协程/Lerp |
| Trigger 尺寸 | 略大于墙 Sprite bounds | 在 Scene 视图 Gizmos 验收 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/ActivateChildOnPlayerFootTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止修改场景、代码、Prefab。只读扫描 + 写溯源报告。

---

## 背景

45 号村民家有一扇「隔断墙」贴图挡在玩家和屋内之间。人不应当被墙挡住走不动，但走近时墙要变透，看见后面的人/物。

---

## 侦探任务清单

### A. 目标物体确认

- `Object/隔断墙` 与 `Design/村民家3合层/隔断墙` 是否重复；改哪一张、另一张如何处理
- Renderer 数量（单 Sprite 还是多子节点）

### B. 玩家脚底契约

- `Player.prefab` 中 `PlayerFoot` 名称、Layer、Collider 形状
- Trigger 应检测 PlayerFoot 还是 Player 根 Tag

### C. 现网缺口表

| 检查项 | 现网 | 应有 |
|--------|------|------|
| 实心 Collider | | 无 |
| Trigger Collider | | 有 |
| 半透明脚本 | | 有 |
| GSM/SceneEntity | | 不需要 |

### D. 推荐方案 + 否决理由

- 主推 A：新脚本 + 子物体 Trigger  
- 是否扩展 `ActivateChildOnPlayerFootTrigger` vs 新建类  
- 最小改动文件列表（脚本路径 + 场景 Hierarchy 变更）

### E. 与遮挡系统关系

- 是否挂 `VillageSceneObjectDepthSort`（通常室内墙用 SortingOrder 即可，侦探裁定）  
- 半透明时 Sorting 是否要保持不变

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 玩家可穿过墙区域 | 不被 Collider 挡住 |
| 2 | 走进 Trigger | 墙明显变半透明 |
| 3 | 走出 Trigger | alpha 恢复 1 |
| 4 | 反复进出 | 不卡 alpha、不报错 |
| 5 | 与 NPC45/物品点击 | 不互相抢射线（物品远程、墙 Trigger 分区） |
| 6 | Console | 无 NullRef |

### G. 开放问题

写入 `OPEN_QUESTIONS.md`「Village_HomeScene45 隔断墙半透明 · 2026-08-22」（如目标 alpha、是否平滑、合层去重）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_隔断墙靠近半透明_架构溯源报告.md`

报告结构：① 结论一句话 ② 原因 ③ 用户验收 ④ 方案对比 + Hierarchy 施工步骤 + 参数表

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_隔断墙靠近半透明_架构溯源报告.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity

你现在是【施工员】。按报告实现隔断墙靠近半透明。

必须遵守：
- 隔断墙无实心碰撞，仅 Trigger 感应；
- 检测 PlayerFoot（与 ActivateChildOnPlayerFootTrigger 一致）；
- 只改 `Object/隔断墙` 及报告指定的合层去重；不改 Village_HomeScene45SceneManager；
- 新脚本放 `SceneEntities/CommonEntity` 或报告指定路径，带中文注释说明 Enter/Exit 与 alpha；
- 禁止 Update 里 Find；可选短 Lerp；
- 详细注释：为何不用 VillageSceneObjectDepthSort。

提交说明：脚本名、Trigger 尺寸、normal/near alpha、合层隔断墙如何处理、验收步骤。
```
