# Village_KenMuNi1 Part3 — 摄像机框完全进入才切 Body（ScreenY=0.88）— 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读定方案（**本阶段未改代码 / 场景**）  
**Unity**：2020.3.48f1 + Cinemachine  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**物体**：`Map/CameraDepthFollowZone_Part3`  
**产品规则（钉死）**：**摄像机画面框（正交可视 AABB / 白框）完全 ⊆ Zone 绿框** 才 Apply Part3 Body（含 **Screen Y = 0.88**）；未完全进入 → 恢复右街默认。  
**判定主语是摄像机，不是角色。**  
**关联**：`执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md` · 提示词 `提示词/0831/…摄像机框完全进入才切Body_架构侦探提示词.md`

---

## ① 结论一句话

**现网用「玩家脚进 Trigger / bounds.Contains(玩家)」切 Profile，与产品不符；应改为「渲染相机正交世界 AABB ⊆ Zone」再 Apply 新 Body 表（ScreenY=0.88 等）。磁盘 Zone 宽 200 盖到右街，不缩盒则新判定仍会在右街误触发——施工须对齐绿框并加滞回。**

---

## ② 原因（通俗）

现在是人走进绿框就改镜头参数；人进了、镜头白框还露在外面时，画面已经变了——不对。  
要对齐 Scene 里看到的：白框整块都进了绿框才改（Screen Y 提到 0.88）；白框有一边出去就改回右街。  
另外磁盘绿盒太宽，盖到右边大街，不先缩盒，右街也会被当成「进区」。

---

## ③ 用户检查清单（验收用）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Scene 开 Cinemachine Game Window Guides，白框**尚未**完全进绿框 | Body 仍街道路：`ScreenY≈0.5`，`DeadZoneHeight=1` |
| 2 | 白框**完全**落进绿框 | `ScreenY→0.88`，DeadZone 全 0，Bias 0.5/0.5，SoftH≈0.351 等与下表一致 |
| 3 | 白框任一边离开绿框 | 恢复街道路 |
| 4 | 边界来回走 | 无明显狂切 / 跳变（滞回生效） |
| 5 | 右街低区（约 x&gt;-93）W/S | **不被**误切 Part3 |
| 6 | Play 后 | Follow 仍为 Player；开场剧情后跟拍正常 |

---

## ④ 给程序

### A. 旧 vs 新判定（改口）

| | 旧（0822 / 现网） | 新（本需求） |
|--|------------------|--------------|
| 主语 | **玩家**脚 / `player.position` | **摄像机可视框** |
| API 入口 | 仍 `SetKenMuNiPart3CameraMode` → `ApplyFramingTransposerProfile` | **同**（只改「何时 true」） |
| `VillageCameraDepthFollowZone` | Trigger 进出 + LateUpdate `bounds.Contains(玩家)` | LateUpdate/CameraUpdated：**camRect ⊆ zoneRect** |
| Part3 参数 | `screenY=0.5`、`yDamping=0.7`、`biasY≈-4.58`、`softH=0.26` | 用户实测新表（§B） |

**现网误用玩家判定（已核实）**：

```
OnTriggerEnter/Exit(PlayerFoot) → SetPart3CameraMode
LateUpdate: zone.bounds.Contains(player.position) → SetPart3CameraMode
  → CameraComponentGSM.SetKenMuNiPart3CameraMode
  → ApplyFramingTransposerProfile(part3 | street)
```

玩家已进区、镜头还露在绿框外 → **现网会切** ❌；新产品 → **不要切**。

---

### B. Body 目标表（用户实测钉死）

#### B1. 进入后（Part3 · Framing Transposer）

| 字段 | 目标值 | 现网静态 `KenMuNiPart3DepthFollow` / 场景序列化 |
|------|--------|-----------------------------------------------|
| Tracked Object Offset | (0,0,0) | 磁盘已是；进出相同 |
| Lookahead Time / Smoothing | 0 / 0 | 同左 |
| Lookahead Ignore Y | 否 | 同左 |
| **X Damping** | **0.7** | 已是 0.7 |
| **Y Damping** | **0** | 旧 Profile **0.7** → **须改** |
| Z Damping | 1 | 磁盘已是；进出相同 |
| Target Movement Only | 是 | 磁盘已是 |
| **Screen X** | **0.5** | 已是 |
| **Screen Y** | **0.88** | 旧 **0.5** → **须改** |
| Camera Distance | 10 | 磁盘已是 |
| Dead Zone W/H/D | **0 / 0 / 0** | H 已 0；W/D 磁盘已 0 |
| Soft Zone Width | **0.25** | 已是 |
| **Soft Zone Height** | **0.351** | 旧 **0.26** → **须改** |
| **Bias X / Y** | **0.5 / 0.5** | 旧 **0 / -4.5849** → **须改** |
| Center On Activate | 是 | 磁盘已是 |

