# Village_HomeScene1 — Npc1 走近无 E（对照 HomeScene23）— 架构溯源报告

**文档性质**：架构侦探产出（对照溯源；**本阶段不改资产/代码**）  
**日期**：2026-08-20  
**现象**：`Village_HomeScene1` / `Object/Npc1` 走近后 **无 E 提示、无法对话**  
**对照**：`Village_HomeScene23` 可对话 NPC（主样板 **`NpcChair`**；辅样板 `Npc1`）  
**依据**：
- 提示词：`Assets/Doc/提示词/0820/Village_HomeScene1_Npc1无E对照HomeScene23_架构侦探提示词.md`
- E 链路样板：`Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md` §3～5（**根 Position Z=0**）
- 黑屏前史：`Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md`
- 代码：`PlayerLogic.checkCanAddKeyTipsInOtherEntity`、`BaseGameSceneManager.GetFirstCanTouchEntiy`、`InteractiveComponent.AreCollidersOverlapping`

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**组件三件套与 HomeScene23 已大体对齐，且磁盘上 `componentsList` None 已清掉；无 E 的首要配置差是 Npc1 根坐标 Z≈0.77（样板为 0）——`AreCollidersOverlapping` 用 `Bounds.Intersects`，Z 不一致会导致永远不相交，GetFirstCanTouch 返回 null。**

---

## ② 原因（生活类比）

门铃外壳、感应器、开关都装好了（Interactive / Body / canTouch / Story 都有），但感应器装在**另一层楼板高度**（纵深 Z 被美术排序拉高）。  
人在一楼走动，感应圈永远对不上 → **不出 E**；不是「没配对话 Prefab」优先。

| 层 | 含义 |
|----|------|
| 黑屏/未注册（已清 None） | 若 Play 仍刷「未注册=>Npc1」→ 实体未活，先修注册；**现网 YAML 已无 None** |
| **Z ≠ 0（主嫌疑）** | 与样板差；村庄交互体约定 Z=0 |
| 对话 Prefab | `Village_Npc1` 磁盘存在；**次要**（有 E 后再验） |

---

## ③ 用户需要做什么（检查清单）

> 只改 **HomeScene1 / Object / Npc1**；**不要**改 HomeScene23。

### 0）先确认实体已活（黑屏案连带）

| # | 检查 | 通过标准 |
|---|------|----------|
| 0.1 | Play 进 HomeScene1 | Console **无** `InitComponents` NRE、**无**「未注册=>Npc1」 |
| 0.2 | `ComponentSystemMono` List | 仅 Interactive，**无 None**（现网 YAML 已是） |

若 0.1 失败 → **先按黑屏报告修**，再谈 E。

### 1）对齐样板：根 Transform Z = 0（本期最优先）

| 物体 | 现网 Position | 应改 |
|------|---------------|------|
| **Npc1** | `(-19.94, -0.425, **0.769**)` | Z 改为 **`0`**（XY 可先不动） |

对照：HomeScene23 `NpcChair` / `Npc1` 根 **Z=0**。  
村庄约定见埃吉尔说明 §：根物体 **Position Z = 0**。

一眼验：Scene 视图选中 Npc1，Inspector Transform Z 必须是 `0`。

### 2）其余项（现网已齐，核对即可）

| # | Inspector | 期望 |
|---|-----------|------|
| 2.1 | `BaseEntityControll.canTouchWithPlayer` | **勾选**（现网 `1`） |
| 2.2 | `entityType` | NPC（现网 `3`，与样板同） |
| 2.3 | `InteractiveComponent.interactiveCollider` | `Clds/Body` 的 BoxCollider2D |
| 2.4 | Body `Is Trigger` | 勾选；Size 现网 `(2, 4)`（够用；可再仿 NpcChair `(2.31, 4.94)`） |
| 2.5 | `raycastListeners` | 含 Body 上 RaycastListener |
| 2.6 | `requirePlayerOverlap` | **true**（近距 NPC；**勿改成远程物品**） |
| 2.7 | `SimpleStoryTrigger.StoryPrefabName` | **`Village_Npc1`**（勿写成龙宫 `HomeScene1Npc1`） |
| 2.8 | 在 `objRoot=Object` 下 | 现网是 |

### 3）验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走近 Npc1 | 出现 **E** |
| 2 | 按 E | 播 `Village_Npc1` |
| 3 | HomeScene23 原 NPC | 不回归 |
| 4 | Console | 无未注册 / NRE |

**禁止**：把 Npc1 改成 `requirePlayerOverlap=false` 来「绕过」无 E；改 HS23 迁就；只查 Prefab 却不查 Z。

---

## ④ 给程序看的补充

