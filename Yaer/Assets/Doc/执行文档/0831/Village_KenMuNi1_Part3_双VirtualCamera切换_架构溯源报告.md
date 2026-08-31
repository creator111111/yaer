# Village_KenMuNi1 Part3 — 双 VirtualCamera 切换（替代单机改 Body）— 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读溯源 + 落地改法拍板（**本阶段未改代码 / 场景**）  
**Unity**：2020.3.48f1 + Cinemachine  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**产品改口（钉死）**：弃用「单 VCam 运行时 `ApplyFramingTransposerProfile`」为主方案；改用 **两台不同 Body 的 VirtualCamera**，由 Brain **Priority / Blend** 切换。  
**手感目标不变**：Part3 = 用户表（ScreenY=**0.88** 等）；街道路 = DeadH=1、ScreenY=0.5、SoftH≈1。  
**关联**：  
- 0822：`执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md`（方案 C 曾 ❌，**本需求启用**）  
- 0831 白框判定：`执行文档/0831/…摄像机框完全进入才切Body_架构溯源报告.md`  
- 0831 施工/返修：`施工说明/0831/…摄像机框完全进入才切Body_施工说明.md`  
- 提示词：`提示词/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构侦探提示词.md`

---

## ① 结论一句话

**单机改 Framing 与「白框 ⊆ Zone」判定形成反馈环（0831 返修已证实）；应落地双 VCam（Street + Part3），Zone 只切 Priority，判定改用「街道路机算出的框」（A1）打断反馈；`CameraComponent` 须双写 Follow / Confiner / Size / CancelFollow。**

---

## ② 原因（通俗）

改同一台相机的 ScreenY，画面白框马上跟着挪——下一帧又觉得「没进区 / 又进区」，来回切；冷却和每帧重刷参数只是压症状。  
换成两台相机：一台一直是右街手感，一台一直是高台手感，进区只换「谁 Live」，参数写死在 Inspector，不再运行时改 Body。

---

## ③ 用户检查清单（施工后验收）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 右街 / 白框未完全进绿框 | Live = Street；ScreenY≈0.5，DeadH=1 |
| 2 | 街道路算出的框完全进绿框 | Blend 切到 Part3；ScreenY=0.88 等与表一致（Inspector 固化） |
| 3 | 框离开绿框（A1 街道路判定） | Blend 回 Street |
| 4 | 边界来回多次 | **无狂切**；无「再也不切」；无需每帧 Apply |
| 5 | 右街约 x&gt;-93 | 不被误切 Part3（Zone 已缩） |
| 6 | Play 后 Follow | **两台** Follow=Player；开场 `CancelFollow/SetLock` 后剧情结束跟拍恢复 |
| 7 | 剧情/缩放/换 Confiner | Size、BoundingArea 两台一致；不漏 Part3 |

---

## ④ 给程序

### A. 为何弃单机改参（反馈环 · 改口理由）

现网（0831 施工+返修后）链路：

```
CameraUpdatedEvent(Brain.OutputCamera 白框)
  → wantPart3 = 白框 ⊆ Zone（滞回）
  → SetKenMuNiPart3CameraMode → ApplyFramingTransposerProfile
       改 ScreenY / DeadZone / Bias …
  → Framing 立刻/阻尼挪机位
  → 下一帧 OutputCamera 白框几何变了 → wantPart3 翻转
  → 狂切

返修压症状（现网仍在）：
  · 0.4s 冷却不翻转
  · 离开外扩滞回
  · 稳态每帧 ReassertCurrentProfile（再次 Apply）
  · 成功才改 _part3Active
```

| 手段 | 定性 |
|------|------|
| 冷却 / 滞回 / 每帧重申 | **压症状**，根因仍在「判定源 = Live 输出」+「切模式 = 改 Live Body」 |
| **双 VCam + 判定脱钩 Live** | **根治**（本期产品拍板） |

0822 曾否决方案 C（「维护两套 vcam」）——**因单机改参反馈环，本需求改口启用方案 C**。

---

### B. 双 VCam 目标架构

```
SceneManager/Camera/          （或现网 Camera 父下）
  ├─ CameraComponent          （序列化 street + part3 两台引用）
  ├─ CameraArea               （Polygon · Confiner 共用）
  ├─ VCam_Street              （现有机可改名；现网 GO 名常为 Cinemachine）
  │    Body = 街道路 Framing（DeadH=1, ScreenY=0.5, SoftH=1…）
  │    Priority 默认 = 10（Live）
  │    Confiner → CameraArea；ImpulseListener；StandbyUpdate=Always
  └─ VCam_Part3               （新建 · 复制 Street 再改 Body）
       Body = 用户表固化（ScreenY=0.88, DeadH=0, YDamp=0, SoftH=0.351, Bias 0.5/0.5…）
       Priority 默认 = 0（Standby）
       Confiner → 同一 CameraArea；ImpulseListener；StandbyUpdate=Always
       Follow 与 Street 同步（见 §E）

Main Camera + CinemachineBrain
  DefaultBlend 现网：Style=EaseInOut(1)，Time=**2s**
  建议：Street↔Part3 用 CustomBlends **0.4s EaseInOut**（Q2）；
        勿贸然把全局 Default 从 2s 改掉（可能影响其它切机；本期村内几乎单机，风险低但 Custom 更干净）
```

