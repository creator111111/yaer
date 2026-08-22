# Village_KenMuNi1 — 第三部分 CameraArea 摄像机纵深（Y）跟随 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读溯源 + 施工指引  
**Unity**：2020.3.48f1 + Cinemachine  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**产品需求**：第三部分（CameraArea **左侧高台 / 树屋** 绿框区）玩家 **纵深 Y（W/S）** 移动时，摄像机应 **跟随**，而非只跟左右或锁死纵深。

关联提示词：`Assets/Doc/提示词/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构侦探提示词.md`

---

## ① 结论一句话

**现网 `CinemachineFramingTransposer` 设了 `m_DeadZoneHeight: 1`（纵向死区占满屏），纵深方向机位 **基本不跟拍**；`CameraArea` / Confiner 只 **圈围栏**，放大第三部分 **不能** 自动开启 Y 跟随。主因 **H1+H2**。推荐 **方案 B+D**：在第三部分（世界 **x ≤ -93** 左翼高台）加 **Trigger 区**，Enter 将 `DeadZoneHeight→0`、`YDamping→0.7`；离开右街低区 **恢复** 现网参数，避免全村手感劣化。**

---

## ② 现网为何不跟 Y

### 2.1 相机栈对拍表（磁盘 YAML）

| 检查项 | Village_KenMuNi1 现网 | 证据 |
|--------|----------------------|------|
| Main Camera + Brain | ✅ `Main Camera` + `CinemachineBrain`（LateUpdate） | `1132163533838781009` / `1022` |
| VirtualCamera | `SceneManager/Camera/Virtual Camera` | `1132163533271977016` |
| **Follow 目标** | 运行时由 `CameraComponent.SetFollow(player)` 绑定 | `BaseGameSceneManager.InitPlayer` L337；YAML 默认 `m_Follow: {fileID: 0}` |
| Body | **CinemachineFramingTransposer**（`cm` 子物体） | `1132163534260424590` |
| **m_XDamping** | **0.7** | 横向跟拍有阻尼 |
| **m_YDamping** | **0** | 纵轴无阻尼（**≠**「不跟」；跟了会瞬移） |
| **m_DeadZoneHeight** | **1** ★ | **纵向死区=整屏高度 → 纵深几乎不触发跟拍** |
| m_SoftZoneHeight | 2 | 软区较大 |
| m_DeadZoneWidth | 0 | 横向无死区 → **A/D 正常跟** |
| Confiner | `CinemachineConfiner` → `CameraArea` PolygonCollider2D | `1692901282550724227` |
| `CameraComponent` 绑定 | `SceneManager/Camera` 上 `CameraComponent` | `5659318442337644682`；`smoothTime=0.3` |
| 村开场锁相机 | `homeDoorStoryComplete==false` → `CancelFollow` + `SetLock(true)` | `Village_KenMuNiSceneManager` L126–129 |

**对照（室内已验证跟 Y）** — `Village_HomeScene1.unity` Framing Transposer：

| 参数 | HomeScene1 | KenMuNi1 |
|------|------------|----------|
| m_YDamping | **1** | **0** |
| m_DeadZoneHeight | **0** | **1** ★ |
| m_XDamping | 1 | 0.7 |

**Cinemachine 语义（钉死）**：

- **Follow + Framing Transposer** = 跟拍（含 Y 纵深轴）。  
- **DeadZoneHeight** = 屏幕空间纵向死区比例（0～1）。**=1 时玩家整屏上下走都不拉镜头**。  
- **Confiner + CameraArea** = 只 **裁剪** 机位可活动范围，**不负责** 是否跟 Y。  
- **`CameraComponent` 手推 SmoothDamp** 仅在 `SetFollow(forceSnap)` 切目标时用，**不替代** 日常跟拍。

### 2.2 跟随机制拆解

```
InitPlayer
  → CameraComponentGSM.SetFollow(player.transform)
  → virtualCamera.Follow = Player

每帧（homeDoorStoryComplete 后）：
  CinemachineBrain.LateUpdate
    → FramingTransposer：目标偏离 DeadZone/SoftZone 时移动 vcam
    → Confiner：把 vcam 钳在 CameraArea 多边形内

第三部分只放大 CameraArea：
  → 仅扩大 Confiner 围栏（机位 *可以* 到更高 y）
  → DeadZoneHeight 仍为 1 → 玩家 W/S 仍 **不拉镜头**  ❌
```

### 2.3 假说裁定

