# Cursor Agent Prompt · Village_KenMuNi1：`精灵池路灯` 对齐 `精灵池中` 的图层变化（DepthSort）

> **角色**：先【架构侦探】只读对拍；报告拍板后再【施工员】最小挂载  
> **日期**：2026-09-22  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **金样物体（用户红箭头 1）**：`Map / Design / Map / 肯姆尼2合层 / **精灵池中**` —— 已有按玩家前后换 Sorting Layer 的「图层变化系统」  
> **目标物体（用户红箭头 2 / 当前选中）**：同合层下 **`精灵池路灯`** —— 要做成和 `精灵池中` **同一套**  
> **现网组件真源**：`VillageSceneObjectDepthSort`（guid `ba2f9bf2…`）—— **禁止**新写第二套排序脚本  
> **不是**：改 `精灵池上`（除非验收同穿帮再开）；改半透明 Fade；改 Walk 障碍；改玩家 Locomotion / DepthZone；改 `VillageSceneObjectDepthSort.cs` 核心  

把下面「侦探」整段交给 Cursor Agent。没对拍金样 Inspector、没确认路灯现网组件之前，不要施工。

---

## 提示词助手预梳理（侦探须 YAML/Hierarchy 复核）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 视觉 | 玩家相对 **精灵池路灯** 前后走时，灯柱/灯罩与人的遮挡关系与 **精灵池中** 一样会换层（DNF 式：人在后被挡、人在前盖住灯） |
| 实现 | **复用** `VillageSceneObjectDepthSort`；字段**先抄 `精灵池中`**（该物体 0830 已按青石围栏抄齐） |
| 范围 | 仅 **`精灵池路灯`**（可含子 SR 一并列入 target） |
| 不做 | 半透明；批量给全合层挂脚本；改 Prefab 源 `肯姆尼2合层.prefab`（除非报告证明路灯只在 Prefab 里、场景无覆盖——默认改**场景实例**） |

### 现场 Hierarchy（用户截图 2026-09-22）

```
Village_KenMuNi1
└─ Map / Design / Map / 肯姆尼2合层
     ├─ … 背景 / 地板 / 灌木 / 商店 …
     ├─ ★ 精灵池中          ← 金样：已有图层变化系统
     ├─ 井
     ├─ ★ 精灵池路灯        ← 目标：要对齐（截图中选中）
     ├─ 精灵池上
     │    └─ Collider (2)
     └─ … 村庄遮罩_肯姆尼* …
```

### 金样现网（0830 已施工；须再读磁盘复核）

对照文档：

- `Assets/Doc/执行文档/8月/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md`
- `Assets/Doc/施工说明/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_施工说明.md`
- `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs`

预扫（`Village_KenMuNi1.unity` 上 `精灵池中` 的 DepthSort）：

| 字段 | 预扫值 |
|------|--------|
| 脚本 | `VillageSceneObjectDepthSort` |
| `targetSpriteRenderers` | 本物体 SR |
| `anchorOverride` | 本 Transform |
| `invert` | 0 |
| Default / SceneObject Order | **6 / 0** |
| `preferTownLocomotionAuthoritativeY` | 1 |

### 目标预扫（须证伪）

| 项 | 预扫 |
|----|------|
| 磁盘名 `精灵池路灯` | **助手扫 `Village_KenMuNi1.unity` / `肯姆尼2合层.prefab` 时未找到同名节点**（合层 Prefab 仅有 `精灵池中` / `精灵池上`；场景另有 `路灯蘑菇`，勿自动等同） |
| 结论假说 | 路灯可能是 **编辑器里新建/改名尚未落盘**，或挂在场景覆盖层、或其它 Unicode 名 —— **侦探第一步必须在 Hierarchy / YAML 钉死真实 GameObject 路径与组件列表**；若磁盘无此节点，报告写「须先 Save 场景再施工」，勿猜挂到 `路灯蘑菇` |

生活类比：池中已经装了「前后换层自动门禁」；路灯还是死海报（或根本还没存进场景文件）——要对齐，就给路灯装**同一款门禁**，旋钮先抄池中。

### 否决方案

| 方案 | 原因 |
|------|------|
| 新写排序 / 图层脚本 | 已有 DepthSort |
| 只改固定 SortingOrder、不按玩家 Y 切换 | 不满足「图层变化系统」 |
| `VillagePlayerDepthZone` | 改的是**玩家**进区层，不是路灯 Sprite |
| `SpriteFadeOnPlayerFootTrigger` | 半透明，不是遮挡换层 |
| DepthComponent + DepthSort 同开 | 双写 Order，脚本已 Warning |
| 顺手改 `精灵池上` | 0830 定为 P1；本票用户只点名路灯 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：让 `Village_KenMuNi1` → `肯姆尼2合层` → **精灵池路灯** 拥有与 **精灵池中** 相同的图层变化系统（`VillageSceneObjectDepthSort`），并写出可施工的最小清单。

