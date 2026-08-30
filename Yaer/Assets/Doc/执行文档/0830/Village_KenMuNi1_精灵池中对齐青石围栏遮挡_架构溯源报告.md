# Village_KenMuNi1 — `精灵池中` 对齐 `青石围栏` 玩家位置渲染遮挡 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 配置拍板（**本阶段未改代码 / Prefab / 场景**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**金样**：`肯姆尼1合层` / **`青石围栏`**  
**目标**：`肯姆尼2合层` / **`精灵池中`**  
**现网组件真源**：`VillageSceneObjectDepthSort`（guid `ba2f9bf2…`）

关联：`技术文档/村庄探索_场景遮挡与Walk区内障碍碰撞.md` §3 · `执行文档/5月/0512/…图层遮挡_程序施工执行说明.md` OC-01～05

---

## ① 结论一句话

**青石围栏已挂唯一场景级 `VillageSceneObjectDepthSort`（按玩家世界 Y vs 锚点切换 Default↔SceneObject）；精灵池中只有 Transform+SpriteRenderer，无 DepthSort，且 SR 固定在 `SceneObject` order=0——会一直压在玩家上、不会随前后换层。拍板：只改场景——在 `精灵池中` 根上添加同一脚本，字段先抄围栏（DefaultOrder=6 / SceneObjectOrder=0 / invert=0 / 锚点先用本 Transform），target 绑本物体 SR；不改 C#；不动 `Collider (1)` 物理；`精灵池上` 默认本期不做（开放）。**

---

## ② 原因（通俗）

青石围栏装了「看你站前站后、自动换渲染层」的开关。  
精灵池中还是一张死海报，而且还钉死在「永远盖住人」那一层（SceneObject）——所以绕着池子走，遮挡不会像围栏那样对。

---

## ③ 用户检查清单（施工后 · 对齐 OC）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 站在精灵池「后方」（相对锚点，玩家 Y 更大/更「深」） | 池子 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 池子 |
| 3 | 纯 A/D、纯 W/S、斜向 | 层级切换无闪烁鬼畜；无卡死 |
| 4 | Inspector 运行时看 SR | `sortingLayerName` 在 **Default ↔ SceneObject** 间变 |
| 5 | 对照 | **青石围栏**行为仍正常 |
| 6 | Console | 无 `[VillageOcclusion]` DepthComponent 双开警告 |
| 7 | （可选）勾 `debugLogOnLayerChange` | 有 `[VillageOcclusion] obj=精灵池中 …` 切换日志 |

前后若整段反了：先勾 **invert**，再动 Order/锚点。

---

## ④ 给程序

### A. 青石围栏金样（磁盘核实）

| 项 | 值 |
|----|-----|
| Hierarchy | `Map` / `Design` / **`肯姆尼1合层`** / **`青石围栏`** |
| 合层 Transform | `(-13.92, -7.8, 0)`；围栏 local `(48.5, 4.925, 8.57)` |
| Physics Layer | **6** |
| 组件 | Transform · SpriteRenderer · **`VillageSceneObjectDepthSort`** · BoxCollider2D · Rigidbody2D(Static) · CompositeCollider2D |
| SR 初始 | SortingLayer **Default** · Order **6** |
| 场景内 DepthSort 数量 | **仅此 1 处** |

**`VillageSceneObjectDepthSort` 序列化**

| 字段 | 金样值 |
|------|--------|
| `targetSpriteRenderers` | 本物体 SR `4278458904742138118` |
| `anchorOverride` | **本物体 Transform** `7308007906505976926`（自锚，非另挂空物体） |
| `playerLogicOverride` | null（Tag Player 缓存） |
| `invertPlayerVersusAnchorComparison` | **0** |
| `sortingOrderWhenDefaultLayer` | **6** |
| `sortingOrderWhenSceneObjectLayer` | **0** |
| `updateEveryNthFrame` | 1 |
| `debugLogOnLayerChange` | 0 |
| `preferTownLocomotionAuthoritativeY` | **1** |

行为契约（脚本）：`playerY > anchorY` → SceneObject（挡玩家）；否则 Default；仅 `Village2_5D` 生效。

### B. 精灵池中现状

