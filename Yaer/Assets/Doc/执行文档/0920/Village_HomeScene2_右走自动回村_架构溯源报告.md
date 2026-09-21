# Village_HomeScene2 右走自动回村 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 对照：村长家 `RightDoor` 走进黑幕回村（0920 施工说明）  
> 场景：`Village_HomeScene2` ↔ 村里 `House_NPC2`

---

## ① 结论一句话

**把右门挪到房间右缘再打开走进触发，空着 EnterPosKey，就会黑幕回到这家门外。** 门现在关着、组件也关着，而且触发盒在世界 X≈27，人在室内（大约 X -24～-2）走不到。落点不在盒内，挪过去也不会一进门就弹出去。左门保持关闭。

---

## ② 原因

大白话：这家回村门在场景最右边的地图锚点上，屋子本身在左边。人走到屋内最右，碰不到那扇关着的门。村长家是门就在屋子右缘，走进去就黑幕回村。

### 两扇门（Map 在原点）

| | LeftDoor | RightDoor |
|--|----------|-----------|
| 物体 | **关** | **关** |
| 换场组件 | 开着，但物体关了等于没用 | **组件本身关着**（`m_Enabled: 0`） |
| NextScene | `Village_KenMuNi1` | **空** |
| TriggerWhenMoveIn | 0（要按交互） | 0 |
| ShowLoadingUI | 0 | 0 |
| EnterPosKey | 空 | 空 |
| 父节点 | MapLeft **(-28.77, 0)**，门本地 (0,0) | MapRight **(28.8, 0)**，门本地 (0,0) |
| 碰撞 | 触发盒，offset x≈-1.24，宽≈2.52，高 20 → 世界 X 约 **-31.3～-28.8** | offset x=-1，宽 2，高 20 → 世界 X **26.8～28.8**，Y **-10～10** |

`Map.OnInit` 会按名字找到左右门并 `OnInit`。`SceneChangeDoor.OnInit` 在组件禁用时直接 return，所以只开物体、不勾组件，走进去仍然不换场。

### 村里怎么进、落到哪

| 项 | 现网 |
|----|------|
| 村门 | `House_NPC2`，`NextSceneName=Village_HomeScene2`，世界 **(-124.73, 8.5)** |
| 出屋落点 | KenMuNi1 `EnterPos`：`lastScene: Village_HomeScene2` → `ExitFrom_HomeScene2` **(-124.73, 3.85)** |
| 是否这家门外 | **是**。和 `House_NPC2` 同一个 X，Y 低约 4.6（门口街上），不是别家 |

空 `EnterPosKey` 时，LastScene 就是卸下来的场景名 `Village_HomeScene2`，已经对上这一行。**不必新键。** 新键却不加 EnterPos 行，反而对不上，会落到别处。

### 进屋落点 vs 右门盒

| 点 | 世界坐标 | 是否在现网右门盒内（X 26.8～28.8） |
|----|----------|--------------------------------------|
| 从村进门 `EnterFrom_Village` | **(-24.12, -3.65)** | **否**（差约 51） |
| `DefaultBornPos` | 同 **(-24.12, -3.65)** | **否** |
| `RightBorn` | (24.68, -3.65) | **否**（在盒左侧约 2） |
| 原点 (0,0) | — | **否**（不像 HomeScene45 那种横跨原点） |

室内可走右缘大约在 CameraArea / `RightWall`：墙挂在 MapRight 下、本地 **(-31.07, 0)** → 世界 X≈**-2.27**。盒在 X≈27，人走到屋内最右也进不了盒。

把门本地 X 收到和墙对齐（约 **-31.1**）后，盒大约在世界 X **-4.3～-2.3**。进门点 X=-24.12 仍在盒外约 20，不会一进屋就触发。

---

## ③ 用户验收

1. 从村里 `House_NPC2` 进屋：人停在室内左侧一带（约 -24, -3.65），**不要**黑幕立刻弹回村。  
2. 往右走到屋子右缘：黑幕，不读条，回到 `House_NPC2` 门外（约 -124.7, 3.85）。  
3. 左边门仍关着，不能再开成第二扇走进门。  
4. 其它民居、村长家右门行为不变。

---

## ④ 给施工员的字段表

只改 `Assets/GameRes/Scenes/Village_HomeScene2.unity` 的 `RightDoor`。村里落点键不用动。

| 字段 | 现网 | 改为 |
|------|------|------|
| 物体 `m_IsActive` | 0 | **1** |
| `SceneChangeDoor.m_Enabled` | **0** | **1** |
| `NextSceneName` | 空 | **Village_KenMuNi1** |
| `TriggerWhenMoveIn` | 0 | **1** |
| `ShowLoadingUI` | 0 | **0**（保持黑幕） |
| `EnterPosKey` | 空 | **保持空** |
| 本地 `m_LocalPosition.x` | 0（世界 X≈28.8） | **约 -31.1**（对齐同父 `RightWall`，世界盒约 X -4.3～-2.3） |

`LeftDoor`：物体保持 **Inactive**，不要改成走进触发。

### 不要改

| 不要改 | 原因 |
|--------|------|
| KenMuNi1 `ExitFrom_HomeScene2` / `House_NPC2` | 已经是这家门外 |
| 新 EnterPosKey 却不配行 | 会对不上 `Village_HomeScene2` |
| 其它民居、村长家 `RightDoor` | 超出这家 |
| 只开走进、不挪 X | 人走不到 X≈27 的盒子 |

### 否决

- 只启用现位置的右门：触发在屋外，右走不会回村。  
- 左门也改成走进：变成两扇出口，和「像村长家走最右」不一致。