### 4.1 E 链路与断点

```text
PlayerLogic.OnUpdate
  → checkCanAddKeyTipsInOtherEntity
  → GetFirstCanTouchEntiy(null)
       玩家 InteractiveCollider.bounds
       与 各实体 InteractiveCollider.bounds
       AreCollidersOverlapping → Bounds.Intersects（含 Z）
  → canTouchWithPlayer
  → AddKeyTipsNode / 显示 E
按 E → OnInteractive → SimpleStoryTrigger → Village_Npc1
```

**现网最可能断在：bounds 永不相交（根因 Z）→ GetFirstCanTouch 返回 null → 无 E。**

`OverlapPadding` 仅 Expand **XY**（`new Vector3(pad, pad, 0)`），**不救 Z**。

### 4.2 完整对照表

样板：`Village_HomeScene23` / **`NpcChair`**（已验收可对话）；辅：同场景 `Npc1`。

| 字段/组件 | HS23 NpcChair（样板） | HS1 Npc1 | 一致？ |
|-----------|----------------------|----------|--------|
| SceneEntity | 有 | 有 | ✅ |
| 在 objRoot 子树 / 重扫 | 是 | 是（`Object`） | ✅ |
| `canTouchWithPlayer` | true | true | ✅ |
| `entityType` | 3 | 3 | ✅ |
| Interactive → Collider | Body BoxCollider2D | Body `9200000014` | ✅ |
| Body IsTrigger | true | true | ✅ |
| Body Size | ≈`(2.31, 4.94)` | `(2, 4)` | △ 略小但应可碰 |
| Body / 根 **世界 Z** | **0** | 根 **≈0.77** | ❌ **主差** |
| RaycastListener + 列表 | 有 | 有（`9200000015`） | ✅ |
| `requirePlayerOverlap` | true | true | ✅ |
| SimpleStoryTrigger | 有 | 有 | ✅ |
| StoryPrefabName | （任务/循环名） | `Village_Npc1` | ✅ 名合法且磁盘有 Prefab |
| `componentsList` None | 无 | **现网无**（曾有，已清） | ✅ 现网 |
| Layer | 21 | 21 | ✅ |

辅样板 HS23 `Npc1`：根 Z=0；Story 为 `HomeScene1Npc1`（龙宫名，属该屋历史配置）；Body Size ≈`(1.24, 5.56)`。

### 4.3 与黑屏案关系

| 时点 | 状态 |
|------|------|
| 黑屏报告时 | 6 物 `componentsList` 含 None → Init NRE → 未注册连带 |
| **本次磁盘** | 7 物 List 均仅 Interactive、**无 None** |
| 推论 | 若开发者已清 None 后仍无 E → **不是**「整实体死」，而是 **overlap/Z**；若 Play 仍未注册 → 先修注册 |

饼干等物品根 Z 更高（约 7～8）：远程点击不依赖 E/overlap；**不能**用「物品能点」证明 Npc1 近距感应正常。

### 4.4 站位粗算（Z=0 之后仍无 E 再查）

| | Y |
|--|---|
| HS1 Born（左/默） | ≈ `-3.65` |
| 玩家 Event 盒 | offset.y≈`2.54`，size.y≈`5.26` → 相对脚底约 `[-0.1, +5.2]` |
| Npc1 + Body `(2,4)` | 中心 y≈`-0.43` → 约 `[-2.43, +1.58]` |

XY 上走近 Npc1 后 **应能相交**；故静态读优先钉 **Z**，而非先狂加 Size。

### 4.5 最小修复顺序（施工员）

1. Play 确认无未注册 / NRE  
2. **Npc1 Transform Z → 0**  
3. 走近验 E → 按 E 验 `Village_Npc1`  
4. 仍无 E：Scene 开 Collider 可视化，看玩家 Event 与 Body 是否相交；再考虑加大 Body / 下移 offset  
5. 不改 HS23；Npc1 保持近距  

### 4.6 OPEN

| ID | 问题 | 建议默认 | 状态 |
|----|------|----------|------|
| Q1 | 互动物品根 Z 很高（排序）是否统一改 0？ | **仅近距要 E 的改 0**；远程物品可保留排序 Z | 待确认 |
| Q2 | Body 尺寸是否强制对齐 NpcChair？ | 先 Z=0；不够再加大 | 待确认 |
| Q3 | `AreCollidersOverlapping` 是否改为忽略 Z？ | 可选加固；**本期优先改资产 Z** | 待确认 |

---

## ⑤ 验收回写（施工后填）

| # | 结果 |
|---|------|
| 走近出 E | |
| 按 E 播 Village_Npc1 | |
| HS23 不回归 | |
| 无未注册/NRE | |
