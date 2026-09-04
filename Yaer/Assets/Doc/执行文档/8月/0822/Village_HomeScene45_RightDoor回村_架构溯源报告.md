# Village_HomeScene45 — RightDoor 回村出口 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读盘点；**本文件不施工**（不改场景 / 代码 / Config）  
**Unity**：2020.3.48f1 / C#  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
**产品拍板（2026-08-22）**：主出口 = **`Map/MapRight/RightDoor`** → **`Village_KenMuNi1`**（`House_Npc45` 门外落点）

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_RightDoor回村_架构侦探提示词.md`
- 先例：`0821/Village_HomeScene45_LeftDoor无法退出`（当时主出口 LeftDoor；**现网已变，见 §4.8**）
- 样板：`Village_HomeScene23`（右门主出口、左门 Disable）

---

## ① 结论一句话

**产品改走右门，但现网仍是 0821 左门方案：LeftDoor 已齐件且 `SceneChangeDoor` 已启用（走进能回村），RightDoor 虽 `Next=Village_KenMuNi1`、Interactive 齐，但 `SceneChangeDoor.m_Enabled=0` 仍出不去。施工只需两步：启用 RightDoor 换场 + 按 HomeScene23 禁用 LeftDoor（`SceneChangeDoor` Disable + 清空 Next）；EnterPos 双侧已配对，不必改 Manager；布局改过后须 Play 验 RightDoor Trigger 是否盖住通道。**

---

## ② 原因（生活类比）

屋里两扇门都挂着「回村子」的牌子。**左门** 0821 施工后门锁电路已接通，**现在走进去能换场**；**右门** 牌子、感应器都在，但 **换场主板电源仍被拔掉**（`SceneChangeDoor` 禁用）。产品说只走右门——要开右门电源，并把左门牌子撕掉（Disable 换场），否则玩家从左门也能出去，形成双出口。

EnterPos、村门 `House_Npc45`、Build、专用 Manager 早已接通；**不是**进屋链断了，是**主出口拍板从 Left 改 Right，场景门开关还没跟着翻**。

---

## ③ 用户需要做什么

1. **认主出口**：本期只走 **`MapRight/RightDoor`**，回 **`Village_KenMuNi1`**。  
2. **认左门处理**：施工后 **LeftDoor 不应再换场**（对齐 HomeScene23）。  
3. **认落点**：出屋落在 **`House_Npc45` 门外**；再从村进屋应落在室内 **`RightBorn`** 一侧。  
4. 布局已改：施工后 **必须 Play 走进右门**，若踩不到 Trigger 只调 RightDoor 碰撞体，不改换场代码。

### 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 进 45 号屋 | 不黑屏；落点合理（近 RightBorn） |
| 2 | 屋内走向 **RightDoor** | 换场到 `Village_KenMuNi1` |
| 3 | 出屋落点 | `House_Npc45` 门外（非森林、非错位） |
| 4 | 再走 **LeftDoor** | **不应**换场 |
| 5 | 再进屋 | `lastScene=村` 时仍落在 RightBorn 合理位置 |
| 6 | Console | 无 LoadScene 失败 / 门相关 NRE |

---

## ④ 给程序看的补充

### 4.1 双门现网对拍表（2026-08-22 YAML）

| 检查项 | LeftDoor | RightDoor |
|--------|----------|-----------|
| GameObject Active | **1** ✅ | **1** ✅ |
| SceneChangeDoor Enabled | **1** ✅（0821 施工后） | **0** ❌ **主断点** |
| NextSceneName | `Village_KenMuNi1` ✅ | `Village_KenMuNi1` ✅ |
| TriggerWhenMoveIn | **1** ✅ | **1** ✅ |
| BoxCollider2D IsTrigger | **1** ✅ | **1** ✅ |
| Collider Size / Offset | `2.52 × 20`；Offset `(-1.24, 0)` | `1.71 × 20`；Offset `(-1.70, 0)` |
| Layer | **8**（MapDoor） | **8** |
| componentsList 有 Interactive | **有** `772001100220033003` ✅ | **有** `1105019192` ✅ |
| InteractiveColliderListener | **有** `772001100220033004` ✅ | **有** `3588313294745101351` ✅ |
| BaseEntityControll | **有** `772001100220033005` ✅ | **有** `3588313294745101353` ✅ |
| 走进会 LoadScene？ | **会**（现网） | **不会**（组件 Disable） |

**Map 组件引用**（`Map` Mono `1661192197712708882`）：`leftDoor` → LeftDoor `SceneChangeDoor`；`rightDoor` → RightDoor `SceneChangeDoor`；`rightBornTsf` → `RightBorn`。**引用正确，无需改 Map 脚本字段。**

### 4.2 主出口裁定与离「能走 RightDoor」差几步

| 步骤 | 状态 | 说明 |
|------|------|------|
| 1. RightDoor `SceneChangeDoor` 启用 | ❌ 未做 | **唯一硬断点** |
| 2. Next / Trigger 正确 | ✅ 已齐 | 无需改 |
| 3. Interactive 链齐全 | ✅ 已齐 | 无需补件 |
| 4. 禁用 LeftDoor 防双出口 | ❌ 未做 | LeftDoor 现网**可走** |
| 5. EnterPos 双侧配对 | ✅ 已齐 | 见 §4.3 |
| 6. Trigger 盖住通道（布局改后） | ⚠️ 待 Play 验 | YAML 见 §4.5 |

**左门关闭建议（对齐 HomeScene23）**：**Disable `SceneChangeDoor` + `NextSceneName` 清空 + `TriggerWhenMoveIn=0`**；GO 可保持 Active、Interactive 可保留（左门不再触发 LoadScene）。**不推荐**只清空 Next 而保持 Enabled（易误导后续维护）。

### 4.3 EnterPos 双侧配对

| 场景 | lastScene | pos Transform | 现网 |
|------|-----------|---------------|------|
| `Village_HomeScene45` | `Village_KenMuNi1` | **`RightBorn`** `3588313296241062192`（`(-5.06, -3.65, 0)`） | ✅ 有 |
| `Village_KenMuNi1` | `Village_HomeScene45` | **`LeftBorn`** `5601461775652594521`（村图 `62.4, -6.1`）≈ House_Npc45 门外 | ✅ 有 |
| `Village_HomeScene45` | `ForestScene`（残留） | 同绑 `RightBorn` | ⚠️ 可选删（不影响本期右门回村） |

**进屋链**（已通）：

```
House_Npc45.NextSceneName = Village_HomeScene45
  → LoadScene(45)
  → LastSceneName = Village_KenMuNi1
  → EnterPos 匹配 → 落 RightBorn
