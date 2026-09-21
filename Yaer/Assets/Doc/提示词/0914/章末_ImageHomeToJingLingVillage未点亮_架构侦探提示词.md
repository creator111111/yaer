# Cursor Agent Prompt · 章末地图路线 `ImageHomeToJingLingVillage` 未点亮

> **角色**：先【架构侦探】只读核实路线贴图开关链与存档；拍板后【施工员】最小改  
> **日期**：2026-09-14  
> **场景 / UI**：章末流程打开的 `MapPanel(Clone)`（与 `ChapterEndPanel(Clone)` 同层 `UI Group - Top`）  
> **目标物体（用户 Hierarchy 截图）**：  
> `MapPanel(Clone)` → `Fg` → `Road` → **`ImageHomeToJingLingVillage`**（Hierarchy 灰色 = **运行时 inactive**）  
> **现象（用户）**：在 **第一章结束 / 序章结束**（见章末面板）时，这条「家→精灵村」路线图 **应该打开（点亮）**，现在是关闭的  
> **产品期望（钉死）**：章末出地图时，`ImageHomeToJingLingVillage` **应为 Active**（路线贴图可见）；关卡钮 `ButtonJingLingVillage` 可点逻辑保持现网（`UnlockPlace`），**不要**因此改成自动进村  
> **不是**：改村场景；改 `ButtonJingLingVillage` 进村黑幕；手改 Prefab 默认勾选当唯一修复（运行时 `ShowUnlockRoad` 会先全关再按存档开）；把 `Design/Interaction` 之类非地图物体当目标  
> **历史对照（必读，勿当唯一真相）**：  
> - 0721「序章结束恢复地图选肯姆尼」：**只做了 `UnlockPlace(JingLingVillage)`**，文档写明 **`UnlockRoad(HomeToJingLingVillage)` 本期不做、路线图另案**  
> - `ChapterEndFormLogic.UnlockChapterEndMapPlace` 注释仍写「本期不做 UnlockRoad」  
> - 本案很可能就是那份「另案」——侦探须核实是否仅缺章末 `UnlockRoad`，还是出门拿地图时就没写入存档  
> **报告落盘**：`Assets/Doc/执行文档/0914/章末_ImageHomeToJingLingVillage未点亮_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 第一章（序章）结束弹出世界地图时，家到精灵村那条路线图应该亮着，现在 Hierarchy 里是灰的关着。  
> 关卡按钮能不能点是另一回事；这次要查的是 **Road 底下那张 Image** 为啥没开。

### 名词拆开（避免再混）

| 名称 | 是什么 | 谁控制 |
|------|--------|--------|
| **`ImageHomeToJingLingVillage`** | 地图上「家→精灵村」**路线贴图** | `MapFormLogic.ShowUnlockRoad` + 存档 `PlayerMapData` 已解锁道路列表 |
| **`ButtonJingLingVillage`** | 肯姆尼/精灵城**关卡点**能否点 | `ShowUnlockPlace` + `UnlockPlace(JingLingVillage)`（0721 已做） |
| **`SelectPlaceLight`** | 高亮/可选中某钮 | **不**负责打开路线 Image |
| **`ImageAllRoad`** | 另一张全路线图（若有） | 勿与本案 Image 混淆，侦探注明现网是否参与章末 |

约定：`ShowUnlockRoad` 用键 **`Image` + roadName`**，故存档道路名应为 **`HomeToJingLingVillage`**（`PlaceName.HomeToJingLingVillage`），对应物体名正好是 `ImageHomeToJingLingVillage`。

### 现网链路（预扫）

```
MapPanel.OnOpen
  → ShowUnlockPlace()          // 关卡钮
  → ShowUnlockRoad()           // 先把 roadImageDic 全部 SetActive(false)
                               // 再按 GetUnlockRoad() 打开 Image{roadName}

路线写入存档的已知入口：
  HomeScene1GoOutStoryCollider.OnHomeScene1_GetMap
    → UnlockRoad(PlaceName.HomeToJingLingVillage)

章末开图：
  ChapterEndFormLogic.OpenMap
    → UnlockChapterEndMapPlace()  // 仅 UnlockPlace(JingLingVillage)，注释写明不做 UnlockRoad
    → OpenUIForm(MapPanel)
```

Prefab `MapPanel` 上该 Image **默认 `m_IsActive: 1`**；运行时变灰，多半是 **`ShowUnlockRoad` 全关后，存档列表里没有对应 road**。

### 「关闭」嫌疑优先级

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A（高优先）** | 0721 刻意不做章末 `UnlockRoad`；若出门 `GetMap` 未写过 / 档丢了，章末开图必关 | Play 章末后查 `PlayerMapData.GetUnlockRoad()` 是否含 `HomeToJingLingVillage`；Console 有无拿地图解锁日志 |
| **B** | 出门剧情 `OnHomeScene1_GetMap` 未触发或 Unlock 失败 | 溯源 Home 出门拿地图对话是否调用到该方法 |
| **C** | `roadImageDic` 键名 / 子物体收集漏了该 Image | 对照 `MapFormLogic` 如何填 `roadImageDic`；键是否必须 `ImageHomeToJingLingVillage` |
| **D** | 用户要的是别的路线（如东郊 `HomeToJingLingVillage2`）但 Hierarchy 指的是本 Image | 以用户箭头为准；若产品要另一条，写入 OPEN |
| **E** | 章末 `chapterId` / 存档已播过导致异常开图顺序 | 对照 0722 章末链；本案以「图已开、路线灰」为主，勿先当成章末被跳过 |

