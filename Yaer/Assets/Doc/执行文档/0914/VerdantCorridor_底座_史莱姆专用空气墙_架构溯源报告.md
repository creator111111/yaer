# VerdantCorridor 底座 · 史莱姆专用空气墙 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**场景 / 挂点**：`VerdantCorridor` → `Map` → `Design` → `Near` → `Part5` → **`底座`**（fileID `86408820`）  
**产品期望**：隐形硬挡，**只拦史莱姆**；自己走、被击退/击飞/冲撞都穿不过；做成可复用组件  
**不是**：挡玩家 / 木虫 / 天琬 / 羊 / 掉落；改史莱姆 AI 数值；改全局 Physics2D 矩阵；复用 `MapLimit`  
**对照**：  
- `MapLimit` + Editor「生成空气墙」= 地图边 Polygon，按 Layer 挡谁碰谁，**不是**只挡史莱姆  
- 0723/0913：`GroundCld`→Trigger 是「怪不挡人」，与本案「墙挡怪」方向相反  
- `VillageWalkObstacleCollisionBootstrap` = 村庄障碍矩阵，**勿**套战斗走廊  
**提示词**：`Assets/Doc/提示词/0914/VerdantCorridor_底座_史莱姆专用空气墙_架构侦探提示词.md`  
**OPEN**：`Assets/Doc/OPEN_QUESTIONS.md` 本节 Q1～Q5

---

## 沟通摘要

### ① 结论一句话

**推荐方案 C**：墙用 Trigger 盒子检测，身份认 `ISlime`/`Slime`，在 `FixedUpdate` 里权威夹紧位置（含扫掠，防止跳攻一帧穿过去）。**能挡住击退**——前提是夹紧必须盖过 `KnockBackComponent` 的 `MovePosition`，撞上后停掉击退曲线。单靠物理硬碰（A/B）现网挡不住。

### ② 原因（通俗）

史莱姆看起来有碰撞盒，但活着的时候三个盒子都是「只检测、不硬顶」的 Trigger。游戏里走路用速度推，被打飞用脚本直接把坐标插值过去。底座目前只是一张图，连碰撞都没有，所以谁都能穿。如果做一道普通空气墙去碰 `OnlyMapObj`，木虫那些带同层盒子的也会被误伤，而且史莱姆的地面盒已经改成 Trigger，碰了也不产生硬解算。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网（未施工） |
|---|------|----------------|
| 1 | Hierarchy：`Design/Near/Part5/底座` | 有 Sprite（约 7.76×5.25），**无 Collider**，Layer=Default，子物体空 |
| 2 | Pause 一只活史莱姆：`Cld/Body`、`Cld/Foot`、`Cld/GroundCld` | Body/Foot 本来就是 Trigger；GroundCld 运行时也被 `Slime.OnInit` 改成 Trigger |
| 3 | 史莱姆自己走向底座 | **穿过**（没有墙） |
| 4 | 朝底座方向把史莱姆打飞 | **穿过**（击退写绝对坐标） |
| 5 | 玩家 / 木虫走同一位置 | 同样穿过（底座本身不挡任何人） |
| 6 | 拍板后施工 | 按下文方案 **C** 挂通用组件；验收矩阵见 §6 |

### ④ 程序补充

见下文 §1～§7。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **现网为何能穿底座** | `底座` 只有 SpriteRenderer，**没有 Collider**；即便补墙，活体史莱姆也 **0 个实心盒** |
| **哪套盒能参与物理硬挡** | **没有**。GroundCld 0913 后 Trigger；Body/Foot Prefab 就是 Trigger。墙↔GroundCld **无效** |
| **刚体** | 根上 `Rigidbody2D`：Dynamic、Continuous、GravityScale=0、Mass=9999、FreezeRotation |
| **走路怎么写位置** | `MoveComponent.OnFixedUpdate` → `rg.velocity = Velocity`（**不是** MovePosition） |
| **击退/击飞** | `KnockBackComponent.FixedUpdate` 按 `startPos + 方向×距离×t + 抛物线抖动` **`rg.MovePosition`**；每帧覆盖目标点 |
| **跳攻** | `SlimeJumpAtkUp/FallState.FixedUpdate` 抛物线 **`MovePosition`**，单帧位移可大于薄墙厚度 |
| **推荐** | **C**（Trigger Overlap + 扫掠夹紧 + 撞墙停击退） |
| **为何不选 A** | Monster 层含木虫等；且全 Trigger → 矩阵对了也不硬碰 |
| **为何不选 B 作首选** | IgnoreCollision 需要对上 **非 Trigger** 盒；现网没有，除非改史莱姆 Prefab 加实心探测盒（影响面大，且击退仍抢 MovePosition） |
| **为何不选 E/F** | E=改全局矩阵，0723 已否；F=`MapLimit` 按层挡全员，本场景还是 **Inactive** |
| **D** | C + 实心探测盒双保险；**本期不必**。Play 仍穿再开（OPEN Q5） |
| **击退能否硬挡住** | **能**，靠夹紧覆盖最后一次 `MovePosition`，不是靠求解器 |