#### B2. 离开后（右街默认）

| 字段 | 倾向值 | 证据 |
|------|--------|------|
| Dead Zone Height | **1** | 磁盘 VCam + street Profile |
| Y Damping | **0** | 同左 |
| Screen Y | **0.5** | 同左 |
| Bias X / Y | **0 / 0** | 同左 |
| Soft Zone Height | **1**（拍板倾向） | **磁盘 VCam YAML = 1**；静态 `KenMuNiStreetDefault` / 场景 streetProfile 现为 **2** → 进出一次后与冷启动不一致（见 Q4） |

**Q4 施工默认**：**离开恢复 SoftZoneHeight = 1**（对齐磁盘 VCam）；同步改 `KenMuNiStreetDefault` 与场景 `streetProfile`，避免「从没进过 Zone」vs「进过再出」手感分叉。

---

### C. 调用链与 Follow 契约（勿改坏）

```
BaseGameSceneManager.InitPlayer
  → CameraComponentGSM.SetFollow(player)     // 运行时绑 Follow；编辑器 YAML m_Follow=0 正常

每帧 CinemachineBrain (LateUpdate)
  → FramingTransposer 跟拍
  → Confiner 钳在 CameraArea（围栏 ≠ 本绿框）

VillageCameraDepthFollowZone（拟改判定后）
  → fullyInside? Apply Part3 : Apply Street
```

- **Body 组件常驻**，不是进区才加。  
- 进 Part3 时现网若 `IsLock` 会 `SetLock(false)+SetFollow`——可保留，勿破坏开场 `CancelFollow/SetLock`。  
- **禁止**每帧手推机位替代 Framing；**禁止**改 CameraArea 当跟拍修复。

---

### D. 算法（方案 A · 推荐）

```
// 建议挂点：LateUpdate，且脚本执行顺序 ≥ Brain；
// 更稳：CinemachineCore.CameraUpdatedEvent（跟实际输出同相）

Camera cam = Camera.main; // 或 brain.OutputCamera（Q1 倾向渲染相机）
float halfH = cam.orthographicSize;           // KenMuNi1 ≈ 7.9
float halfW = halfH * cam.aspect;
Vector3 c = cam.transform.position;

Bounds camBounds = new Bounds(c, new Vector3(halfW * 2f, halfH * 2f, 0f));
// 或显式四角：
// min=(c.x-halfW, c.y-halfH), max=(c.x+halfW, c.y+halfH)

Bounds zone = _zoneCollider.bounds;           // BoxCollider2D 世界 AABB

// 滞回：进入用内缩盒，离开用原盒（或离开略外扩）
Bounds enterZone = Inflate(zone, -hysteresis); // hysteresis 建议 0.2～0.5 世界单位
Bounds exitZone  = zone;                       // 或 Inflate(zone, +hysteresis)

bool fullyInside;
if (_part3Active)
    fullyInside = exitZone.Contains(camMin) && exitZone.Contains(camMax);
else
    fullyInside = enterZone.Contains(camMin) && enterZone.Contains(camMax);

if (fullyInside != _part3Active)
    SetPart3CameraMode(fullyInside);
```

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · 相机世界 AABB ⊆ Zone** | 上式 | ✅ **本期主方案** |
| B · 玩家进 Trigger | 现网 | ❌ 产品已否（可删或降级为仅 Debug） |
| C · 第二套 VCam + Blend | — | ❌ 本期不需要 |
| D · 更新 Profile 后整组 Apply | 补 ScreenY=0.88 等 | ✅ 配合 A |

**Trigger 处理**：`OnTrigger*` / 玩家 `Contains` **不再作为主条件**；读档补检改为相机框判定即可。

---

### E. Profile 缺口

`CinemachineFramingProfile` **已有**：screenX/Y、deadZoneW/H、x/yDamping、softZoneW/H、biasX/Y。  
`ApplyFramingTransposerProfile` 已整组写入上述字段。

| 用户表字段 | Profile 是否覆盖 | 施工建议 |
|------------|------------------|----------|
| ScreenY=0.88、DeadH=0、YDamp=0、SoftH=0.351、Bias 0.5/0.5、XDamp、SoftW… | ✅ 已有字段 | **改静态默认 + 场景序列化 Profile** |
| Camera Distance / CenterOnActivate / TargetMovementOnly / Lookahead / Offset / ZDamp | ❌ 未进 struct | **进出目标值与磁盘街道路相同** → **本期可不扩 struct**（P2 可选） |
| Dead Zone Depth / Unlimited Soft | ❌ | 磁盘已满足目标 → 可不扩 |