| 项 | 值 |
|----|-----|
| Hierarchy | `Map` / `Design` / **`肯姆尼2合层`** / **`精灵池中`** |
| GO fileID | `1718875339417547372` |
| 合层 Transform | `(-93.22, -7.8, 0)`；池中 local **`(30.525, 7.76, 2.857)`** |
| 根组件 | **仅** Transform `244632720003562929` + SpriteRenderer `8162121874168452420` |
| DepthSort / DepthComponent | ❌ **均无** |
| SR 初始 | SortingLayer **`SceneObject`**（uniqueID `694918277`）· Order **0** |
| 子物体 | **`Collider (1)`**（local `0.1, -1.49`；Layer 6；CompositeCollider2D 等）——**Walk/阻挡物理**，非遮挡换层 |

**精灵池上（对照，本期默认可不做）**

| 项 | 值 |
|----|-----|
| 路径 | 同合层 / `精灵池上` |
| 组件 | Transform + SR；**无** DepthSort |
| SR | 亦为 **SceneObject** · Order 0；子 Collider 另有 |

### C. 差异与方案拍板

| 维度 | 青石围栏 | 精灵池中 | 施工 |
|------|----------|----------|------|
| DepthSort | ✅ | ❌ | **添加** |
| 动态换层 | ✅ | ❌（钉死 SceneObject） | 由脚本接管 |
| 锚点 | 自 Transform | — | **先自 Transform**（对齐金样） |
| Order 表 | 6 / 0 | — | **先抄 6 / 0** |
| Collider | 根上有（障碍） | 子 `Collider (1)` | **不改物理** |
| C# | — | — | **不改** `VillageSceneObjectDepthSort.cs` |

**否决**：新写排序脚本；只改固定 Order；`VillagePlayerDepthZone`（改玩家层）；`SpriteFadeOnPlayerFootTrigger`；与 DepthComponent 同开。

### D. 推荐 Inspector 初值（精灵池中）

| 字段 | 初值 | 备注 |
|------|------|------|
| 组件挂点 | **`精灵池中` 根** | 有 SR 的那层 |
| `targetSpriteRenderers` | 本物体 SR | 仅一片；无多片波纹 |
| `anchorOverride` | **本 Transform**（可空=同效） | 备选：拖 `Collider (1)`（更靠脚底 Y−1.49）若站位切换偏 |
| `invert` | **false** | 前后反了再勾 |
| `sortingOrderWhenDefaultLayer` | **6** | 抄围栏；不对再调 |
| `sortingOrderWhenSceneObjectLayer` | **0** | 抄围栏 |
| `preferTownLocomotionAuthoritativeY` | **true** | 与围栏一致 |
| `updateEveryNthFrame` | 1 | — |
| `debugLogOnLayerChange` | 验收时可临时 true | 交前关 |

**调参顺序（实机不对时）**：① invert → ② 锚点改 `Collider (1)` → ③ 微调 Default/SceneObject Order（池子美术可能要单独表，开放 Q3）。

### E. 边界

| 项 | 决议 |
|----|------|
| `精灵池上` | **P1 / 开放**：上沿也穿帮再同挂；默认本期不做 |
| 玩家 `TownPlayerLocomotion` Y | DepthSort 读权威 Y，**勿改** Locomotion |
| DepthZone | 正交；遮挡换层不替代障碍 |
| Walk 障碍 `Collider (1)` | **保持**；遮挡不靠它 |

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `精灵池中` Add `VillageSceneObjectDepthSort` | **P0** |
| 2 | 绑 SR + 字段抄围栏初值 | **P0** |
| 3 | Play 验 OC；微调 invert/锚点/Order 写入提交说明 | **P0** |
| 4 | （可选）`精灵池上` 同挂 | P1 |
| 5 | 改 DepthSort.cs | ❌ |

**预期 diff**：仅 `Village_KenMuNi1.unity`（精灵池中组件序列化）。

### G. 验收清单

同 §③（OC-01～05 语义）。

### H. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | `精灵池上` 是否必须同挂？ | **否**，验收穿帮再做 | ✅ |
| Q2 | 锚点用自 Transform 还是 `Collider (1)`？ | **先自 Transform**；偏了再 Collider | ✅ |
| Q3 | Order 是否必须 6/0？ | **先抄**；池子可单独调 | ✅ |
| Q4 | 池子初始已在 SceneObject，捕获初始态是否影响出村还原？ | 脚本会还原进场景时状态；村庄内由 DepthSort 接管——验收出村/回村 | ⏳ 施工验 |

（已追加 `OPEN_QUESTIONS.md`。）
