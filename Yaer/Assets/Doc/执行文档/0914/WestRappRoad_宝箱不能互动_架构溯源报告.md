# WestRappRoad · 拉普路西宝箱不能互动 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 场景坐标 / 存档 / Git  
**Unity**：2020.3.48f1  
**场景**：`WestRappRoad` → `Design / Near / Box`（PrefabInstance）  
**旁注**：`Design / Interaction` 是 SortingGroup 分组，**不是**宝箱；勿当修点  
**现象（静态钉死）**：走近 **完全没有** 互动键提示（类型 **1**）；本机未 Play，须用 `[WestRappRoadHpMpBox]` 日志复核  
**产品期望**：未开箱可走近提示、确认开箱、发 HP/MP 球 + Tips、写 `WestRappRoadData.hpMpBoxOpened`；已开则造型开且不可再互动  
**不是**：改村巨树箱；重写 Interactive 全局；挂回 `HomeScene2Box`；硬开故事对白  
**提示词**：`Assets/Doc/提示词/0914/WestRappRoad_宝箱不能互动_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q4

---

## 沟通摘要

### ① 结论一句话

**主因 F：宝箱挂在 `Map/Design/Near` 下，不在 `SceneEntityComponentGSM.objRoot`（`Objects`）子树里 → 运行时扫不到 `SceneEntity` → `WestRappRoadHpMpBox.OnInit` 永不跑 → 不进可互列表、也不订 `OpenBox`。**  
次因 **B**：实例 Y≈**−3.44**，西境地面玩家≈**−6.61**；Body 盒高约 1.55，修好注册后仍可能 **永不 Overlap**。推荐：把 Box **挂回 Objects**（或扩扫根）+ **降 Y 对齐地面**；勿改 Interactive 全局。

### ② 原因（通俗）

箱子摆在「美术 Near 层」里，游戏只认 `Objects` 下的互动实体，等于没报名，走近当然没 E。就算报上名，箱子还悬在半空（约 −3.4），人在地上（约 −6.6），碰撞盒对不上。

### ③ 用户检查清单

| # | 操作 | 期望 / 现网 |
|---|------|-------------|
| 1 | Hierarchy：`Box` 父级 | 现网 `Design/Near`；应在 **`Objects`** 下（或等价进 objRoot） |
| 2 | Console 进西境 | 现网很可能 **没有** `[WestRappRoadHpMpBox][OnInit-…]` |
| 3 | 新档走到 x≈66.6 | 现网无键提示；修好后应有 E → 开箱 |
| 4 | 对照同图告示牌等 | 在 Objects 下的应正常 → **仅此箱** |

### ④ 程序补充

见下文。施工说明待拍板：`施工说明/0914/WestRappRoad_宝箱不能互动_施工说明.md`。

---

## 1. 现象类型（钉死）

| # | 类型 | 本票 |
|---|------|------|
| **1** | 走近 **完全没有** 互动键提示 | **主判**（未注册 → `GetFirstCanTouchEntiy` 扫不到） |
| 2 | 有提示，确认无反应 | 次级：即便点到，未 OnInit 则 `OpenBox` 未订阅 |
| 3 | 有 OpenBox 无奖励 | 否（`useStoryOnOpen=0` 直开） |
| 4 | 已是打开造型 | 仅当 `hpMpBoxOpened=true`；新档/未开旗时不是主因 |

Play 复核：有无 `[WestRappRoadHpMpBox][OnInit-…]`；无则坐实 F。

---

## 2. 现网挂法

| 项 | 值 |
|----|-----|
| 路径 | `WestRappRoad` → **`Map` → `Design` → `Near` → `Box`** |
| Prefab | `Assets/Prefabs/Box.prefab` guid `3d24231045a0614438f37d7cba4b5649` |
| 实例 Local | **(66.582, −3.4398, 0)**；Near/Design 父级均为 (0,0,0) → 世界同 |
| 逻辑脚本 | 附加 **`WestRappRoadHpMpBox`**（`1573113632`）；`useStoryOnOpen: 0`；球×2；`enableDebugLog: 1` |
| Prefab `HomeScene2Box` | 实例 **`m_RemovedComponents`** 已移除（曾 `m_Enabled: 0`）→ **不参与** |
| `SceneEntity` | Prefab 仍保留；`GetComponent<BaseSceneEntityLogic>()` 会拿到西境脚本 |
| `objRoot` | `Objects`（`627450016`）——**不含** Design/Near/Box |
| 存档 | `WestRappRoadData.hpMpBoxOpened` / key `WestRappRoadData_hpMpBoxOpened` |

互动预期链：

```
走近 Body / 点选 Click
  → GetFirstCanTouchEntiy（只扫已 OnInit 的 sceneObjs）
  → 键提示 + onClick → WestRappRoadHpMpBox.OpenBox
  → 直开 + GetHpMp（useStoryOnOpen=false）