### 复现 / 验收矩阵

| 操作 | 期望 |
|------|------|
| 正规：出门拿地图 → … → 拉普路西右缘章末 → 章末标题 → 地图 | `ImageHomeToJingLingVillage` **Active**；`ButtonJingLingVillage` 可点（现网） |
| 仅查存档：章末当下 `GetUnlockRoad()` | 应含 `HomeToJingLingVillage`（修完后至少如此） |
| 点肯姆尼 | 仍走现网进村，**不**自动 LoadScene |
| Prefab 默认勾选 | 可保持；**不能**只靠 Prefab Active 当修复 |

### 侦探须回答

1. 章末当下存档 `GetUnlockRoad()` 是否已有 `HomeToJingLingVillage`？没有 → 主因偏 A/B；有但仍灰 → 主因偏 C。  
2. `ShowUnlockRoad` 全关再开逻辑是否覆盖本案物体？  
3. 推荐最小修：  
   - **R1**：章末 `UnlockChapterEndMapPlace`（或紧挨开图处）**补** `UnlockRoad(PlaceName.HomeToJingLingVillage)`，再依赖 `OnOpen → ShowUnlockRoad`；若 Map 已 Open，可能需再刷一次路线。  
   - **R2**：只修出门 `GetMap` 写入（若章末前本应已有）。  
   - **R3**：R1+R2 双写（幂等 AddUnlockRoad）。  
4. 与 0721「本期不做 UnlockRoad」关系：本案是否正式撤销该决议？报告写清。  
5. 施工清单 3～8 条 + 验收句。

### 必读

1. `Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md`  
2. `MapFormLogic.ShowUnlockRoad` / `ShowUnlockPlace` / `OnOpen`  
3. `ChapterEndFormLogic.UnlockChapterEndMapPlace` / `OpenMap`  
4. `PlayerMapData.AddUnlockRoad` / `GetUnlockRoad`；`PlayerDataComponentGM.UnlockRoad` / `PlayerHandlerComponentGSM.UnlockRoad`  
5. `HomeScene1GoOutStoryCollider.OnHomeScene1_GetMap`  
6. `Assets/Doc/执行文档/7月/0721/序章结束_恢复地图选肯姆尼_架构溯源与施工执行说明.md`（UnlockRoad 另案）  
7. Prefab：`Assets/GameRes/Prefabs/UI/MapPanel.prefab` → `Road/ImageHomeToJingLingVillage`  
8. 用户 Hierarchy 截图 + 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / Prefab / 存档 / Git  
- 禁止把「关卡钮可点」当成「路线图已亮」结案  
- 禁止章末加自动 `LoadScene` 进村  

### 侦探输出

1. 溯源报告 → 文首落盘路径  
2. 结论一句话 + 主因字母 + 推荐 R1/R2/R3  
3. 给施工员的最小改清单  

---

## 【施工员】提示词（侦探拍板后整段交给 Agent）

> **角色**：【施工员】  
> **前置**：已读 `Assets/Doc/执行文档/0914/章末_ImageHomeToJingLingVillage未点亮_架构溯源报告.md`  
> **目标**：章末出 `MapPanel` 时 `ImageHomeToJingLingVillage` Active；不破坏 0721 关卡解锁与点选进村  
> **施工说明落盘**：`Assets/Doc/施工说明/0914/章末_ImageHomeToJingLingVillage未点亮_施工说明.md`

### 默认按报告推荐（常见为 R1 或 R3）

1. 在章末解锁关卡处（与 `UnlockPlace` 同级、开图前或 Open 回调内）调用 **`UnlockRoad(PlaceName.HomeToJingLingVillage)`**（经现有 `PlayerDataComponentGM` / `PlayerHandlerComponentGSM`，禁止旁路直接改 UI）。  
2. 若 `MapPanel` 已打开且不会再次 `OnOpen`：补一次刷新路线显示（抽 `ShowUnlockRoad` 为可调、或关闭重开 Map——以报告最小方案为准）。  
3. 更新 `UnlockChapterEndMapPlace` 注释：撤销「本期不做 UnlockRoad」；说明路线贴图与关卡点是两套数据。  
4. 日志建议：`[MapSelect] 章末 UnlockRoad=HomeToJingLingVillage newlyAdded=…`  
5. **禁止**：只改 Prefab `m_IsActive`；禁止改 Interactive 全局；禁止自动进村；禁止动村宝箱等无关案。  

### 完工自检

- [ ] 章末地图：`ImageHomeToJingLingVillage` 亮（Hierarchy 非灰）  
- [ ] `ButtonJingLingVillage` 仍可点；点选进村行为不变  
- [ ] 存档 `GetUnlockRoad` 含该键；再次开图仍亮  
- [ ] 施工说明已落盘  

---

## 用户怎么用

1. Agent 先跑侦探段 → 出溯源报告（重点打存档道路列表 + `ShowUnlockRoad`）。  
2. 确认 R1/R2/R3 后跑施工员段。  
3. 走一遍：出门拿地图 → 章末出图，看路线是否亮。  
