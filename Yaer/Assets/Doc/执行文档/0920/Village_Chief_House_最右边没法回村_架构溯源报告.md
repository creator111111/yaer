# Village_Chief_House 最右边没法回村 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 对照：0822「Chief 出门用 LeftDoor；RightDoor 已禁用」。以现网 YAML 为准。

---

## ① 结论一句话

**右边回村确实没做成。** 最右边的 `RightDoor` 整物体是关掉的，目标还写着东郊 `ForestEastScene`，不是村。现网唯一回村出口在**左边** `LeftDoor`（要按 E）。若产品要「走到最右就回村」，就把右门打开并改成进 `Village_KenMuNi1`，不要打开去东郊。

---

## ② 原因

大白话：村长家右边看着像能出门，那扇门在场景里被关掉了，而且以前接的是东郊，不是肯姆尼村。回村门装在左边。你往右走只会顶到可走区边，不会切场。

### 现网两扇门

| 门 | 物体开关 | NextScene | 走进切 / 按 E | 黑幕 | EnterPosKey | 世界位置（约） |
|----|----------|-----------|---------------|------|-------------|----------------|
| **LeftDoor** | **开** | **Village_KenMuNi1** | `TriggerWhenMoveIn=0` → **按 E** | 黑幕（`ShowLoadingUI=0`） | `Village_Chief_House_Door` | MapLeft **x=-14.03**，门本地 (0,0) → **≈-14** |
| **RightDoor** | **关**（`m_IsActive: 0`） | **ForestEastScene**（残留） | 曾设走进即切 | 曾勾 Loading | 空 | MapRight **x=26.04**，门本地 (0,0) → **≈26** |

村侧落点（左门出去）：`ExitFrom_HomeSceneChief` **(-156.5, -5.5)**，EnterPos 行 `lastScene: Village_Chief_House_Door`。

可走区 `VillageWalkArea` 右缘本地 X≈**26.06**，和 MapRight 对齐——人能走到最右，但门关着，所以「走到头出不去」。

### 和旧决议的关系

0822 OPEN 已拍板：**出门用 LeftDoor 回村；RightDoor 禁用。**  
所以右边「没做回村」是**按当时决议关掉的**，不是漏存场景。  
现在用户明确要最右边回村 → 产品意图变了，要重开右门并改目标，不能原样启用（否则会进东郊）。

### 调用链

```
【现网能回村】走到左边 ≈x=-14 → 按 E
  LeftDoor.EnterDoor
  → LastScene 键 = Village_Chief_House_Door
  → LoadScene(Village_KenMuNi1, 黑幕)
  → ExitFrom_HomeSceneChief (-156.5, -5.5)
  → 可能播「出村长家送树屋」（G1 认门前键）

【最右边】走到 ≈x=26
  RightDoor 物体 Inactive → 无交互、无 [SceneChangeDoor]
  → 不换场
```

---

## ③ 用户需要做什么

1. 先试**左边**门口：走到约 x=-14，看有没有按 E 提示；按 E 应黑幕回村门前。  
2. 再走**最右边**：Console 不应出现 `[SceneChangeDoor]`（门关着）。  
3. Hierarchy 看 `RightDoor`：勾是否没开；`NextSceneName` 是否仍是 `ForestEastScene`。  
4. 施工后：最右走进（或按 E，看拍板）应回 `Village_KenMuNi1` 门前，**不要**进东郊。

---

## ④ 给施工员的补充

### 推荐方案（只此一种）

把右门做成回村主出口（对齐其它村民家「右门回村」习惯），字段一次改齐：

| 文件 | 物体 / 字段 | 现网 | 改为 |
|------|-------------|------|------|
| `Assets/GameRes/Scenes/Village_Chief_House.unity` | `RightDoor` `m_IsActive` | **0** | **1** |
| 同上 | `NextSceneName` | `ForestEastScene` | **`Village_KenMuNi1`** |
| 同上 | `EnterPosKey` | 空 | **`Village_Chief_House_Door`**（与左门同键 → 仍落 `ExitFrom_HomeSceneChief`） |
| 同上 | `ShowLoadingUI` | 1 | **0**（黑幕，对齐左门 / 进屋） |
| 同上 | `TriggerWhenMoveIn` | 1 | **保持 1**（走到最右就出，符合「走出去」） |
| 同上 | 确认已进 `sceneObjs`、Interactive 仍在 `componentsList` | 现网有 Interactive | 勿拆 |

**左门**：先**保留**（仍可按 E 回村），避免施工当周只剩一扇门验收翻车。若产品只要单出口，另票再禁 `LeftDoor` 的 `SceneChangeDoor`（对齐 HomeScene23 禁左开右）。

**不要**新建第三扇门；**不要**改 KenMuNi1 的 `ExitFrom_HomeSceneChief` 坐标（送树屋戏依赖门前键）。

### 不要改

| 不要改 | 原因 |
|--------|------|
| 右门仍指向 `ForestEastScene` 只开 Active | 室内最右会进东郊，错 |
| 把 `House_Chief` / 村侧 2 楼回程案搅进来 | 另票 |
| `VillageWalkArea` 点集 | 右缘已够到门，不是框太窄 |
| 上楼 `StairsDoor_ToTree2f` | 无关 |

### 否决方案

- 只告诉玩家「请走左边」当唯一交付：用户已明确要最右回村，和其它民居右出习惯不一致。  
- 启用右门却不改 Next、或改成空名：会进东郊或报错，比关掉更糟。

### 会误伤的其它场景

右门与左门共用 `Village_Chief_House_Door` 键时，出屋都会落门前并可能触发「送树屋」戏（G1）。若只要左门播戏、右门静默回村，须另开 EnterPos 键——**默认不拆**，与左门同戏；要拆再记 OPEN。

### OPEN

0822 节 Q2 已注明：产品现要最右回村，见本报告；原「RightDoor 禁用」作废。