| 机 | 职责 | Priority |
|----|------|----------|
| **VCam_Street** | 右街 / 默认 Live | 常驻 **10** |
| **VCam_Part3** | 高台 Live | 进区 → **20**（&gt;Street）；出区 → **0** |

切换手段：**Priority** ✅；关 enabled ❌（Blend 差）；继续 Apply Profile ❌。

**Part3 Body（固化 Inspector，勿运行时刷）**

| 字段 | 值 |
|------|-----|
| Screen X / Y | 0.5 / **0.88** |
| Dead Zone W/H/D | 0 / 0 / 0 |
| X / Y Damping | 0.7 / **0** |
| Soft Zone W/H | 0.25 / **0.351** |
| Bias X / Y | **0.5 / 0.5** |
| Camera Distance | 10 |
| Target Movement Only | 是 |
| OrthographicSize | **7.9**（与 Street 同） |

街道路：DeadH=1、ScreenY=0.5、SoftH=1、Bias 0、YDamp=0（对齐现网磁盘 / `KenMuNiStreetDefault`）。

---

### C. 判定与滞回（必须打断反馈环）

> 若仍用 **当前 Live 输出白框** 判定，切到 Part3 后白框又变 → **双机也会抖**。

| 方案 | 判定输入 | 裁定 |
|------|----------|------|
| **A1** | 用 **VCam_Street**（街道路）状态算出的正交世界 AABB ⊆ Zone | ✅ **推荐主条件** |
| A2 | 玩家在 Zone / 世界 x≤-93 | 可选**辅**条件；不作唯一主条件 |
| A3 | Brain.OutputCamera 白框 | ⚠️ 易复发反馈环；**弃作主** |
| A4 | 仅玩家脚 Trigger | ❌ 与「镜头框」口不完全一致 |

**A1 计算式（伪）**：

```
// Street 即使 Standby 也要有新鲜 State → StandbyUpdate = Always
var street = vcamStreet;
Vector3 c = street.State.FinalPosition;   // 或 Force 更新后
float halfH = street.m_Lens.OrthographicSize;
float halfW = halfH * outputAspect;       // aspect 取 OutputCamera / Screen
camMin/Max = (c ± halfW/halfH)

进：Inflate(zone, -h) 完全包含 camAABB → Part3 Priority↑
出：Inflate(zone, +h) 不再完全包含 → Part3 Priority↓
h ≈ 0.35（可复用现网）
```

**Blend 期间防抖**：

| 规则 | 说明 |
|------|------|
| 边沿才改 Priority | `want != _part3Active` 才写；稳态**禁止**每帧 Apply / 每帧改 Priority |
| 冷却可选 | Blend≈0.4s 时冷却 ≥ Blend 时长，避免 Blend 中途再翻 |
| 删 `ReassertCurrentProfile` | 双机后无「标志与 Framing 脱节」问题，每帧 Apply **必须删除** |

Zone 几何：现网已缩 Center **(-133,21)** Size **(80,58)**，右缘≈-93——**保持**，勿再扩回 200。

---

### D. Zone 改切 Priority（伪代码级）

```
// VillageCameraDepthFollowZone — 主路径不再 Apply Profile
OnCameraUpdated / LateUpdate:
  want = IsStreetFrustumFullyInsideZone()   // A1
  if want == _part3Active: return           // 无每帧重申 Apply

  if inCooldown: return

  cameraGsm.SetKenMuNiPart3CameraMode(want) // 内部改 Priority，不改 Framing
  _part3Active = want
  startCooldown(blendSeconds)
```

`part3Profile` / `streetProfile` 序列化字段：可留作文档对照，**运行时切机不再读取写 Body**（Q5）。

进区若 `IsLock`：保留现网「解锁 + SetFollow(player)」——但 SetFollow 须双写（§E）。

---

### E. CameraComponent 双机契约（现网只认一台 · 风险表）

现网 `[SerializeField] CinemachineVirtualCamera virtualCamera` 单引用。调用面：

| API | 现网行为 | 双机契约 |
|-----|----------|----------|
| `SetFollow` / 手推 snap | 只绑 / 只推 Street | **Street + Part3** 都 `Follow=player`；手推两台对齐或 Live+Invalidate Part3 State |
| `CancelFollow` | 只清 Street | **两台 Follow=null**（村开场锁相机） |
| `ChangeVirtualCameraShowSize` / Reset | 只改一台 Ortho | **两台** Lens=同值（7.9） |
| `ChangeCameraBoundingArea` | 只打一台 Confiner | **两台** → 同一 `CameraArea` |
| `InitImpulseListener` | 只挂一台 | **Part3 也要** ImpulseListener（或复制 Extension） |
| `ApplyFramingTransposerProfile` | 改 Street Body | **保留 API**；Part3 Zone **不走** |
| `SetKenMuNiPart3CameraMode` | → Apply | **改为** `part3.Priority = active ? 20 : 0`（Street 保持 10） |
| `VirtualCamera` 属性 | 外露单机 | 保持「主/Street」；Shop 等继续吃 Street；新增 `VirtualCameraPart3` 可选 |
| `SetLock`（GSM） | 挡 SetFollow | 语义不变；解锁后双写 Follow |

