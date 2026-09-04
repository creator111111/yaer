# Cursor Agent Prompt · Village_KenMuNi1：`精灵池中` 对齐 `青石围栏` 的玩家位置渲染遮挡

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **金样物体（用户红箭头 1）**：`肯姆尼1合层`（或同合层树）下 **`青石围栏`** —— 已有「按玩家位置改渲染层级」效果  
> **目标物体（用户红箭头 2）**：`肯姆尼2合层` 下 **`精灵池中`**（子物体可见 `Collider (1)`）  
> **产品目标（白话）**：**精灵池中**也要和**青石围栏**一样——根据玩家前后位置切换渲染层级，走在池子后面被挡住、走在前面盖住池子（DNF 式遮挡）  
> **现网组件真源**：`VillageSceneObjectDepthSort`（勿新建第二套排序脚本）  
> **本阶段**：只读；禁止改代码 / Prefab / 场景  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 视觉 | 玩家相对 **精灵池中** 前后移动时，**Sorting Layer / Order** 动态切换，观感对齐青石围栏 |
| 实现 | **复用**现网 `VillageSceneObjectDepthSort`，参数对照青石围栏抄齐再微调 |
| 范围 | 仅 **`精灵池中`**；`精灵池上` 是否同挂 → 开放问题（默认本期可不做，除非验收发现上沿也穿帮） |
| 不做 | 半透明淡入（那是隔断墙另一套）；不改 Walk 障碍物理；不改玩家 Locomotion |

### 现场 Hierarchy（用户截图）

**金样：**

```
Village_KenMuNi1 / Map / Design / … / 肯姆尼1合层 / …
  ★ 青石围栏          ← 已有按玩家位置改渲染层级
```

**目标：**

```
… / 肯姆尼2合层 / …
  商店 / 商店门 / 井 / 精灵池上
  ★ 精灵池中          ← 红箭头：要对齐遮挡
      └─ Collider (1)
```

### 现网预扫（须证伪）

| 物体 | 预扫组件 | 结论假说 |
|------|----------|----------|
| **青石围栏** | 已挂 `VillageSceneObjectDepthSort`（guid `ba2f9bf2…`）；`targetSpriteRenderers` 有绑定；`anchorOverride` 有；`sortingOrderWhenDefaultLayer=6`；`SceneObjectLayer=0`；`invert=0` | ✅ **金样配置** |
| **精灵池中** | 预扫仅 Transform + SpriteRenderer（Layer 0）；**无** DepthSort；有子 Collider | ❌ **缺脚本** |

生活类比：青石围栏已经装了「前后换层的自动门禁」；精灵池中还是死海报——玩家绕过去图层不变，会穿帮。

### 技术真源（禁止另造）

| 文档 / 脚本 | 用途 |
|-------------|------|
| `VillageSceneObjectDepthSort.cs` | 村庄 `Village2_5D` 下比较玩家世界 Y vs 锚点 Y → `Default` ↔ `SceneObject` |
| `技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md` §3 | 挂载与验收 OC |
| `执行文档/5月/0512/村庄场景物体图层遮挡_程序施工执行说明.md` | OC-01～05、与 DepthComponent 互斥 |
| `SortingLayerName` / TagManager | `Default` / `SceneObject` / `Player` |

**否决：**

| 方案 | 原因 |
|------|------|
| 新写排序脚本 | 已有 DepthSort |
| 只改固定 sortingOrder、不按玩家切换 | 不满足「根据玩家位置」 |
| `VillagePlayerDepthZone`（改玩家层） | 那是改**玩家**进区图层，不是改池子 Sprite；除非侦探证明池子应走 Zone——默认否 |
| `SpriteFadeOnPlayerFootTrigger` | 半透明，不是遮挡换层 |
| 同挂 `DepthComponent` + DepthSort | 双写 Order 打架 |

### 施工倾向（侦探拍板参数）

| 项 | 倾向 |
|----|------|
| 挂点 | **`精灵池中` 根**（有 SpriteRenderer 的那层） |
| targetSpriteRenderers | 拖本物体 SR；若有多片（池中/波纹）列全 |
| anchorOverride | 对照青石围栏：用脚底/池沿锚点子物体，或新建空锚点；**勿瞎抄围栏的 fileID** |
| sortingOrder WhenDefault / SceneObject | **先抄青石围栏 (6 / 0)**，实机不对再调；写入报告调参表 |
| invert | 默认关；前后反了再勾 |
| Collider (1) | 若仅为美术/别用，**不必**为遮挡改物理；遮挡不靠 Collider |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 对照表：青石围栏 vs 精灵池中 组件/参数 | ❌ 改 `VillageSceneObjectDepthSort` 核心逻辑（除非发现村级 Bug） |
| ✅ 拍板挂载与锚点 / Order | ❌ 精灵池上强制本期（开放） |
| ✅ 验收前后遮挡清单 | ❌ 半透明、Walk 障碍、进屋 |
| ✅ 最小施工清单 | ❌ 批量给全村合层物体挂 DepthSort |

