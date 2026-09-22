# Cursor Agent Prompt · 背包点地图关闭后 · 系统 UI（菜单）打不开

> **角色**：【架构侦探】只读；查「背包用地图 → 关地图 → 再开系统 UI 不显示」  
> **日期**：2026-09-22  
> **现象（用户原话）**：背包里点地图后关了，再开**系统 UI 不显示**  
> **用户附图**：加载屏艺术图 + 底部 **`35 %`**（须裁定：是否关图后卡在 Loading/黑幕盖住，还是仅示意进游戏；**勿当唯一真相**）  
> **入口预扫**：`ItemMap.OnClick`（关 `MenuPanel` → 关 `ItemShowPanel` → 开 `MapPanel`）  
> **不是**：改章末出图/`ChapterEndFormLogic` 进村链；改地图路线点亮；改商店 ESC 离店；改换场相机闪滑  

把下面「侦探」整段交给 Agent。没钉死「系统 UI」指什么、没对拍 `cantOpenMenu` / `isOpenMenu` / Loading 残留之前，不要施工。

---

## 提示词助手预梳理（须证伪）

### 产品钉死

| 项 | 要求 |
|----|------|
| 关地图后 | ESC / 系统菜单应能再开，界面可见可用 |
| 日常 | 未点地图时 ESC 开菜单仍正常 |
| 边界 | 勿破坏地图选关进村、章末开图、店内 ESC=离店 |

### 「系统 UI」候选（侦探必须钉死用户路径）

| 候选 | 操作 | 现网入口 |
|------|------|----------|
| **A. MenuPanel（ESC 菜单/含背包）** | 关地图后再按 ESC | `InputComponentGSM.OnEscPressed` → `OpenUIForm(MenuPanel)` |
| B. 局内 HUD（血条/快捷栏） | 关地图后 HUD 没了 | 另查 Player/HUD Form |
| C. 被 Loading/黑幕盖住 | 画面像附图卡在 xx% | `LoadScene` / BlackPanel / Loading UI 未关 |

默认按 **A** 主查；附图 **35%** 必须单独证伪是否 C。

### 现网可疑链（预扫，勿当终裁）

```
菜单开背包 → 点道具 Map
  ItemMap.OnClick
    → CloseUIForm(MenuPanel)
    → ClosePanel(ItemShowPanel)
    → OpenUIForm(MapPanel) + PlayerMapData

MapFormLogic.OnOpen
  → AllowOpenMenu(false)   // cantOpenMenu=true → ESC 被挡
  → PauseGameHandle()

MapFormLogic.OnClose
  → AllowOpenMenu(true)
  → ResumeGameHandle()（现网还要求 commonSfxCpn != null，须复核是否漏恢复）
```

假说：

1. **`cantOpenMenu` 关图后仍 true**（OnClose 未跑 / 异常提前 return / 其它 Form 又锁）→ ESC 日志 `ESC ignored`  
2. **`isOpenMenu` 仍 true**：`ItemMap` 关菜单后 `MenuFormProxy.OnMenuActive(false)` 未落到 → ESC 仍被 `isOpenMenu` 挡  
3. **菜单其实 Open 了但看不见**：Group/排序被 Map 或 Loading 盖住；Alpha=0；Pause/TimeScale  
4. **Loading 残留（附图）**：误点关卡触发换场后关图，Loading 停在 35%；或黑幕未 `CloseFormFade`  
5. **Resume 条件过严**：`OnClose` 仅 `commonSfxCpn != null` 才 Resume，移动/输入半残，体感「系统没了」

### 强制排除

| 勿当主因 | 理由 |
|----------|------|
| 章末 `ChapterEnd` 开图 | 入口不同（非背包 `ItemMap`） |
| 地图路线未点亮 | 0914 另案 |
| 换场相机闪滑 | 另票 |
| 未持有 Map 道具 | 用户已能点开地图 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：查清「背包点地图 → 关闭地图 → 再开系统 UI 不显示」的根因，给出最小修复推荐。

### 必读 / 必搜

@Assets/Scripts/Game/GameRuntime/BagPack/ItemMap.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Map/MapFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormProxy.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/InputComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Base/BaseUIFormLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs

Grep：`AllowOpenMenu` / `SetAllowOpenMenu` / `cantOpenMenu` / `isOpenMenu` / `OnMenuActive` / `ItemMap` / `MapPanel` / `CloseUIForm` / `ItemShowPanel` / Loading 进度 UI。

### A. 复现矩阵（必须填）

