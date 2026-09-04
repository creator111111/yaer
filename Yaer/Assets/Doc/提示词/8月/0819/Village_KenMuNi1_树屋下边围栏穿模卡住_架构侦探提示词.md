# Cursor Agent Prompt · 树屋下边围栏穿模卡住（要保险措施）

> **角色**：先【架构侦探】只溯源、不改代码；报告拍板后再开【施工员】  
> **日期**：2026-08-19  
> **场景**：`Village_KenMuNi1` 树屋螺旋楼梯  
> **已确认现象（开发者 + 截图）**：在树屋上走路时，很容易**突破「下边围栏」的 Collider**，然后**穿模卡在里面不动**。  
> **产品口径**：要做**保险措施**——人不要穿进围栏里；万一已经进去，也要能被推回可走区，不能卡死。  
> **范围**：仅村庄 `Village2_5D` 的 Walk 障碍。不改龙宫、不改 Forest 战斗、不推翻 0514「脚本挡人」总策略、**不回退** 0818 合速度归一、不把本案改成「只把围栏 Collider 加厚就交差」（可作辅修，不能当唯一方案）。  
> **本阶段**：只读 + 写补丁级溯源。禁止改 C# / Prefab / 场景。

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品口径

| 操作 | 现网体感 | 期望 |
|------|----------|------|
| 树屋楼梯上沿围栏走（含斜向） | 容易穿进「下边围栏」线框里 | **脚底不得进入围栏内侧** |
| 已经穿进去 | **卡在里面不动** | **必须有保险**：推回线框外侧可走区，恢复走路 |
| 贴着围栏走 | 可能抖 / 提前挡 | 可贴边滑，不要整段锁死 |
| 上边围栏 / 其它 VillageWalkObstacle | 未报 | 修法须通用，不要只特判这一块 |

开发者原话：「很容易就会突破这个下边围栏的 collider，然后就会穿模卡在里面不动」；「看看有没有办法做个保险措施让玩家不要穿模」。

### 生活类比

围栏是斜着的薄墙。现网拦人像只查「这一步是纯左右还是纯上下」，斜着走等于两步合成一步，容易从缝里钻进去。钻进去之后，护栏又把左右速度掐死，前后挤出只沿着上下找空位——斜墙里面上下都是墙，于是人被焊在栏杆里。

### 截图与场景锚点（以 Scene 实例为准）

截图（同目录）：

- `Village_KenMuNi1_树屋下边围栏_Scene线框.png`：螺旋楼梯外缘绿线 = 下边围栏
- `Village_KenMuNi1_树屋下边围栏_Collider1_Inspector.png`：目标物体 Inspector
- `Village_KenMuNi1_树屋下边围栏_Hierarchy.png`：Hierarchy 红箭头

路径：

```
Village_KenMuNi1
  Map / Design / 肯姆尼1合层 / 树屋
    DepthZone&Colliders / Root / 下边围栏 / Collider (1)
```

Inspector 预扫（Play 下再核对，可能被 Composite 改写）：

| 项 | 截图值 | 预扫含义 |
|----|--------|----------|
| Layer | `VillageWalkObstacle` | 走脚本 Cast/Overlap 挡人，**不是**刚体硬碰 |
| PolygonCollider2D | Used By Composite | 真正查询形状多半是父级 **CompositeCollider2D** |
| Rigidbody2D | **Kinematic**、Discrete、Full Kinematic Contacts 关 | 方案 1 下矩阵已 Ignore，Discrete **不是**主因；别把 CCD 当灵丹 |
| CompositeCollider2D | Geometry=Polygons，**Is Trigger 截图为关** | 现网 Cast 的 `useTriggers=true`；Trigger 开/关侦探必须写清「查不查得到」 |
| Transform | Z 旋转 **-72.3°**，Y scale **≈3.47** | 斜薄墙 + 非均匀缩放，最易被「只扫 X 或只扫 Y」漏拦 |
| 父节点 | `DepthZone&Colliders` | 门控会整包 `SetActive`；穿模是否只发生在树屋激活后 |

对照物体：同级 `上边围栏/Collider (1)`。侦探写清：下边更容易穿，是形状更斜/更薄，还是走位更贴这条边。

### 现网挡人链路（预扫：为何能穿、为何卡住）

0514 已定：**Collider 只定范围，挡人靠脚本**。`VillageWalkObstacleCollisionBootstrap` 对障碍层 **全部 Ignore**（含 PlayerFoot），刚体不会把人顶出来。

`TownPlayerLocomotion.OnFixedUpdate` 顺序（简化）：