---

## 2. 调用链

### 2.1 现网：走向 / 击退 → 穿过底座

```
底座（Near/Part5/底座）
  Sprite 7.76×5.25，Layer Default，无 Collider
  → 无接触、无解算、无脚本约束

史莱姆走路
  SlimeMoveState.TurnToRight
    → MoveComponent.moveSpeedX = ±baseMoveSpeed
    → MoveLeft / MoveRight
  BaseMonster.FixedUpdate → componentSystem.OnFixedUpdate
    → MoveComponent.OnFixedUpdate
      GroundCheck（自研重力，走廊 GravityScale=0）
      rg.velocity = Velocity
  活体三个 Collider 全 Trigger → 没有墙可挡 velocity
  → 穿过底座图

玩家打中史莱姆
  BattleComponent.OnApplyStatusEffects
    → SlimeWoundState
    → KnockBackComponent.SetKnockBaseData(breakHight, breakTime)
    → ApplyKnockBack(dirPos * -1, breakWidth)
  或旧口 Wound(value, dir, backDistance) → 同样 ApplyKnockBack
  KnockBackComponent.FixedUpdate（Unity 消息，不走 ComponentSystem）
    newPosition = startPos + dir * distance * t + bounceY
    rg.MovePosition(newPosition)   ★ 绝对插值，不读障碍
  → 穿过底座图

跳攻
  SlimeJumpAtkUpState / FallState.FixedUpdate
    CalculateParabolicPosition* → rg.MovePosition(target)
  Prefab jumpHeight=2.5；水平可跨过薄墙
```

### 2.2 方案 C 插入点（施工后）

```
各史莱姆写完 velocity / MovePosition 之后
  SlimeOnlyAirWall.FixedUpdate   [DefaultExecutionOrder(1000)]
    OverlapBox（useTriggers=true，不靠矩阵）
    GetComponentInParent<Slime>() / ISlime
    死尸按 OPEN Q1 跳过
    用上一帧位置做 BoxCast 扫掠（防跳攻穿）
    若进入禁侧 / 体积重叠：
      挤出到墙外
      rg.MovePosition(夹紧点)     ★ 覆盖 KnockBack 的目标
      清掉朝墙内的 Velocity / rg.velocity
      StopKnockBackEffect()       ★ OPEN Q4 默认是
```

`KnockBackComponent` 用自己的 `FixedUpdate`，和 `MoveComponent.OnFixedUpdate` 不是同一调度。夹紧脚本必须 **执行序晚于** KnockBack（默认 0），否则会被下一拍击退目标盖回去。

---

## 3. 证据表

### 3.1 史莱姆 Collider / Layer / Trigger / RB

来源：`Assets/GameRes/Prefabs/Entity/Monster/Slime.prefab` + `BaseMonster.OnInit` + `Slime.OnInit`。

| 节点 | Prefab Layer | 运行时 Layer | Prefab Trigger | 运行时 Trigger | 盒大约（本地） |
|------|--------------|--------------|----------------|----------------|----------------|
| 根 `Slime` | 17 MonsterCenter | `MonsterUp/Center/Down`（随 GroundType） | 无 Collider | — | RB 在根上 |
| `Cld/Body` | 19 AtkCheck | `atkCheckLayer(19)` | **1** | **1** | ≈7.36×4.13 |
| `Cld/Foot` | 17 | 跟根层走 | **1** | **1** | ≈2.36×4.02 |
| `Cld/GroundCld` | 7 OnlyMapObj | **7 OnlyMapObj** | **0** | **`Slime.OnInit` 改为 true** | ≈7.27×2.93 |

刚体：`m_BodyType: 0` Dynamic，`m_CollisionDetection: 1` Continuous，`m_GravityScale: 0`，`m_Constraints: 4` FreezeRotation，`m_Mass: 9999`。

死亡：`bodyCld`/`footCld` 保持 Trigger；`groundCld.enabled=false`；`CldController.SetActiveAll(false)`；`BodyRg.isKinematic=true` + FreezeAll。尸体 **更没有** 可硬碰的盒。

**问答 1**：墙↔GroundCld 硬碰 **无效**。Body/Foot 也是 Trigger。没有「指定一个现成实心盒去 Un-ignore」这条路，除非新建盒子（方案 D）。

### 3.2 位移写位置方式