| ID | 假说 | 裁定 |
|----|------|------|
| **H1** | `DeadZoneHeight=1` 导致纵深不跟 | ✅ **主因** |
| **H2** | Confiner 够大但 Body 不跟 Y | ✅ **机制澄清**（围栏≠跟拍） |
| H3 | 村开场 `CancelFollow/SetLock` | ⚠️ 仅 **首次进村剧情前**；`homeDoorStoryComplete` 后 `InitPlayer` 会 `SetFollow`；剧情 Node 会 `SetLock(false)` |
| H4 | Follow 未绑 Player | ❌ 正常进村后已绑 |
| H5 | 其它脚本读 CameraArea 推机位 | ❌ 预扫 **无** 村专用跟 Y 脚本 |

### 2.4 Play 观测表（施工员实测填空）

| 阶段 | 第三部分只按 W/S | 右街低区只按 W/S | vcam.y 是否变 |
|------|------------------|------------------|---------------|
| 现网 | 玩家 y 变 | 玩家 y 变 | **预期：vcam.y 基本不变** |
| 方案 B 后 | 玩家 y 变 | 玩家 y 变 | **第三部分：vcam.y 跟随**；低区：仍不变 |

> 记录方式：Inspector 看 `Virtual Camera` Transform.y，或临时 `Debug.Log`。

---

## ③ 第三部分范围定义

### 3.1 CameraArea L 形（世界坐标）

`Camera` 根 **(32.56, 0)** + `CameraArea` **(-32.82, 0)** ≈ 世界原点对齐。多边形顶点（世界）：

| 顶点 | 世界 (x, y) | 分区 |
|------|-------------|------|
| 右下 | **(65.85, -7.68)** | 低区底边 |
| 右上门槛 | **(65.84, 8.02)** | 低区顶 |
| **L 形内角** | **(-92.57, 7.66)** | 低区左界 / 高区右界 |
| 高区顶 | **(-92.73, 50.26)** | 第三部分上界 |
| 高区左上 | **(-172.75, 50.37)** | 第三部分 |
| 高区左下 | **(-172.61, -7.71)** | 第三部分底 |

**示意图**：

```
世界 Y
 50 ┤     ┌──────────── 第三部分（左翼高台）
    │     │
  8 ┤─────┘  ┌──────────────── 右街低区顶
    │        │
 -8 ┤────────┴────────────────── 底边
    └──────────────────────────── 世界 X
         -173      -93        66
```

### 3.2 第三部分判定（施工用）

| 项 | 建议 |
|----|------|
| **第三部分（跟 Y）** | 世界 **x ≤ -93** 且 **y ∈ [-8, 51]**（CameraArea 左翼多边形内） |
| **右街低区（保持现手感）** | **x > -93**，y 顶约 **8** |
| `House_Npc45` | **(-4.39, 5.67)** → **右低区**（本期 **不强制** 跟 Y，除非产品扩区） |
| 树屋 / 石拱门 / 精灵池高台 | **x < -93** 一带 → **第三部分** |
| `DepthZone&Colliders` | 树屋区有独立纵深障碍；**与相机跟 Y 正交**（排序/挤出，不替代 CM） |

### 3.3 为何要分区而非全村开 Y

右街低区 y 跨度仅 **≈16 单位**（-7.7～8），产品现手感为 **横移主、纵深稳**；全村 `DeadZoneHeight→0` 会让主街 W/S 也拉镜头（**晃**）。第三部分 y 跨度 **≈58**，不跟拍会 **丢目标**。

---

## ④ 方案对比与施工

### 4.1 方案表

| 方案 | 做法 | 裁定 |
|------|------|------|
| A | 全村 `DeadZoneHeight=0`、`YDamping=0.7` | ⚠️ 最快验证；**右街回归风险** |
| **B（推荐）** | 第三部分 **Trigger** + 进出切换 Framing 参数 | ✅ **本期主方案** |
| C | 第二套 VirtualCamera + Blend | ❌ 维护成本高 |
| **D（配合 B）** | `CameraComponent` 增 API 集中改 Framing Transposer | ✅ **小改 C#**，可复用 |
| E | `PlayerOffsetCameraFollow` | ❌ 与 CM 栈冲突 |

### 4.2 参数建议表

| 参数 | 全村现网（右低区恢复值） | 第三部分跟 Y（Enter） | 参考 HomeScene1 |
|------|-------------------------|----------------------|---------------|
| **DeadZoneHeight** | **1** | **0** | 0 |
| **YDamping** | **0** | **0.7**（与 XDamping 对齐，可试 1.0） | 1 |
| **XDamping** | 0.7 | **0.7**（不变） | 1 |
| **SoftZoneHeight** | 2 | **0.8～1.0**（可选，减轻纵轴 lag） | 0.8 |
| Confiner | CameraArea 整块 | **不改**（已圈高台） | — |

