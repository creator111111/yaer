# Village_KenMuNi1 — `树根外` 对齐 `精灵池路灯` 遮挡换层 — 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 场景：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> 金样：`肯姆尼2合层` / **`精灵池路灯`**（已挂 DepthSort，Order+3 后为 9/3）  
> 目标：`肯姆尼3合层` / **`树根外`**  
> 真源脚本：`VillageSceneObjectDepthSort`（guid `ba2f9bf2…`）  
> 提示词：`Assets/Doc/提示词/0922/Village_KenMuNi1_树根外对齐路灯遮挡_架构侦探提示词.md`  
> 对照：`执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md`

---

## ① 结论一句话

`树根外` 在 `肯姆尼3合层` 下仅有 Transform + SpriteRenderer，**无** DepthSort，SR 钉在 **SceneObject / Order 30**（死海报）。金样路灯已挂 DepthSort 且磁盘为 **Default=9 / SceneObject=3**。最小修法：**方案 A** — 只在**场景实例**根上 Add `VillageSceneObjectDepthSort`，字段**整表抄路灯**；**不改** C#、**不改** Prefab 源、**不做** `树根内`。若 Play 与 3 合层静图穿帮，再按调参序微调 Order（仍先抄 9/3）。

---

## ② 路灯 vs 树根外对照表

| 项 | `精灵池路灯`（金样） | `树根外`（目标） | `树根内`（近邻·本期不做） | `青石围栏`（易混·勿抄） |
|----|---------------------|------------------|---------------------------|-------------------------|
| Hierarchy | `…/肯姆尼2合层/精灵池路灯` | **`…/肯姆尼3合层/树根外`** | 同合层 `/树根内` | `…/肯姆尼1合层/青石围栏` |
| GO | `1222846987` | `7070296266979326876` | `1577135741050305205` | `7405960369267094666` |
| Prefab | 不在 2 合层源 | **`肯姆尼3合层.prefab` 有同名** | Prefab 有 | — |
| 场景组件 | Transform + SR + **DepthSort** | Transform + SR **仅此** | Transform + SR | Transform + SR + DepthSort + 碰撞 |
| DepthSort | ✅ `8828300002` | ❌ | ❌ | ✅ 仍 **6/0**（旧表） |
| SR 初始层 | SceneObject · Order **3** | SceneObject · Order **30** | SceneObject · Order **29** | Default · 6 |
| 本地坐标 | `(16.994, 3.945)` | `(26.045, 7.585, 2.31)` | `(15.17, 4.665)` | — |
| 精灵约尺寸 | 2.48×8.64 | **11.53×8.21** | 11.64×9.33 | — |
| 子物体 | 无 | **无** | 无 | — |

**金样 DepthSort 磁盘全字段（路灯，以现网为准）**

| 字段 | 值 |
|------|-----|
| `targetSpriteRenderers` | 本 SR `1222846989` |
| `anchorOverride` | 本 Transform `1222846988` |
| `invertPlayerVersusAnchorComparison` | **0** |
| `sortingOrderWhenDefaultLayer` | **9** |
| `sortingOrderWhenSceneObjectLayer` | **3** |
| `updateEveryNthFrame` | 1 |
| `debugLogOnLayerChange` | 0 |
| `preferTownLocomotionAuthoritativeY` | **1** |

生活类比：路灯会按人前后换谁盖住谁；树根外还是钉在 SceneObject 的死海报。

**合层差异提示**：树根在肯姆尼**3**，静图 Order 约 29～30；抄路灯 SceneObject**=3** 后，挡人时可能落到 3 合层其它 SceneObject 图**下方**。产品要求仍**先抄路灯**；穿帮再调 Order，**不要**改抄围栏 6/0，**不要**新脚本。

---

## ③ 推荐 Inspector 初值（整表抄路灯）

| 字段 | 初值 | 备注 |
|------|------|------|
| 挂点 | **`树根外` 根** | GO `7070296266979326876` |
| `targetSpriteRenderers` | 本 SR `8775965915123942743` | 仅 1 片 |
| `anchorOverride` | 本 Transform `4137765881751695108` | 第一版自锚 |
| `invert` | **false** | 前后反了再勾 |
| Default / SceneObject Order | **9 / 3** | 抄路灯 |
| `preferTownLocomotionAuthoritativeY` | **true** | — |
| `updateEveryNthFrame` | 1 | — |
| `debugLogOnLayerChange` | 验收可临时 true | 交前关 |

---

## ④ 调参顺序

1. **invert**（前后整段反了）  
2. **锚点**（根冠 pivot 偏高 → 空物体贴根脚）  
3. **Order**（与 3 合层静图穿帮：优先抬 `sortingOrderWhenSceneObjectLayer` 向现网 30 靠拢，再视情况调 Default=9）  

否决：新脚本；只改死 Order；DepthZone；Fade；DepthComponent 双开；顺手改 `树根内`；误抄围栏 6/0。

---

## ⑤ 要改文件（路径级）

| 全路径 | 做什么 |
|--------|--------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | **仅** `树根外` Add DepthSort，字段抄上表 |
| `Assets/ArtRes/Scene/Village/Prefab/肯姆尼3合层.prefab` | **不改**（与路灯/池中「只改场景」一致） |
| `VillageSceneObjectDepthSort.cs` | **不改** |
| 路灯 / 池中 / 围栏 / `树根内` | **不改** |

施工说明建议：`Assets/Doc/施工说明/0922/Village_KenMuNi1_树根外对齐路灯遮挡_施工说明.md`

---

## ⑥ 验收表

| # | 操作 | 期望 |
|---|------|------|
| 1 | 站在树根外「后方」（相对锚点，玩家 Y 更大/更深） | 根 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 根 |
| 3 | A/D、W/S、斜向绕根 | 换层无狂闪 |
| 4 | 运行时看树根外 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 再绕 `精灵池路灯` / `精灵池中` | 金样仍正常 |
| 6 | Console | 无 DepthComponent 双开 Warning |
| 7 | （可选）勾 `debugLogOnLayerChange` | `[VillageOcclusion] obj=树根外 …` |

---

## ⑦ 开放问题

| ID | 问题 | 施工默认 |
|----|------|----------|
| Q1 | `树根内` 是否本期？ | **否**（用户只点名树根外） |
| Q2 | 抄 9/3 与 3 合层 Order≈30 静图穿帮？ | **仍先抄路灯**；验收再调 Order |
| Q3 | 是否写回 `肯姆尼3合层.prefab`？ | **否** |
| Q4 | 第一版就加脚底锚点？ | **否**（先自 Transform） |
