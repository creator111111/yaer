# 村庄探索：Walk 区楼梯 / 障碍阻挡（方案 1）— 技术说明

| 项 | 内容 |
|----|------|
| **适用范围** | 精灵村等 `PlayerLocomotionMode.Village2_5D`、WalkArea 多边形内；**楼梯、斜栅栏、平台边**等用 Collider 摆出来的挡位 |
| **产品结论** | **不靠 Unity 物理把玩家「顶在障碍外」**；障碍 Collider 只当**几何数据**，**能不能走由脚本算**（横移 + 纵深一致） |
| **关联执行文档** | `Assets/Doc/执行文档/0514/村庄Walk障碍_方案1_仅Physics2D查询阻挡_关闭脚障物理碰撞_执行文档.md` |
| **Unity** | 2020.3.48f1（与工程主规范一致） |

---

## 1. 大白话：这功能在解决什么

以前容易出现这种情况：**策划在场景里摆了楼梯或挡板（Collider）**，Unity 会当真去**用物理挤玩家脚**；同时程序又在用**另一套逻辑**管玩家**前后走（纵深，世界 Y）**。两套力量一起拽一个人，就容易 **卡边、横移被顶死、有时只能上下不能左右**。

**方案 1** 的意思可以记成三句话：

1. **障碍还是照常在场景里摆**（楼梯、斜边、窄口都用 Collider 画出来），策划工作流基本不变。  
2. **脚和障碍在「物理矩阵」里不再硬碰硬**——引擎不会再用接触求解去「顶」你。  
3. **挡不挡、停在哪**：交给 **`TownPlayerLocomotion`** 里用 **Physics2D 的 Cast / Overlap / Distance** 对着障碍层去算；**横移（A/D）和纵深（W/S）都走这一套思路**，避免「只有一头能挡」。

---

## 2. 策划 / 场景侧要怎么做（检查清单）

| 检查项 | 建议 |
|--------|------|
| 障碍物体 **Layer** | **`VillageWalkObstacle`**（与 `LayerName` 常量一致） |
| 障碍 **Collider2D** | **建议勾选 `Is Trigger`**：表示「只参与逻辑/查询，不当刚体墙」；与程序里 `ContactFilter2D.useTriggers = true` 对齐 |
| 障碍 **Rigidbody2D** | **Static**（推荐），无重力 |
| 玩家脚底 | 仍在 **`PlayerFoot` 层**的 Collider 上；**脚底一般不必改成 Trigger**（与障碍不同） |
| WalkArea | 仍由多边形约束可走区外轮廓；**区内细挡位**靠障碍 Collider + 脚本 |

**不要**把对话圈、拾取圈等剧情 Trigger 和 **`VillageWalkObstacle` 混在同一层**，否则查询和回调容易乱；阻挡与交互应 **分物体、分 Layer**。

---

## 3. 程序侧：改了什么、入口在哪

### 3.1 Physics 2D 层矩阵（「脚不再和障碍物理解算」）

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageWalkObstacleCollisionBootstrap.cs` | 游戏 **`BeforeSceneLoad`**：将 **`VillageWalkObstacle` 与所有层（含 `PlayerFoot`）设为 Ignore**，从根上取消脚↔障碍的物理解算链 |
| `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/Editor/VillageWalkObstacleCollisionMatrixMenu.cs` | 菜单 **Yaer → Physics2D → 应用村庄障碍层（方案1：障碍不与任何层碰撞）**，与 Bootstrap **同源**，便于提交 `ProjectSettings/Physics2DSettings.asset` |

### 3.2 村庄移动与阻挡逻辑（核心）

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs` | **纵深**：对权威世界 Y 做障碍 Cast/Overlap 夹紧；**横移**：在 `MoveComponent` 写入速度之后，用 **PlayerFoot** 沿 **±X** Cast，夹紧 **`velocity.x`** 并同步 **`PlayerMoveComponent.moveSpeedX`**（避免下一帧被 Move 写回） |
|  | **`BuildVillageObstacleContactFilter`**：`useTriggers = true`，否则 Trigger 障碍会出现「线框在却查不到」的假穿障 |
|  | **`ApplyVillageWalkObstacleFootPenetrationSeparation`**：嵌入时用 `OverlapCollider` + `Physics2D.Distance` 做短迭代分离（与矩阵是否碰撞无关，纯查询） |

### 3.3 常量与说明

| 路径 | 职责 |
|------|------|
| `Assets/Scripts/Game/Static/Name/Settings/LayerName.cs` | `PlayerFoot`、`VillageWalkObstacle` 等名称；注释已按方案 1 更新（矩阵不表达「挡人」，挡人靠脚本） |

### 3.4 执行顺序（便于以后改代码不断层）

- **Prefab** 里 `TownPlayerLocomotion` 在 `ComponentSystemMono` 的 **`componentsList` 中须排在 `PlayerMoveComponent` 之后**，这样每帧 FixedUpdate 才是：**先 Move 写速度 → 再 Town 夹纵深/多边形/分离/横移速度**。  
- 同一帧内：**脚穿透分离之后再做横移速度夹紧**，避免分离改位形后仍保留「指向障碍内」的 `vx`。

---

## 4. 与旧文档《场景遮挡与 Walk 区内障碍碰撞》的关系

同目录下的 **`村庄探索_场景遮挡与Walk区内障碍碰撞.md`** 中 **§4** 仍描述旧策略（**障碍非 Trigger + 矩阵仅碰 PlayerFoot + 横移靠物理顶停**）。**自方案 1 合入后**，Walk 区内障碍请以 **本文档 + 0514 执行文档** 为准；旧文可逐步修订 §4 表述，避免新人按旧矩阵去配关。

**遮挡排序**（`VillageSceneObjectDepthSort` 等）与本文 **无关**，仍按遮挡文档执行。

---

## 5. 验收要点（QA 速查）

| 编号 | 操作 | 期望 |
|------|------|------|
| Q1 | 进村，贴楼梯/斜边 **A/D 横移** | 不穿障，可稳定贴边 |
| Q2 | 同位置 **W/S 纵深** | 可被障碍挡住，与 WalkArea 组合无异常穿出 |
| Q3 | **离村 / 非村庄场景** | 无因全局矩阵误伤战斗穿模（若战斗共用 `PlayerFoot`，须在 PR/OPEN_QUESTIONS 中单独立项） |
| Q4 | 障碍为 **Trigger** 时 | 纵深与横移阻挡仍生效（依赖 `useTriggers`） |

调试：可在 `TownPlayerLocomotion` 上短时打开 **`villageObstacleDepthDebugLog`**，看 `[VillageBlockerDepth]` 中带 **`horizontal vx clamp`** 等日志（用完关闭，避免刷屏）。

---

## 6. 维护记录

| 日期 | 说明 |
|------|------|
| 2026-05-14 | 初版：对应 0514 方案 1 合入（矩阵全 Ignore + Trigger 查询 + 横移 Cast 夹紧） |
