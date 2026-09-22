# Cursor Agent Prompt · Village_KenMuNi1：`肯姆尼2合层` 全体 Order in Layer +3

> **角色**：先短核对，再【施工员】批量改；机械票，不必长篇溯源  
> **日期**：2026-09-22  
> **场景**：`Village_KenMuNi1`  
> **范围**：Hierarchy `Map / Design / Map / **肯姆尼2合层**` 下**所有**带 `SpriteRenderer` 的物体（含子孙；截图里含 `背景`、`地板`、…、`精灵池路灯`、`精灵池上`、`衣` 等）  
> **动作**：每个 SpriteRenderer 的 **Order in Layer = 当前值 + 3**（例：`背景` 现为 `0` → `3`）  
> **不是**：改 Sorting Layer 名；改肯姆尼1/3合层；改村庄遮罩 Order；修 `背景` Missing Sprite；改 Walk/Collider；重写 DepthSort 逻辑  

把下面整段交给 Cursor Agent。

---

## 产品钉死

| 项 | 要求 |
|----|------|
| 对象 | **仅** `肯姆尼2合层` 子树内的 SpriteRenderer |
| 公式 | `m_SortingOrder += 3`（每个 SR 各自在原值上加，不是统一改成 3） |
| Sorting Layer | **不动**（保持 Default / SceneObject 等原层名） |
| 无 SR 的子物体 | 跳过（如纯 Collider） |
| Missing Sprite | 仍改 Order；**不要**顺手修缺图 |

生活类比：合层里每张贴纸原来叠放序号差 1；整叠一起抬高 3 格，相对前后关系不变，只整体挪一层。

---

## 助手预扫（施工须以磁盘/Hierarchy 为准复核）

`Assets/ArtRes/Scene/Village/Prefab/肯姆尼2合层.prefab` 内部分 Order（+3 后期望）：

| 物体（Prefab 内名） | 现 Order | +3 后 |
|---------------------|----------|-------|
| 背景 | 0 | **3** |
| 地板 | 1 | 4 |
| 灌木3 | 2 | 5 |
| 中景树干 | 3 | 6 |
| 近灌木1 | 4 | 7 |
| 灌木2 | 5 | 8 |
| 田 | 6 | 9 |
| 商店 | 7 | 10 |
| 商店门 | 8 | 11 |
| 商店牌坊 | 9 | 12 |
| 精灵池中 | 10 | 13 |
| 井 | 11 | 14 |
| 精灵池上 | 12 | 15 |
| 农 | 13 | 16 |

截图另有 **`精灵池路灯` / `衣`** 等：可能只在**场景实例**或未进 Prefab——**必须扫场景子树**，不能只改 Prefab 漏实例。

### DepthSort 连带（易漏）

合层内若物体挂了 `VillageSceneObjectDepthSort`（金样：`精灵池中`；若已挂则含 `精灵池路灯`），运行时会**覆写** `sortingOrder`：

| 字段 | 池中现网预扫 | 建议 |
|------|--------------|------|
| `sortingOrderWhenDefaultLayer` | 6 | **同步 +3 → 9**（与整层抬高一致） |
| `sortingOrderWhenSceneObjectLayer` | 0 | **同步 +3 → 3**（相对关系仍差 6；若产品只要「静图 +3、动态仍用旧表」须在报告写明——**默认两边都 +3**） |

只改 SR 初始 Order、不改 DepthSort 字段 → Play 下一绕池子会跳回旧 Order，等于白加。

---

## Agent 任务（复制）

```
你先做 5 分钟核对，再施工。不要改代码逻辑，只改序列化数值。

### 核对

1. 钉死根节点：`Village_KenMuNi1` → `Map/Design/Map/肯姆尼2合层`（勿动肯姆尼1/3、勿动 `村庄遮罩_肯姆尼2`）。
2. 列出该子树下**全部** SpriteRenderer：路径 | 当前 SortingLayer | 当前 Order | 是否挂 DepthSort。
3. 分清改哪里：
   - Prefab 源：`Assets/ArtRes/Scene/Village/Prefab/肯姆尼2合层.prefab`
   - 场景覆盖 / 仅场景存在的子物体：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`
   - 若是 PrefabInstance：改源 + 清掉会挡住的 Order 覆盖，或按 Unity 规范改实例 —— **结果以 Play/Hierarchy 显示 +3 为准**，禁止只改一边导致进 Play 又变回去。
4. DepthSort：凡挂在肯姆尼2子树内的，`sortingOrderWhenDefaultLayer` 与 `sortingOrderWhenSceneObjectLayer` **默认各 +3**（与上表一致）。

### 施工

对核对表中每一行：
- `SpriteRenderer.sortingOrder += 3`
- 若有 DepthSort：两个 order 字段各 `+= 3`
- Sorting Layer 名、Mask、Transform、Sprite 引用：**不动**
- Missing Sprite：**不修**

### 验收

| # | 检查 | 通过 |
|---|------|------|
| 1 | `背景` Order | 原 0 → **3**（截图对照） |
| 2 | 表内其它 Prefab 子物体 | 均为原值+3 |
| 3 | `精灵池路灯` / `衣` 等场景特有 | 有 SR 的也都 +3 |
| 4 | 肯姆尼1合层 / 肯姆尼3合层 / 遮罩 | Order **未变** |
| 5 | 挂 DepthSort 的物体 | 两字段已 +3；Play 绕前后换层，Order 落在新表（如 Default≈9） |
| 6 | 合层内相对前后 | 仍合理（整层平移，相对差不变） |

### 交付

1. 写入施工说明：`Assets/Doc/施工说明/0922/Village_KenMuNi1_肯姆尼2合层_Order加3_施工说明.md`  
   含：改了 Prefab 还是场景、完整前后对照表、DepthSort 是否已跟。  
2. 大白话一句：肯姆尼2合层所有图层序号整体抬了 3。  
3. 若某物体无法 +3（只读/丢引用），记入说明，不要静默跳过整层。

### 禁止

- 改 Sorting Layer 名称或把全体改到同一 Order
- 波及肯姆尼1/3、夜景合层、村庄遮罩
- 为「对齐」去改玩家 Sorting 或 VillageSceneObjectDepthSort.cs
- 删除/合并子物体；修 Missing Sprite
```

---

## 使用顺序

1. 确认场景已 Save（含 `精灵池路灯` 等新物体）  
2. 把「Agent 任务」整段丢给 Agent  
3. Play：看 `背景` Order=3；再绕精灵池确认 DepthSort 换层用的是新 Order
