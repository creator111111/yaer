# 章末 · `ImageHomeToJingLingVillage` 未点亮 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / Prefab / 存档 / Git  
**Unity**：2020.3.48f1  
**UI**：章末 `MapPanel(Clone)` → `Fg` → `Road` → **`ImageHomeToJingLingVillage`**（Hierarchy 灰 = 运行时 inactive）  
**现象**：第一章/序章结束出世界地图时，家→精灵村**路线贴图**应亮，现网关闭  
**产品期望**：章末出图时该 Image **Active**；`ButtonJingLingVillage` 可点逻辑保持（`UnlockPlace`）；**不**自动进村  
**不是**：改村场景；改进村黑幕；只勾 Prefab Active；把关卡钮可点当成路线已亮  
**历史**：0721 只做 `UnlockPlace`，明确 **`UnlockRoad` 本期不做、路线图另案** —— **本案即该另案**  
**提示词**：`Assets/Doc/提示词/0914/章末_ImageHomeToJingLingVillage未点亮_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q4（并正式撤销 0721「本期不做 UnlockRoad」）

---

## 沟通摘要

### ① 结论一句话

**主因 A：章末 `UnlockChapterEndMapPlace` 只 `UnlockPlace(JingLingVillage)`，按 0721 决议故意不做 `UnlockRoad`；`MapFormLogic.ShowUnlockRoad` 开图时先全关路线 Image，再只按存档 `GetUnlockRoad()` 打开 → 列表无 `HomeToJingLingVillage` 则本案 Image 必灰。**  
出门 `OnHomeScene1_GetMap` 虽会写路，但章末不保证该路径已走过/仍在档。推荐 **R3（R1+R2）**：章末开图前补 `UnlockRoad(HomeToJingLingVillage)`（主），保留出门 GetMap（辅）；**正式撤销** 0721「本期不做 UnlockRoad」。

### ② 原因（通俗）

地图上「能不能点肯姆尼」和「家到村那条线画亮不亮」是两本账。0721 只开了关卡点，路线图留到另案。开地图时程序会先把所有路线图关掉，再按存档里解锁过的路一条条打开——章末从不写这条路，图就一直灰。

### ③ 用户检查清单

| # | 操作 | 期望 |
|---|------|------|
| 1 | 正规：出门拿地图 → … → 章末出图 | `ImageHomeToJingLingVillage` **亮** |
| 2 | 章末当下 `GetUnlockRoad()` | 含 `HomeToJingLingVillage` |
| 3 | `ButtonJingLingVillage` | 仍可点；点选进村不变 |
| 4 | Prefab 默认勾选 | 可保持；**不能**当唯一修复 |

### ④ 程序补充

见下文。施工说明待拍板：`施工说明/0914/章末_ImageHomeToJingLingVillage未点亮_施工说明.md`。

---

## 1. 名词拆开

| 名称 | 是什么 | 谁控制 |
|------|--------|--------|
| **`ImageHomeToJingLingVillage`** | 家→精灵村**路线贴图** | `ShowUnlockRoad` + `PlayerMapData` 道路列表 |
| **`ButtonJingLingVillage`** | 肯姆尼关卡点能否点 | `ShowUnlockPlace` + `UnlockPlace`（0721 已做） |
| **`SelectPlaceLight`** | 高亮某钮 | **不**开路线 Image |
| **`ImageAllRoad`** | Road 下另一张全路线图 | 同被 `ShowUnlockRoad` 先全关；本案箭头不是它 |

键约定：存档道路名 **`HomeToJingLingVillage`** → UI 键 **`Image` + roadName** = `ImageHomeToJingLingVillage`（与物体名一致）。

---

## 2. 现网链路（已核实）

```
MapPanel.OnOpen
  → ShowUnlockPlace()
  → ShowUnlockRoad()
       全部 roadImageDic SetActive(false)
       foreach GetUnlockRoad()：打开 Image{roadName}

路线写入：
  HomeScene1GoOutStoryCollider.OnHomeScene1_GetMap
    → PlayerHandlerComponentGSM.UnlockRoad(HomeToJingLingVillage)
    （HomeScene1GoOutStory.prefab 节点 ExecuteFunction 会调到）

章末：
  ChapterEndFormLogic.OpenMap
    → UnlockChapterEndMapPlace()   // 仅 UnlockPlace；注释「本期不做 UnlockRoad」
    → OpenUIForm(MapPanel)         // OnOpen → ShowUnlockRoad