1. 纵深积分 → 合速度归一  
2. **只沿世界 Y** `ApplyVillageWalkObstacleDepthClamp`（默认可改用脚底射线，注释写明斜栅栏侧棱会「刷边」）  
3. 写权威 Y（**保留 vx**）  
4. WalkArea 多边形修正（可能改 X/Y）  
5. **`ApplyVillageWalkObstacleFootPenetrationSeparation`**：`Physics2D.Distance` 短迭代，单步上限约 **0.07**  
6. **只沿世界 X** `ApplyVillageWalkObstacleHorizontalVelocityClamp`

挤出 `TryDepenetrateFootFromWalkObstacles`：**只沿 Y** 以 0.04 步进搜空位，最多 24 步。搜不到就留在障碍里。

横移夹紧还有一条：脚已重叠且沿 X 的 Cast **零命中** → **本帧 vx=0**。斜围栏在壳内时，X 向 Cast 经常扫空 → **人被锁死在栏杆里**。这和「穿模后卡住不动」高度吻合，侦探必须打假。

斜向一帧同时有 vx 和 depthVelocity：现网 **没有沿实际位移向量的 2D 扫掠**。合速度归一后斜向更快/分量仍约 0.707×11.2，单帧步长更大。0819 惯性若未施工，松键滑行会再加大穿透。

### 与旧文档边界（不要修错病）

| 文档 / 补丁 | 做什么 | 本案 |
|-------------|--------|------|
| 0514 方案 1 | 矩阵全 Ignore，脚本 Cast 挡 | **禁止**改回纯物理硬碰当主方案（已观察挤出不稳） |
| 0512 WalkArea | 可走区外边界 | 围栏是区内障碍，**不要**用裁切整块楼梯冒充 |
| 0513 树屋 DepthZone 门控 | 激活 `DepthZone&Colliders` | 可查「未激活时无墙 / 激活后才穿」；**不要**改门控表 |
| 0818 合速度 | 斜向 0.707 | **禁止回退**；可写「斜向步长是否加重穿透」 |
| 0819 斜向惯性 | 松手立刻停（若已合入） | 惯性不是本案主修；未合入则记为加重因素 |

### 严禁的施工方向（预判）

1. 只把这一块 Composite 勾成 Continuous / 打开 Full Kinematic Contacts，当修复（矩阵已 Ignore）。  
2. 关掉 `VillageWalkObstacleCollisionBootstrap`，让刚体硬顶。  
3. 在 `MoveComponent` 全局限速 / 全局 CCD。  
4. 删掉下边围栏 Collider「就不会卡住」（人会掉出楼梯）。  
5. 只改 `contactSkin` 数字交差（可能更早挡，仍无「进去后拉回来」的保险）。  
6. 为这一棵树屋写死坐标传送点。

### 侦探须比较（只推荐一个主方案；保险必须有）

目标：**进不去** + **进去了也能出来**。后者是开发者点名的保险。

| 方案 | 摘要 | 防穿 | 卡死保险 | 风险 |
|------|------|------|----------|------|
| **A 进障后 2D 拉回 + 上一帧安全点** | 每帧若 Foot 与障碍 Overlap：用 Distance 法向推出（提高步数/步长或循环直到不重叠）；若仍重叠，把根位置/权威 Y 恢复到本帧开始前的 **last-free** 点并清本帧速度 | 高（恢复点） | **高** | 最小，接着现有 Distance 分离；须防 WalkArea 把人再推进墙 |
| **B 按实际位移做 2D 扫掠** | 用 `(vx*dt, depthVel*dt)` 合成方向 Cast，命中则停在接触前 | 高（斜向） | 中：已在内部时 Cast 仍可能扫空 | 要改夹紧主循环；射线纵深开关须一起定 |
| **C 只加厚 / 重画围栏** | Composite 加厚、拆段、去掉 -72° 非均匀缩放 | 中 | **无**（进去仍卡） | 可作辅修，不能当唯一方案 |
| **D 恢复物理碰撞** | 障碍与 Foot 硬碰 + Continuous | 不稳 | 可能挤飞 | 违背 0514，否 |
| **E 重叠则整帧禁止移动** | 已嵌入就把 vx/depth 清零 | 低 | **更卡** | 现网横移零命中分支已有此味，疑为主因之一 |

预扫建议 **A 为主、B 为加强防穿**（报告里写清能否一期内做完）。C 可给策划：斜薄墙不要非均匀缩放。  
**保险验收**：故意站进线框内（或日志确认 overlap）后，下一拍必须出现在线框外且能走。禁止推荐 E。

