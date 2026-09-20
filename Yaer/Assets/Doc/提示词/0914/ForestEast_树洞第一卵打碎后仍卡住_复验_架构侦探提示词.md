# Cursor Agent Prompt · ForestEast 树洞第一卵打碎后仍无法前进（恶性残留复验）

> **角色**：【架构侦探】必须 **Play 卡死帧取证** 钉死唯一主因；再【施工员】按证据最小修。禁止再猜着叠补丁。  
> **日期**：2026-09-14（复验票）  
> **严重度**：恶性 — 用户验收 **仍无法前进**；上一版施工视为 **未过验收**  
> **现象**：打碎第一个虫卵（Type3/`spcWormEgg`≈276）之后卡在原地，无法继续往洞内爬  
> **产品期望（钉死）**：碎卵后 **不按 E** 也能越过碎壳继续爬；E 旁白可点；第二颗卵仍挡路可砸；出洞正常  
> **不是**：再写一遍「关 GroundCld / 孵虫放身后」交差（v1.1 已做）；删 E；改死羊；回滚贴底相机；再拖三块地板碰运气  
> **已施工且用户仍报失败（必须先核是否进包）**：  
> - `WormEggLogic`：`EnsureGroundCldDisabled`、`UnlockCrawlAfterEggBreak`、`PlaceHatchedWormBehindPlayer`、`[WormEggBreak]` 日志  
> - `WoodWormLogic.skipPlayerBodyStopMove` 4s + `PlayerBodyCollider` 跳过  
> - `InTreePlayerYOffset` 从 **1 → 0.35**（施工说明写明 1 会嵌 `GroundUp`）  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_树洞第一卵打碎后仍卡住_复验溯源报告.md`  
> **前案**：`执行文档/0914/ForestEast_树洞第一虫卵打碎后卡住_架构溯源报告.md` + `施工说明/0914/ForestEast_树洞第一虫卵打碎后卡住_施工说明.md`（v1.1）

把下面整段交给 Cursor Agent。

---

## 提示词助手预梳理（侦探须用 Play 推翻或坐实，勿当结案）

### 产品白话

> 修了还是过不去。不要再猜。进 Play 砸开第一颗卵，卡死那一帧把人为什么动不了钉死：是顶在天花板上、被虫刹车、输入锁死，还是上一版代码根本没跑到。

### 为何「修了仍卡」很像还没查完

v1.1 已处理：**碎壳盒、自动爬停、孵虫放到玩家身后、免木虫 StopMove 4s**。  
若用户仍「完全无法前进」，主因多半已 **不是** 那几条，而是：

| # | 残留主嫌 | 现网证据（预扫） | 为何像恶性 |
|---|----------|------------------|------------|
| **R1（优先）人嵌 `GroundUp`** | 洞顶 `GroundUp` Layer13 实心盒，中心 **(300.47, -7.35)** Size **(56.1×2)** → 约占 Y **-8.35～-6.35**。进洞 `InTreePlayerYOffset=0.35`：洞外脚≈-6.6 → 洞内≈**-6.95**。`ForestEastTreeEnterTrigger` 注释已写：**offset=1 会 Body 深嵌 GroundUp，像顶死墙**。用户还手动降过 `GroundCenter`。碎卵处正好在洞内段。 | 物理挤死，方向键/爬行都像「坏了」；脚本关盒也没用 |
| **R2 爬区盒与脚 Y 错开** | `CanNotSomeActionArea` 世界 Y 约 **-6.18～-3.10**；脚在 **-6.95** 时可能 **出触发盒**。洞内 `isEnableSquatUp=false` 仍在。 | 自动爬不接手；须靠手动 Climb；若再叠 R1 更像死机 |
| **R3 前方场景木虫 StopMove** | 截图右前方平台幼虫；`storyWoodWormLogicList` 两条 **无** `skipPlayerBodyStopMove`。`PlayerBodyCollider` 对木虫、在 `IsClimbMove` 且虫在右侧时 **每帧 StopMove**。孵虫身后免刹 **挡不住前方虫**。 | 碎卵后一爬就刹；像「过不了碎壳」 |
| **R4 新代码未进 Play** | 未编译 / 未进对场景 / Domain 未 Reload | Console **没有** `[WormEggBreak]` → 前案等于没落地 |
| **R5 输入锁残留** | `StopAutoCrawl` 里先 `SetAllowMove(false)` 再 `DisablePlayerMove(false)`（应恢复）；须证卡死帧 `cantMove`/`Disable` | 完全无位移意图 |

**已降级（勿再当主修，除非日志证明没跑）**：仅关 GroundCld、仅孵虫 Behind、仅 4s skip（v1.1 已做）。

### 强制取证（没有这些禁止写「推荐方案结案」）

新档、左口进洞、蹲击 Type3、**不按 E**、卡死时立刻 Pause，填表：

| 证据 | 命令 / 看什么 | 结果（侦探填） |
|------|----------------|----------------|
| A | Console 有无 `[WormEggBreak] GroundCld off` / `hatch worm BEHIND` / `StopAutoCrawl after break` | |
| B | `[TreeBridgePlayerY] after=` 脚 Y | |
| C | Hierarchy：`GroundUp` 与 Player Body/Foot **是否相交**（Physics2D / Contact） | |
| D | `WormEggType3/GroundCld` activeSelf + enabled | |
| E | 孵虫世界 X vs 玩家 X（应同侧身后） | |
| F | 右前方平台虫：名、Active、是否 `skipPlayerBodyStopMove`、是否触发 StopMove | |
| G | `cantMove` / `DisablePlayerMove` 语义 / `AutoMoveState` / `IsClimbMove` / `canInStateSetPos` / `HasRunningStory` | |
| H | 临时把 `InTreePlayerYOffset` 改 **0** 进洞再砸卵：还卡吗？ | |

**判定树（必须选一条主因）**

```
无 [WormEggBreak] 日志 → R4（代码没跑）→ 先保证编译进包，再测
有日志 + H 改 0 后能过 → R1（GroundUp 嵌死）主因
有日志 + offset=0 仍卡 + 前方虫 Stay 刹 → R3
有日志 + GroundCld 仍 enabled → 回归 A（断言失败）
有日志 + cantMove/Disable 真锁 → R5
```

### 方案（按判定树，禁止全上）

| 主因 | 最小修 | 禁止 |
|------|--------|------|
| **R1** | ① 默认 `InTreePlayerYOffset = 0`；② 仍夹则 **只抬洞内 `GroundUp` 的 Y**（或缩小盒高）让蹲爬净空 ≥ 胶囊；注释写清与地板无关 | 再降 GroundCenter 顶人；offset 加回 1 |
| **R2** | 爬区盒 Offset/Size **下扩**盖住脚 Y，或进洞后不依赖 AutoCrawl、保证手动 Climb | 缩掉洞内必须爬 |
| **R3** | 树洞内爬行对 **非敌对/故事木虫** 短时免 StopMove，或 `playerIsInTreeBridge` 时对木虫不刹（须防穿透设计）；或把挡路虫移开一点 | 全局 Ignore 矩阵 |
| **R4** | 确认 Assembly 刷新、进对 ForestEast、日志出现后再谈逻辑 | 重复粘贴 v1.1 当新修复 |
| **R5** | 修 `StopAutoCrawl`/`Unlock` 顺序，碎卵后断言 `SetAllowMove(true)` | 删输入系统 |

硬约束：

- 活卵 `GroundCld` 开局必须仍挡路  
- 不删 `ViewBrokenEgg`  
- 不改死羊 / 倒树贴底公式（无因果）  
- 本票验收：**用户本地 Play 能越过第一碎壳**；本机未 Play 不得标「已修好」

### 必读

1. 现网：`WormEggLogic.cs`（v1.1 全文）  
2. `ForestEastTreeEnterTrigger.cs`（`InTreePlayerYOffset`、GroundUp 注释）  
3. `PlayerBodyCollider.OnCollisionMonster` + `WoodWormLogic.skipPlayerBodyStopMove`  
4. 场景：`GroundUp` / `GroundCenter` / `CanNotSomeActionArea` / Type3 / 平台木虫  
5. `施工说明/0914/ForestEast_树洞第一虫卵打碎后卡住_施工说明.md`（v1.1 失败声明）  
6. 用户卡死截图  

### 报告必须包含

1. **一句话主因**（只能一个 R#）+ 取证表填满  
2. 是否证明 v1.1 日志已出现  
3. `InTreePlayerYOffset=0` 对比结果  
4. 方案 1 条推荐 + 文件列表 ≤3  
5. OPEN：净空目标值（胶囊高 vs GroundUp 底）

---

## 【施工员】（仅取证报告拍板后）

> **前置**：报告已用 Play 表钉死 R#；写明 v1.1 哪些保留、哪些无效。  
> **目标**：碎 Type3 后不按 E 也能前进。  
> **默认若主因 R1**：`InTreePlayerYOffset=0`；不够再最小抬 `GroundUp`（只动树洞段）。详细注释。  
> **禁止**：再叠一套重复的 GroundCld/Behind；删 E；改死羊；无证据回滚贴底。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_树洞第一卵打碎后仍卡住_复验施工说明.md`（写清相对 v1.1 差什么）  
> **验收**：  
> 1. Console 必有 `[WormEggBreak]`  
> 2. 新档左口 → 碎第一卵 → 不按 E → **越过碎壳**  
> 3. 第二卵仍挡  
> 4. 出洞脚高恢复  
> Debug：`[WormEggBreak]` + `[TreeBridgePlayerY]` + 卡死帧打 `GroundUp` 相交与 `cantMove`。

---

## 【验收员】（施工后必须跑）

> 若仍卡：贴满强制取证表，**禁止**再开「再猜一个嫌疑」的施工。  
> 输出：通过 / 失败主因 R# / 是否需抬 GroundUp 毫米级数值。
