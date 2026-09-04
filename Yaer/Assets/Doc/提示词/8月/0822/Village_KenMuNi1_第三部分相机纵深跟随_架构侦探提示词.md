# Cursor Agent Prompt · Village_KenMuNi1：第三部分 CameraArea 摄像机纵深（Y）跟随

> **角色**：先【架构侦探】只读溯源现网相机链 + 方案对比，报告拍板后【施工员】实现  
> **日期**：2026-08-22  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **产品需求（开发者）**：摄像机现由 **CameraArea** 约束；需要在场景 **第三部分**（CameraArea **较高/较大** 的树屋一带，见截图绿框）玩家 **上下移动**（村庄 2.5D **纵深 Y**）时，摄像机 **跟随**，而不是只跟左右或锁死在某一纵深。  
> **本阶段侦探**：只读；不改场景 / 代码 / Prefab

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 这个场景的摄像机跟随有个新功能：上下跟随。现在受 CameraArea 控制，我不知道怎么跟随玩家的，但需要在 **第三部分 CameraArea 大的地方**，玩家上下移动时做摄像机跟随。

### 术语钉死（避免侦探与施工误解）

| 词 | 在本项目 `Village_KenMuNi1` 的含义 |
|----|-------------------------------------|
| **上下移动** | 玩家 **纵深 Y**（`TownPlayerLocomotion` / `Rigidbody2D.position.y`），**不是**跳起跳的屏幕高度 |
| **CameraArea** | 带 **`PolygonCollider2D`** 的空物体，绑到 **`CinemachineConfiner.m_BoundingShape2D`**，限制机位 **可移动范围** |
| **跟随** | VirtualCamera **Follow 玩家** + Body（Framing Transposer）在 DeadZone/SoftZone 外 **追踪目标 Y**；Confiner **不负责跟拍**，只 **裁剪** |

生活类比：CameraArea 是「摄像机活动围栏」；Framing Transposer 是「镜头要不要跟着人走」。围栏已经圈大了（第三部分），但镜头可能 **只跟人左右走、纵深方向钉死**。

### 现网相机链（磁盘预扫，侦探须 YAML 复核）

```
BaseGameSceneManager.InitPlayer
  → CameraComponentGSM.SetFollow(player.transform)
  → CameraComponent.SetFollow → vcam.Follow = Player

Cinemachine VirtualCamera（SceneManager/Camera 下）
  ├─ Body: CinemachineFramingTransposer（cm 子物体）
  │     m_XDamping: 0.7
  │     m_YDamping: 0          ← ★ 纵深轴阻尼为 0，须查 CM 语义
  │     m_DeadZoneHeight: 1    ← ★ 死区高度=1（可能几乎不跟 Y）
  │     m_SoftZoneHeight: 2
  │     m_ScreenX/Y: 0.5
  └─ CinemachineConfiner
        m_BoundingShape2D → CameraArea 的 PolygonCollider2D（fileID 1692901282550724227）

CameraArea（单块，L 形多边形）
  localPosition ≈ (-32.82, 0)
  顶点概要：左侧高台 x∈[-172,-92] y∈[-7.7, 50.4]；右侧低区 x∈[-92, 66] y∈[-7.7, 8]（局部坐标）
  → 与用户截图「第三部分左侧树屋区更高」一致
```

**关键代码**：

- `CameraComponent.cs`：`SetFollow` / `ChangeCameraBoundingArea(Collider2D)`
- `CameraComponentGSM.cs`：GSM 封装
- `02_SYSTEM_SPEC.md` §3：跟拍走 `SetFollow`，禁固定 Wait
- **无** `Village_KenMuNi1` 专用「分区改 Y 跟随」脚本（预扫）

### 先例（可复用模式）

| 先例 | 路径 | 做了什么 |
|------|------|----------|
| 森林东树桥换包围盒 | `TreeBridgeLogic` + `ForestEastTreeBridgeStoryMgr` | `ChangeCameraBoundingArea(new/old)` **切换 Confiner 多边形** |
| NodeCanvas 改包围 | `CameraChangeBoundingArea.cs` | 剧情里换 `BoundingShape2D` |
| 编辑器生成矩形 CameraArea | `CameraAreaEditor.cs` | MapLeft/Right + MapHeight 生成 Polygon |

**注意**：先例只换 **围栏**，未显式切换 **Y 跟随参数**；本期可能要 **围栏 + Framing Transposer 参数** 双改。

### 第三部分范围（侦探须 Scene 视图划定）

用户截图：绿框 **L 形 CameraArea** 的 **左侧高段**（大树 / 树屋 / 石拱门 / 精灵池上方平台）。  
侦探须在报告中：

1. 用 **世界 X/Y 区间** 定义「第三部分」（例如 x < ? 且 y > ?）  
2. 标 `House_Npc45`、树屋楼梯、`DepthZone` 是否落在该区间  
3. 说明与 **右侧低区**（y 顶约 8）行为差异期望

### 侦探须回答的核心问题

1. **现网为什么不跟 Y？** 是 `YDamping=0`、`DeadZoneHeight=1`、还是 Follow 未绑 / 村开场 `CancelFollow`？  
2. **只放大 CameraArea 够不够？**（预判：**不够**，Confiner 只扩围栏不开启跟 Y）  
3. **第三部分要不要与全村不同参数？**（产品倾向：**仅大区跟 Y**，小区保持现手感）  
4. **最小改动路径**：纯场景调 CM 参数 vs 触发器分区 vs 新组件 vs 第二 VCam

