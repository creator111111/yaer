# 门口对白结束 → Loading 进村长家 → 自动播「继续对话」— 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】只读定方案（**本阶段未改代码 / 场景 / Prefab**）  
**Unity**：2020.3.48f1  
**产品**：屋外 `Village_村长家门口初次对话` 播完 →（既有）Loading 进 `Village_Chief_House` → **进屋后自动** `TriggerStory("Village_村长家继续对话")`（无需点 E / 点 NPC）  
**提示词**：`提示词/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构侦探提示词.md`  
**上游依赖（已落地）**：`执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_*` · `ChiefNearDoorStoryTrigger.OnStoryFinished` → `LoadSceneWithLoadingPanel`  
**关联**：`Village_KenMuNiStart` / `Village_ShopStart` 进场景播对白样板 · `StoryTriggerCountData` · `DialoguePath` · Prefab 缺失教训（0831）

---

## 沟通摘要

### ① 结论一句话

**挂点 C1（`Village_Chief_HouseSceneManager.OnEnterScene`）+ 门闩 F1（门口戏已用 ∧ 续聊未用）自动 `TriggerStory("Village_村长家继续对话")`；P0 须先 Setup 落盘续聊 Prefab（磁盘现缺，否则必再现「加载资源失败」）；保留既有 Loading 进屋，勿跨场景 Trigger、勿混晚宴台本。**

### ② 原因（通俗）

进村长家这条 Loading 路已经通了，但进屋后没人喊「接着聊」——室内管理器 `OnEnterScene` 只打了 Debug。  
续聊的 CSV 和对话树都在，缺的是对话 Prefab 成品壳；不补壳就 Trigger，会和门口戏当初一样报加载失败。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 屋外三人戏完整播完 | |
| 2 | 结束后自动 Loading → `Village_Chief_House`，落点正确 | |
| 3 | **进屋后无需操作**即开始 `Village_村长家继续对话` | |
| 4 | 续聊可完整播完；Console **无**该 Prefab「加载资源失败」 | |
| 5 | 同档再进村长家：**不**重复播续聊 | |
| 6 | `House_Chief` 手动进：门口已播且续聊未播时 **补播一次**；已播过则静默 | |
| 7 | `House_Tree` / 晚宴台本未被误改 | |

### ④ 程序补充

见下文 §①～§⑩。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 产品全链 | 门口戏结束 →（既有）Loading 进屋 → **自动**续聊 |
| 挂点 | **C1** · `Village_Chief_HouseSceneManager.OnEnterScene` → `TryTriggerChiefContinueOnce` |
| 门闩 | **F1** · `CheckStoryUsed(门口初次对话)` ∧ `!CheckStoryUsed(继续对话)` |
| Story 名 | 钉死 **`Village_村长家继续对话`**（与 CSV / Generated / Prefab 同名） |
| Prefab | **❌ 磁盘缺失** → **P0 Setup**（复用门口 Setup 流水线改名） |
| 立绘 | **三人**（村/雅/古；CSV + Generated 已三角色） |
| Loading 进屋 | **复用**；禁止拆掉 / 改回纯黑幕主表现 |
| 晚宴 | **`Village_村长家晚宴对白台本` ≠ 本期续聊** |

---

## ② 现网缺口

### A. 已通（上游 0831）

```
KenMuNi1 · Npc_Chief
  → ChiefNearDoorStoryTrigger：BlackPanel → TriggerStory(门口初次对话)
  → OnStoryFinished → LoadSceneWithLoadingPanel(Village_Chief_House)
  → blackFade:false；EnterPos lastScene=Village_KenMuNi1 → EnterFrom_Village
```

磁盘核实：`ChiefNearDoorStoryTrigger.cs` L231–259 已实现；`LoadSceneComponentGSM.LoadSceneWithLoadingPanel` 已存在。

### B. 缺口（本期）

| 位置 | 现状 | 缺口 |
|------|------|------|
| `Village_Chief_HouseSceneManager.OnEnterScene` | 仅 `SetNowPlace` + Debug | **无** `TriggerStory(继续对话)` |
| 续聊 Prefab | 见 §③ | **未落盘** → Trigger 必失败 |

**禁止理解成**：只进屋不播续聊；或续聊仍挂门外 / 门上点 E。

---

## ③ 续聊资源三态

| 资源 | 路径 | 磁盘 |
|------|------|------|
| CSV | `Assets/Dialog/Village_村长家继续对话.csv` | ✅（村 Face1～3 / 雅 / 古 Happy） |
| Generated 树 | `Assets/GameRes/DialogueTrees/Generated/Village_村长家继续对话.asset` | ✅（Actor：村长 / 雅尔 / 古莎） |
| Prefab | `Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab` | ❌ **缺失** |

**加载契约**（与门口戏同一条）：

```
TriggerStory("Village_村长家继续对话")
  → DialoguePath.GetPath
  → Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab
  → ResMgr.LoadAsset → 文件不存在 →「加载资源失败」
```

