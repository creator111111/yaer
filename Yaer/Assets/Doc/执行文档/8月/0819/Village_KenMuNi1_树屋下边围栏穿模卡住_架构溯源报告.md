# Village_KenMuNi1 树屋下边围栏穿模卡住 — 架构溯源报告

**文档版本**：v1.0（2026-08-19）  
**文档性质**：【架构侦探】只读溯源；**本文件不施工**  
**Unity**：2020.3.48f1 / C#  
**已确认现象（开发者 + 截图）**：树屋楼梯上走路，容易突破「下边围栏」Collider，然后**穿模卡在里面不动**  
**已拍板（开发者）**：要**保险措施**——尽量别穿进去；**已经进去也必须被推回可走区，不能焊死**  
**范围**：仅村庄 `Village2_5D` 的 Walk 障碍。不改龙宫、不改 Forest 战斗、不推翻 0514「脚本挡人」、**不回退** 0818 合速度、不改树屋双 Trigger 门控表。加厚 Collider **只能辅修**，不能当唯一方案

关联：

- 提示词：`Assets/Doc/提示词/0819/Village_KenMuNi1_树屋下边围栏穿模卡住_架构侦探提示词.md`
- 截图：同目录 `*_Scene线框.png` / `*_Collider1_Inspector.png` / `*_Hierarchy.png`
- 0514 方案 1：`Assets/Doc/执行文档/5月/0514/村庄Walk障碍_方案1_仅Physics2D查询阻挡_关闭脚障物理碰撞_执行文档.md`
- 0512 区内障碍：`Assets/Doc/执行文档/5月/0512/村庄WalkArea内部阻挡碰撞体_程序施工执行说明.md`
- 0513 树屋门控：`Assets/Doc/执行文档/5月/0513/树屋双触发顺序激活DepthZoneColliders_架构溯源与施工执行说明.md`
- 0818 合速度 / 0819 惯性（A′ **代码已合入** `TownPlayerLocomotion`：无 V 则 `depthVelocity=0`）

---

## ① 结论一句话

**容易穿，是因为斜着的厚多边形只被「纯左右 Cast」和「纯上下射线」分开拦，斜向一步等于从缝里钻进去；脚底默认用底边中心射线扫 Y，斜墙在脚旁边时根本扫不着。穿进去之后动不了，是因为横移在「已经重叠且沿 X 扫空」时把 vx 锁成 0，纵深挤出又只沿 Y 找空位——斜墙里面上下都是墙，找不到出口，人就被焊在栏杆里。** 推荐 **方案 A**：脚还在墙外时记下 last-free；已重叠先用 Distance 往法向推；仍重叠就把人放回 last-free 并清本帧速度。禁止用「重叠就锁死速度」冒充保险。

---

## ② 原因（生活类比）

围栏是斜着的薄墙，场景里还被拉成 **Y 缩放约 3.47、Z 转 -72°** 的一大块多边形。村里拦人像安检只查「这一步是纯左右还是纯上下」，斜着走等于两步合成一步，容易从缝里钻进去。钻进去之后，护栏又把左右速度掐死，前后挤出只沿着上下找空位——斜墙里面上下都是墙，于是人被焊在栏杆里。

刚体不会把人顶出来：0514 已经让障碍层和所有层 **Ignore**。Discrete / 不开 CCD **不是**主因，打开也救不了。

下边围栏比上边更容易中招：楼梯外侧就是这条边，人一直贴着走；它比上边更斜（-72° vs -85°）。不是单独给这块写了另一套脚本。

---

## ③ 用户需要做什么

产品「要保险、进去也要出来」已拍板。施工按方案 A。你这边只验收（先上楼把树屋 DepthZone 激活，再贴着下边围栏绿线框走）：

| # | 操作 | 期望 |
|---|------|------|
| 1 | 楼梯上只按 D，贴下边围栏走 ≥5 秒 | 人在线框**外侧**，不进栏杆 |
| 2 | 只按 W / 只按 S，贴边走 ≥5 秒 | 同上 |
| 3 | **D+W 斜向**贴弯角反复走 ≥5 秒 | **不穿** |
| 4 | **保险**：人已经和围栏重叠（调试传送进线框，或日志确认 Overlap） | **1 秒内**必须在线框外，并且还能走 |
| 5 | 贴边走 | 可以贴着滑，不要整段锁死在原地 |
| 6 | 上边围栏、村里其它障碍、WalkArea 外边界 | 不回归（不无故穿、不无故弹飞） |
| 7 | 树屋双 Trigger 上下楼 | 门控仍正常，围栏该出现时出现 |
| 8 | 斜向按住仍约 0.707；点 A 往左 | 0818 不回归 |