| 路径 | API | 会不会 Discrete 穿薄墙 |
|------|-----|------------------------|
| 走路 | `rg.velocity = Velocity` | 无实心接触时直接穿；有实心时 Continuous 较稳 |
| 击退 | `rg.MovePosition(绝对插值点)` | **会**。下一帧仍朝 `startPos+offset` 写，求解器挡不住脚本 |
| 跳攻升/落 | `rg.MovePosition(抛物线点)` | **会**。单帧跨度可大于墙厚 |
| `Slime.SetVelocity` | `MovePosition(pos+v)` | 同左 |
| 寻路（Move 态里已注释关闭） | `PathfindingComponent` 也是 MovePosition | 若再启用同样穿 |
| Snap 同轴 | `transform.position` / `rg.position` 只改 Y | 与横向墙无关 |

`MoveComponent.MovePosition` / `AddForce` 存在，**走路主路径不用它们**。

### 3.3 MapLimit 差异（F 否决）

| | `MapLimit` | 本案空气墙 |
|--|------------|------------|
| 用途 | 地图边界 | 场景内局部挡史莱姆 |
| 生成 | Editor `GenerateAirWall` 填 Polygon 路径 | 通用组件 + Box |
| 本场景 | `VerdantCorridor` 的 `MapLimit` **`m_IsActive: 0`** | 要新建 |
| 筛选 | 靠 Layer 矩阵 | 必须按 `ISlime`，不能靠 OnlyMapObj |

### 3.4 底座节点（磁盘已核实）

```
Near (135556671, SortingGroup -2)
  └ Part5 (2038935905)  local (342.79, 0, 0)
       └ 底座 (86408820)  local (23.331, -4.52, 0)
            SpriteRenderer size ≈ 7.76 × 5.25，sortingOrder=2
            无 Collider、无子物体
```

组件 **不要**写死节点名「底座」。挂载只是这一次的场景步骤。

---

## 4. 影响面

| 对象 | 方案 C |
|------|--------|
| 玩家 | Overlap 过滤非 `ISlime` → **不挡**。勿在 `底座` 本体加 Default **实心**盒（会误挡 Default 矩阵里的东西） |
| 木虫 / 天琬 / 羊 | 无 `ISlime` → **不挡** |
| 掉落物 | 子物体 Trigger，不是 `Slime` 根身份（夹紧走根 RB）→ **不挡** |
| 多只史莱姆同时顶墙 | 每只独立夹紧；可沿墙外切面滑（挤出沿法线，允许切向） |
| 对象池 / 后刷怪 | 每帧 Overlap，**不必** IgnoreCollision 注册表 |
| 睡眠 | 仍是 `ISlime` + 同一套盒；默认仍挡（OPEN Q2） |
| 尸体 | `IsDead` 后 kinematic FreezeAll；默认 **不挡**，避免和尸体 Snap/Freeze 抢位（OPEN Q1） |
| 史莱姆 Prefab / AI / 伤害 | **不改** |
| Physics2D 全局矩阵 | **不改** |
| `MapLimit` | **不改** |

`Physics2DComponent`（旧的 Trigger 夹紧、打 Debug、写 `transform.position`）**不要复用**。

---

## 5. 方案对比与推荐

| # | 方向 | 现网是否成立 | 击退 | 只挡史莱姆 | 结论 |
|---|------|--------------|------|------------|------|
| **A** 专用 Layer + 矩阵 | 无实心盒则硬碰不发生；Monster 层会误伤木虫 | 否 | 易误伤 | **否** |
| **B** IgnoreCollision 只对史莱姆盒 | 没有非 Trigger 盒可配对；要改 Prefab；击退仍抢坐标 | 弱 | 筛选可以准 | **不首选** |
| **C** Trigger + 权威夹紧 | 不依赖矩阵；身份 `GetComponentInParent<ISlime>`；扫掠防跳攻 | **是** | **是** | **推荐** |
| **D** A/B + C | 多一套实心探测盒，有 0723「盒挡人」风险 | 更硬 | 要小心 PlayerFoot | 本期否，作退路 |
| **E** 改全局矩阵 | 影响面大 | — | — | **否决** |
| **F** 复用 MapLimit | 挡全员；本场景还关着 | — | 否 | **否决** |

**替代方案说明（给施工员）**：若产品以后坚持「纯物理、零脚本」，必须先给史莱姆加专用实心探测盒，并把它与 PlayerFoot / 其它怪全部 Ignore，再走 B+D。那是另一张工单，不要塞进本期。

---

## 6. 组件职责与挂载草案

### 6.1 API 草案

| 项 | 建议 |
|----|------|
| 类名 | `SlimeOnlyAirWall` |
| 命名空间 | `Game.GameRuntime.Entities.Component.Physics` |
| 路径 | `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/SlimeOnlyAirWall.cs` |
| 依赖 | `[RequireComponent(typeof(BoxCollider2D))]`；`[DefaultExecutionOrder(1000)]` |
| 身份 | `col.GetComponentInParent<Slime>()`（`Slime : BaseMonster, ISlime`）；不要用节点名、不要用 OnlyMapObj |

