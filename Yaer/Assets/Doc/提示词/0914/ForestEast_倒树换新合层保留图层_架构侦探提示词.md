# Cursor Agent Prompt · ForestEastScene 倒树：换新合层素材，保留原图层关系

> **角色**：先【架构侦探】只读核对对照表与风险；拍板后【施工员】最小替换素材  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` → `Objects` → **`倒树`**（用户 Hierarchy 红框）  
> **新素材**：`Assets/ArtRes/Scene/SuburbEast/4.5/倒树合层.prefab`  
> **产品期望（钉死）**：  
> 1. 倒树**看起来换成新图**（SuburbEast/4.5 合层）  
> 2. **保留原来的图层关系**：Hierarchy 子树结构、各层 **SortingLayer / SortingOrder**、遮罩语义、逻辑引用不丢  
> 3. **玩法不断**：树桥进出、外层淡入淡出、碰撞、SFX、虫卵/木虫演出引用仍可用  
> **不是**：整棵删掉 `倒树` 再拖合层当根；把 SortingLayer 改成 Prefab 默认 Default；改 `TreeBridgeLogic` 玩法；顺手重做东郊地图  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 东郊倒树要换成 4.5 新合层图，但树洞那一套分层（内/缝/光/外/遮罩）和碰撞、脚本都要留着，不能换成一张「只有美术、没玩法」的空壳。

### 现网 Hierarchy（须保留骨架）

```
Objects/倒树                    ← TreeBridgeLogic + Animator + 交互 Trigger 等（玩法根）
  ├─ 内
  ├─ 缝                         ← 新合层 Prefab **没有**同名层（开放问题）
  ├─ 光
  ├─ 外                         ← TreeBridgeLogic.OuterSprite 指向此 SR（靠近淡出）
  ├─ 遮罩只影响人物
  ├─ Components
  ├─ CollisonMap_1
  ├─ CollisonMap_2
  └─ SFX
```

### 新 Prefab 子树（仅美术源）

`Assets/ArtRes/Scene/SuburbEast/4.5/倒树合层.prefab` 根名 `倒树合层`：

| 子物体 | Prefab SortingOrder（Default 层） | 备注 |
|--------|-----------------------------------|------|
| 内 | 0 | 有 |
| 遮罩只影响人物 | 1 | 有 |
| 光 | 2 | 有 |
| 外 | 3 | 有 |
| **缝** | — | **无** |

另：同仓库还有旧路径 `Assets/ArtRes/Scene/倒树合层.prefab`（层名更乱，含「图层 143」等）——**本案真源以用户指定的 SuburbEast/4.5 为准**，勿混用。

### 图层关系：场景 vs 新 Prefab（关键）

场景现网（预扫，须再核实 SortingLayer **名字**）：

| 子物体 | LocalPos（约） | SortingLayer（索引） | SortingOrder |
|--------|----------------|----------------------|--------------|
| 内 | (-29.48, 7.01, 0) | **1** | **-2** |
| 缝 | (-26.35, 3.68, 0) | **1** | **-2** |
| 光 | (-19.91, 2.91, 0) | **9** | **1** |
| 外 | (-29.12, 4.82, 0) | **8** | **3** |
| 遮罩只影响人物 | (-29.47, 6.97, 0) | **9** | **0** |

新合层 Prefab：

| 子物体 | LocalPos（PSD 导出坐标，含 Z） | SortingLayer | SortingOrder |
|--------|-------------------------------|--------------|--------------|
| 内/光/外/遮罩 | 约 (44~54, 4~7, **Z≠0**) | **Default(0)** | 0/1/2/3 |

→ **「保留图层关系」= 保留场景这套 SortingLayer + Order（及 Z=0 约定），不要照搬 Prefab 的 Default/Order/带 Z 坐标。**  
换的是 **Sprite（及必要时微调 XY 对齐新轴心）**，不是整棵替换。

### 玩法锚点（禁止弄丢）

- 脚本：`TreeBridgeLogic`（`OuterSprite` → `外` 的 SpriteRenderer；进出节点、相机盒、虫卵、SFX 等大量序列化引用）  
- 根上还有 Animator、Interactive、碰撞盒等  
- 子：`Components` / `CollisonMap_*` / `SFX` **一律保留**

### 方案优先级（侦探排，施工勿自选）

| # | 方向 | 利弊 |
|---|------|------|
| **A（推荐）** | **同名子物体只换 Sprite**（从 4.5 合层对应层拷 Sprite 引用）；**原样保留** SortingLayerID / SortingOrder / MaskInteraction / 父子结构 / 逻辑组件 | 最小；对齐「保留图层关系」 |
| **B** | 换 Sprite + **按新图轴心微调 LocalXY**（Z 仍 0）；Layer/Order 仍用场景旧值 | 图偏了才用；须验收重叠/入口 |
| **C** | 在 `倒树` 下嵌套实例化合层，再 Disable 旧 内/光/外… | 易双影、引用仍指旧 SR；仅当 A 轴心差极大时备选，且须改 OuterSprite 等引用 |
| **D（否决首选）** | 删除场景 `倒树`，直接拖 `倒树合层` 当根 | **玩法全丢** |
| **E（否决）** | 把场景各层 SortingLayer 改成 Prefab 的 Default 0/1/2/3 | **破坏遮挡/纵深**，与「保留图层」相反 |

### 「缝」怎么处理（须写入报告 / OPEN）

新 Prefab **无缝**。施工默认候选（侦探择一并记 OPEN）：

1. **保留旧缝 Sprite**（仅换有同名层的图）  
2. **隐藏/Disable 缝**（若新外/内已含缝视觉）  
3. 等美术补「缝」再换（本期不删节点）

### 复现 / 验收矩阵

| 项 | 期望 |
|----|------|
| Hierarchy | 仍为 `倒树` 下原 9 子节点结构（名称不丢） |
| 各视觉层 SortingLayer / Order | 与换图前一致（或报告明确批准的等价表） |
| 外观 | 内/光/外/遮罩为 4.5 新图；无双影 |
| 靠近倒树 | `外` 仍可淡出（OuterSprite 引用有效） |
| 进树桥 / 出树桥 | 碰撞、相机盒、存档 `isInTreeBridge` 不坏 |
| Z | 场景视觉子物体保持 **Z=0**（勿抄 Prefab 的 Z） |
| 其它场景 | 不误改 `ArtRes/Scene/倒树合层.prefab` 旧资源（除非报告要求） |

### 侦探须回答

1. 场景 SortingLayer 索引 1/8/9 的**层名**是什么？与玩家/遮罩的关系？  
2. `遮罩只影响人物` 现网 MaskInteraction / 材质是否已正确？换 Sprite 后要不要改 Mask 设置？  
3. 推荐 **A 还是 A+B**？新图轴心相对旧 LocalPos 偏差多大？  
4. **缝** 选 1/2/3 哪种？  
5. Animator / 倒下演出是否绑死旧 Sprite 尺寸？换图后有无裁切/错位风险？  
6. 是否存在第二处倒树实例（别的场景）需要同期换？

### 必读

1. `Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md`  
2. 场景：`Assets/GameRes/Scenes/ForestEastScene.unity` → `Objects/倒树`  
3. 新图：`Assets/ArtRes/Scene/SuburbEast/4.5/倒树合层.prefab`  
4. `TreeBridgeLogic.cs`（OuterSprite 淡出、树桥流程）  
5. 用户 Hierarchy 截图  
6. 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / 场景 / Prefab / Git  
- 禁止建议「整棵替换为合层 Prefab」当首选  
- 禁止建议把 SortingLayer 统一改 Default  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md`

