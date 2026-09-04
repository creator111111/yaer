# Cursor Agent Prompt · 村长家楼梯上楼 → 村巨树 2 楼（VillageWalkArea2 / ExitFrom_HomeSceneChief2f）

> **角色**：先【架构侦探】只读定换场配对，报告后再【施工员】  
> **日期**：2026-09-01  
> **产品设定（钉死）**：  
> 1. 在 **`Village_Chief_House`** 走楼梯 **上楼** → **切换场景**到 **`Village_KenMuNi1`** 巨树上方 **2 楼**地区  
> 2. 村侧落点用用户已摆的 **`ExitFrom_HomeSceneChief2f`**  
> 3. 村侧可走区用用户已摆的 **`VillageWalkArea2`** —— **区域大小已手调固定，禁止改多边形尺寸/形状**  
> **用户 Hierarchy（红箭头）**：`KenMuNi1 / Map / MapLimit` 下 **`VillageWalkArea2`**、**`ExitFrom_HomeSceneChief2f`**  
> **关联**：0901 室内划区 2.5D + 楼梯树屋化（室内先能走上楼，再换场）  
> **本阶段（侦探）**：只读；禁止改场景 / 代码（含禁止动 WalkArea2 形状）  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
Village_Chief_House
  → 玩家沿室内楼梯走到楼上触发区
  → SceneChangeDoor（或等价）LoadScene("Village_KenMuNi1")
  → LastSceneName = Village_Chief_House
  → 村 GSM EnterPos 匹配 → 落在 ExitFrom_HomeSceneChief2f
  → 可走约束生效于 VillageWalkArea2（2 楼平台）
  → 玩家出现在巨树 2 楼，可村式移动
