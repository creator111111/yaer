# Cursor Agent Prompt · KenMuNi1：地图边缘双机切换硬切

> **角色**：【架构侦探】只读查清边缘硬切是「切机」还是「围栏顶死」，并给出最小优化；报告通过后再施工  
> **日期**：2026-09-20  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **现象（用户截图）**：人走到**地图边缘**（镜头红框贴着绿线，再往外已是灰底空位）时，两台摄像机切换会**硬切**，不够丝滑  
> **产品期望（钉死）**：Street ↔ Part3 的切换在边缘也要能看出是**慢慢过渡**，不要瞬间跳一下。切换该发生的地段可以微调，但不要改成全村一套相机  
> **不是**：改日常左右走路的 XDamping（0919 另案）；改出店揭幕 `smoothTime`（0920 出店案）；重做 CameraArea 形状当跟拍；改 Part3 上下跟随的产品（高台仍要跟纵深）  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_KenMuNi1_地图边缘双机硬切_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「硬切那一帧有没有切 Priority」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 两台镜头一切换，在地图边上会猛地跳一下。  
> 用户以为是不是该改感应区。结论预判：**只挪区域消不掉硬跳**；区域只决定在哪切。硬跳来自两台机位差 + 混合太短，贴边时围栏再把镜头钉死，跳得更明显。

### 现网怎么切（0831 已施工）

```
玩家 XY 进/出 Map/CameraDepthFollowZone_Part3
  → VillageCameraDepthFollowZone.LateUpdate
  → 只改 Priority（Street 10 / Part3 激活 20、待命 0）
  → Brain 用 KenMuNi1_StreetPart3_Blends
       Street ↔ Part3，EaseInOut，0.4 秒
```

| 东西 | 现网 |
|------|------|
| 感应区 | `CameraDepthFollowZone_Part3`，盒约中心 (-133, 21)、大小 80×58；滞回 0.35；冷却 0.4 秒 |
| 两台机 | `VCam_Street`、`VCam_Part3`；Body 写死在 Inspector，切机**不**再 Apply Framing |
| 构图差 | 街机几乎不跟 Y（DeadZoneHeight=1）；Part3 ScreenY≈0.88。切的瞬间理想机位往往差一截 |
| 围栏 | 两台 Confiner **共用** `CameraArea`。贴边时机位被钳住 |
| 待命更新 | `StandbyUpdate = Always`，避免待命机位置发霉后再跳 |

### 边缘硬切要先分成两种（报告①必须写死）

| 类型 | 怎么认 | 优化重心 |
|------|--------|----------|
| **A. 贴边时发生了切机** | 硬切瞬间 Console 有 `part3Live=true/false`（须开 `logStateTransitions` 或从代码推断边界与玩家坐标相交） | 切换点往**地图内侧**挪，避开贴死围栏时切；同时加长 Blend |
| **B. 没切机，只是围栏顶住** | 走到外缘镜头突然钉死，Priority 没变 | 略放大 `CameraArea` 外缘 / 调 Confiner 阻尼；**不要**拿 Zone 当主药 |
| **C. 又切又顶** | 人在绿线附近，同时跨过 Zone | A+B 都做，但 Zone 与围栏不要叠在同一条线上 |

用户截图：人在红框内，右侧绿线外已是灰底。**高度可疑是 C 或 B**。侦探用场景坐标对一下：硬切点的玩家 XY 是否同时贴 `CameraArea` 边、又在 Zone 边上。

### 优化默认（侦探可改口，但须写否决原因）

1. **主**：`KenMuNi1_StreetPart3_Blends.asset` 双向 `m_Time` 从 0.4 提到约 **0.8～1.2**；Zone `modeSwitchCooldownSeconds` 不少于 Blend，避免混到一半再切。  
2. **若 A/C**：把 Zone 的切换边从「贴 CameraArea 外缘」往里收，让切机发生在镜头还没被围栏钉死的地方。不要只改滞回 0.35 来假装平滑。  
3. **若 B/C 仍硬**：`CameraArea` 外缘略放开半步镜头空间（人走不到也行）。两台共用这一块，改一处即可。  
4. **不要**：删双机改回单机每帧改 Body；全局 `smoothTime=0`；在 Update 里手推机位。

