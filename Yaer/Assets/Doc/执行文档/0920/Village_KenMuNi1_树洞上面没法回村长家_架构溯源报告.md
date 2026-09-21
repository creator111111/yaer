# Village_KenMuNi1 树洞上面没法回村长家 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 对照：0901 楼梯上楼换场（OPEN Q5「回程本期不做」）。以现网 YAML / 代码为准。  
> 截图：人站在巨树 2 楼树枝上，脚底下方树干有一处黑洞（树洞口）。

---

## ① 结论一句话

**不是走路卡死，是回程门根本没做。** 0901 只修了「村长家楼梯 → 巨树 2 楼」，回程明确本期不做。树洞是美术黑口，旁边没有换场门；唯一的 `House_Chief` 在 1 楼门口（Y≈1.9），2 楼站不到。要回村长家，须在 2 楼落点旁新加回程门，并落到室内楼梯顶，不能落到大门。

---

## ② 原因

大白话：从村长家上楼能进树顶，树顶想再回屋里，程序里没有这条门。截图里脚下的黑洞只是画，走进去也不会切场。1 楼门口那扇进村长家的门在地面高度，人在树枝上够不着。

### 现网对照

| 方向 | 有没有门 | 落点 |
|------|----------|------|
| 村长家楼梯 → 村 2 楼 | 有。`StairsDoor_ToTree2f`（Chief，约 -4.51, 4.8）→ `Village_KenMuNi1`，走进黑幕 | `ExitFrom_HomeSceneChief2f` **(-157.65, 41.66)** + 绑 `VillageWalkArea2` |
| 村 1 楼门 → 村长家 | 有。`House_Chief` **(-158.3, 1.9)** → `Village_Chief_House`，`ShowLoadingUI=0` | Chief `EnterFrom_Village` **(17.1, -6.61)**（大门内侧） |
| 村 2 楼 / 树洞旁 → 村长家 | **没有** | — |

OPEN 与交接文档原话：

- 0901 报告：回程本期不做；村 2 楼再进 Chief 仍走现网门 / `EnterFrom_Village`
- OPEN Q5：`2 楼回程进村长家？` → **本期不做**（⏳）
- 交接文档：`2 楼回程进村长家 | 明确本期不做`

### 树洞是什么

截图树干上的黑口是背景/合层美术，不是 `SceneChangeDoor`。  
`TreeDoor1` / `TreeDoor2` 在场景里约 Y=-7.62，挂的是别的逻辑，**不是**回村长家的换场门。  
2 楼可走区是 `VillageWalkArea2`（世界大约 X -164～-107，Y 33.65～45.35）。脚在 Y≈42 时，ClosestPoint 把人留在这块绿框里，**下不到** 1 楼的 `House_Chief`（Y=1.9）。

### 若硬用现网 `House_Chief` 回屋会怎样

即便人能下到 1 楼门再进，LastScene 是 `Village_KenMuNi1`，Chief 只有一行 EnterPos → `EnterFrom_Village`（大门）。产品期望是从楼梯顶回来，不是从大门闪进去。所以回程不能只「复用大门门」。

### 调用链（现网能走的 / 缺的）

```
【能走】Chief StairsDoor_ToTree2f
  → LastScene = Village_Chief_House（EnterPosKey 空）
  → LoadScene(KenMuNi1, 黑幕)
  → ExitFrom_HomeSceneChief2f + WalkArea2

【缺】KenMuNi1 2 楼（树洞旁 / ExitFrom 附近）
  → （无 SceneChangeDoor）
  → 人按 E / 走进黑洞：无 [SceneChangeDoor] 日志，不换场

【1 楼大门，够不着】House_Chief (-158.3, 1.9)
  → Village_Chief_House → EnterFrom_Village（大门）
```

---

## ③ 用户需要做什么

1. 站在截图那个树洞上方，走近黑口、按 E：Console **不应**出现 `[SceneChangeDoor]`（证明没门）。  
2. Hierarchy 搜 `ExitFrom_HomeSceneChief2f` 附近：应只有落点空物体，**没有**指向 `Village_Chief_House` 的换场门。  
3. 对比：下到村街再进 `House_Chief` 能进屋——那是 1 楼大门，不是 2 楼回程。  
4. 施工后：在 2 楼树洞旁走进触发区 → 黑幕 → 落在村长家**楼梯顶**（不是大门 `EnterFrom_Village`）。

---

## ④ 给施工员的补充

### 推荐方案（只此一种）

对齐上楼的 `Stairs.prefab` / `StairsDoor_ToTree2f`，做对称回程：

| # | 文件 | 做什么 |
|---|------|--------|
| 1 | `Village_KenMuNi1.unity` | 在 `ExitFrom_HomeSceneChief2f` / 树洞口旁（须在 `VillageWalkArea2` 内）新建换场门，建议名 **`StairsDoor_BackToChief`**（复用 `Stairs.prefab`） |
| 2 | 同上门 | `NextSceneName=Village_Chief_House`；`TriggerWhenMoveIn=1`；`ShowLoadingUI=0`；进 `sceneObjs` |
| 3 | 同上门 | `EnterPosKey` 填新键，例如 **`Village_KenMuNi1_Tree2f`**（勿空：空会走大门 `EnterFrom_Village`） |
| 4 | `SceneName.cs`（若项目用常量） | 增加同名常量，与 LeftDoor 的 `Village_Chief_House_Door` 同级 |
| 5 | `Village_Chief_House.unity` | 楼梯顶附近新建落点，例如 **`EnterFrom_Tree2f`**（建议靠近 `StairsDoor_ToTree2f` 的 (-4.51, 4.8)，略偏室内可走区） |
| 6 | Chief `EnterPosConfig` | 新一行：`lastScene: Village_KenMuNi1_Tree2f` → 上表 Transform |

起步坐标建议：回程门世界约 **(-157.5～-156, 41～42)**，与 2f ExitFrom 错开半个身位，避免一落地立刻再触发。

### 不要改

| 不要改 | 原因 |
|--------|------|
| 把 `House_Chief` 抬到 Y≈41 | 1 楼大门进屋断掉 |
| `VillageWalkArea2` 点集 | 与回程无关；走路案另票 |
| 上楼 `StairsDoor_ToTree2f` / W1 绑区 | 去程已通 |
| 东郊 `TreeBridge` / ForestEast 树洞 | 另一套爬行，不是村长家 |

### 否决方案

- 指望走进美术黑洞自动切场：没有 Collider + `SceneChangeDoor`，永远不会换场。  
- 回程也落到 `EnterFrom_Village`（大门）：体感是「从门口闪进屋」，不是从楼梯下来。

### 会误伤的其它场景

按推荐方案：仅多一条 EnterPos 键。大门 `House_Chief` 仍走 `Village_KenMuNi1` → `EnterFrom_Village`。勿把大门的 lastScene 改成新键。

### OPEN

已把 0901 节 Q5 从「本期不做」改为「产品要做，见本报告」。