### 严禁（本阶段）

- 改代码 / Prefab / 场景  
- 把遮挡需求做成半透明 Fade  
- 未对拍青石围栏 Inspector 就自创一套 Order 公式  
- 与 DepthComponent 同开不写警告  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| 场景 `青石围栏` | 金样 Inspector 全字段 |
| 场景 `精灵池中` | 缺什么、SR 在哪 |
| `VillageSceneObjectDepthSort.cs` | 行为契约 |
| 技术文档 + 0512 执行说明 | OC 验收 |
| （可选）`精灵池上` | 是否也要挂 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md
@Assets/Doc/执行文档/5月/0512/村庄场景物体图层遮挡_程序施工执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs
@Assets/Scripts/Game/Static/Name/Settings/SortingLayerName.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景。只读扫描 + 写「精灵池中对齐青石围栏遮挡」溯源报告。

---

## 背景（策划白话）

1. **青石围栏**已经能根据玩家站前/站后换渲染层级。  
2. **精灵池中**也要同样效果。  
3. 本阶段只摸清：围栏挂的什么、池子缺什么、锚点/Order 怎么抄、要不要动代码。

---

## 侦探任务清单

### A. 钉死青石围栏金样
出表：完整 Hierarchy 路径、组件列表、`VillageSceneObjectDepthSort` 每个序列化字段、锚点物体是谁、target SR 是谁。

### B. 钉死精灵池中现状
路径、组件、有无 SpriteRenderer、子 Collider 用途、有无 DepthComponent / DepthSort / Zone。

### C. 差异与方案拍板
推荐：在 `精灵池中` 加 `VillageSceneObjectDepthSort`，字段对齐围栏后按池体微调锚点/Order。  
明确：改不改 C#（倾向 **只改场景配置**）。

### D. 锚点与 Order
建议锚点放哪（池底脚线 / 现有 Collider 中心 / 新空物体）；Default/SceneObject 的 Order 初值；何时勾 invert。

### E. 边界
`精灵池上` 要否同期；与玩家 `TownPlayerLocomotion` Y 排序、DepthZone 是否冲突。

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `精灵池中` 添加 `VillageSceneObjectDepthSort` | **P0** |
| 2 | 绑定 targetSpriteRenderers + 配置锚点 / Order（对照围栏） | **P0** |
| 3 | Play 验收前后遮挡；不对则调 Order/invert/锚点 | **P0** |
| 4 | （可选）`精灵池上` | P1 |
| 5 | 改 DepthSort.cs | ❌ 默认不做 |

### G. 验收清单（对齐 OC）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 站在精灵池「后方」（相对锚点） | 池子 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 池子 |
| 3 | 纯 A/D、纯 W/S、斜向 | 层级切换无闪烁鬼畜；无卡死 |
| 4 | Inspector 运行时 | `sortingLayerName` 在 Default ↔ SceneObject 间变 |
| 5 | 对照 | 青石围栏行为仍正常（回归） |
| 6 | Console | 无 DepthComponent 双开警告（若未挂 Depth） |

### H. 开放问题
- `精灵池上` 是否必须同挂？  
- 锚点用新建空物体还是复用 `Collider (1)` 中心？  
- Order 是否必须与围栏同为 6/0，还是池子要单独美术表？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md`

MASTER 四段式：  
① 结论（挂 DepthSort + 是否只改场景）  
② 原因（通俗：围栏有自动换层，池子没有）  
③ 用户检查清单（站前站后看谁挡谁）  
④ 给程序：金样字段表 + 目标差异 + 推荐 Inspector 初值 + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md
@Assets/Doc/技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs

你现在是【施工员】。按报告让「精灵池中」获得与「青石围栏」相同的玩家位置渲染遮挡。

必须遵守：
- 复用 VillageSceneObjectDepthSort，禁止新写排序系统；
- 优先只改场景配置；不要无故改 DepthSort.cs；
- 勿与 DepthComponent 同开；勿做成半透明 Fade；
- 锚点/Order 按报告初值，实机微调写入提交说明；
- 重要取舍写清原因。

提交说明：挂在哪个物体、各字段终值、如何验收前后遮挡、未做项（如精灵池上）。
```