| 操作 | ESC 能开菜单？ | Console ESC 日志 | Loading/黑幕残留？ | 备注 |
|------|----------------|------------------|--------------------|------|
| 正常 ESC 开/关菜单（不碰地图） | ？ | ？ | ？ | 基线 |
| 菜单→背包点 Map→**只关地图**（不点关卡）→ESC | ？ | ignored？ / Open？ | ？ | **主路径** |
| 同上但关地图用不同方式（ESC / 返回钮 / 点空白） | ？ | ？ | ？ | 关法是否影响 |
| 地图上点肯姆尼等触发换场中途 | ？ | ？ | 卡 xx%？ | 对照附图 |
| 章末强制出图再关（若可关） | ？ | ？ | ？ | 对照非 ItemMap |

钉死：「系统 UI」= MenuPanel / HUD / 被 Loading 盖。录 `cantOpenMenu`、`isOpenMenu`、当前已 Open 的 UIForm 列表。

### B. 对拍调用链

1. `ItemMap.OnClick`：`CloseUIForm(Menu)` 是否同步触发 `MenuFormLogic.OnClose` → `proxy.OnMenuActive(false)` → `isOpenMenu=false`？若异步，与开 Map 是否竞态？  
2. `MapFormLogic.OnOpen/OnClose`：`AllowOpenMenu(false/true)` 是否成对？异常路径（Shutdown、换场 stayAction 关图）是否漏 `true`？  
3. `OnEscPressed`：`isOpenMenu || cantOpenMenu` 谁为 true？Console 是否已有 `ESC ignored`？  
4. `ResumeGameHandle` 是否因 `commonSfxCpn==null` 跳过？与 Pause 是否不对称？  
5. 附图 35%：对应哪个 Loading Prefab/脚本？关背包地图是否会误触 `LoadScene`？  
6. `ItemMap` 若 `GetUIForm(MapPanel)!=null` 则**不再 Open**——关不净时二次点击行为？

### C. 方案对比

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A** | 关地图路径保证 `SetAllowOpenMenu(true)` + 强制 `OnMenuActive(false)` 清 `isOpenMenu` | ESC ignored 坐实 |
| **B** | `ItemMap` 关菜单与开地图顺序/回调：先确保菜单 Active 事件落地再开图，或开图前显式清 flag | 竞态坐实 |
| **C** | 修 Loading/黑幕未关；关图时若未换场则杀 Loading | 附图卡进度坐实 |
| **D** | `OnClose` Resume 与 Pause 对称（去掉过严 null 门闩） | 仅输入锁死、菜单其实能开 |
| **E** | 菜单 Open 但不可见：修 Group/覆盖 | Hierarchy 有 Form 但看不见 |

必答：主因一句话；最小文件路径；如何验收「关图后再 ESC 必出菜单」且「店内 ESC 离店 / 章末开图」不回潮。

### D. 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | ESC 开菜单 → 背包点地图 → 关地图 → 再 ESC | **菜单显示且可操作** |
| 2 | 重复 3 次开图关图再 ESC | 仍稳定 |
| 3 | 开图后点关卡进村（正当换场） | Loading 正常走完；进村后 ESC 可用（场景规则内） |
| 4 | 未持地图时 ESC/背包 | 回归正常 |
| 5 | Village_Shop 店内 ESC | 仍离店，不误开菜单 |
| 6 | 章末出图进村 | 不回潮 |

### E. 报告结构

① 结论（系统 UI=？；主因）  
② 复现矩阵  
③ 调用链 / flag 状态  
④ 方案对比与推荐  
⑤ 文件路径级  
⑥ 验收表  
⑦ 风险（商店 ESC、章末地图）

写入：`Assets/Doc/执行文档/0922/背包点地图后系统UI不显示_架构溯源报告.md`
```

---

## 施工员（侦探闭环后再复制）

```
@Assets/Doc/执行文档/0922/背包点地图后系统UI不显示_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/BagPack/ItemMap.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Map/MapFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/InputComponentGSM.cs

你是【施工员】。按报告最小修「背包点地图关闭后系统 UI 不显示」。

约束：
- 勿改章末进村 / 地图 Unlock 逻辑（除非报告证明必须且最小）
- 勿破坏 Village_Shop「ESC=离店、不开菜单」
- 关地图必须恢复可开菜单；若动 Loading，仅清「未真正换场」的残留
- 写入：`Assets/Doc/施工说明/0922/背包点地图后系统UI不显示_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ 主路径打 ESC 日志 + flag  
2. 分清 A（菜单锁）/ C（Loading 盖）后再施工  
3. 复制「施工员」→ 按验收表回归商店 ESC