```

**出屋链**（RightDoor 启用后）：

```
走进 RightDoor Trigger
  → SceneChangeDoor.EnterDoor
  → LoadScene(Village_KenMuNi1)
  → LastSceneName = Village_HomeScene45
  → 村 EnterPos 匹配 → 落 House_Npc45 门外（LeftBorn 区）
```

### 4.4 场景管理器 / 三位一体

| 检查项 | 现网 | 施工 |
|--------|------|------|
| `Village_HomeScene45SceneManager.nowSceneName` | `SceneName.Village_HomeScene45` ✅ | **不改** |
| Build Settings | 含 `Village_HomeScene45.unity` ✅ | 不改 |
| `Village_HomeScene45.asset` | `isFightingScene=0`，可移动/存档 ✅ | 不改 |
| 村 `House_Npc45.NextSceneName` | `Village_HomeScene45` ✅ | 不改 |
| `SceneName` 常量 | `Village_HomeScene45` / `Village_KenMuNi1` ✅ | 不改 |

### 4.5 布局更新后的门位与碰撞（YAML 推算，须 Play 证伪）

| 节点 | Map 下 localPosition | 备注 |
|------|---------------------|------|
| `MapLeft` | `(-28.77, 0, 0)` | 左墙侧 |
| `LeftDoor` | `(0, 0, 0)`（MapLeft 子） | 世界 X ≈ **-28.77**；Trigger 宽约 2.5 |
| `MapRight` | `(18.36, 0, 0)` | 右墙侧 |
| `RightDoor` | `(-18.16, 0, 0)`（MapRight 子） | 世界 X ≈ **0.2**；Trigger 宽约 1.7 |
| `RightBorn` | `(-5.06, -3.65, 0)` | 从村进屋落点 |
| `LeftBorn` | `(-24.12, -3.65, 0)` | 未绑 EnterPos（本期不用） |

**碰撞验收点**：RightDoor Trigger 高 `y=20`，垂直覆盖通常足够；**水平**是否盖住玩家从屋内走向右侧的出口，取决于开发者挪门后的实际通道——**静态 YAML 无法替代 Scene 视图 Gizmos + Play 踩门**。若走进不触发，**只调** `RightDoor` 的 `BoxCollider2D` Offset/Size，勿改 `Map` 逻辑或换场代码。

### 4.6 推荐施工方案（最小改动）

1. 打开 `Village_HomeScene45.unity`。  
2. **`Map/MapRight/RightDoor`**：勾选 **`SceneChangeDoor`**（`m_Enabled: 1`）；确认 `NextSceneName=Village_KenMuNi1`、`TriggerWhenMoveIn=1`。  
3. **`Map/MapLeft/LeftDoor`**（对齐 HomeScene23）：**取消勾选 `SceneChangeDoor`**；`NextSceneName` 清空；`TriggerWhenMoveIn=0`。  
4. **EnterPos**：保持 `Village_KenMuNi1` → `RightBorn`；村侧 `Village_HomeScene45` → `LeftBorn`。**可选**删室内 `ForestScene` 残留行。  
5. **不改** `Village_HomeScene45SceneManager.cs`、龙宫 / HomeScene23、Item 预制体。  
6. Play：进屋 → 走 **RightDoor** 出村 → 验落点 → 试 **LeftDoor** 无换场。

### 4.7 与 HomeScene23 对拍（左关右开）

| 门 | HomeScene23 | HomeScene45 现网 | HomeScene45 施工后 |
|----|-------------|------------------|-------------------|
| LeftDoor SceneChangeDoor | **Disable**；Next 空 | **Enable**；Next 村 | **Disable**；Next 空 |
| RightDoor SceneChangeDoor | **Enable** → 村 | **Disable** | **Enable** → 村 |

### 4.8 与 0821 diff（证伪旧结论）

| 项 | 0821 侦探 | 0821 施工决议 | **现网 2026-08-22** | 本期施工 |
|----|-----------|---------------|---------------------|----------|
| LeftDoor Interactive | ❌ 空 | 补齐 | ✅ **已齐** | 保留件，**关换场** |
| LeftDoor 主出口 | — | **是** | **仍 Enable**（与新产品冲突） | **否** |
| RightDoor SceneChangeDoor | Disable | 保持 Disable | **仍 Disable** | **Enable** |
| 产品主出口 | 未拍板 | LeftDoor | **RightDoor**（2026-08-22 拍板） | RightDoor |

> **0821 OPEN Q1（LeftDoor 主出口）已被本产品决议取代**，见 `OPEN_QUESTIONS.md` §「RightDoor 回村 · 2026-08-22」。

### 4.9 最小改动文件列表

| 文件 | 动作 |
|------|------|
| `Assets/GameRes/Scenes/Village_HomeScene45.unity` | RightDoor 启用 SceneChangeDoor；LeftDoor 按 23 禁用；可选删 Forest EnterPos |
| `Village_HomeScene45SceneManager.cs` | **不改** |
| `Village_KenMuNi1.unity` | **不改**（EnterPos 已齐） |
| `Village_HomeScene23.unity` | **禁止改** |

### 4.10 严禁

- RightDoor 指 `ForestScene`  
- 只改 `NextSceneName` 不启用 RightDoor `SceneChangeDoor`  
- 双门同时 Enable 且 Next 均为村（双出口）  
- 用 `bornPos` 代替 EnterPos  
- 未验碰撞就宣称「已能出门」

### 4.11 开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 RightDoor 回村 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探：LeftDoor 0821 已修通；产品改 RightDoor 主出口；差启用右门 + 关左门；EnterPos 已齐 |