`TryDepenetrate` 只搜 Y、横移重叠且 Cast 空则锁 vx：这两条必须在报告里裁定「是不是卡死主因」。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md
@Assets/Doc/执行文档/5月/0514/优化VillageWalkObstacle判定问题_架构溯源与执行说明.md
@Assets/Doc/执行文档/5月/0512/村庄WalkArea内部阻挡碰撞体_程序施工执行说明.md
@Assets/Doc/执行文档/5月/0513/树屋双触发顺序激活DepthZoneColliders_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/0818/村庄斜向移动速度叠加_架构溯源报告.md
@Assets/Doc/执行文档/0819/村庄斜向走路惯性_架构溯源报告.md
@Assets/Doc/提示词/0819/Village_KenMuNi1_树屋下边围栏_Scene线框.png
@Assets/Doc/提示词/0819/Village_KenMuNi1_树屋下边围栏_Collider1_Inspector.png
@Assets/Doc/提示词/0819/Village_KenMuNi1_树屋下边围栏_Hierarchy.png
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageWalkObstacleCollisionBootstrap.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageTreehouseDepthZoneGate.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageWalkObstacleTurnImmediateBlock.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、Layer 矩阵。只读扫描 + 写补丁报告。

---

## 背景

1. 玩家在 `Village_KenMuNi1` 树屋楼梯上走，容易穿进「下边围栏」`Collider (1)`，然后卡在里面不动。
2. 开发者要的是**保险措施**：不要穿模；穿进去也要能出来，不能焊死。
3. 不是 DepthZone 排序错、不是门控开关、不是点 A 走反。那些另案。

---

## 必读

### A. 这一块墙是什么
- Hierarchy：`树屋/DepthZone&Colliders/Root/下边围栏/Collider (1)`
- Composite 最终几何、Is Trigger、Kinematic、旋转 -72°、非均匀 Scale
- 查询命中的是 Polygon 还是 Composite；`useTriggers=true` 与截图 Trigger 关是否导致「线框在、查询空」或「查得到却夹不稳」
- 对照 `上边围栏`：形状/厚度/角度差异

### B. 挡人脚本为何拦不住斜墙
- 纵深只扫 Y（脚底射线 vs 形状 Cast）
- 横移只扫 X；重叠且 hitCount==0 时是否把 vx 锁 0
- Distance 分离步长 0.07、迭代次数；`TryDepenetrate` 只沿 Y
- 斜向一帧 Δx+Δy 有无合成扫掠
- WalkArea 修正是否会把人推进围栏
- 树屋门控 SetActive 与障碍生效时机

### C. 卡住不动
- 穿入后：moveSpeedX、depthVelocity、Overlap、Cast 命中数、是否仍 CombatRun
- 锁 vx + Y 挤出失败 是否就是卡死
- 与 0819 惯性（若未修）是否加重，但本案主修是穿模保险

### D. 不要误伤
- 0514 脚本挡人总策略保留
- 贴长条障碍切向滑行不要整段锁死（现网注释已提）
- 纯 W/S 仍不得叠横向（0513）
- 不改树屋双 Trigger 门控表
- 不回退 0818 斜向 0.707

---

## 侦探任务

1. **结论一句话**：为什么容易穿下边围栏，以及穿进去为什么动不了。
2. **推荐方案**（A/B/C/…）只选一个主方案；**必须包含「已嵌入时拉回可走区」的保险**，并写清 last-free 存在哪一帧、和 WalkArea 谁先谁后。
3. **最小文件列表**；优先只动 Town 障碍夹紧/分离，不要改战斗 Move。
4. **场景辅修**：这块 Collider 是否建议加厚/拆段/去掉非均匀缩放（辅修清单，不是唯一修复）。
5. **验收**：
   - 楼梯上只 D、只 W、D+W 贴下边围栏走 ≥5 秒：人在线框外侧，不进栏杆
   - 斜向贴弯角反复走：不穿
   - **保险**：人已与围栏 Overlap（或调试传送进线框）后 1 秒内必须在线框外且能走
   - 上边围栏、村内其它障碍、WalkArea 外边界不回归
   - 树屋门控上下楼仍正常
6. OPEN「树屋下边围栏穿模 · 2026-08-19」：贴边滑移 vs 硬停；last-free 失败时是否允许闪回楼梯中线。
7. **禁止**：改资产当主修；恢复物理硬碰；用锁死速度冒充保险。

---

## 输出

写入：`Assets/Doc/执行文档/0819/Village_KenMuNi1_树屋下边围栏穿模卡住_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：斜薄墙 + 横竖分开拦 + 进去反锁死）  
③ 用户验收清单  
④ 给程序：查询几何、夹紧缺口、卡死分支、推荐保险算法、回归  

口头汇报用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0819/Village_KenMuNi1_树屋下边围栏穿模卡住_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按报告做最小化修改：树屋下边围栏不得穿模；若已嵌入必须拉回可走区，禁止卡在栏杆里不动。

必须：保留 0514 脚本挡人；不恢复障碍物理硬碰；不回退 0818 合速度；不改树屋门控；贴边切向滑动不要整段锁死。保险优先 last-free / Distance 推出，不要写死传送坐标。

提交说明：防穿怎么拦斜向、嵌入后怎么拉回、如何验收「进线框也能出来」。
```