**结论**：本期 **不必**为 Distance 等扩 struct；**必须**刷新 Part3 / Street 的现有 Profile 数值（含场景 Inspector 上已序列化的那份，否则会盖掉代码默认）。

---

### F. Zone 几何（磁盘 · 须核对绿框）

| 项 | 磁盘值（`Village_KenMuNi1.unity`） |
|----|-------------------------------------|
| 父节点 | `Map` @ (0,0,0) |
| `localPosition` | **(-55, 21, 0)** |
| `BoxCollider2D.size` | **(200, 58)** |
| 世界 AABB（约） | X **[-155, +45]**，Y **[-8, 50]** |

对照 0822 右街分界 **x ≈ -93**：现盒右缘到 **+45**，**盖过整段右街**。  
正交半宽 ≈ `7.9 * aspect`（16:9 时 ≈14）→ 相机在右街中部时，**白框仍可完全落在超大绿盒内** → **即使用摄像机判定也会误切 Part3**。

| 动作 | 说明 |
|------|------|
| **施工须缩盒** | 对齐用户 Scene 绿框；若用户未存盘，默认回到 0822 建议：Center **(-133, 21)**，Size **(80, 58)** → X≈**[-173, -93]**，勿盖 x&gt;-93 |
| 用户已手调绿框 | **以当前 Scene 可见为准**，Ctrl+S 后以磁盘为准；侦探阶段磁盘仍为 200 宽 |
| CameraArea | **≠** 本绿框；本期不改多边形 |

---

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 文件 | 优先级 |
|---|------|------|--------|
| 1 | 判定改为相机 AABB ⊆ Zone + 滞回；去掉玩家 Trigger 主逻辑 | `VillageCameraDepthFollowZone.cs` | **P0** |
| 2 | 更新 `KenMuNiPart3DepthFollow` = 用户新表（ScreenY=0.88 等） | `CameraComponent.cs` | **P0** |
| 3 | `KenMuNiStreetDefault.softZoneHeight` → **1**（对齐磁盘 VCam，Q4） | `CameraComponent.cs` | **P0** |
| 4 | 场景 Zone 上序列化 `part3Profile` / `streetProfile` 同步新表 | `Village_KenMuNi1.unity` | **P0** |
| 5 | **缩** `CameraDepthFollowZone_Part3` Box 对齐绿框 / 左翼 | 同场景 | **P0** |
| 6 | 脚本执行顺序或 `CameraUpdatedEvent`，与 Brain 同相 | Zone 脚本 | P1 |
| 7 | （可选）Profile 扩 Distance 等 | `CinemachineFramingProfile` | P2 |
| 8 | 不动 | CameraArea、开场 SetLock、手推替代 Framing | — |

**预期 diff**

- `VillageCameraDepthFollowZone.cs`（判定重写）  
- `CameraComponent.cs`（两套 Profile 数值）  
- `Village_KenMuNi1.unity`（Zone 尺寸 + 序列化 Profile）  
- `OPEN_QUESTIONS.md` + 施工说明 `施工说明/0831/…施工说明.md`

---

### H. 开放问题

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 用 Brain 输出相机还是 VCam Transform 算框？ | **`Camera.main` / Brain.OutputCamera**（与白框一致）；勿只用未 Confiner 前的 VCam 位 | ⏳ |
| Q2 | Zone 是否必须再缩？ | **磁盘必须缩**；以用户绿框为准 | ⏳ 施工核对 |
| Q3 | YDamping=0 + DeadZoneHeight=0 瞬时跟 Y？ | **照用户表** | ✅ 默认照表 |
| Q4 | SoftZoneHeight 离开恢复 1 还是 2？ | **1**（对齐磁盘 VCam；改静态默认与场景） | ⏳ |
| Q5 | 滞回幅度？ | **0.2～0.5** 世界单位内缩进、原盒出 | ⏳ |

---

### I. 与 0822 报告关系

0822 解决「第三部分要跟 Y / 右街不要晃」——**分区切 Framing** 方向仍有效。  
本报告覆盖其 **「玩家进 Trigger」触发条件** 与 **Part3 参数表**（旧 biasY=-4.58 / yDamping=0.7 → 新 ScreenY=0.88 等）。  
写参链路 `ApplyFramingTransposerProfile` **复用**，不另起第二套 VCam。