不要：关掉围栏当修复（人会从楼梯掉出去）；不要指望把这块改成 Continuous 就好。

---

## ④ 给程序看的补充

### 4.1 这一块墙是什么（场景 YAML，非 Play 改写）

路径（与 Hierarchy 截图一致）：

```
Village_KenMuNi1 / Map / Design / 肯姆尼1合层 / 树屋
  DepthZone&Colliders          （始终 Active；挂 VillageTreehouseDepthZoneGate）
    Root                       （门控 SetActive 的真正目标；Awake 按 initialTargetActive=0 关掉）
      DepthZone
      上边围栏 / Collider (1)
      下边围栏 / Collider (1)   ← 本案
```

**下边围栏 / Collider (1)**（fileID `1992526794`，父 `545485855`）：

| 项 | 磁盘值 | 含义 |
|----|--------|------|
| Layer | **6 = `VillageWalkObstacle`** | 脚本 Cast/Overlap 挡人 |
| Transform | 本地 `(-13.94, -6.57, 0)`，**Z=-72.341°**，scale **(0.820, 3.473, 1)** | 斜 + 非均匀缩放 |
| PolygonCollider2D | **`m_UsedByComposite: 1`**，`isTrigger=0` | 只给 Composite 提供路径，自己不单独参与查询 |
| CompositeCollider2D | GeometryType=**Polygons(1)**，**`isTrigger=0`** | **真正被查询的形状** |
| Rigidbody2D | Kinematic、Discrete、Full Kinematic Contacts 关 | 方案 1 下矩阵已 Ignore，CCD 当不了主修 |

**上边围栏 / Collider (1)**（fileID `1960759012`）：同一套组件，Z 旋转 **-85.36°**（更接近竖直），同样非均匀缩放。下边更容易穿：**走位更贴外侧 + 更斜**，不是两套代码。

**查询能不能打到（打假「Trigger 关导致线框在、查询空」）：**

