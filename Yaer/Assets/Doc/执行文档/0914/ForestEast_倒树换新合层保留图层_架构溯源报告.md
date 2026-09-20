# ForestEastScene 倒树 · 换新合层保留图层 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核对；**未改**代码 / 场景 / Prefab / Git  
**Unity**：2020.3.48f1  
**场景节点**：`ForestEastScene` → `Objects` → **`倒树`**（fileID `604494291`，根世界坐标 ≈ `(327.07, -7.67, 0)`）  
**新素材真源**：`Assets/ArtRes/Scene/SuburbEast/4.5/倒树合层.prefab`  
**产品期望**：看起来换成 4.5 合层图；**保留** Hierarchy / SortingLayer / SortingOrder / 遮罩语义 / 树桥玩法引用  
**不是**：删根拖合层当根；把 Sorting 改成 Prefab 的 Default 0/1/2/3；改 `TreeBridgeLogic`；重做东郊地图  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_倒树换新合层保留图层_架构侦探提示词.md`  
**OPEN**：`OPEN_QUESTIONS.md` 本节 Q1～Q5

---

## 沟通摘要

### ① 结论一句话

推荐 **方案 A**（先不挪坐标）：只把 `内 / 光 / 外 / 遮罩只影响人物` 的 Sprite 换成合层 Prefab 里那套图，**图层排序一律沿用场景现网**。`缝` **节点留下**，Sprite 改成合层文件夹里的 `缝.png`（Prefab 没挂这层，但磁盘有图）。光变高了，Play 对不齐再补 **A+B 只调光的 XY**。

### ② 原因（通俗）

倒树不是一张画，是「里面几层图 + 外面淡出 + 碰撞 + 倒下动画」。合层 Prefab 只是美术源，Sorting 全在 Default、还带 PSD 的 Z，直接拖进去会挡错人、玩法引用全断。另外：场景里这棵树 **已经在用** `4.5` 文件夹根上的散图；用户指定的 Prefab 用的是 **子文件夹 `倒树合层/` 里另一套 GUID**。所以这次换的是「散图 → 合层那一套」，不是从更老的图换起。

### ③ 用户需要做什么（检查清单）

| # | 看什么 | 现网 |
|---|--------|------|
| 1 | Hierarchy `倒树` 9 个子物体 | 内、缝、光、外、遮罩只影响人物、Components、CollisonMap_1、CollisonMap_2、SFX |
| 2 | 各视觉层 SortingLayer 名 + Order | 见 §2，**不要**改成 Default |
| 3 | `TreeBridgeLogic.OuterSprite` | 指向场景 **`外`** 的 SpriteRenderer（`62705658`） |
| 4 | 靠近倒树 | `外` 仍应淡出（脚本认的是这个引用，不是图文件名） |
| 5 | 拍板后施工 | 按 §5 勾选清单；**零代码** |

### ④ 程序补充

见下文。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **推荐** | **A**：同名层只换 `SpriteRenderer.sprite`；SortingLayerID / Order / MaskInteraction / 父子 / 逻辑组件 **原样**；Z 保持 0 |
| **A+B** | 仅当 Play 里 **光** 对不齐（合层光高度 6.02→9.24）。**禁止**把 Prefab 的 LocalPos（含 Z）整段贴进场景 |
| **C/D/E** | C 易双影；D 玩法全丢；E 破坏遮挡。**否决首选** |
| **缝** | Prefab 无子物体「缝」，但 `4.5/倒树合层/缝.png` 存在。默认 **留节点 + 换成该图**（OPEN Q1） |
| **遮罩** | 现网已是 `树洞新遮罩.png`，不是 Unity `SpriteMask`。换合层 `遮罩只影响人物.png` 时 **不要改** MaskInteraction / 材质 / Sorting（OPEN Q2） |
| **Animator** | `Fall.anim` 只打根 Transform（位置 + 欧拉 Z），**不绑**子物体 Sprite 尺寸。换图不改 clip |
| **其它场景** | `Village_OutSide` / `WestRappRoad` 也有名叫「倒树」的物体，**无** `TreeBridgeLogic`。本期 **不换**（OPEN Q4） |
| **旧 Prefab** | `Assets/ArtRes/Scene/倒树合层.prefab`（图层 143 等）**勿动** |

---

## 2. 对照表（场景 ↔ 4.5 合层 Prefab）

SortingLayer 索引来自 `TagManager.asset`：

| 索引 | 名字 | uniqueID（场景 YAML 有时写成有符号） |
|------|------|--------------------------------------|
| 0 | Default | 0 |
| **1** | **Map** | `-691675099` / 3603292197 |
| 6 | Player | （角色本体排序，夹在 Map 与 SceneObject 之间） |
| **8** | **SceneObject** | `694918277` |
| **9** | **Effect** | `-1083784733` / 3211182563 |

语义：**内/缝在玩家后面（Map）**；**外/光/遮罩在玩家前面（SceneObject / Effect）**。名字「遮罩只影响人物」靠的是 **画在 Effect 层盖住角色**，不是 `SpriteMask` 组件。

### 2.1 视觉层

| 子物体 | 场景 LocalPos | 场景 Sorting | 场景 Sprite（现网） | 合层 Prefab LocalPos | Prefab Sorting | 合层 Sprite（要换成） | 尺寸 场景 → 合层 |
|--------|---------------|--------------|---------------------|----------------------|----------------|------------------------|------------------|
| **内** | (-29.48, 7.01, **0**) | **Map / -2** | `4.5/内.png` `021f5870…` | (44.69, 7.25, **10**) | Default / 0 | `倒树合层/内.png` `0cc5befc…` | 85.64×14.51 → **同** |
| **缝** | (-26.35, 3.68, **0**) | **Map / -2** | `4.5/缝.png` `73df3d23…` | **Prefab 无此节点** | — | 磁盘有 `倒树合层/缝.png` `a224f95f…` | 2.93×7.8（合层缝需 Play 看） |
| **光** | (-19.91, 2.91, **0**) | **Effect / 1** | `4.5/光.png` `cbd9006b…` | (54.00, 4.64, **6.67**) | Default / 2 | `倒树合层/光.png` `0a6dc224…` | **55.79×6.02 → 56.67×9.24** |
| **外** | (-29.12, 4.82, **0**) | **SceneObject / 3** | `4.5/外.png` `44196fb8…` | (45.02, 5.05, **5**) | Default / 3 | `倒树合层/外.png` `7d013075…` | 84.77×10.1 → **同** |
| **遮罩只影响人物** | (-29.47, 6.97, **0**) | **Effect / 0** | `4.5/树洞新遮罩.png` `f4daac1a…` | (44.68, 7.49, **8.33**) | Default / 1 | `倒树合层/遮罩只影响人物.png` `8596789c…` | 86.18×14.98 → **89.44×14.98** |

共同点：双方 `MaskInteraction=0`，材质都是 Sprites-Default（`10754`）。Pivot 都是中心 `(0.5,0.5)`，PPU=100。

相对「内」的偏移（说明 **不要抄 Prefab 绝对坐标**）：

| | 场景 相对内 | 合层 Prefab 相对内 |
|--|-------------|-------------------|
| 外 | (0.36, -2.19) | (0.33, -2.20) ≈ 齐 |
| 遮罩 | (0.01, -0.04) | (-0.01, 0.25) Y 差约 0.3 |
| 光 | (9.57, -4.10) | (9.32, -2.61) **Y 差约 1.5**（叠在光变高上） |

另：`4.5/倒树合层/光_2.png`、旧路径 `ArtRes/Scene/倒树合层.prefab` **本期不用**。

### 2.2 非视觉子物体（必须原样保留）

| 子物体 | 作用 |
|--------|------|
| `Components/InteractiveComponent` | 靠近触发 `OuterSprite` 淡出 |
| `CollisonMap_1` | Layer **Map(8)**，实心 Box，本地 (-67.90, 7) |
| `CollisonMap_2` | 同上，本地 (7.87, 7) |
| `SFX` | `SoundToggleComponent`，倒下/落水音效 |

---

## 3. 玩法引用清单（禁止弄丢）

根 `倒树` 组件（均留着，不改序列化指向）：

| 组件 | 要点 |
|------|------|
| `TreeBridgeLogic` | `OuterSprite` → **`外` SR `62705658`**；`OuterSpriteFadeTime=1`；进出节点、新旧相机盒、`spcWormEgg`、`eggStoryTrigger`、`storyWoodWormLogicList`、`soundSfxCpn`、`aniEventCpn` |
| `Animator` | Controller guid `d90884cbed215424…`；`Fall()` 打 Trigger `Fall` |
| `Fall.anim` | `path` 空 = **只动根** `localPosition`（从 `(327.07,-7.67,0)` 起）+ 欧拉 Z 转到约 54.9°。不采样子 Sprite |
| `CldInteractiveListener` + 根 `BoxCollider2D` | Trigger，Offset `(-29.83, 4.1)` Size `(83.9, 8.21)` |
| `SceneEntity` + `ComponentSystemMono` | 实体注册 |
| `AnimationEventComponent` | `AfterFallDown` |
| Cinemachine Impulse | 倒下震屏 |
| `CheckFall()` | 存档 `ForestEastSceneData.TreeBridgeFall` 为真则 **Destroy 整棵倒树** |

`Fall()` 还会 `SetActive(false)` 一批 `AttachedGameObject`（含 **遮罩只影响人物** `399899081`）。换 Sprite 不改这份列表。

交互链：

```
靠近根 Trigger
  → InteractiveComponent onEnter
  → OuterSprite.DOFade(0)     // 必须仍是场景「外」
