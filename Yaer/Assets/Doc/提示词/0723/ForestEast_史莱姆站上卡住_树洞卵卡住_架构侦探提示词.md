# Cursor Agent Prompt · ForestEast 碰撞卡住（史莱姆 / 树洞卵）

> **角色**：先【架构侦探】，报告通过后再【施工员】  
> **日期**：2026-07-23  
> **场景**：`ForestEastScene` 树洞（`TreeBridge` / `isInTreeBridge`）  
> **截图**：同目录 `树洞_史莱姆与卵卡住_截图.png`（玩家趴姿、左侧史莱姆、前方红色虫卵、头顶 E 提示）

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

截图路径：`Assets/Doc/提示词/0723/树洞_史莱姆与卵卡住_截图.png`
地点：东郊 `ForestEastScene` 树洞内（左侧蓝绿扁史莱姆，玩家前方深红虫卵簇，有 E 交互）。

### 问题 A — 落到史莱姆身上会卡死

1. 玩家跳起后，若落点压在史莱姆身上 → **卡住、动不了**。
2. **期望**：应被**挤开 / 滑落**到地面，继续正常移动，而不是站在/压在史莱姆碰撞体上僵住。
3. **被击飞**后，若碰撞落在史莱姆身上 → 同样卡住（与跳跃落地同源或同类）。

### 问题 B — 树洞被卵挡住无法前进

1. 树洞通道里玩家**无法继续前进**。
2. 玩家会被**虫卵（卵 / WormEgg）**卡住；截图中人贴在卵前，呈趴/爬状态，出 E 提示但仍过不去。
3. **期望**：树洞可玩通（按现有设计：可绕开 / 可打碎卵开路 / 碰撞不应把通道堵死到永久卡死——侦探须先查清「设计意图是什么」，再对照现状）。

两个问题都是「玩家与怪物/场景实体的物理或脚本碰撞」导致位移失效，可同报告、分两条根因链写。

---

## 必读上下文（按需展开）

- `Assets/Doc/技术文档/敌人/史莱姆的攻击系统.md`
- `Assets/Doc/技术文档/Player/玩家受击系统.md`（击飞落地）
- `Assets/Doc/执行文档/0722/ForestScene_普通跳跃跳出屏幕_架构溯源报告.md`
- `Assets/Doc/执行文档/0722/ForestScene_跳跃飞出场景_解耦施工说明.md`
  （刚修过 `CapsuleGroundChecker` / `GroundLayerMask`：落地误判曾导致半空卡住类问题，查清本次是否同类「把史莱姆/卵当地面」或「实体碰撞互卡」）
- `Assets/Doc/技术文档/演出相关/ForestEastScene音乐音效系统.md`（树洞 / TreeBridge 入口）
- 相关脚本线索（侦探自行确认是否完整，可增补）：
  - 玩家：`PlayerMoveComponent`、`CapsuleGroundChecker` / 其它 GroundChecker、击退/击飞组件、`Player.prefab` 碰撞层
  - 史莱姆：`Slime.cs`（注意 Body/Foot Collider、`isTrigger` 切换时机）
  - 虫卵：`WormEggLogic`、`CldControllerComponent`、Prefab `WoodWormEgg` / 场景内卵实例
  - 树洞：`TreeBridgeLogic`、`ForestEastTreeBridgeStoryMgr`、`PlayerSceneData.isInTreeBridge`
  - Physics2D Layer 碰撞矩阵、Monster / Player / Ground 相关层

---

## 侦探任务清单

### A. 史莱姆「站上卡住」

1. 画出：跳跃落地 / 击飞落地 → 与史莱姆 Body/Foot 碰撞 → 玩家 `IsGrounded` / Velocity / 状态机 的完整链路。
2. 确认史莱姆碰撞在 Idle/Move/Attack/Sleep 等状态下：`isTrigger`、Layer、是否参与物理挤开。
3. 判断根因更像哪一类（可多选，须给证据）：
   - 地面检测把史莱姆当成 Ground → `IsGrounded=true` 但位移被挡；
   - 非 Trigger 实体碰撞 + 脚本每帧写 `position`/`velocity` 互抢 → 卡死；
   - 击飞结束落地未清碰撞忽略 / 未恢复可移动标志；
   - 其它（写明）。
4. 「正常挤开掉落」在现有架构里是否已有设计（IgnoreCollision、Trigger、单向平台、脚本推开）？缺失还是坏了？

### B. 树洞卵「挡住无法前进」

1. 树洞内卵的摆放、Collider 形状（是否过大/贴地封死通道）、是否 Trigger、是否可攻击打碎。
2. 玩家在树洞的特殊状态（爬行/低头、`isInTreeBridge`、相机/碰撞盒切换）是否缩小了可通过缝隙，却仍被卵盒挡住。
3. E 提示来自哪套交互；交互是否要求先打碎卵，还是误把「可前进」做成「必须交互却交互无效」。
4. 设计意图 vs 现状对照表（通关条件一句话）。

### C. 公共

1. 是否与 0722 落地检测修复**同源回归**，还是独立碰撞问题。
2. 村内 DNF / `TownPlayerLocomotion` 是否无关（预期无关则明确写「勿改」）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（两个问题可各一句，或合并一句）  
② 原因（生活类比 + 技术锚点：文件 / 类型 / 字段 / Prefab 路径）  
③ 用户需要做什么（检查清单：进哪场景、看 Inspector 哪几个字段、如何复现）  
④ 给程序看的补充：可疑根因优先级表、建议修复方向（**只建议，本阶段不施工**）、相关文件清单、与 0722 落地修复的关系、开放问题记入 `Assets/Doc/OPEN_QUESTIONS.md`（仅当设计意图不清时）

禁止：
- 临时 Ignore 全层、删碰撞、在 Update 堆业务糊弄；
- 未溯源先改 Prefab；
- 改动村庄 `TownPlayerLocomotion`（除非证据证明同源且必须，并先写入开放问题）。

完成后用 MASTER 固定四段式口头汇报结论；详细内容以报告文件为准。
```

---

## 施工员续跑（侦探报告通过后，另开一轮再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md

你现在是【施工员】。按上述溯源报告做**最小化修改**。

目标：
1. 跳到 / 击飞落到史莱姆身上时，能被挤开或滑落，恢复可移动，不卡死。
2. 树洞可按设计前进（打碎卵或修正碰撞），不再被卵永久卡住。

约束：
- 保持现有架构与组件解耦；禁止 Update 堆业务；禁止无关重构。
- 不改 `TownPlayerLocomotion`（除非报告明确要求且已记录开放问题）。
- 若动落地检测，必须回归 Forest 普通跳不飞出场景（见 0722 施工说明）。

交付：
- 写入 `Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_施工执行说明.md`
- 列：改了哪些文件、实现了什么、如何验证（含问题 A/B 复现步骤与期望）
```

---

## 提示词助手备注

| 项 | 说明 |
|----|------|
| 推荐顺序 | 先贴「架构侦探」整段 → 报告评审 → 再贴「施工员」整段 |
| 为何先侦探 | 可能是落地 Mask / Trigger / 场景碰撞盒 / 树洞爬行碰撞 中的一种或组合，盲改易回归 0722 |
| 两问题同报 | 同场景同帧截图，碰撞栈重叠；允许报告内分 A/B 两条链 |