结构：

1. **结论一句话**（推荐 A/A+B；缝如何处理）  
2. **对照表**：场景各层 ↔ 4.5 合层（Sprite GUID、Layer、Order、LocalPos）  
3. **玩法引用清单**（OuterSprite、Collider、Components、SFX…）  
4. **方案 ≥2** + 推荐 + 施工步骤清单（逐步、可勾选）  
5. 不清处记 `Assets/Doc/OPEN_QUESTIONS.md`  

回答风格：①结论 ②原因白话 ③检查清单 ④程序补充。

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告确认推荐 **A（或 A+B）**；SortingLayer/Order **禁止**改成 Prefab Default；缝已裁定。  
> **目标**：`ForestEastScene/Objects/倒树` 视觉换成 `SuburbEast/4.5/倒树合层` 对应层 Sprite；**保留**原 Hierarchy、SortingLayer、SortingOrder、逻辑组件与引用。  
> **做法**：  
> 1）对 `内`/`光`/`外`/`遮罩只影响人物`：只替换 `SpriteRenderer.sprite`（及报告批准的 LocalXY）；  
> 2）**不要**改 SortingLayerID / SortingOrder / 父节点；**不要**抄 Prefab 的 Z；  
> 3）保留 `缝`/`Components`/`CollisonMap_*`/`SFX` 按报告处理；  
> 4）核对 `TreeBridgeLogic.OuterSprite` 仍指向场景 `外`；  
> 5）详细注释若改脚本（预期尽量零代码）。  
> **禁止**：删根重建；嵌套合层造成双影未处理；改全局 SortingLayer 表；改树桥玩法逻辑。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_倒树换新合层保留图层_施工说明.md`  
> **验收**：按上文矩阵；进/出树桥各 1 次；靠近看外层淡出；截图对比新旧遮挡关系。

---

## 【验收员】Prompt（可选）

> 在 Hierarchy 选中 `倒树` 各视觉子物体，记录 SortingLayer 名 + Order + Sprite 名；对比换图前表。  
> 进 Play：靠近淡出、进洞、出洞；Console 无丢引用。  
> 输出：通过项 + 剩余风险（缝、Animator 倒下、轴心微偏）。