### 4.3 施工步骤（方案 B + D）

#### C#（`CameraComponent` / `CameraComponentGSM`）

新增（命名可微调）：

```csharp
/// <summary>
/// 村庄探索：切换 Framing Transposer 纵深（Y）跟拍强度。
/// 原因：全村 DeadZoneHeight=1 时纵深不跟；仅第三部分需要跟 Y。
/// 替代方案：复制第二套 VCam（方案 C）——维护成本高。
/// </summary>
public void SetFramingTransposerDepthFollow(
    bool followDepthY,
    float yDamping = 0.7f,
    float deadZoneHeightWhenOff = 1f)
{
    var ft = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    if (ft == null) return;
    if (followDepthY)
    {
        ft.m_DeadZoneHeight = 0f;
        ft.m_YDamping = yDamping;
        ft.m_SoftZoneHeight = 0.8f; // 可选，对齐 HomeScene1
    }
    else
    {
        ft.m_DeadZoneHeight = deadZoneHeightWhenOff;
        ft.m_YDamping = 0f;
        ft.m_SoftZoneHeight = 2f;    // 恢复现网
    }
}
```

`CameraComponentGSM` 透传同名方法。

#### 场景（`Village_KenMuNi1.unity`）

1. 在 `Map` 或 `Camera` 下新建 **`CameraDepthFollowZone_Part3`**（Layer 建议与 Walk 触发一致）。  
2. 挂 **BoxCollider2D（IsTrigger）**，建议初值：  
   - **Center (x, y) ≈ (-133, 21)**  
   - **Size (w, h) ≈ (80, 58)**  
   - 覆盖 L 形 **左翼**（x∈[-173,-93]，y∈[-8,50]），**勿盖住** 右街 x>-93。  
3. 挂脚本 **`VillageCameraDepthFollowZone`**（新建，参考 `ForestEastTreeBridgeStoryMgr.ChangeCameraBoundingArea` 先例）：  
   - `OnTriggerEnter2D`（Player）→ `SetFramingTransposerDepthFollow(true)`  
   - `OnTriggerExit2D` → `SetFramingTransposerDepthFollow(false)`  
   - **防抖**：可用 `OnTriggerStay` + 滞后 0.1s，或合并相邻 Trigger 避免边界抖。  
4. **不改** `CameraArea` 多边形（已够高）；**不改** Confiner 绑定。  
5. **禁止** `Wait` 硬对齐相机（违 `02_SYSTEM_SPEC` §3）。

#### 快速验证（方案 A，仅 Play 摸底）

Inspector 将全村 `DeadZoneHeight→0`、`YDamping→0.7`：若第三部分立刻跟 Y → **证实 H1**；**勿当最终方案提交**。

### 4.4 严禁

- 用 **Z 轴** 做村庄纵深  
- 未读 Framing Transposer 就改 CameraArea 形状当「跟拍修复」  
- 全村开 Y 跟却不测右街低区  
- 破坏村开场 `CancelFollow/SetLock` 契约

### 4.5 最小改动文件

| 文件 | 动作 |
|------|------|
| `CameraComponent.cs` | 增 `SetFramingTransposerDepthFollow`（方案 D） |
| `CameraComponentGSM.cs` | 透传 |
| **新建** `VillageCameraDepthFollowZone.cs` | Trigger 进出（方案 B） |
| `Village_KenMuNi1.unity` | 放 Trigger + 挂脚本 |
| `CameraArea` 多边形 | **本期不改**（已含高台） |

---

## ⑤ 用户验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 第三部分内 **W/S 纵深** | 摄像机 Y **明显跟随** |
| 2 | 第三部分内 **A/D 横移** | 正常，无过度 lag |
| 3 | 右街低区（x>0 主道）W/S | 手感 **不劣化**（或符合分区预期） |
| 4 | 进出第三部分 Trigger 边界 | **无跳变/抖动** |
| 5 | CameraArea 绿框 | 机位 **不越界** |
| 6 | 村开场剧情后（`homeDoorStoryComplete`） | 跟拍恢复 |
| 7 | 进屋/出屋返回村 | 无 MissingReference；Follow 不丢 |

---

## ⑥ 开放问题

见 `OPEN_QUESTIONS.md` §「KenMuNi1 第三部分相机纵深跟随 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0：H1 DeadZoneHeight=1；方案 B+D；第三部分 x≤-93 |