### 须比较的方案

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A** | 全局调 Framing Transposer：`YDamping>0`、`DeadZoneHeight` 降低 | 最快 | 全村手感变；右侧低区可能晃 |
| **B（推荐候选）** | 第三部分加 **Trigger 区** + 小脚本/`VillageCameraZone`：Enter 调 Y 跟随参数 + 可选换 Confiner 子多边形；Exit 恢复 | 分区精确；可复用 ForestEast 换框思路 | 要划 Trigger、防抖动 |
| C | 复制第二套 VirtualCamera（仅第三部分启用 Blend） | CM 原生 | 维护两套 vcam；Blend 复杂 |
| D | 扩展 `CameraComponent.ChangeCameraBoundingArea` 同时改 YDamping | 集中 API | 改 C#，回归面中等 |
| E | 用 `PlayerOffsetCameraFollow` 代替 CM | 文档说与 CM 冲突 | ❌ 与现网 CM 栈打架 |

### 严禁

- 把 **Z 轴位移** 当村庄纵深（违 `02_SYSTEM_SPEC`）  
- 未读 Framing Transposer 就断定「CameraArea 坏了」  
- 全村改 Y 跟随却不评估右侧低区回归  
- 用 `Wait` 硬对齐相机（违规范 §3）  
- 开场剧情 `CancelFollow/SetLock` 状态未纳入验收

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/开发日志/0513/开发进度与明日待办.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Camera/CameraChangeBoundingArea.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestEastScene/TreeBridgeLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Editor/Tool/Map/CameraAreaEditor.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/ForestEastScene.unity

你现在是【架构侦探】。Unity 2020.3.48f1 + Cinemachine。
禁止改场景/代码。只读 + 写溯源报告。

---

## 背景

肯姆尼村场景第三部分（树屋高台区）CameraArea 已圈得较高，但玩家纵深上下走时镜头不跟或跟得不对。要摸清现网跟随链，并给出 **仅在该区启用 Y（纵深）跟随** 的最小方案。

---

## 侦探任务清单

### A. 现网相机栈对拍表

| 检查项 | Village_KenMuNi1 现网 | 证据 |
|--------|----------------------|------|
| Main Camera + Brain | | |
| VirtualCamera Follow 目标 | | |
| Body 类型 / Framing Transposer 参数 | XDamping/YDamping/DeadZoneH/SoftZoneH | |
| Confiner → CameraArea | | |
| CameraArea 多边形顶点范围 | | |
| `CameraComponentGSM` / `cameraComponent` 绑定 | | |
| 村开场 `CancelFollow`/`SetLock` 条件 | `homeDoorStoryComplete` | |

### B. 「跟随」机制拆解

1. **谁**在跟：CM Follow vs `CameraComponent` 手推 SmoothDamp  
2. **跟哪些轴**：X / Y（纵深）/ Z（应冻结）  
3. **CameraArea 作用**：仅 Confiner 还是另有脚本读 Collider  
4. Play：玩家在第三部分 **只按 W/S**（纵深）时，记录 **vcam.position.y** 是否变化

### C. 第三部分区域定义

- 根据 CameraArea L 形 + 美术分区，给出 **世界坐标 AABB 或 Trigger 建议**  
- 标树屋、Npc45、精灵池与区域关系  
- 与 **右侧低区**（y 顶约 8）对比：为何需要 **分区** 而非全局开 Y

### D. 根因裁定

| ID | 假说 |
|----|------|
| H1 | Framing Transposer **YDamping=0 + DeadZoneHeight=1** 导致纵深不跟 |
| H2 | Confiner 已够大，但 Body 不跟 Y（围栏≠跟拍） |
| H3 | 村开场相机锁定未释放 |
| H4 | Follow 目标不是 Player 根节点 |
| H5 | 其它（须证据） |

### E. 方案对比 + 推荐

- 主推方案（A/B/C/D）与施工步骤  
- 若用 Trigger：Collider 放哪、Enter/Exit 恢复哪些参数  
- 若扩 API：是否在 `CameraComponent` 增 `SetFramingTransposerYFollow(bool)` 等  
- **参数建议表**（YDamping、DeadZoneHeight、SoftZoneHeight 初值）

### F. 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 第三部分内 **W/S 纵深移动** | 摄像机 Y **明显跟随** |
| 2 | 第三部分内 **A/D 横移** | 仍正常；不抖不 lag 过度 |
| 3 | 右侧低区（村街主道） | 手感 **不劣化**（或符合产品分区预期） |
| 4 | 进出第三部分 Trigger 边界 | 无相机跳变 / 抖动 |
| 5 | CameraArea 绿框 | 机位 **不越界** |
| 6 | 村开场剧情后 | 跟拍正常恢复 |
| 7 | 进屋/出屋换场返回 | 相机无 MissingReference / 不跟丢 |

### G. 开放问题

`OPEN_QUESTIONS.md`「KenMuNi1 第三部分相机纵深跟随 · 2026-08-22」（如是否全村统一 Y 跟、OrthographicSize 是否随区变化）

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md`

结构：① 结论 ② 现网为何不跟 Y ③ 第三部分范围 ④ 方案+参数 ⑤ 验收

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。按报告实现第三部分纵深（Y）相机跟随。

必须遵守：
- 村庄 Y=纵深，不用 Z 位移；
- 优先报告推荐的最小方案（场景调 CM / 分区 Trigger / 扩展 CameraComponent 三选一）；
- 第三部分内 W/S 时 vcam 跟 Y；右侧低区回归报告要求；
- Confiner 仍绑 CameraArea，不越界；
- 若改 C#：中文注释说明与 ForestEast 换框先例关系；
- 村开场 CancelFollow/SetLock 逻辑不破坏；
- 禁止 Wait 硬对齐相机。

提交说明：改了哪些 CM 参数 / Trigger / 脚本、第三部分坐标范围、验收结果。
```
