# Village_KenMuNi1 — `精灵池路灯` 对齐 `精灵池中` 遮挡换层 — 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 场景：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> 金样：`肯姆尼2合层` / **`精灵池中`**（0830 已挂 DepthSort）  
> 目标：同合层 / **`精灵池路灯`**  
> 真源脚本：`VillageSceneObjectDepthSort`（guid `ba2f9bf2…`）  
> 提示词：`Assets/Doc/提示词/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构侦探提示词.md`  
> 对照：`执行文档/8月/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md`

---

## ① 结论一句话

磁盘上 **`精灵池路灯` 已存在**（提示词预扫「找不到」已过时）：路径 `Map/Design/…/肯姆尼2合层/精灵池路灯`，仅 Transform + SpriteRenderer，**无** DepthSort，SR 钉死 **SceneObject / Order 0**（与池中施工前同病）。最小修法：**方案 A** — 只在场景实例根上 Add `VillageSceneObjectDepthSort`，字段**整表抄 `精灵池中`**（6/0、invert=0、本 Transform 锚点、preferTownY=true、target=本 SR）；**不改** C#、**不改** Prefab 源、**不挂**到 `路灯蘑菇`、**不动** `精灵池上`。

---

## ② 金样 vs 路灯对照表

### A. 物体钉死（本票最易翻车处 — 已复核）

| 项 | `精灵池中`（金样） | `精灵池路灯`（目标） | `路灯蘑菇`（近义·禁挂） |
|----|-------------------|---------------------|------------------------|
| 磁盘是否存在 | ✅ | ✅（GO `1222846987`） | ✅（另物体） |
| Hierarchy | `…/肯姆尼2合层/精灵池中` | **`…/肯姆尼2合层/精灵池路灯`**（父子同合层；RootOrder 12，池中为 10） | 在 **另一合层**（非本票目标） |
| Prefab `肯姆尼2合层.prefab` | 有 | **无**（场景后加实例） | — |
| 根组件 | Transform + SR + **DepthSort** | Transform + SR **仅此** | Transform + SR（无 DepthSort） |
| 子物体 | 有 `Collider (1)`（Walk 物理） | **无子** | — |
| DepthSort | ✅ 已挂 | ❌ | ❌ |
| DepthComponent | 无 | 无 | — |
| SR 初始层 | SceneObject（id `694918277`）· Order **0** | **同** SceneObject · Order **0** | Default 系（非本票） |
| 本地坐标 | `(30.525, 7.76, 2.86)` | `(16.994, 3.945, 0)` | — |
| 精灵尺寸（约） | 宽≈28.6 × 高≈9.4 | 宽≈2.48 × 高≈**8.64**（细高灯柱） | — |

**结论**：Hierarchy 与截图一致；**禁止**把 DepthSort 挂到 `路灯蘑菇`。路灯只在场景 YAML，不在 Prefab 源 → **只改场景实例**（与 0830 池中同口径）。

### B. 金样 `VillageSceneObjectDepthSort`（磁盘现网，非仅 0830 文档）

挂在 `精灵池中`（fileID `8828300001`）：

| 字段 | 现网值 |
|------|--------|
| `targetSpriteRenderers` | 本 SR `8162121874168452420` |
| `anchorOverride` | 本 Transform `244632720003562929` |
| `playerLogicOverride` | null |
| `invertPlayerVersusAnchorComparison` | **0** |
| `sortingOrderWhenDefaultLayer` | **6** |
| `sortingOrderWhenSceneObjectLayer` | **0** |
| `updateEveryNthFrame` | 1 |
| `debugLogOnLayerChange` | 0 |
| `preferTownLocomotionAuthoritativeY` | **1** |

契约：`Village2_5D` 下 `playerY > anchorY` → SceneObject（挡玩家）；否则 Default；仅管 Layer/Order，不改物理。

### C. 路灯现状（为何不对）

生活类比：池中已装「前后换层门禁」；路灯还是钉在 SceneObject 的死海报——绕灯走永远压在人上（或层关系死板），不会跟池中一样换。

---

## ③ 推荐 Inspector 初值（抄池中）

| 字段 | 初值 | 备注 |
|------|------|------|
| 挂点 | **`精灵池路灯` 根** | GO `1222846987` |
| `targetSpriteRenderers` | 本 SR `1222846989` | **仅 1 片**；无灯罩/光晕子 SR |
| `anchorOverride` | **本 Transform** `1222846988` | 第一版对齐池中自锚 |
| `invert` | **false** | 前后整段反了再勾 |
| Default / SceneObject Order | **6 / 0** | 抄池中；不对再微调 |
| `preferTownLocomotionAuthoritativeY` | **true** | 与池中/围栏一致 |
| `updateEveryNthFrame` | 1 | — |
| `debugLogOnLayerChange` | 验收时可临时 true | 交前关 |

**多 SR**：现网无子 SR → 根上一份 + target 只列本片即可。若以后加光晕且要求「永远在人前」，该片**不要**进 target。

**锚点备选（调参，非第一版必做）**：灯柱高≈8.6，pivot 若在中部，切换线可能偏高。可新建空子物体贴灯座脚底，或将 `anchorOverride` 拖到脚底空节点——**先实机再做**，勿第一版改 C#。

---

## ④ 调参顺序

1. **invert**（前后整段反了）  
2. **锚点**（切换偏：空物体贴灯脚）  
3. **Default / SceneObject Order**（与邻层抢画时微调）  

否决：新写排序脚本；只改死 Order；`VillagePlayerDepthZone`；`SpriteFadeOnPlayerFootTrigger`；DepthComponent 与 DepthSort 同开；顺手改 `精灵池上`；挂到 `路灯蘑菇`。

---

## ⑤ 要改文件（路径级）

| 全路径 | 做什么 |
|--------|--------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | **仅** `精灵池路灯` Add `VillageSceneObjectDepthSort`，字段抄上表 |
| `肯姆尼2合层.prefab` | **不改**（路灯不在 Prefab；除非产品要合层源同步——OPEN） |
| `VillageSceneObjectDepthSort.cs` | **不改** |
| `精灵池中` / 青石围栏 | **不改** |
| `精灵池上` | **本期不做** |

施工说明建议：`Assets/Doc/施工说明/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_施工说明.md`（对照 0830 池中施工说明）。

---

## ⑥ 验收表

| # | 操作 | 期望 |
|---|------|------|
| 1 | 站在路灯「后方」（相对锚点，玩家 Y 更大/更深） | 灯 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 灯 |
| 3 | A/D、W/S、斜向绕灯 | 换层无狂闪、无卡死 |
| 4 | 运行时看路灯 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 再绕 `精灵池中`、青石围栏 | 金样仍正常 |
| 6 | Console | 无 DepthComponent 双开 Warning |
| 7 | （可选）勾 `debugLogOnLayerChange` | 有 `[VillageOcclusion] obj=精灵池路灯 …` |

---

## ⑦ 开放问题

| ID | 问题 | 施工默认 |
|----|------|----------|
| Q1 | `精灵池上` 是否本期？ | **否**（用户只点名路灯；0830 仍为 P1） |
| Q2 | 是否要把路灯写回 `肯姆尼2合层.prefab`？ | **否**（与 0830 只改场景一致；合层源同步另票） |
| Q3 | 灯脚空锚点是否第一版就做？ | **否**；自 Transform 先测，偏了再加 |
| Q4 | `路灯蘑菇` 要不要同款？ | **否**（非本票；勿误挂） |