### 禁止

- 本阶段不改场景、不改 Blend 资源、不改代码。  
- 不要把「挪 Zone」写成唯一修复。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/8月/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md
@Assets/Doc/执行文档/8月/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工执行说明.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/GameRes/Config/Camera/KenMuNi1_StreetPart3_Blends.asset
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab、Blend 资源。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_KenMuNi1_地图边缘双机硬切_架构溯源报告.md

---

## 背景（策划白话）

Village_KenMuNi1 左侧高台（Part3）和街道（Street）是两台虚拟相机，人走进感应区就切。
在地图边缘，这个切换会硬切，不够丝滑。用户截图是镜头已经贴着地图外缘（外面是灰底）。

要优化成：边缘切换也能看出来是过渡，不要瞬间跳。
可以微调「在哪切」，但不要拆掉双机，不要改成全村同一套跟拍。

---

## 必读 / 优先扫描

0831 双机文档作对照，以场景 YAML 和现网代码为准。

### A. 先钉硬切是 A / B / C

用数字写清，不要只说「在边缘」：

1. `CameraDepthFollowZone_Part3` 的世界包围盒（中心、Size、外缘 X/Y）
2. `CameraArea` 在同一侧的边界（贴灰底的那条边的世界坐标）
3. 两台 VCam 的名字是否仍是 `VCam_Street` / `VCam_Part3`（Blend 资产按名字匹配，对不上就会 Cut）
4. Brain 是否引用 `KenMuNi1_StreetPart3_Blends`，双向 Time 是否仍是 0.4、Style 是否 EaseInOut
5. 贴边那一帧：玩家根坐标是否同时在 Zone 边上、又在 Confiner 边上

结论写成 A（切机）、B（只顶围栏）、或 C（又切又顶）。

### B. 为什么 0.4 秒在边缘像硬切

对比两台在「贴边那一点」各自想去的机位差：

- ScreenY / DeadZoneHeight / Bias（街机不跟 Y，Part3 ScreenY≈0.88）
- Orthographic Size 是否不同
- Confiner 把两台都钳在同一条边上时，Blend 的起点和终点差多少

写明：加长 Blend 能缓解多少；若两台理想 Y 差很大，只加时间仍会觉得「拖着跳」，要不要把切换边挪进围栏内侧。

### C. 推荐一种最小优化

只推一种组合，并写清改哪些资产的哪些字段（全路径）：

必须同时满足：

1. 边缘切换不再是瞬间硬切（有可见过渡）
2. 不拆双机，不每帧改 Framing Body
3. 不改村里日常横移阻尼当主修复
4. 高台纵深跟随产品不变
5. 滞回和冷却仍能防边界来回抖；冷却不要短于新的 Blend 时长

另外两种各用一句话否决（例如「只挪 Zone 不改 Blend」「只把 CameraArea 放大一圈当唯一修复」）。

---

## 报告结构（固定）

① 结论一句话（A/B/C + 改 Blend / Zone / CameraArea 里的哪几项）  
② 原因（大白话 + 贴边时两台机位差）  
③ 用户需要做什么（走到截图那个外缘，看是切机跳还是纯贴边跳；勾 log 则看 part3Live）  
④ 给施工员的补充：文件、字段、推荐数值、不要改什么

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_KenMuNi1_地图边缘双机硬切_架构溯源报告.md
@Assets/GameRes/Config/Camera/KenMuNi1_StreetPart3_Blends.asset
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告的推荐字段做最小改。不要拆双机，不要每帧改 Framing，不要改出店 smoothTime，不要改日常 XDamping。

目标：
- 地图边缘 Street ↔ Part3 切换能看出过渡，不要瞬间硬切
- 若报告要求把切换边挪离围栏，只挪报告写的那一条边
- 冷却时间不要短于新的 Blend 时长

限制：
- 禁止在 Update 里手推相机
- 注释或施工说明写明：Zone 只决定在哪切，丝滑靠 Blend 时长和贴边时机位差
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_KenMuNi1_地图边缘双机硬切_施工说明.md`

完成后用大白话给验收清单：走到截图那条地图外缘来回走两次，看切换是否还硬跳；再到不贴边的 Zone 边界走一次，确认不会来回抖切。
```