离开
  → DOFade(1)
```

---

## 4. 侦探须答

1. **SortingLayer 1/8/9 层名**：`Map` / `SceneObject` / `Effect`。玩家在 **Player(6)**。内/缝在玩家后；外、光、遮罩在玩家前。  
2. **遮罩**：没有 `SpriteMask`；`MaskInteraction=0`；普通 Sprite 盖在 Effect。换图 **不要**改 Mask 设置或材质。  
3. **A 还是 A+B**：先 **A**。内/外相对位置几乎一致；光高度和相对 Y 变化最大，不对齐再 B **只动光**。  
4. **缝**：选 **留节点 + 换成合层缝图**（提示词选项 1 的加强版）。不要删节点。  
5. **Animator**：绑根 Transform，不绑子图尺寸。风险只是倒下时新图视觉外沿不同，不是 clip 裁切。  
6. **第二处**：东郊玩法这棵是唯一 `TreeBridgeLogic`。别的场景「倒树」不同步。`scr/suburb-east.unity` 是旧拷贝，勿改。

---

## 5. 方案与施工勾选清单

| # | 方向 | 结论 |
|---|------|------|
| **A** | 只换 Sprite，保留图层关系 | **推荐执行** |
| **A+B** | A + 微调光（或遮罩）LocalXY，Z=0 | Play 对不齐再用 |
| C | 嵌套实例化合层 | 否（双影、引用仍指旧 SR） |
| D | 删根拖 Prefab | 否（玩法全丢） |
| E | Sorting 改 Prefab Default | 否（与「保留图层」相反） |

**替代方案**：若合层散图与 Prefab 像素被确认完全相同，可只换 GUID、不验收尺寸——磁盘上光/遮罩尺寸已经不同，**不能假设相同**。

### 5.1 施工步骤（可勾选，预期零代码）

只改 `Assets/GameRes/Scenes/ForestEastScene.unity` 里 `倒树` 各视觉层的 `m_Sprite`（及 OPEN 批准的 LocalXY）。**不要**改 Prefab 文件本身。

- [ ] 备份/确认 `OuterSprite` 仍是 `62705658`
- [ ] `内`：sprite → `guid: 0cc5befcc1a97ff4b8160eeac6f0ebb3`；**不改** SortingLayerID `-691675099`、Order `-2`、Pos、Z
- [ ] `光`：sprite → `guid: 0a6dc2247b2f7ea4b9c2ac50b6da28ea`；保留 Effect / Order 1
- [ ] `外`：sprite → `guid: 7d01307521622774babcb2b112064fd5`；保留 SceneObject / Order 3
- [ ] `遮罩只影响人物`：sprite → `guid: 8596789cec1c9d74a92f4def48945293`；保留 Effect / Order 0、MaskInteraction 0（OPEN Q2）
- [ ] `缝`：节点保留；sprite → `guid: a224f95f1c0d78c4d95217b8d902a625`；Sorting 仍 Map / -2（OPEN Q1）
- [ ] 确认未改：Components、CollisonMap_*、SFX、根 Collider、Animator、`AttachedGameObject`
- [ ] 确认未改任何 `m_SortingLayer` / `m_SortingLayerID` / `m_SortingOrder`
- [ ] 确认视觉子物体 `z=0`（没抄 Prefab 的 5/6.67/8.33/10）
- [ ] 未改 `ArtRes/Scene/倒树合层.prefab`（旧）
- [ ] Play：无双影；靠近 `外` 淡出；进/出树桥；Sorting 与换图前表一致

---

## 6. 验收矩阵

| 项 | 期望 |
|----|------|
| Hierarchy | 仍 9 子节点，名称不丢 |
| SortingLayer / Order | 与 §2.1 场景列 **完全一致** |
| 外观 | 内/光/外/遮罩为 **合层嵌套图**；无双影 |
| 靠近 | `外` 仍淡出 |
| 树桥进出 | 碰撞、相机盒、`isInTreeBridge` / 存档不坏 |
| Z | 视觉子物体 Z=0 |
| 其它场景 | 不动 |

剩余风险：缝与新外叠缝；光变高后洞口光斑偏；`树洞新遮罩` 若是后期特调，换成合层遮罩可能裁切不同（OPEN Q2）。

---

*拍板后把提示词【施工员】段交给 Agent。本期最小改动：场景四个（加缝则五个）Sprite GUID，零脚本。*
