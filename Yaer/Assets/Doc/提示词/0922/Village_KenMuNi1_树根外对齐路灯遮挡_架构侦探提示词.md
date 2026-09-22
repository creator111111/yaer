# Cursor Agent Prompt · Village_KenMuNi1：`树根外` 对齐 `精灵池路灯` 的图层变化（DepthSort）

> **角色**：先【架构侦探】只读对拍；报告拍板后再【施工员】最小挂载  
> **日期**：2026-09-22  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **金样物体**：`肯姆尼2合层 / **精灵池路灯**` —— 已有与池中同款的图层变化（`VillageSceneObjectDepthSort`）  
> **目标物体（用户红箭头 / 选中）**：`肯姆尼3合层 / **树根外**` —— 要做成和路灯**一样效果**  
> **现网组件真源**：`VillageSceneObjectDepthSort`（guid `ba2f9bf2…`）—— **禁止**新写第二套排序脚本  
> **不是**：改 `树根内`（除非验收同穿帮再开）；改半透明 Fade；改 Walk 障碍；改玩家 Locomotion / DepthZone；改 DepthSort.cs 核心；批量给整棵肯姆尼3合层挂脚本  

把下面「侦探」整段交给 Cursor Agent。没对拍路灯 Inspector、没确认树根外现网组件之前，不要施工。

---

## 提示词助手预梳理（侦探须 YAML/Hierarchy 复核）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 视觉 | 玩家相对 **树根外** 前后走时，遮挡换层观感与 **精灵池路灯** 相同（DNF：人在后被根挡住、人在前盖住根） |
| 实现 | **复用** `VillageSceneObjectDepthSort`；字段**先抄路灯**（不是再发明一套 Order） |
| 范围 | 仅 **`树根外`** |
| 不做 | `树根内` 强制本期；半透明；改 Prefab 源（默认只改**场景实例**，除非报告证明只存在于 Prefab） |

### 现场 Hierarchy（用户截图）

```
Village_KenMuNi1
└─ … / 肯姆尼3合层
     ├─ 背景 / 天 / 云 / 远景 / 中景 / 近景…
     ├─ 村长家门 / 村长 / 兵 / 艾米艾莉 / 乱石…
     ├─ 树根内
     └─ ★ 树根外          ← 目标：加路灯同款图层变化
```

### 金样现网（路灯；须再读磁盘）

对照：

- `Assets/Doc/执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md`
- `Assets/Doc/施工说明/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_施工说明.md`（若已有）
- `Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs`

预扫（`Village_KenMuNi1.unity` 上路灯 DepthSort，**肯姆尼2 Order+3 之后**）：

| 字段 | 路灯预扫值 |
|------|------------|
| `sortingOrderWhenDefaultLayer` | **9** |
| `sortingOrderWhenSceneObjectLayer` | **3** |
| `invert` | 0 |
| `anchorOverride` | 本 Transform |
| `preferTownLocomotionAuthoritativeY` | 1 |
| `targetSpriteRenderers` | 本物体 SR |

（若磁盘与上表不一致，以磁盘路灯为准整表抄。）

### 目标预扫（须证伪）

| 项 | 预扫 |
|----|------|
| 路径 | `…/肯姆尼3合层/树根外`（场景 + Prefab `肯姆尼3合层.prefab` 均有同名） |
| 组件 | 场景实例现为 **Transform + SpriteRenderer** 两件套，**无** DepthSort |
| 近邻 | 同合层有 `树根内`（同样预扫无 DepthSort）——**本期默认不做**，写入开放问题即可 |
| 易混 | 附近 `青石围栏` 也有 DepthSort（6/0 旧表）；金样以用户指定的 **路灯** 为准，不要误抄围栏旧 6/0，除非报告证明肯姆尼3 Order 体系必须不同 |

生活类比：路灯已经会「人走到前后自动换谁盖住谁」；树根外还是死海报。给它装**同一款门禁**，旋钮先抄路灯。

### 否决方案