**对照**：门口戏 0831 根因 H1 即 Prefab 未落盘；续聊现状同构。

**晚宴台本**（勿混用）：

| 资源 | 存在 | 本期 |
|------|------|------|
| `Village_村长家晚宴对白台本` CSV / Generated | ✅ | ❌ 禁止当续聊 |

---

## ④ 挂点与门闩

### 挂点

| 方案 | 做法 | 裁定 |
|------|------|------|
| **C1 · Chief_House GSM `OnEnterScene`** | 对齐 `TryTriggerVillageStartStoryOnce` / ShopStart 兜底 | ✅ **采用** |
| C2 · Loading `callBack` 里 Trigger | 场景 / GSM / EnterPos 可能未 Ready | ❌ 竞态 |
| C3 · 室内 `SimpleStoryTrigger` Enter | 落点漂移漏播；「自动」弱 | ❌ |
| C4 · 门口图末跨场景 Trigger 室内名 | 未进屋就播室内 Prefab | ❌ |

**原则**：门口 `ChiefNearDoorStoryTrigger` **只**负责「结束 → Loading 进屋」；续聊 **只**挂室内 GSM，不跨场景直接 `TriggerStory(继续对话)`。

### 门闩

| 门闩 | 条件 | 裁定 |
|------|------|------|
| **F1** | `CheckStoryUsed("Village_村长家门口初次对话")` ∧ `!CheckStoryUsed("Village_村长家继续对话")` | ✅ **采用** |
| F2 | 门口 `onStoryEnd` 置 Pending，进屋消费 | 备选；多字段，仅当产品要「严格只自动链」 |
| F3 | 每次进 Chief_House 都播 | ❌ 产品单次 |

**存档键**（既有 `StoryTriggerCountData`，无需新字段）：

| 键 | 何时写入 |
|----|----------|
| `Village_村长家门口初次对话` | 门口戏 `OnStoryEnd`（进屋前已记） |
| `Village_村长家继续对话` | 续聊 `OnStoryEnd` |

**伪码（施工参照）**：

```csharp
// Village_Chief_HouseSceneManager
const string DoorStory = "Village_村长家门口初次对话";
const string ContinueStory = "Village_村长家继续对话";

bool ShouldPlayChiefContinue()
{
    var counts = GetArchiveData<StoryTriggerCountData>();
    if (counts == null) return false; // 无档不播；或按团队惯例 counts==null 当未用
    return counts.CheckStoryUsed(DoorStory) && !counts.CheckStoryUsed(ContinueStory);
}

void TryTriggerChiefContinueOnce()
{
    if (!ShouldPlayChiefContinue()) return;
    var storyGsm = GetModule<StoryComponentGSM>();
    if (storyGsm == null || storyGsm.HasRunningStory) return;
    bool started = storyGsm.TriggerStory(ContinueStory);
    Debug.Log(started
        ? "[ChiefContinue] OnEnterScene TriggerStory " + ContinueStory
        : "[ChiefContinue] TriggerStory 未启动（Prefab 可能缺失）");
}
```

> 注：`VillageStart` 对 `counts==null` 当「未用可播」；续聊依赖「门口已用」，无档时宜 **不播**（避免裸进房误播）。施工按上表；若团队统一 null=未用，须同时要求门口键存在——F1 已隐含。

---

## ⑤ 时序 / 遮罩

### 期望序列

```
门口戏 OnStoryEnd（记档门口键）
  → LoadSceneWithLoadingPanel(Chief_House)   // Loading Top 假读条 2～3s
  → LoadScene(..., blackFade:false)
  → GSM Ready → OnBlackFadeEnd → OnEnterScene
       → TryTriggerChiefContinueOnce → TriggerStory(继续对话)  // 异步开壳
  →（Loading 仍盖住画面）壳 Open / 立绘就绪
  → Loading 关 → 玩家看到对白（或极短室内 + 对白跟上）
  → 续聊结束 → OnStoryEnd 记档续聊键 → 还控
```

### 与进村开场差异

| | 进村 `KenMuNiStart` | 本期续聊 |
|--|---------------------|----------|
| 转场主表现 | BlackPanel | **LoadingPanel**（无系统黑幕主控） |
| 露景风险 | 高 → 曾用 `TryDeferBlackFadeForCover` | Loading 盖住期间可提前 Trigger，**天然遮罩** |
| 本期 | — | **不**强行上 DeferBlack；**Q3：先可接受短露景** |

**裁定**：在 `OnEnterScene` **立刻** Trigger（趁 Loading 未关）；勿等 Loading 关完再 Trigger（关完再开壳反而更容易露景）。若验收穿帮，再对齐进村 A′（壳 Ready 前压一层）——单列 P1，不进本期最小集。