### 必读

@Assets/Doc/执行文档/8月/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_施工说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/ArtRes/Scene/Village/Prefab/肯姆尼2合层.prefab

### A. 先钉死物体（本票最易翻车处）

1. Hierarchy 路径是否与截图一致：`…/肯姆尼2合层/精灵池路灯`  
2. 磁盘 YAML 是否已有该名；若无：是未保存、改名、还是误认 `路灯蘑菇`？**禁止**在未确认时把 DepthSort 挂到蘑菇或其它节点  
3. 路灯当前组件：Transform / SpriteRenderer（几片？）/ Collider / 是否已有 DepthSort / DepthComponent  
4. 初始 SortingLayer / Order 是钉死 SceneObject 还是 Default  

输出「金样 vs 路灯」对照表。

### B. 对拍金样配置

读出现网 `精灵池中` 的 `VillageSceneObjectDepthSort` 全字段（以磁盘为准，勿只信 0830 文档）。  
拍板路灯初值：**默认整表抄池中**（含 6/0、invert=0、本 Transform 锚点、preferTownY=true）。  
若路灯 pivot 偏高（灯杆顶在上、脚在下），写明是否建议新建空锚点贴灯座脚底，或拖子 Collider——列入调参顺序，不要第一版就改 C#。

### C. 多 SR / 子物体

若路灯有灯罩、灯杆、光晕等多张图：  
- `targetSpriteRenderers` 列全哪些、哪些固定不换层（例如永远在人前的光效）——必须写清  
- 是否只挂根、还是子物体各挂一份（倾向：**根上一份 + target 列全需换层的 SR**，与池中同模式）

### D. Prefab vs 场景实例

钉死改 `Village_KenMuNi1.unity` 实例，还是改 `肯姆尼2合层.prefab` 源。  
倾向：与 0830 池中一致 —— **只改场景实例**（除非路灯只存在于 Prefab 且无场景覆盖）。

### E. 方案与否决

推荐：**方案 A** — 路灯根 Add `VillageSceneObjectDepthSort`，字段抄 `精灵池中`。  
一句话否决：新脚本 / 只改死 Order / DepthZone / Fade / 双开 DepthComponent / 顺手改精灵池上。

### F. 验收清单（写入报告）

| # | 操作 | 期望 |
|---|------|------|
| 1 | 站在路灯「后方」（相对锚点，玩家 Y 更大/更深） | 灯 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 灯 |
| 3 | A/D、W/S、斜向绕灯 | 换层无狂闪、无卡死 |
| 4 | 运行时看路灯 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 再绕 `精灵池中`、青石围栏 | 金样仍正常 |
| 6 | Console | 无 DepthComponent 双开 Warning |
| 7 | （可选）临时勾 `debugLogOnLayerChange` | 有 `[VillageOcclusion] obj=精灵池路灯 …` |

前后整段反了：勾 `invert`。切换偏：改锚点再调 Order。

### G. 报告结构

① 结论一句话（挂哪个物体、抄哪套字段）  
② 金样 vs 路灯对照表（含「磁盘是否存在路灯」）  
③ 推荐 Inspector 初值表  
④ 调参顺序（invert → 锚点 → Order）  
⑤ 要改文件路径级  
⑥ 验收表  
⑦ 开放问题（精灵池上是否本期；多 SR；Prefab 是否要跟）

写入：`Assets/Doc/执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md`
```

---

## 施工员（侦探闭环后再复制；未闭环勿用）

```
@Assets/Doc/执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_施工说明.md

你是【施工员】。只按溯源报告给 `精灵池路灯` 挂与 `精灵池中` 同款 `VillageSceneObjectDepthSort`。

约束：
- 不改 `VillageSceneObjectDepthSort.cs`（除非报告点名村级 Bug）
- 不改 `精灵池中` / 青石围栏已有配置
- 不做半透明 Fade；不挂 DepthZone 冒充遮挡
- 禁止 DepthComponent 与 DepthSort 同开
- 禁止未经报告把脚本挂到 `路灯蘑菇` 或其它近义名物体
- 默认只改场景实例；Prefab 仅当报告要求
- 写入：`Assets/Doc/施工说明/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ 确认 Hierarchy 里 `精灵池路灯` 已 **Save** 进场景（磁盘预扫曾找不到同名）  
2. 确认报告初值表（通常整表抄池中 6/0）  
3. 复制「施工员」→ Play 按验收表绕灯走一圈
