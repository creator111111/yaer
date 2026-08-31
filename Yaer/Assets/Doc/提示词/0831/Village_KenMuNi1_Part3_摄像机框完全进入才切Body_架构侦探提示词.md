# Cursor Agent Prompt · KenMuNi1 Part3：摄像机框完全进入 Zone 才切换 Body（ScreenY=0.88）

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】最小化落地  
> **日期**：2026-08-31  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **物体**：`Map/CameraDepthFollowZone_Part3`（Scene 绿框范围；用户已在调）  
> **产品规则（用户实测钉死）**：  
> **当「摄像机画面框」（白框 / Orthographic 可视矩形）完全进入该绿色范围时**，把 Virtual Camera 的 **Body = Framing Transposer** 改成下方数值；  
> **未完全进入**（有一边还在绿框外）→ 恢复右街默认 Body（现网街道路）。  
> **本阶段（侦探）**：只读；禁止改代码 / 场景  
> **报告落盘**：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末，**须等报告拍板后再开**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 与旧方案的关键区别（改口）

| 旧（0822 / 现网 `VillageCameraDepthFollowZone`） | 新（本需求） |
|--------------------------------------------------|--------------|
| **玩家脚**进 Trigger / `bounds.Contains(玩家)` → 切 Profile | **摄像机可视框完全 ⊆ Zone 绿框** → 切 Body |
| Part3 Profile：`deadZoneHeight=0`、`yDamping=0.7`、`biasY≈-4.58` | 用户实测新表（见下）；**Screen Y = 0.88** 是关键 |
| Zone 盒曾过大（中心 -55、宽 200，盖到右街） | 绿框以用户 Scene 为准；报告须核对世界范围，过大则写进施工「缩盒」 |

**判定主语是摄像机，不是角色。** 玩家已进区但镜头还露在绿框外 → **不要**切；镜头整框都进了 → 才切。

### 用户实测 · 进入后 Body 目标值（Inspector 截图 · 钉死）

Play 中 Follow 已是 Player（运行时 `SetFollow`，编辑器空属正常）。Body = **Framing Transposer**：

| 字段 | 目标值 |
|------|--------|
| Tracked Object Offset | (0, 0, 0) |
| Lookahead Time / Smoothing | 0 / 0 |
| Lookahead Ignore Y | 否 |
| **X Damping** | **0.7** |
| **Y Damping** | **0** |
| Z Damping | 1 |
| Target Movement Only | 是 |
| **Screen X** | **0.5** |
| **Screen Y** | **0.88** ← 用户高亮 |
| Camera Distance | 10 |
| Dead Zone Width / Height / Depth | **0 / 0 / 0** |
| Unlimited Soft Zone | 否 |
| Soft Zone Width | **0.25** |
| Soft Zone Height | **0.351** |
| Bias X | **0.5** |
| Bias Y | **0.5** |
| Center On Activate | 是 |

离开（摄像机框未完全在区内）→ 恢复 **右街默认**（现网磁盘 / `KenMuNiStreetDefault` 口径）：

| 字段 | 街道路（预扫） |
|------|----------------|
| Dead Zone Height | **1** |
| Y Damping | **0** |
| Soft Zone Height | 现网 VCam 约 **1** 或 Profile **2**（侦探对拍后定一处，避免进出抖） |
| Screen Y | **0.5** |
| Bias X / Y | **0 / 0** |

### 现网锚点（预扫）

| 层 | 路径 |
|----|------|
| Zone | `Map/CameraDepthFollowZone_Part3` + `VillageCameraDepthFollowZone` |
| 写参 API | `CameraComponent.ApplyFramingTransposerProfile` / `SetKenMuNiPart3CameraMode` |
| Profile 结构 | `CinemachineFramingProfile`（已有 screen/dead/soft/damping/bias；**未必覆盖** Camera Distance / CenterOnActivate / TargetMovementOnly——侦探列缺口） |
| Follow | `InitPlayer` → `SetFollow(player)`；Body 组件常驻，不「开始后再加 Body」 |
| Confiner | `CameraArea` 多边形 = 机位围栏，**≠** 本绿框；勿混 |

### 判定算法倾向（侦探拍板）

```
每帧（建议 LateUpdate，跟 Brain 同相）：
  camRect  = 正交相机世界空间 AABB
             （center=vcam/brain 输出位，半高=orthoSize，半宽=orthoSize*aspect）
  zoneRect = Zone Collider2D.bounds（或专用 Polygon）

  fullyInside = zoneRect.Contains(camRect.min) && zoneRect.Contains(camRect.max)
                // 或四角都在内；2D 轴对齐盒用 min/max 即可

  if fullyInside != _part3Active → Apply 对应 Profile
```

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · 摄像机世界 AABB ⊆ Zone.bounds** | 上式 | ✅ 推荐（对齐「白框完全进绿框」） |
| B · 仍用玩家进 Trigger | 现网 | ❌ 产品已否 |
| C · 第二套 VCam + Blend | — | ❌ 本期不需要 |
| D · 扩展 Profile 字段后整组 Apply | 补 ScreenY=0.88 等 | ✅ 配合 A |

**滞回（防边界抖）**：可要求「完全进入」用内缩 0.1～0.5 单位；「离开」用原盒——侦探写进清单，勿无滞回硬切。

### Zone 几何（施工前核对）

