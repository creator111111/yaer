# Village · 村庄遮罩层盖顶盖住玩家 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动（Editor 幂等挂载）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_村庄遮罩层盖顶盖住玩家_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**五个白天场景挂上村庄遮罩后，走进窗光 / 树荫，角色会被压暗盖住。**

### ② 原因（通俗）

遮罩画本来在人后面，盖不住。现在把它和合层摆成兄弟，关掉会双影的「背景」，再抬到 Effect 层，就能盖在玩家上面。村长家只有 PSD、没有 Prefab，本期不做。

### ③ 用户检查清单

1. 切回 Unity 刷新场景（YAML 已直写；若丢实例可再跑菜单 `Tools/Scene/Setup Village 村庄遮罩盖顶`）。
2. Hierarchy：`Village_HomeScene1/2/45/23`、`Village_KenMuNi1` 各有 `村庄遮罩_*`，与合层**并列**（同父 `Design`），不在合层子级。
3. 遮罩根 LocalPos 与对应合层一致；子物体 `背景` **未勾选**。
4. 其余遮罩子层 Sorting Layer = **Effect**，Order 约 11/12/13。
5. Play：走进窗光 / 树荫，人被压暗；退出恢复。无双影；无 SpriteMask。
6. 村长家、夜景场景无遮罩。

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `Assets/Editor/Tool/Scene/VillageOverlayMaskSetupEditor.cs` | 新增幂等菜单（可重跑纠偏） |
| `Village_HomeScene1/2/45/23.unity`、`Village_KenMuNi1.unity` | 已挂 7 份遮罩 PrefabInstance（关背景 + Effect/11–13） |

| 场景 | 合层 | 遮罩 Prefab | 实例名 | 根坐标（与合层同） |
|------|------|-------------|--------|-------------------|
| HomeScene1 | 村民家1合层 | 村民家 | 村庄遮罩_村民家1 | (-28.81, -5.38, 0) |
| HomeScene2 | 村民家2合层 | 村民家2 | 村庄遮罩_村民家2 | (-28.84, -5.39, 0) |
| HomeScene45 | 村民家3合层 | 村民家3 | 村庄遮罩_村民家3 | (-28.98, -5.39, 0) |
| HomeScene23 | 村民家4合层 | 村民家4 | 村庄遮罩_村民家4 | (-28.84, -5.39, 0) |
| KenMuNi1 | 肯姆尼1/2/3合层 | 肯姆尼1/2/3 | 村庄遮罩_肯姆尼1/2/3 | (-13.92/-93.22/-172.42, -7.8, 0) |

每份实例：关 `背景`；其余 SR → Effect，Order 原值 +10；子 Z=0；MaskInteraction=None。

**未改**：合层源 Prefab、遮罩 ArtRes 源、Player Sorting、夜景、村长家。

---

## 为什么这样改

Default/0～3 在 Player 层后面，盖不住人。Effect 整层在 Player / SceneObject 之上，不必追玩家动态 Order。关背景避免与合层双影。作兄弟实例而不嵌合层源，避免污染共用 Prefab。

菜单幂等：可重复跑；删旧同名实例再摆。
