# Cursor Agent Prompt · ForestEast 跳跃落到藤蔓（TenWan）身上卡住

> **角色**：先【架构侦探】，报告通过后再【施工员】  
> **日期**：2026-07-23  
> **场景**：`ForestEastScene`（或同图含史莱姆 + 藤蔓怪的战斗场；侦探以截图与场景实例为准）  
> **截图**：同目录 `ForestEast_藤蔓站上卡住_截图.png`（玩家踩在细高绿色藤蔓顶端，左侧有紫色史莱姆）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、Layer 矩阵。只读扫描 + 写溯源报告。

---

## 现象（玩家白话，已用截图对齐）

截图路径：`Assets/Doc/提示词/0723/ForestEast_藤蔓站上卡住_截图.png`

1. 玩家跳跃后，若落点压在**藤蔓怪物（TenWan）**身上（尤其是细长茎/顶端）→ **卡住、下不来、动不了**。
2. 截图中人像「踩」在藤蔓尖上悬空，无法正常落地或走开。
3. **期望**：跳到藤蔓上应被**挤开 / 滑落**到地面，继续正常移动；不应把藤蔓当成可站踏板并僵住。
4. （可选对照）若被击飞后落到藤蔓上是否同样卡住——与跳跃落地是否同源，侦探一并确认。

---

## 必读上下文（强烈相关，勿跳过）

今日刚修过「跳到史莱姆身上卡住」，根因与决议如下，**本案高度疑似同族残留**：

- `Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`
- `Assets/Doc/OPEN_QUESTIONS.md` 一节「ForestEast · 史莱姆站上卡住…」：
  - **Q2 已施工**：史莱姆 `GroundCld.isTrigger=true`；`BaseMonster.OnDead` 关 `groundCld`；**不改** Physics2D 矩阵、不改 GroundLayerMask
  - **Q5**：本期**不恢复** `PlayerBodyCollider` 挤出订阅
- 史莱姆侧参考实现：`Slime.cs` 里对 `groundCld.isTrigger = true` 的注释与写法
- 落地检测背景（勿回归）：
  - `Assets/Doc/执行文档/0722/ForestScene_普通跳跃跳出屏幕_架构溯源报告.md`
  - `Assets/Doc/执行文档/0722/ForestScene_跳跃飞出场景_解耦施工说明.md`

相关脚本 / Prefab 线索（侦探自行确认是否完整，可增补）：

- 藤蔓战斗怪：`TenWanLogic`、`TenWanSM` / 各 `TenWan*State`、`Assets/GameRes/Prefabs/Entity/Monster/TenWan.prefab`
- 场景藤蔓障碍（若实例不是战斗怪）：`TenWanSceneObjLogic`（注释写明「场景藤蔓不是怪物」——须先分清截图里是哪一种）
- 基类：`BaseMonster`（`groundCld` 强制 OnlyMapObj、死后关盒）
- 玩家：`PlayerMoveComponent`、`CapsuleGroundChecker`、`JumpFallState` / `DamageFlyFallState`、`PlayerBodyCollider`
- Physics2D Layer：PlayerFoot / OnlyMapObj / Player / Monster 相关矩阵

静态线索（侦探须**运行时接触**验证，勿当结论）：
- `TenWan.prefab` 内存在 `GroundCld`，且 Prefab 里 `m_IsTrigger: 0`（实心），与史莱姆修复前形态一致。

---

## 侦探任务清单

1. **分清实体**：截图藤蔓是 `TenWan` 战斗怪，还是 `TenWanSceneObj` 场景障碍？场景路径 / 实例名写进报告。
2. 画出链路：跳跃（或击飞）下落 → 与藤蔓各 Collider（Body / Foot / GroundCld / 攻击盒）接触 → 玩家 `IsGrounded` / Velocity / 状态机（尤其 `JumpFall*`）是否卡死。
3. 对照史莱姆报告：是否仍是「**OnlyMapObj 实心 GroundCld 托住 PlayerFoot + 下落态死等 IsGrounded + 挤开已关**」？若是，标为**同族残留**；若否，写明差异（例如细长盒形状、Freeze、Mass、中立单位特殊逻辑）。
4. 确认藤蔓各碰撞在 Idle / Awake / Attack / Dead 下：`isTrigger`、Layer、尺寸是否过大或尖顶易卡人。
5. `BaseMonster.OnDead` 关 `groundCld` 对藤蔓是否已覆盖；存活态是否仍应用史莱姆同款「GroundCld→Trigger」策略（只建议，本阶段不施工）。
6. 是否与 0722 落地 Mask 修复同源回归？村内 `TownPlayerLocomotion` 是否无关（预期无关则写「勿改」）。
7. 若设计意图不清（例如「藤蔓本就可当平台」），记入 `Assets/Doc/OPEN_QUESTIONS.md`，**不要擅自改核心设计**。

---

## 输出要求

写入：`Assets/Doc/执行文档/0723/ForestEast_藤蔓站上卡住_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比 + 技术锚点：文件 / 类型 / 字段 / Prefab 路径）  
③ 用户需要做什么（检查清单：进哪场景、Inspector 看哪几个字段、如何复现）  
④ 给程序看的补充：可疑根因优先级表、建议修复方向（**只建议，本阶段不施工**）、相关文件清单、与「史莱姆 GroundCld→Trigger」施工的关系、开放问题（仅当设计不清）

禁止：
- 临时 Ignore 全层、删碰撞、在 Update 堆业务糊弄；
- 未溯源先改 Prefab；
- 改动村庄 `TownPlayerLocomotion`；
- 为修本案去改 Physics2D 矩阵（OPEN_QUESTIONS Q1/Q2 已决议保卵/挡板）；若仍认为必须改矩阵，先写入开放问题再停。

完成后用 MASTER 固定四段式口头汇报结论；详细内容以报告文件为准。
```

---

## 施工员续跑（侦探报告通过后，另开一轮再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0723/ForestEast_藤蔓站上卡住_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**。

目标：
1. 跳到 /（若报告确认）击飞落到藤蔓身上时，能被挤开或滑落，恢复可移动，不卡死。
2. 优先沿用今日史莱姆决议：怪侧 `GroundCld` 处理 + 死后关盒；**不改**矩阵、**不恢复**挤出订阅（除非报告与开放问题明确要求改决议）。

约束：
- 保持现有架构与组件解耦；禁止 Update 堆业务；禁止无关重构。
- 不改 `TownPlayerLocomotion`。
- 若动落地检测，必须回归 Forest 普通跳不飞出场景（见 0722 施工说明）。
- 史莱姆 / 虫卵已修路径勿无故回滚。

交付：
- 写入 `Assets/Doc/执行文档/0723/ForestEast_藤蔓站上卡住_施工执行说明.md`
- 列：改了哪些文件、实现了什么、如何验证（含复现步骤与期望）
```

---

## 提示词助手备注

| 项 | 说明 |
|----|------|
| 推荐顺序 | 先贴「架构侦探」整段 → 报告评审 → 再贴「施工员」整段 |
| 为何先侦探 | 虽高度像史莱姆同族 `GroundCld`，但须先确认是战斗 `TenWan` 还是场景 `TenWanSceneObj`，避免误改障碍挡板 |
| 与今日史莱姆修复关系 | 史莱姆已 `GroundCld→Trigger`；藤蔓 Prefab 静态仍见实心 `GroundCld`，优先按「同族残留」溯源 |
| 截图 | `Assets/Doc/提示词/0723/ForestEast_藤蔓站上卡住_截图.png` |
