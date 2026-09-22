# Village_KenMuNi1 — 肯姆尼2合层 Order in Layer +3 — 施工说明

> 日期：2026-09-22  
> 角色：施工员（已改序列化数值；未改 C# / Sorting Layer 名 / Collider）  
> 提示词：`Assets/Doc/提示词/0922/Village_KenMuNi1_肯姆尼2合层_Order加3_施工提示词.md`  
> 场景：`Village_KenMuNi1` → `Map/Design/Map/肯姆尼2合层`

---

## ① 大白话

肯姆尼2合层里每张图的 **Order in Layer 都在原值上抬了 3**（相对前后不变，整叠上移）；挂了 DepthSort 的池中/路灯，换层用的两个 Order 字段也各 +3，避免 Play 一绕又跳回旧表。

---

## ② 改了哪里

| 文件 | 动作 |
|------|------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | 合层子树 **15** 个 SpriteRenderer `m_SortingOrder += 3`；`精灵池中` / `精灵池路灯` 的 DepthSort 两字段各 +3 |
| `Assets/ArtRes/Scene/Village/Prefab/肯姆尼2合层.prefab` | Prefab 内 **14** 个 SpriteRenderer 同式 +3（与场景解耦源对齐；Prefab **无** DepthSort、**无** `精灵池路灯`） |

**未改**：肯姆尼1/3合层、青石围栏 DepthSort（仍 6/0）、村庄遮罩、Sorting Layer 名、Missing Sprite、Walk/Collider、`VillageSceneObjectDepthSort.cs`。

**场景特有**：`精灵池路灯`（有 DepthSort）已 +3；Hierarchy 截图里的 `衣` **现网合层子树内无此节点**（无 SR 可改，记入说明）。

---

## ③ 前后对照表

### 场景实例（Play 真源；合层已 Unpack）

| 物体 | Sorting Layer（未改） | Order 前 → 后 | DepthSort 前 → 后 |
|------|----------------------|---------------|-------------------|
| 背景 | Default | 0 → **3** | — |
| 地板 | Default | 1 → **4** | — |
| 灌木3 | Default | 2 → **5** | — |
| 中景树干 | Default | 3 → **6** | — |
| 近灌木1 | Default | 4 → **7** | — |
| 灌木2 | Default | 5 → **8** | — |
| 田 | Default | 6 → **9** | — |
| 商店 | Default | 7 → **10** | — |
| 商店门 | Default | 8 → **11** | — |
| 商店牌坊 | Default | 9 → **12** | — |
| 井 | Default | 11 → **14** | — |
| 农 | Default | 13 → **16** | — |
| 精灵池中 | SceneObject | 0 → **3** | **6/0 → 9/3** |
| 精灵池上 | SceneObject | 0 → **3** | —（无 DepthSort） |
| 精灵池路灯 | SceneObject | 0 → **3** | **6/0 → 9/3** |

说明：场景里池中/池上/路灯 SR 已在 SceneObject 且初始 Order=0（与 Prefab 里 Default+高 Order 不同）；本票仍按「当前值 +3」执行，相对抬升一致。

### Prefab 源

| 物体 | Order 前 → 后 |
|------|---------------|
| 背景 | 0 → **3** |
| 地板 | 1 → **4** |
| 灌木3 | 2 → **5** |
| 中景树干 | 3 → **6** |
| 近灌木1 | 4 → **7** |
| 灌木2 | 5 → **8** |
| 田 | 6 → **9** |
| 商店 | 7 → **10** |
| 商店门 | 8 → **11** |
| 商店牌坊 | 9 → **12** |
| 精灵池中 | 10 → **13** |
| 井 | 11 → **14** |
| 精灵池上 | 12 → **15** |
| 农 | 13 → **16** |

---

## ④ 验收

| # | 检查 | 期望 |
|---|------|------|
| 1 | Hierarchy 选 `背景` | Order = **3** |
| 2 | Prefab / 场景表内其它物体 | 均为原值+3 |
| 3 | `精灵池路灯` | Order=3；DepthSort Default/SceneObject = **9 / 3** |
| 4 | 肯姆尼1（青石围栏）/ 肯姆尼3 / 村庄遮罩 | Order **未变**（围栏 DepthSort 仍 **6/0**） |
| 5 | Play 绕精灵池前后 | 换层落在新表（Default≈9） |
| 6 | 合层内相对前后 | 仍合理（整层平移） |

---

## ⑤ 备注

- 合层在场景中为 **解耦实例**（`m_PrefabInstance: {fileID: 0}`），故 **场景 + Prefab 都改**，避免以后再拖 Prefab 时源仍是旧 Order。  
- `衣`：本票核对子树未找到，若编辑器另有未保存节点，Save 后再补 +3。