**C2 否决理由**：`LoadSceneWithLoadingPanel` 的 `callBack` 只保证 Loading UI 已 Open，随后 `LoadScene` 时旧 GSM 已 ShutDown，新 GSM 尚未 Ready——在 callBack 里 Trigger 室内名会错场景 / 丢模块。

---

## ⑥ 与手动门 `House_Chief` 关系

| 进房方式 | F1 行为 |
|----------|---------|
| 门口戏结束自动 Loading | 门口已用、续聊未用 → **自动播** |
| 点 `House_Chief`（门口已播、续聊未播，如 Prefab 曾失败） | **补播一次** |
| 点 `House_Chief`（续聊已播完） | **不播**，正常进屋还控 |
| 未播门口戏直接点门 | **不播**续聊（门闩要求门口已用） |

`House_Chief` **保留**（0831 已决议）；本期不改门目标场景。  
`House_Tree` / `Village_TreeHouseLock`：**禁止**误接。

若产品改口「仅自动链播、手动门永不播续聊」→ 改 **F2**（Pending 旗），见 OPEN Q2。

---

## ⑦ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | **Setup** 落盘 `Prefabs/Dialogue/Village_村长家继续对话.prefab`：复用 `VillageChiefDoorDialogueSetupEditor` 流水线（拷壳→改 Target/CSV/AssetBaseName→Import）；壳可用门口成品或 `Village_KenMuNiStart` | **P0** |
| 2 | `Village_Chief_HouseSceneManager`：常量 + `ShouldPlay` / `TryTriggerChiefContinueOnce`；`OnEnterScene` 调用 | **P0** |
| 3 | 保持 `ChiefNearDoorStoryTrigger` Loading 进屋；**不**在该组件跨场景 Trigger 续聊 | — |
| 4 | **不改**晚宴台本 / `House_Tree` / 进屋目标 SceneName | — |
| 5 |（可选）Setup 独立菜单 `Tools/Dialogue/Setup Village 村长家继续对话 Prefab`；或参数化门口 Setup | P0 交付手段 |
| 6 |（穿帮再做）Loading 关后仍露景 → P1 二次遮罩 | P1 |

**不改**：`LoadSceneWithLoadingPanel` 主路径；Update 堆业务；新建 Pending 存档字段（F1 足够）。

---

## ⑧ 验收清单

- [ ] 屋外门口三人戏可完整播完  
- [ ] 结束后自动 Loading → `Village_Chief_House`，落点 `EnterFrom_Village`  
- [ ] 进屋后无需操作即自动开始 `Village_村长家继续对话`  
- [ ] 续聊完整可播；Console 无 `…/Village_村长家继续对话.prefab`「加载资源失败」  
- [ ] 同档再进村长家：不重复播续聊  
- [ ] `House_Chief`：门口已播且续聊未播 → 补播一次；已播过 → 不播  
- [ ] `House_Tree` / `Village_村长家晚宴对白台本` 未被误改  
- [ ] Console 可见 `[ChiefContinue] OnEnterScene TriggerStory …`（建议）

---

## ⑨ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 挂点 C1/C2/C3？ | **C1** | ✅ 侦探拍板 |
| Q2 | 手动进门是否补播？ | **F1 补播一次**；产品只要自动链则改 F2 | ⏳ 待产品确认 |
| Q3 | Loading 关→壳 Ready 要二次遮罩？ | **先可接受短露景**；穿帮再 A′ | ⏳ |
| Q4 | 续聊 Prefab 三人立绘？ | **是**（CSV/Generated 已三角色） | ✅ |
| Q5 | Prefab Setup：新建菜单 vs 复用门口？ | **最小复用门口流水线改名** | ✅ |

---

## ⑩ 程序补充（速查）

| 锚点 | 用途 |
|------|------|
| `ChiefNearDoorStoryTrigger.OnStoryFinished` | 门口结束 → Loading 进屋（已有） |
| `LoadSceneComponentGSM.LoadSceneWithLoadingPanel` | 进屋主表现；`blackFade:false` |
| `Village_Chief_HouseSceneManager.OnEnterScene` | **本期挂点** |
| `Village_KenMuNiSceneManager.TryTriggerVillageStartStoryOnce` | 单次 Trigger 样板 |
| `Village_ShopSceneManager.TryTriggerShopStartStoryOnce` | 进场景兜底样板 |
| `StoryTriggerCountData.CheckStoryUsed` / `OnStoryTriggered` | 门闩；**结束时**记次 |
| `StoryComponentGSM.TriggerStory` | 唯一合法开剧情入口 |
| `DialoguePath.GetPath` | `Assets/GameRes/Prefabs/Dialogue/{名}.prefab` |
| `VillageChiefDoorDialogueSetupEditor` | 门口 Setup；续聊 Setup 应克隆改路径 |
| `SceneName.Village_Chief_House` | 目标场景常量 |

**一句话合并上下游**：0831 只负责「门口戏完 → Loading 进屋」；0901 只补「进屋后 C1+F1 自动续聊 + Prefab 可加载」——二者职责分离、顺序衔接。