磁盘预扫曾：`localPos (-55,21)`，`BoxCollider2D size (200,58)` → 世界 X 约 **[-155, +45]**，**盖过右街**。  
用户 Scene 绿框若已手改，**以当前场景为准**；若仍过大，报告写「须缩到左翼高台 / 与绿框一致」，否则「完全进入」在右街也可能误触发。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 判定改为「摄像机框完全 ⊆ Zone」 | ❌ 玩家脚 Trigger 作为主条件 |
| ✅ 进入后 Body = 上表（含 ScreenY=0.88） | ❌ 新做日期/剧情相机大改 |
| ✅ 离开恢复街道路；进出尽量无跳变 | ❌ 改 CameraArea 多边形当跟拍修复 |
| ✅ 扩展 Profile / Zone 脚本最小改 | ❌ 每帧手推机位替代 CM Follow |

### 严禁

- 用玩家进区代替「摄像机框完全进入」  
- 编辑器 Follow 为空就当 bug 去绑死场景引用（运行时 `SetFollow` 才是契约）  
- 无滞回导致边界疯狂切 Profile  
- 破坏村开场 `CancelFollow` / `SetLock`  

### 开放（写入 OPEN_QUESTIONS）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 用 Brain 输出相机还是 VirtualCamera Transform 算框？ | 倾向 **实际渲染相机**（`Camera.main` / Brain 输出）与白框一致 |
| Q2 | Zone 是否必须再缩？ | 以用户绿框为准；过大则施工缩 |
| Q3 | YDamping=0 + DeadZoneHeight=0 是否接受瞬时跟 Y？ | 用户已试；默认照表 |
| Q4 | SoftZoneHeight 离开恢复 1 还是 2？ | 对拍现网 VCam YAML，定一处 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / Cinemachine / C#。禁止修改任何代码与场景。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
Village_KenMuNi1 第三部分：
当「摄像机画面框」完全进入 Map/CameraDepthFollowZone_Part3 的绿色范围时，
把 Framing Transposer Body 改成用户实测数值（Screen Y = 0.88 等，见提示词助手预梳理表）；
未完全进入则恢复右街默认 Body。

判定主语是摄像机框，不是玩家碰撞。覆盖旧「玩家进 Trigger 就切 Profile」主逻辑。

## 必读
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponentGSM.cs
@Assets/Doc/执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity（CameraDepthFollowZone_Part3 / Virtual Camera / CameraArea）
@Assets/Doc/OPEN_QUESTIONS.md（KenMuNi1 第三部分相机纵深跟随）

## 侦探任务
1. 画清：Follow 何时绑定、Body 谁在改、现网 Zone 如何误用玩家判定。
2. 给出「正交摄像机世界 AABB ⊆ Zone」的计算式与挂点（LateUpdate / 跟 Brain 同相）。
3. 对照用户 Body 表与 `CinemachineFramingProfile`：缺哪些字段、是否扩展 struct。
4. 核对 Zone 世界范围是否与用户绿框一致；过大是否导致右街误触发。
5. 最小改动清单 + 滞回建议 + 验收步骤。
6. 开放问题写入报告，并建议更新 OPEN_QUESTIONS。

## 报告落盘
Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md

结构：①结论 ②旧 vs 新判定 ③Body 目标表 ④算法 ⑤Profile 缺口 ⑥Zone 几何 ⑦最小清单 ⑧验收 ⑨OPEN ⑩程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md

## 目标
1. CameraDepthFollowZone_Part3：仅当摄像机可视框完全进入 Zone 时 Apply 用户 Body 表：
   ScreenY=0.88, DeadZoneHeight=0, YDamping=0, SoftZoneHeight=0.351,
   BiasX=0.5, BiasY=0.5, XDamping=0.7, ScreenX=0.5, SoftZoneWidth=0.25 …
2. 未完全进入 → 恢复右街默认 Framing。
3. 去掉（或降级）「仅玩家进 Trigger 就切」为主条件；保留报告允许的辅助逻辑。
4. 必要时缩 Zone 对齐用户绿框；加滞回防抖。
5. 扩展 CinemachineFramingProfile / Apply 若报告要求覆盖全部字段。

## 约束
- 保持 CM Follow 运行时绑定契约；禁止 Update 堆业务替代 Framing。
- 不改 CameraArea 当跟拍修复；不破坏开场 SetLock。
- 施工说明：
  Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_施工说明.md
- 同步 OPEN_QUESTIONS 该条状态。

## 验收
- [ ] 镜头白框尚未完全进绿框：Body 仍为街道路（ScreenY≈0.5, DeadZoneHeight=1）
- [ ] 白框完全进绿框：ScreenY→0.88 等与表一致
- [ ] 白框任一边离开绿框：恢复街道路
- [ ] 边界来回走无明显狂切/跳变
- [ ] 右街低区不被误切（若 Zone 已按报告缩小）
- [ ] Play 后 Follow 仍为 Player；开场剧情后跟拍正常

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先**复制「侦探 Prompt」→ 出报告（尤其：用哪台 Camera 算框、Zone 要不要缩）。  
2. 确认 Body 表无误（Screen Y **0.88**）。  
3. **再**复制「施工 Prompt」落地。  
4. Scene 里可开 Cinemachine Game Window Guides，肉眼看白框是否完全落在绿框内再切。
