# Village_HomeScene1 物品悬停光标变眼睛 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 对照：`Assets/Doc/执行文档/8月/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md`。0820 只说明这些物品要能远程点击，不能当成已经有眼睛光标。

---

## ① 结论一句话

缺的是 `CursorChangeTrigger`，碰撞体已经在每个物品的 `Clds/Body` 上。只在 `Village_HomeScene1` 这 6 个场景实例的 `Body` 上加组件、`TargetState = View`。不要改 Prefab 资产：饼干和面包还被 `Village_HomeScene45` 用着。

---

## ② 原因

大白话：鼠标放上去变眼睛，靠物体自己挂一个现成组件去告诉光标中枢「我是观察」。这 6 个物品能点、有碰撞，但没挂这个组件，所以光标一直是普通箭头。眼睛贴图和切换逻辑都已经有了，不用新画，也不用自己 `SetCursor`。

现成调用链（StoneBrand 同款，不要新写）：

```
鼠标世界坐标
  CursorChangeTrigger.Update
    自己身上的 Collider2D.OverlapPoint
      刚进入 → OnPointerEnter
        CursorComponentGM.OnEnterChangeTrigger(TargetState, Priority)
          队列按 Priority 从高到低排，取第一条
          TargetState = View(2) → 播现有眼睛两帧（眼 副本 29 / 31）
      刚离开 → OnPointerExit
        按本次 Guid 出队 → 队列空则回到 Normal
```

`CursorChangeTrigger` 带 `[RequireComponent(typeof(Collider2D))]`，并且 `GetComponent<Collider2D>()` 只认**同一物体**上的碰撞，不认子物体。挂在根上会多出一个对不齐的碰撞盒，鼠标对不准图。

样例 `Village_KenMuNi1` / `StoneBrand`（场景里手摆，不是 Prefab）：`BoxCollider2D` 和 `CursorChangeTrigger` 在同一个物体上。`TargetState: 2`（View），`Priority: 1`。该碑当前 `m_IsActive: 0`，挂法仍以这份字段为准。

对话打开时：`NormalDialogueNewPanel` 上有 `CursorChangeUI`，`TargetState = Normal(0)`，`Priority = 100`。面板 Enable 就进队列，Disable 就出队。物品默认 Priority 是 1，排序是优先级高的在前，100 盖过 1。眼睛**不会**盖住对话框自己的普通光标。不要把物品优先级再改低，也不要改成 100。

---

## 六个物体（`Object` 下，不含 Npc1）

六个都是 Prefab 实例，场景里没有覆写出 `CursorChangeTrigger`。`Village_HomeScene1.unity` 全文没有该脚本。六个都要加，没有「已经是 View、不用改」的。

点击对白都在根上的 `SimpleStoryTrigger`，`requirePlayerOverlap: 0`（远程点击）。本期不要拆。

| 场景层级 | Prefab | 碰撞（鼠标碰得到的） | CursorChangeTrigger | 点击对白 | 改 Prefab 还会影响 |
|----------|--------|----------------------|---------------------|----------|-------------------|
| `Object/饼干/Clds/Body` | `Assets/GameRes/Prefabs/Item/饼干.prefab` | `Body` 上 `BoxCollider2D`，Trigger，约 1.5×1.2 | **没有** | 根上还在，`Village_Npc1_bingan` | `Village_HomeScene45` 也摆了这份 |
| `Object/面包/Clds/Body` | `Assets/GameRes/Prefabs/Item/面包.prefab` | 同上，约 1.5×1.2 | **没有** | `Village_Npc1_mianbao` | 同上，HomeScene45 |
| `Object/木桶/Clds/Body` | `Assets/GameRes/Prefabs/Item/木桶.prefab` | 同上，约 1.5×2 | **没有** | `Village_Npc1_muzhiyuantong` | 只有本场景 |
| `Object/米袋/Clds/Body` | `Assets/GameRes/Prefabs/Item/米袋.prefab` | 同上，约 1.8×1.8 | **没有** | `Village_Npc1_huangmi` | 只有本场景 |
| `Object/木箱/Clds/Body` | `Assets/GameRes/Prefabs/Item/木箱.prefab` | 同上，约 2×1.5 | **没有** | `Village_Npc1_muxiang` | 只有本场景 |
| `Object/土豆/Clds/Body` | `Assets/GameRes/Prefabs/Item/土豆.prefab` | 同上，约 1.5×1.2 | **没有** | `Village_Npc1_tudou` | 只有本场景 |

`Object/Npc1` 是场景里手摆的，不是这 6 个 Prefab。它没有 `CursorChangeTrigger`。本期不要给它加，悬停应仍是普通光标。它的对白是 `Village_Npc1`，近距点击（`requirePlayerOverlap: 1`），不要动。

---

## ③ 用户需要做什么

Play 进 `Village_HomeScene1`。鼠标依次放到饼干、面包、木桶、米袋、木箱、土豆上：应变成和观察碑一样的眼睛；移开恢复普通光标。再移到 `Npc1` 上：不要变成眼睛。

再点其中一个物品，确认对白还能打开；对白开着时，光标应是对话框那套普通光标，不是眼睛压在对话框上。关对白后，鼠标若还在物品上，眼睛应再出现。

不要拿 `Village_HomeScene45` 当这场验收。若施工误把饼干/面包 Apply 回 Prefab，那一场景的这两样也会变眼睛，那是误伤。

---

## ④ 给施工员的补充

### 每个物体改什么 / 不要改什么

只改场景 `Assets/GameRes/Scenes/Village_HomeScene1.unity` 里上述 6 个实例：

在 **`Clds/Body`**（已有 `BoxCollider2D` 的那个物体）上加 `CursorChangeTrigger`。

- `TargetState = View`（枚举值 2，和 StoneBrand 的 `TargetState: 2` 相同）
- `Priority` 保持默认 **1**
- 不要改 `BoxCollider2D` 大小、不要改根上的 `SimpleStoryTrigger`、不要改 `requirePlayerOverlap`

| 不要改 | 原因 |
|--------|------|
| 六份 `Assets/GameRes/Prefabs/Item/*.prefab` | 饼干、面包还在 `Village_HomeScene45`。Apply 会让那一场景这两样也变眼睛 |
| `Npc1` | 本期不在范围 |
| `CursorComponentGM` / `CursorChangeTrigger.cs` | 中枢已能切 View，不要新贴图，不要在别处 `SetCursor` |
| `NormalDialogueNewPanel` 的 `CursorChangeUI` | 已是 Normal、Priority 100，能压过物品的 1 |
| `Village_KenMuNi1` 的 StoneBrand 及其它场景的 Chat/Catch | 不碰 |

编辑器里加在实例的 `Body` 上之后，**不要 Apply 到 Prefab**。存的是场景覆写。

### 推荐方案（只此一种）

按 StoneBrand：组件与碰撞体同一物体。六个 `Body` 各加一个 `CursorChangeTrigger`，`TargetState = View`，`Priority = 1`。移入、移开仍走现有 `OnPointerEnter` / `OnPointerExit`。对白面板 Priority 100 已经盖住眼睛，不用再调优先级。

### 否决方案

- 改成 Chat：那是对话气泡，不是观察碑的眼睛。  
- 在 Update 里自己射线再 `SetCursor`：中枢队列和对话框 Priority 100 都会被绕开，眼睛会盖住对话框光标。

### 会误伤的其它场景

按推荐方案（只改本场景实例、不 Apply）：**无。**

若误改 Prefab：`Village_HomeScene45` 的饼干、面包会一起变眼睛。木桶、米袋、木箱、土豆目前只有本场景在用。