- `BuildVillageObstacleContactFilter`：`useTriggers = true`，层掩码只有 `VillageWalkObstacle`。  
- Unity 里 `useTriggers=true` = **额外包含 Trigger**，**不会排除**非 Trigger。这块 Composite **Trigger 关**，Cast/Overlap/**Distance 能打到**。  
- 穿模不是「查不到」，是「查到了但夹不稳 / 进去后锁死」。  
- 0514 口径希望障碍 `isTrigger=true` 当语义标注；这块仍是关。辅修可勾上，**勾上不是修复**。

**门控：** `VillageTreehouseDepthZoneGate.initialTargetActive = 0`，进树屋前 Root（含两道围栏）关闭。穿模只发生在激活之后是正常的。**禁止改决策表。**

### 4.2 挡人脚本为何拦不住斜墙

`TownPlayerLocomotion.OnFixedUpdate` 现网顺序：

1. 纵深积分（0819 A′：无 V 则 `depthVelocity=0`）→ 0818 合速度归一  
2. **只沿世界 Y** `ApplyVillageWalkObstacleDepthClamp`（默认 **脚底底边射线**，`villageObstacleUseFootBottomRayForDepthCast=true`）  
3. `WriteRootTransformWithAuthoritativeDepthY`（**保留 vx**）  
4. WalkArea 多边形修正（可改 X/Y）→ 必要时再跑一遍纵深夹紧  
5. `ApplyVillageWalkObstacleFootPenetrationSeparation`：`Physics2D.Distance`，迭代 **3**，单步上限 **0.07**  
6. **只沿世界 X** `ApplyVillageWalkObstacleHorizontalVelocityClamp`

**没有**沿 `(vx*dt, depthVelocity*dt)` 的 2D 扫掠。斜向稳态约 `7.92 × 0.02 ≈ 0.16` 每轴，合位移约 0.22。横竖拆开后，对角线上的薄截面可以被「先挪 Y 再挪 X」插进去。

纵深默认射线（代码注释原话）：宽脚形状 Cast 扫斜栅栏会「刷边」提前挡，所以改成底边中心射线。**-72° 的下边围栏正好是斜栅栏**：墙在脚侧面时，沿 +Y/-Y 的射线从底边中心出去可能 **零命中**，权威 Y 照写，脚已经和 Composite 重叠。这是「容易穿」的主缺口之一。不要只把射线改回形状 Cast 当修复（会回归台阶侧棱提前挡）。

`VillageWalkObstacleCollisionBootstrap`：障碍层对 **0..31 全部 Ignore**（含 PlayerFoot）。刚体不会挤出。与 0514 一致，**禁止关掉 Bootstrap 当修复**。

WalkArea：围栏是**区内**障碍，多边形外边界管不住「栏杆里面」。WalkArea 的 ClosestPoint 修正还可能在贴边时改 X，把人推进围栏；后面虽有 `ApplyVillageWalkObstacleClampAfterWalkPolygonIfNeeded`，但仍是 **只夹 Y**。方案 A 的 last-free 必须放在 **WalkArea 之后**，避免「拉出来又被多边形推进去」。

0819 惯性 A′ **已合入**，松键不再靠摩擦多滑一段。本案主修仍是 **按住斜向时的轴分离穿透**；惯性不是主因。

### 4.3 卡住不动（必须裁定的两条）

进入障碍后同一物理帧：

| 量 | 典型 | 谁造成 |
|----|------|--------|
| Foot `OverlapCollider` | **>0** | 已在 Composite 内 |
| 沿 X 的 `Cast` `hitCount` | 经常 **0**（壳在体内，正向扫不到「外侧壳」） | 斜/厚多边形内部 |
| `moveSpeedX` / vx | **被写成 0** | 横移夹紧：`footEmbedded && hitCount==0 → allowedAlong=0` |
| `TryDepenetrateFootFromWalkObstacles` | **失败留下** | **只沿 Y**，步长 0.04，最多 24 步（±0.96）。斜墙沿 Y 的厚度常大于 1 |
| Distance 分离 | 最多推出 **0.07×3≈0.21** | 嵌得更深就剩重叠 |
| CombatRun | 多半仍在跑（按着键） | 不是退 Idle 卡动画；是位移被锁 |

**裁定：**

1. **`TryDepenetrate` 只搜 Y → 是卡死的一半。** 斜 Composite 内部沿世界 Y 往往仍重叠，搜不到空位就 `return false`，权威 Y 留在墙里。  
2. **横移「重叠且 Cast 空则锁 vx」→ 是卡死的另一半，且和开发者「卡住不动」高度吻合。** 注释本意是防大块 Trigger 内切向穿出，落在斜围栏内部就变成焊死。这就是方案 E 的味道，**禁止当保险**。  
3. 两条同时成立：左右不能动，上下挤不出去 → **卡死主因成立（高置信）**。

转身护栏 `VillageWalkObstacleTurnImmediateBlock`：重叠时也会清 vx，在墙内转身会再锁一次。A 的 last-free 在 Town FixedUpdate 末尾拉回即可，本期不必改这个静态类。

### 4.4 推荐方案（只选一个主方案）：A

目标：**进不去** + **进去了也能出来**（开发者点名的保险）。

| 方案 | 摘要 | 防穿 | 卡死保险 | 选用 |
|------|------|------|----------|------|
| **A 进障后 2D 拉回 + last-free** | 见下 | 高（回滚） | **高** | **推荐（必须含保险）** |
| B 按实际位移 2D 扫掠 | `(dx,dy)` 合成 Cast，停在接触前 | 高（斜向） | 中：已在内部时 Cast 仍可能扫空 | 可作 A 之后的加强，**不能替代保险** |
| C 只加厚/重画围栏 | 去非均匀缩放、拆段 | 中 | **无** | **辅修**，禁止唯一方案 |
| D 恢复物理硬碰 | 关 Bootstrap + Continuous | 不稳 | 可能挤飞 | **否**（0514） |
| E 重叠则整帧禁止移动 | 清 vx/depth | 低 | **更卡** | **禁止**（现网锁 vx 已是卡死主因之一） |

**方案 A 算法（只放 Town，障碍夹紧/分离之后）：**

1. **记 last-free（本帧开始前）**  
   - 在积分 / WalkArea / 写根 **之前**（建议 `OnFixedUpdate` 一进来、已确认 Village2_5D）：若 Foot 与障碍层 Overlap 为 0，保存 `_lastFreeRootPos`（刚体 XY）和 `_lastFreeAuthY`。  
   - 已重叠时 **不要**用当前点覆盖 last-free。  
   - 进村 `ApplyVillageMode` 时若当时不重叠，写一次初值。

2. **现有纵深夹紧、WalkArea、Distance 分离、横移夹紧照旧跑完**（保留 0514 轴 Cast，贴边滑还靠它们）。

3. **保险（必须在 WalkArea 之后，建议在横移夹紧之后、本函数末尾再跑一遍）：**  
   - 若仍 Overlap：加大 Distance 推出——循环直到不重叠，或达到安全上限（建议迭代提到 8～12，单步可到 0.15；累计位移设硬顶，例如 0.5，防止一帧飞出楼梯）。推出后同步 `_villageWorldY`、刚体、Transform，**清 `depthVelocity` 和指向墙内的 vx**。  
   - 若仍 Overlap 且 last-free 有效：把根位置 / 权威 Y **恢复到 last-free**，`depthVelocity=0`，`WriteVillagePlanarHorizontalSpeed(0)`。  
   - `PostPhysicsResyncDepthCoroutine` 在 WaitForFixedUpdate 后也会 WalkArea + 分离 + 横移夹紧：**同一套保险要再跑一次**，否则物理步后又会嵌回去。

4. **锁 vx 分支：** `footEmbedded && hitCount==0 → allowedAlong=0` 与保险冲突。A 落地后：  
   - **不要**靠它当保险；  
   - 施工默认：有 last-free 或本帧分离已处理重叠时，**不要**再因「Cast 空」把 vx 锁死（可贴边切向滑，OPEN T1）。  
   - 若完全没有 last-free（开局就嵌在墙里），才允许清本帧速度并拼命 Distance 推，仍禁止焊死在原地不管。

**last-free 和 WalkArea 谁先谁后：** 记点在最前；**恢复在最后**（WalkArea 之后）。若恢复点碰巧在 WalkArea 外：优先「墙外」；下一帧 WalkArea 再收进多边形。不要在恢复后再无条件 ClosestPoint 一次（可能再次推进围栏）。OPEN T2：last-free 无效时 **本期不**闪回楼梯中线。

**为何不把 B 当主修：** B 防斜向穿透好，但人已经在 Composite 内部时 Cast 仍可能扫空，**没有「拉回来」**。开发者要的保险是 A。一期若有余量，可在积分前加一条合成方向的短 Cast 作加强，失败仍回退 A。

### 4.5 最小文件列表

| 文件 | 是否改 | 原因 |
|------|--------|------|
| `TownPlayerLocomotion.cs` | **是，主改** | last-free 字段；FixedUpdate 末尾 + PostPhysics 保险；放宽「重叠且 Cast 空锁 vx」 |
| `VillageWalkObstacleTurnImmediateBlock.cs` | **否** | 转身清 vx 仍合理；拉回由 Town 做 |
| `VillageWalkObstacleCollisionBootstrap.cs` | **否** | 禁止恢复硬碰 |
| `VillageTreehouseDepthZoneGate.cs` | **否** | 不改门控 |
| `MoveComponent.cs` | **否** | 禁止全局 CCD / 限速 |
| Prefab / 场景 | **否（代码主修）** | 围栏加厚见 §4.6 辅修，不挡 A 合入 |

### 4.6 场景辅修（不是唯一修复）

给策划 / 关卡，**可与 A 并行，不能代替 A**：

1. **去掉非均匀缩放**：把 rotation/scale 打进 Polygon 顶点，物体 scale 回到 `(1,1,1)`。Unity Composite + 非均匀缩放几何容易脏。  
2. **下边围栏不要做成「楼梯+灌木一整块实心」**：沿线框拆成沿栏杆的窄条，内部留空，Y 向挤出才找得到出口。  
3. Composite **`isTrigger=true`**，对齐 0514 语义（查询已经 `useTriggers=true`，行为不应变差）。  
4. **不要删 Collider**；不要只加 `contactSkin`。

### 4.7 不要误伤 / 严禁

1. **禁止**恢复脚↔障碍物理硬碰 / 关掉 Bootstrap。  
2. **禁止**只改这一块 Continuous / Full Kinematic Contacts 当修复。  
3. **禁止**在 `MoveComponent` 全局限速或全局 CCD。  
4. **禁止**删下边围栏。  
5. **禁止**为这棵树屋写死世界坐标传送点。  
6. **禁止**用方案 E（重叠就锁死）冒充保险。  
7. **禁止回退** 0818 斜向 0.707；**禁止改**树屋双 Trigger 表。  
8. **禁止**关掉纵深射线当唯一防穿（会回归斜台阶刷边早挡）；射线可留，靠 A 兜底。  
9. 贴长条障碍切向滑行：A 放宽锁 vx 之后应仍能滑；回归时测村里其它直墙。

### 4.8 回归清单

见 §3。程序额外：

- 保险日志建议 `[VillageBlockerDepth] insurance overlap→push` / `insurance restore last-free`（用完关现有 debug 开关）。  
- 验收「故意站进线框」：Editor 里把玩家根挪进 Composite 内，Play 1 秒内应在外侧且 A/D、W/S 能走。  
- 上边围栏用同一套 A，不要 `if (name==下边围栏)`。

---

## OPEN 摘要（已写入 `OPEN_QUESTIONS.md`）

见「树屋下边围栏穿模 · 2026-08-19」：

| ID | 施工默认 |
|----|----------|
| **T1** | 贴边允许切向滑，不要硬停。A 放宽「重叠且 X-Cast 空则锁 vx」 |
| **T2** | last-free 失败时 **本期不**闪回楼梯中线；继续 Distance 推 + 打日志。中线传送另案 |