序列化字段（建议）：

| 字段 | 默认 | 含义 |
|------|------|------|
| 使用身上的 `BoxCollider2D` | 必有 | **必须 `isTrigger = true`** |
| `blockDeadSlimes` | false | OPEN Q1 |
| `blockSleepingSlimes` | true | OPEN Q2 |
| `bidirectional` | true | false 时只拦 `blockNormal` 指向的一侧 |
| `blockNormal` | `(1,0)` 本地 | 单向时的禁侧法线 |
| `stopKnockbackOnHit` | true | OPEN Q4 |
| `gizmoColor` | 半透明黄 | Scene 里画墙 |
| `skin` | 0.05 | 挤出后离开表面，防下一帧再判进 |

**禁止**：字符串 `"底座"` / `"VerdantCorridor"` 分支；在 `Update` 堆逻辑；改 `LayerName` 全局矩阵。

生命周期：

- `Awake`：强制本墙 `isTrigger=true`；缓存 Box。
- `FixedUpdate`：Overlap + 扫掠 + 夹紧。字典记 `instanceId → lastPos`，物体销毁时清。
- **不**调用 `Physics2D.IgnoreCollision` / `IgnoreLayerCollision`。
- `OnDrawGizmos`：画 Box 边界（无美术可见体）。

夹紧时序：

1. 从 `Slime.BodyRg` 和 `MoveComponent` 读写。  
2. `MovePosition` 夹紧点必须发生在 KnockBack 之后（执行序 1000）。  
3. 同时把朝墙内的 `MoveComponent.Velocity.x`（及 `rg.velocity`）清零，否则走路速度下一拍继续拱。  
4. 切向（沿墙滑动）保留，击飞 Y 抖动可保留在墙外。

### 6.2 场景挂载（走廊这一次）

1. 不要把 Collider 打在 `底座` 根上（根是 Default + 可见 Sprite）。  
2. 在 `底座` 下建子物体，命名 **`SlimeAirWall`**（清晰；逻辑仍不依赖这名字）。  
3. 挂 `SlimeOnlyAirWall` + `BoxCollider2D`（Is Trigger）。Layer 建议 **Ignore Raycast**，减少和射线/互动误交；检测用 Overlap 全层 + 身份过滤，不靠该层矩阵。  
4. 尺寸对齐美术：Sprite 约 **7.76×5.25**，本地 `(23.331, -4.52)`。墙一般是竖直板：宽 **0.8～1.5**、高盖战斗轴 **Y≈−6.61** 到跳攻顶点（`jumpHeight=2.5` 再留余量）。左右对齐「不让史莱姆走进底座内侧」的那条边，**不强制改美术图**。  
5. Z=0，与走廊其它 2D 体一致。  
6. 通用性验收：再拖一个空物体挂同一组件，行为应一致。

---

## 7. 验收清单（施工后勾）

| 操作 | 期望 |
|------|------|
| 史莱姆从墙一侧走向底座 | 贴墙停，不穿 |
| 朝墙方向击退 / 击飞 | **不穿**；可贴墙停或沿墙滑；Console 可选 `[SlimeAirWall]` |
| 玩家穿过该位置 | **无阻挡** |
| 同图木虫穿过 | **无阻挡** |
| 打死史莱姆，尸体/掉落 | 不卡在墙上飞掉（默认不挡尸体） |
| 组件挂到临时空物体 | 行为一致 |
| 睡眠史莱姆被打向墙 | 按 OPEN Q2：默认仍不穿 |

Debug 前缀建议（给验收员）：`[SlimeAirWall]` —— Awake 打 Trigger/尺寸；靠近时打身份、夹紧前后 `rg.position`；击退帧打 delta 是否越过墙平面。

---

## 8. 侦探须答（汇总）

1. **硬挡盒**：现网没有。GroundCld 已 Trigger，墙↔GroundCld 无效。夹紧应对 **根 `Rigidbody2D`**。  
2. **击退写法**：`MovePosition` 绝对插值，会穿薄墙；走路是 `velocity`。  
3. **推荐 C**；A/E/F 否；B 缺实心盒且打不过击退脚本。  
4. **API**：见 §6.1。  
5. **挂载**：`底座/SlimeAirWall` 子物体，Trigger Box 对齐美术，不改原图。  
6. **OPEN Q1～Q5** 已写入 `OPEN_QUESTIONS.md`。

---

*拍板后把提示词文末【施工员】段交给 Agent。本期最小改动：新组件 + 场景挂一个子物体，不改史莱姆/矩阵/MapLimit。*