```

---

## 3. 嫌疑裁定

| # | 嫌疑 | 裁定 | 证据 |
|---|------|------|------|
| **F 未 OnInit / 未进 Entity** | **唯一主因** | Box 在 `Map/Design/Near`；`SceneEntityComponentGSM` 只 `objRoot.GetComponentsInChildren<SceneEntity>`（`Objects`）。YAML `sceneObjs` 亦无本箱。→ 永不 `OnInit`、不订 OpenBox、不进 `GetFirstCanTouchEntiy` |
| **B Y/碰撞** | **次因（F 修好后必查）** | 箱 Y≈−3.44；`LeftBorn`/`DefaultBorn`≈−6.61；Body Size.y≈1.55 Offset.y≈−0.12 → 盒约 **−4.34～−2.78**；玩家脚≈−6.61；Padding 0.2 仍差 **≈2+** 单位。地面 `Ground1` 顶约 −6.6 量级 |
| **A 已开档** | 次要 | 合法锁互动；解释不了「从未 OnInit」；新档对照 |
| **C 串档** | 弱 | 脚本读 `WestRappRoadData`；有防 Home 串档 Error；未 OnInit 则告警也不打 |
| **D Missing** | 否 | Interactive/Body/Click Prefab 完好；西境脚本已加 |
| **E Collider 关** | 否 | Body `isTrigger=1` enabled |
| **G 故事模式** | 否 | `useStoryOnOpen: 0` |

**否决**：把 `Design/Interaction` 当宝箱修；未证实改 Interactive 全局。

---

## 4. Body bounds vs 玩家（B）

| | Y |
|--|---|
| 箱根 | ≈ −3.44 |
| Body 世界约 | −4.34 ～ −2.78 |
| 玩家出生/地面 | ≈ −6.61 |
| 结论 | **不重叠**（非高台设计：同图其它互动点多在 −6.x） |

对照：同场景 NPC/告示等 Local Y 常见 **−6.28 / −6.61**。箱 −3.44 更像 **摆错高度**，不是故意高台。

---

## 5. `HomeScene2Box` 是否参与？

**否。** 实例已 `m_RemovedComponents` 去掉；主逻辑为 `WestRappRoadHpMpBox`。Prefab 母体仍带 Home 脚本，**勿**为修西境改母体回挂。

---

## 6. 推荐方案（最小修）

| 步 | 动作 | 文件 |
|----|------|------|
| **1（主 · F）** | 将 `Near/Box` **Reparent 到 `Objects`**（或改 `objRoot` 能扫到 Design——不推荐扩大根） | `WestRappRoad.unity` |
| **2（次 · B）** | 本实例 **Local Y → ≈ −6.5～−6.6**（对齐地面/Born；注释对照 LeftBorn）；必要时略扩 Body，优先挪 Y | 同场景实例 |
| **3** | Play：必有 `[WestRappRoadHpMpBox][OnInit-…]` 且 `hpMpBoxOpened=false` 时 `canTouch=true` | — |
| **4** | 已开档测：造型开、不可再互动（勿为再开忽略存档） | — |

**禁止**：改 `VillageKenMuNi*`；恢复 HomeScene2Box 当西境主逻辑；大重构 Interactive；改 `useStoryOnOpen` 为 true 交差。

### 给施工员清单（5 条）

1. Hierarchy：拖 `Box` 到 `Objects` 下（保持世界坐标先记下再调）。  
2. 将实例 Y 降到与西境地面一致（建议对齐 `LeftBorn.y≈-6.61` 附近，按美术微调）。  
3. 确认 `WestRappRoadHpMpBox` / `SceneEntity` / Interactive 仍在；`HomeScene2Box` 保持移除。  
4. 新档验收：走近有 E → 确认开箱 → 双球 Tips → 存档旗 true。  
5. 读档已开：不可再互动；Console 无串档 Error。

---

## 7. OPEN

| ID | 问题 | 默认 | 状态 |
|----|------|------|------|
| Q1 | 现象是否类型 1？ | 静态判 1；Play 无 OnInit 日志则坐实 | 待 Play |
| Q2 | 只挪父级不降 Y？ | **否**；F+B 都要 | 待施工 |
| Q3 | 目标 Y 精确值？ | 先对齐 Born≈−6.61，美术可微调 | 待定 |
| Q4 | 已开档用户？ | 说明清 `WestRappRoadData_hpMpBoxOpened`；勿强开 | 文档 |

---

## 8. 侦探声明

未改场景 / 代码 / Git。主因 **F**，次因 **B**；本机未 Play，施工后必须见 OnInit 日志与听感/键提示验收。