| 方案 | 原因 |
|------|------|
| 新写排序脚本 | 已有 DepthSort |
| 只改死 SortingOrder、不按玩家 Y 切换 | 不是「图层变化功能」 |
| `VillagePlayerDepthZone` | 改玩家层，不是改树根 Sprite |
| `SpriteFadeOnPlayerFootTrigger` | 半透明 |
| DepthComponent + DepthSort 同开 | 双写 Order |
| 顺手改 `树根内` | 用户只点名树根外 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：让 `Village_KenMuNi1` → `肯姆尼3合层` → **树根外** 拥有与 **精灵池路灯** 相同的图层变化系统（`VillageSceneObjectDepthSort`），并写出可施工的最小清单。

### 必读

@Assets/Doc/执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md
@Assets/Doc/执行文档/8月/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/ArtRes/Scene/Village/Prefab/肯姆尼3合层.prefab

### A. 钉死物体

1. Hierarchy：`…/肯姆尼3合层/树根外` 是否与截图一致  
2. 当前组件列表；SR 的 SortingLayer / Order  
3. 是否已有 DepthSort（预扫无）  
4. 与 `树根内`、路灯、青石围栏对照表（金样=路灯）

### B. 对拍金样配置

读出磁盘上 **精灵池路灯** 的 `VillageSceneObjectDepthSort` 全字段（预扫 Default/SceneObject = 9/3，以磁盘为准）。  
拍板树根外初值：**整表抄路灯**。  
若树根 pivot 偏高（根冠在上、贴地处在下），写明调参顺序：invert → 锚点改脚底空物体/Collider → 微调 Order；**第一版不要改 C#**。

注意：树根在肯姆尼**3**合层，路灯在肯姆尼**2**；若抄 9/3 后与 3 合层静图前后关系穿帮，报告写「仍先抄路灯，验收再调 Order」，不要因此改用另一套脚本。

### C. Prefab vs 场景

钉死改 `Village_KenMuNi1.unity` 实例还是 `肯姆尼3合层.prefab`。  
倾向：与路灯/池中一致 —— **只改场景实例**（除非树根外仅 Prefab 有、场景无独立覆盖）。

### D. 方案与否决

推荐 **方案 A**：`树根外` 根 Add `VillageSceneObjectDepthSort`，字段抄路灯。  
否决：新脚本 / 死 Order / DepthZone / Fade / 双开 DepthComponent / 顺手改树根内。

### E. 验收清单

| # | 操作 | 期望 |
|---|------|------|
| 1 | 站在树根外「后方」（相对锚点，玩家 Y 更大/更深） | 根 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 根 |
| 3 | A/D、W/S、斜向绕根 | 换层无狂闪 |
| 4 | 运行时看树根外 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 再绕 `精灵池路灯` / `精灵池中` | 金样仍正常 |
| 6 | Console | 无 DepthComponent 双开 Warning |
| 7 | （可选）`debugLogOnLayerChange` | `[VillageOcclusion] obj=树根外 …` |

前后反了：勾 `invert`。

### F. 报告结构

① 结论一句话  
② 路灯 vs 树根外对照表  
③ 推荐 Inspector 初值（抄路灯全字段）  
④ 调参顺序  
⑤ 要改文件路径级  
⑥ 验收表  
⑦ 开放问题（树根内是否本期）

写入：`Assets/Doc/执行文档/0922/Village_KenMuNi1_树根外对齐路灯遮挡_架构溯源报告.md`
```

---

## 施工员（侦探闭环后再复制；未闭环勿用）

```
@Assets/Doc/执行文档/0922/Village_KenMuNi1_树根外对齐路灯遮挡_架构溯源报告.md
@Assets/Doc/执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md

你是【施工员】。只按溯源报告给 `树根外` 挂与 `精灵池路灯` 同款 `VillageSceneObjectDepthSort`。

约束：
- 不改 `VillageSceneObjectDepthSort.cs`
- 不改路灯 / 池中 / 青石围栏已有配置
- 不做 `树根内`（除非报告 P0 点名）
- 禁止 Fade / DepthZone 冒充；禁止 DepthComponent 双开
- 默认只改场景实例
- 写入：`Assets/Doc/施工说明/0922/Village_KenMuNi1_树根外对齐路灯遮挡_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ 确认抄的是**路灯**现网字段（预扫 9/3）  
2. 确认只做 `树根外`、不动 `树根内`  
3. 复制「施工员」→ Play 绕树根走一圈验换层