**开场**（`Village_KenMuNiSceneManager`）：`CancelFollow` + `SetLock(true)` → 必须清掉 **两台** Follow，否则 Part3 仍可能跟玩家抢 Live。

**手推 LateUpdate**：今日只推 `virtualCamera.transform`。双机时：  
- 方案 E1：手推期间临时只让 Street Priority 最高，Part3 Standby；完成后双 Follow。  
- 方案 E2：手推目标写到两台 Transform。  
倾向 **E2 简单**（村开场后日常跟拍为主）；施工选一并写进说明。

---

### F. Brain Blend 建议

| 项 | 现网 | 建议 |
|----|------|------|
| Default Blend | EaseInOut **2s** | 全局可先不动 |
| Street ↔ Part3 | （无） | **CustomBlends 0.4s EaseInOut**（Q2 起步） |
| Cut | — | ❌ 进出会跳 |

Blend 时长与 Zone 冷却对齐（冷却 ≥ Blend），减少「Blend 中途判定又翻」。

---

### G. 最小改动文件表

| # | 文件 / 资源 | 动作 | 优先级 |
|---|-------------|------|--------|
| 1 | `Village_KenMuNi1.unity` | 复制 VCam → `VCam_Part3`；Street 可改名；Part3 Body 调表；两台 Confiner→CameraArea；Impulse；StandbyUpdate=Always；Priority 10/0 | **P0** |
| 2 | Brain | 挂 CustomBlends Street↔Part3 0.4s（或临时改 Default=0.4 并记 OPEN） | **P0** |
| 3 | `CameraComponent.cs` | 序列化第二台；SetFollow/Cancel/Size/Confiner/Impulse **双写**；`SetKenMuNiPart3CameraMode`→Priority | **P0** |
| 4 | `CameraComponentGSM.cs` | 透传；注释更新（不再说「只改 Framing」） | **P0** |
| 5 | `VillageCameraDepthFollowZone.cs` | 判定改 **A1 街道路算框**；切机不 Apply；**删除** Reassert 每帧 Apply；冷却对齐 Blend | **P0** |
| 6 | 场景 Zone 上 Profile 字段 | 可留空/文档；不再驱动运行时 Body | P1 |
| 7 | `ApplyFramingTransposerProfile` / 旧 Profile 静态 | **保留**兼容；Part3 主路径停用 | — |
| 8 | CameraArea 多边形 / 开场剧情图 | **不改** | — |

**预期 diff 焦点**：Zone 判定源 + CameraComponent 双引用 + 场景第二台 VCam。  
**不做**：全村每区双机；手推替代 Framing；重做 CameraArea。

---

### H. 风险清单

| 风险 | 后果 | 缓解 |
|------|------|------|
| SetFollow 只绑 Street | Part3 Live 时不跟玩家 | 双写 Follow |
| Confiner 漏 Part3 | 高台机位越界 | 两台同绑 CameraArea |
| 判定仍用 OutputCamera | 双机仍狂切 | **强制 A1** |
| Standby 不更新 Street State | A1 框过期 | Street `StandbyUpdate=Always` |
| Default Blend 2s | 进出拖沓 | Custom 0.4s |
| CancelFollow 漏 Part3 | 开场锁不住 | 双清 Follow |
| 每帧仍 Apply | 反馈环复活 | 删 Reassert |

---

### I. 开放问题

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 判定 A1 街道路算框还是 A2 玩家区？ | **A1 为主**；A2 可选辅 | ⏳ |
| Q2 | Blend 时长？ | **0.4s EaseInOut**；CustomBlends 优先于改全局 Default | ⏳ |
| Q3 | 旧 Apply / SetKenMuNiPart3？ | **保留 API**；Part3 Zone 改内部切 Priority，停用主路径 Apply | ⏳ |
| Q4 | Part3 是否复制 Impulse / Confiner？ | **要**，与 Street 对齐 | ⏳ |
| Q5 | 场景旧 Profile 序列化？ | 可留文档；运行时以两台 Inspector Body 为准 | ⏳ |
| Q6 | 手推 SetFollow 双机策略？ | 倾向 E2 两台对齐 | ⏳ |

---

### J. 与旧文档关系（状态机）

| 文档 | 本报告态度 |
|------|------------|
| 0822 方案 C 双 VCam ❌ | **改口启用**（因反馈环） |
| 0831 白框 ⊆ Zone 思想 | **保留「框进区」产品口**；实现改为 **Street 算框（A1）** |
| 0831 单机 Apply + 冷却重申 | **主方案废弃**；冷却可降级为 Blend 对齐；重申 Apply **删除** |
| Zone 缩盒 (-133,21)/(80,58) | **保持** |
| Part3 数值表 ScreenY=0.88 | **固化到 VCam_Part3**，不运行时刷 |
