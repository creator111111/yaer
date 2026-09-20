# ForestEastScene 倒树 · 换新合层保留图层 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **A**（先不挪坐标）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**东郊倒树五层图已换成 `4.5/倒树合层/` 那一套；Hierarchy、图层、坐标、树桥引用都没动。**

### ② 原因（通俗）

合层 Prefab 只是美术源，Sorting 全在 Default、还带 PSD 的 Z。直接拖进去会挡错人、玩法引用会断。所以只换每层贴的那张图。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy `Objects/倒树` | 子物体名称仍在（内/缝/光/外/遮罩/Components/碰撞/SFX） |
| 2 | 选各视觉层看 Sorting | 内、缝 = Map / -2；光 = Effect / 1；外 = Effect / 3；遮罩 = Effect / 0（**未改**） |
| 3 | 各层 Sprite 名 | 指向 `SuburbEast/4.5/倒树合层/` 下同名 png |
| 4 | 靠近倒树 | `外` 仍淡出 |
| 5 | 进/出树桥各 1 次 | 碰撞、相机盒正常 |
| 6 | 无双影 | 没有再嵌一套合层 Prefab |
| 7 | 光斑 / 遮罩裁切 | 若偏了再开 A+B（只调光 XY）；裁切变差可把遮罩改回 `树洞新遮罩` |

### ④ 程序补充

见下文。

---

## 改动清单

只改 `Assets/GameRes/Scenes/ForestEastScene.unity` 五处 `m_Sprite` GUID。

| 子物体 | 旧 GUID（4.5 散图） | 新 GUID（`倒树合层/`） |
|--------|---------------------|------------------------|
| **内** | `021f5870…` | `0cc5befcc1a97ff4b8160eeac6f0ebb3` |
| **缝** | `73df3d23…` | `a224f95f1c0d78c4d95217b8d902a625` |
| **光** | `cbd9006b…` | `0a6dc2247b2f7ea4b9c2ac50b6da28ea` |
| **外** | `44196fb8…` | `7d01307521622774babcb2b112064fd5` |
| **遮罩只影响人物** | `f4daac1a…`（树洞新遮罩） | `8596789cec1c9d74a92f4def48945293` |

已核对：`TreeBridgeLogic.OuterSprite` 仍是场景 `外` SR `62705658`。

### 未改

- SortingLayerID / SortingOrder / MaskInteraction / 材质 / LocalPos / Z  
- Components、CollisonMap_*、SFX、根 Collider、Animator、`Fall.anim`、`AttachedGameObject`  
- `TreeBridgeLogic` 脚本  
- `Village_OutSide` / `WestRappRoad` 的「倒树」  
- 旧资源 `ArtRes/Scene/倒树合层.prefab`  
- 合层 Prefab 本身  

**说明**：侦探表写「外 = SceneObject / 3」，现网 YAML 实际是 **Effect / 3**（`SortingLayerID: -1083784733`）。方案 A 要求保留场景现网，故未改成 SceneObject。

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 缝 | **留节点** + 换成合层 `缝.png` |
| Q2 遮罩 | **换成**合层 `遮罩只影响人物.png`，Mask 设置不动 |
| Q3 光 XY | **先不挪**；Play 对不齐再 B |
| Q4 其它场景 | **不换** |
| Q5 旧 Prefab | **不动** |

---

## 剩余风险

- 光变高（6.02→9.24），洞口光斑可能偏 → 再开 A+B 只动光 XY，Z 保持 0。  
- 缝与新外可能叠缝 → 再 Disable「缝」。  
- 合层遮罩与后期特调 `树洞新遮罩` 裁切可能不同。  
- 本机未 Play；请在 Unity 按清单验收。