```

Prefab 该 Image **默认 `m_IsActive: 1`**；运行时变灰 = `ShowUnlockRoad` 全关后存档无对应 road。

`roadImageDic`：`OnInit` 对 `road` 下所有 `Image` 按 **name** 入表 → 含本案物体；键匹配无 C 嫌疑。

---

## 3. 嫌疑裁定

| # | 嫌疑 | 裁定 | 证据 |
|---|------|------|------|
| **A 章末不做 UnlockRoad** | **主因** | `UnlockChapterEndMapPlace` 注释+代码只 `UnlockPlace`；0721 文档写明路线图另案；开图必走全关再按列表开 |
| **B 出门 GetMap 未写/档丢** | **加重/常见叠加** | GetMap 入口存在；若跳过出门剧情、或 `PlayerMapData.SerializeInternal` **空实现**导致落盘不稳，章末列表仍空。R1 可兜底 |
| **C roadImageDic 漏键** | **否** | 物体名与 `Image{HomeToJingLingVillage}` 一致；收集逻辑覆盖 Road 子 Image |
| **D 看错路线** | **否** | 用户箭头即本 Image；`HomeToJingLingVillage2` 是东郊地点键，非本案 |
| **E 章末被跳过** | **弱** | 用户已见章末面板+Map；本案是「图开了路线灰」 |

**侦探答 Q1**：本机未 Play 读内存列表；按代码因果，章末**不保证**有 `HomeToJingLingVillage` → 主因 **A（可叠 B）**。若 Play 证明列表已有仍灰 → 再查 C（现网不像）。

---

## 4. 与 0721 关系（正式撤销）

| 0721 决议 | 本案 |
|-----------|------|
| 只 `UnlockPlace`；**不做 UnlockRoad**；路线图另案 | **另案开做**：章末须 `UnlockRoad(HomeToJingLingVillage)` |
| 关卡钮可点 ≠ 路线贴图亮 | 再次确认：两套数据 |

→ **撤销** OPEN / 0721「本期不做 UnlockRoad」；关卡解锁与点选进村逻辑**保留不动**。

---

## 5. 推荐方案：**R3（R1 为主 + R2 保留）**

| 方案 | 内容 | 裁定 |
|------|------|------|
| **R1** | `UnlockChapterEndMapPlace`（开图**前**，与 UnlockPlace 同级）补 `UnlockRoad(PlaceName.HomeToJingLingVillage)`，经 `PlayerDataComponentGM`/`PlayerHandlerComponentGSM` | **必做**；Open 前写入则 OnOpen→ShowUnlockRoad 自动亮，**无需**再刷 |
| **R2** | 保留出门 `OnHomeScene1_GetMap`（幂等 Add） | **保留**；中途开地图也依赖它 |
| **R3** | R1+R2 | **推荐默认** |
| 仅勾 Prefab Active | 运行时仍被全关 | **否决** |

### 给施工员清单（6 条）

1. `ChapterEndFormLogic.UnlockChapterEndMapPlace`：在 `UnlockPlace` 成功路径旁调用 `UnlockRoad(PlaceName.HomeToJingLingVillage)`（`PlayerDataComponentGM.UnlockRoad`）。  
2. 改注释：撤销「本期不做 UnlockRoad」；写明路线贴图 ≠ 关卡点。  
3. 日志：`[MapSelect] 章末 UnlockRoad=HomeToJingLingVillage newlyAdded=…`。  
4. **不要**改 `ShowUnlockRoad` 语义；**不要**只改 Prefab Active。  
5. **禁止**章末自动 `LoadScene`；禁止动 `ButtonJingLingVillage` 进村行为。  
6. 验收：章末 Hierarchy 该 Image 非灰；存档列表含键；再开图仍亮；关卡可点进村不变。

**文件（主要 1）**：`Assets/Scripts/Game/GameRuntime/UI/FormLogic/ChapterEndPanel/ChapterEndFormLogic.cs`  
（可选核对出门调用仍在：`HomeScene1GoOutStoryCollider.cs` / 对话 Prefab，通常无需改。）

---

## 6. OPEN

| ID | 问题 | 默认 | 状态 |
|----|------|------|------|
| Q1 | 是否撤销 0721「不做 UnlockRoad」？ | **是**（本案正式开做） | ✅ 本报告决议 |
| Q2 | R1 / R2 / R3？ | **R3** | 待施工 |
| Q3 | `PlayerMapData` 空 Serialize 是否导致路不落盘？ | 另票加固 Parse/Serialize；本期靠章末 R1 兜底当次开图 | 待开票 |
| Q4 | 是否还要亮 `ImageAllRoad`？ | **否**（用户箭头为本 Image） | 降级 |

---

## 7. 侦探声明

未改代码 / Prefab / Git。证据链以 0721 决议 + `ShowUnlockRoad` 全关逻辑 + 章末仅 UnlockPlace 钉死主因 A。