```

**不是**：改 WalkArea2 大小；不是只挪出生点不配门；不是进树屋室内场景（无该 SceneName）。

### 用户已配资产（预扫 · 须证伪）

| 物体 | 场景 | 预扫 | 施工约束 |
|------|------|------|----------|
| **`VillageWalkArea2`** | KenMuNi1 / MapLimit | ✅ 磁盘有 | **禁止改 Collider 尺寸/点集** |
| **`ExitFrom_HomeSceneChief2f`** | KenMuNi1 / MapLimit | ✅ 约 `(-159.34, 41.66, 0)` | 落点 Transform；可微调节奏但产品已摆好优先保留 |
| KenMuNi1 `EnterPosConfig` | `lastScene: Village_Chief_House` | ✅ 已指向 **`ExitFrom_HomeSceneChief2f`**（fileID 与物体一致） | 侦探核实是否保持；勿误绑回 1 楼门前 |
| Chief_House `LeftDoor` | → `Village_KenMuNi1` | ✅ 有 | ⚠️ 与楼梯同目标场景 → **同一 lastScene 只能对应一个 EnterPos** |

### 换场七件套（室内楼梯门 · 须填表）

对齐 `场景切换.md` / Stairs / Home 出门样板：

| # | 检查项 |
|---|--------|
| 1 | GO Active；挂在可交互层级 |
| 2 | `SceneChangeDoor` Enabled |
| 3 | `NextSceneName` = **`Village_KenMuNi1`**（与 Build / SceneName 一致） |
| 4 | `TriggerWhenMoveIn`：上楼倾向 **1**（走进即切；对拍树屋/序章）或按 E——报告拍板 |
| 5 | Interactive / Collider / `sceneObjs` 登记 |
| 6 | 表现：黑幕默认或 Loading——对拍 `House_Chief` / 其它出门 |
| 7 | 合层 **`楼梯`** 仍是美术；触发体 **旁挂**，勿把 SR 当唯一门 |

### 关键缺口（助手预扫 · 侦探必须答）

#### 缺口 1 · EnterPos 单键冲突

`EnterPos` 只认 **`lastScene` 字符串**。  
现网：`Village_Chief_House` → **2 楼** `ExitFrom_HomeSceneChief2f`。  
若 **`LeftDoor`（1 楼出门）** 也 `NextSceneName=Village_KenMuNi1`，则从 1 楼门出去也会落到 **2 楼**。

| 方案 | 做法 | 倾向 |
|------|------|------|
| **E1** | 产品接受：凡从 Chief_House 回村都落 2 楼；1 楼门改指别处或禁用 | 须产品确认 |
| **E2** | 1 楼门落点另建 `ExitFrom_HomeSceneChief`（1f）+ 改 EnterPos 指 1f；楼梯用 **另一 lastScene 伪名** 或换场后强制 SetPos——侵入大 | ⚠️ |
| **E3** | 楼梯专用门；1 楼 LeftDoor 的 EnterPos 改回门前，楼梯换场用 **stayAction / 自定义 LastScene 后缀**（若现网无此能力则扩最小 API） | 侦探查是否已有先例 |

报告必须拍板：**1 楼门与 2 楼楼梯** 如何共用/拆分落点。

#### 缺口 2 · `VillageWalkArea` vs `VillageWalkArea2`

`TownPlayerLocomotion` 现网按名只解析 **`VillageWalkArea`**（单多边形）。  
落点 Y≈41 在 2 楼，若仍只套 **1 楼 WalkArea**，会被 **ClosestPoint 拉回低处** → 传送「成功」却站不住 2 楼。

| 方案 | 做法 | 倾向 |
|------|------|------|
| **W1 · 分区切换** | 进 2 楼触发区 / 落点后把生效 WalkArea 切到 **`VillageWalkArea2`**（Override 或按名解析扩展）；回 1 楼切回 `VillageWalkArea` | ✅ 不改 WalkArea2 **形状** |
| W2 · 合并两大多边形 | 改点集 | ❌ 用户禁止改 WalkArea2 大小；合并易误伤 |
| W3 · 关掉 WalkArea 校正 | 2 楼可掉出 | ❌ |

**硬约束**：可以改 **引用/切换逻辑**；**不可以**改 `VillageWalkArea2` 的多边形尺寸。

### 与「室内楼梯树屋化」关系

| 前期（0901 划区） | 本期 |
|-------------------|------|
| 室内能走上楼 | ✅ 依赖 |
| 本期内换场出门 | ✅ 楼梯顶 Trigger → 村 2 楼 |
| 村 2 楼 WalkArea2 | ✅ 站住 + 可走 |

若室内还走不到楼上，换场门摆了也验不了——报告写清依赖。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 楼梯上楼 → `LoadScene(Village_KenMuNi1)` + 落 `ExitFrom_HomeSceneChief2f` | ❌ 改 `VillageWalkArea2` 点集/尺寸 |
| ✅ 七件套门 + EnterPos 配对写清 | ❌ 新造第三套换场 |
| ✅ WalkArea2 生效方案（切换/扩展查找） | ❌ 默认同开其它 Home |
| ✅ 与 1 楼出门落点冲突写清并拍板 | ❌ 把目标改成不存在的树屋 Scene |

### 严禁

- 改 WalkArea2 大小「为了好走」  
- 只配 NextSceneName 不配/不核实 EnterPos  
- 落 2 楼仍只套 `VillageWalkArea` 导致被拉回 1 楼高度  
- 合层 `楼梯` SR 直接当 SceneChangeDoor 唯一体却无 Collider/sceneObjs  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 上楼 TriggerWhenMoveIn？ | **true**（走进即切） |
| Q2 | 1 楼 LeftDoor 与 2 楼落点冲突？ | 必须拍板 E1/E2/E3 |
| Q3 | WalkArea2 如何生效且不改形状？ | **W1** |
| Q4 | 黑幕还是 Loading？ | 对齐现网 LeftDoor（黑幕）除非产品要 Loading |
| Q5 | 村 2 楼是否要「下楼回村长家」对开？ | 本期可只做上楼；回程 OPEN |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。
禁止改 VillageWalkArea2 的多边形尺寸/点集。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md

## 产品目标
Village_Chief_House 走楼梯上楼 → 切换到 Village_KenMuNi1 巨树 2 楼。
落点：ExitFrom_HomeSceneChief2f（用户已摆）。
可走区：VillageWalkArea2（用户已摆，大小锁定勿改）。
建立传送/换场关系；补齐门与 EnterPos / WalkArea 生效链。

## 必读
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/LoadSceneComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
（VillageWalkArea 按名解析；尚无 WalkArea2）
@Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_KenMuNi1_House_NPC2与村长门无法进屋_架构溯源报告.md
@Assets/Prefabs/Stairs.prefab
（样板）

检索：ExitFrom_HomeSceneChief2f、VillageWalkArea2、VillageWalkArea、
EnterPosConfig、Village_Chief_House、LeftDoor、SceneChangeDoor、楼梯。

## 侦探任务
1. 核实用户两锚点坐标、EnterPos 是否已绑 Chief_House→ExitFrom_HomeSceneChief2f。
2. 设计室内楼梯顶换场门（七件套）；与合层「楼梯」美术解耦。
3. 拍板 1 楼 LeftDoor 与 2 楼落点的 EnterPos 冲突（E1/E2/E3）。
4. 拍板 WalkArea2 如何在不改尺寸下成为生效可走区（W1 等）；落点不被 1 楼 WalkArea 拉走。
5. 最小清单 + 验收 + OPEN（含是否做回程）。
6. 依赖：室内 2.5D/能走上楼是否已具备。

## 报告落盘
Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md

结构：①结论 ②已配锚点 ③换场链 ④EnterPos 冲突裁定 ⑤WalkArea2 生效
⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md

## 目标
1. 村长家楼梯上楼 → LoadScene(Village_KenMuNi1)，落点 ExitFrom_HomeSceneChief2f。
2. 落点后可走约束使用 VillageWalkArea2（按报告切换/解析）。
3. 【硬禁止】修改 VillageWalkArea2 多边形尺寸、点集、整体 Scale 用来「扩大可走区」。
4. 按报告处理 1 楼门与 2 楼落点冲突；门进 sceneObjs；表现对齐报告。

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_楼梯上楼换场巨树2楼_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 室内走上楼触发区 → 进入 KenMuNi1
- [ ] 落点在 ExitFrom_HomeSceneChief2f 附近（巨树 2 楼）
- [ ] 落点后不被拉回 1 楼高度；在 WalkArea2 内可移动
- [ ] VillageWalkArea2 点集/尺寸与施工前一致（未改）
- [ ] 1 楼出门行为符合报告决议
- [ ] Console 无 LoadScene / EnterPos / 空引用 Error
- [ ] 回村后仍为 Village2_5D

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑侦探**——重点拍板两件事：① 1 楼门和 2 楼楼梯会不会抢同一个 EnterPos；② 落 2 楼后怎么用上 `VillageWalkArea2`（现网默认只认 `VillageWalkArea`）。  
2. **`VillageWalkArea2` 大小已定死，施工不许改。**  
3. 村侧落点物体你已摆好：`ExitFrom_HomeSceneChief2f`；磁盘上 EnterPos 已指向它——侦探须核实并接上楼梯门。
