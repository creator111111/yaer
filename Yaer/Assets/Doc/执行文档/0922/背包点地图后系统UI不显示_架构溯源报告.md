# 背包点地图后系统 UI 不显示 · 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（**只读**，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 现象：背包点地图 → 关地图 → 再开**系统 UI 不显示**；附图含加载艺术图 + **`35 %`**  
> 入口：`ItemMap.OnClick`（关 `MenuPanel` → 关 `ItemShowPanel` → 开 `MapPanel`）  
> 提示词：`Assets/Doc/提示词/0922/背包点地图后系统UI不显示_架构侦探提示词.md`

---

## ① 结论一句话

**「系统 UI」主钉 = `MenuPanel`（ESC 系统菜单，含背包）**，不是章末图、不是地图路线。  
现网关图路径上 `MapFormLogic.OnClose` **会** `AllowOpenMenu(true)`，成对设计成立；ESC 被挡的唯一门闩是 `InputComponentGSM` 的 **`isOpenMenu || cantOpenMenu`**（有日志 `ESC ignored`）。  
**主因按概率分两层**（须用 ESC 日志 + 是否见 Loading 分裁）：

| 层 | 裁定 | 证据 |
|----|------|------|
| **C（附图强相关）** | 关图同时或之后进入了 **Loading / 换场**，`onStartLoadingSceneEvent → CantResponse` 把 `cantOpenMenu=true`，进度条停在约 35% 时菜单被挡且画面被盖 | 附图 35%；`LoadScene`/`LoadingPanel`；点精灵村 / ButtonHome 会开加载 |
| **A（纯关图无换场）** | `cantOpenMenu` 或 **`isOpenMenu` 仍 true**（菜单关了但 `OnMenuActive(false)` 未落地 / Map 未真正 OnClose）→ ESC 直接 ignored，菜单根本不 Open | `OnEscPressed` 门闩；`ItemMap` 只 `CloseUIForm(Menu)` 不显式清 flag |

次因：**D** — `ResumeGameHandle` 要求 `commonSfxCpn != null`，Pause 无此门闩 → 移动/半残，易被说成「系统没了」（菜单其实可能能开）。  
**否决**当主因：章末 `ChapterEnd`、地图未点亮、相机闪滑、未持 Map。

推荐：**先用 Console 钉死 A vs C**；最小修 **方案 A（+B 清 flag）** 保证关图必可 ESC；若坐实 Loading 残留则 **C**；对称 **D** Resume。

---

## ② 复现矩阵（代码推演；Play 须勾日志）

| 操作 | ESC 能开菜单？ | Console ESC 日志 | Loading/黑幕残留？ | 备注 |
|------|----------------|------------------|--------------------|------|
| 正常 ESC 开/关菜单（不碰地图） | **推演：能** | `ESC → OpenUIForm` | 否 | 基线 |
| 菜单→背包点 Map→**只关地图**（ESC/`allowEscapeClose`）→再 ESC | **应能**；若不能看 flag | `ignored`？ | 否 | **主路径**；验 A |
| 同上，关法不同（若仅有 ESC） | 同左 | 同左 | 否 | Map `allowEscapeClose: 1` |
| 地图点肯姆尼 / ButtonHome 触发换场中途 | **不能**（设计） | `cantOpenMenu=true` | **是（可停在 xx%）** | **对照附图 C** |
| 点未开放地点 → UnOpenTips | 地图仍开时 ESC 关图逻辑照旧 | — | 否 | Tips 另层 |
| 章末强制出图 | 另链 `ChapterEnd` | — | — | 非 ItemMap |

钉死字段（Play 时录）：`cantOpenMenu`、`isOpenMenu`、已 Open 的 UIForm（是否仍有 MapPanel/LoadingPanel/BlackPanel）。

---

## ③ 调用链 / flag 状态

### 开图（背包）

```
ItemMap.OnClick
  → CloseUIForm(MenuPanel)     // 应触发 MenuFormLogic.OnClose → OnMenuActive(false) → isOpenMenu=false
  → ClosePanel(ItemShowPanel)
  → if GetUIForm(MapPanel)==null → OpenUIForm(MapPanel, Middle)

MapFormLogic.OnOpen
  → AllowOpenMenu(false)       // cantOpenMenu=true  ★ ESC 开菜单被挡（故意）
  → PauseGameHandle()          // 禁移动
```

### 关图

```
Map allowEscapeClose=1 → OnReveal 订阅 CloseFormOnEsc
  → CloseUIForm(MapPanel)
MapFormLogic.OnClose
  → AllowOpenMenu(true)        // cantOpenMenu=false  ★ 应恢复
  → ResumeGameHandle() 仅当 commonSfxCpn!=null   ★ 不对称
```

### ESC 开菜单（系统 UI）

```
InputComponentGSM.OnEscPressed
  if (isOpenMenu || cantOpenMenu)
      log "ESC ignored…" ; return
  else OpenUIForm(MenuPanel, Top)
```

### 换场 / Loading（附图 35%）

```
点 ButtonJingLingVillage → LoadScene(…, stayAction: CloseMapPanel…)
  → onStartLoadingSceneEvent → CantResponse() → cantOpenMenu=true
点 ButtonHome → RestartNewGameFromProgress → 同类 Loading/CantResponse
LoadingFormLogic：假进度 2～3s，可停在约 35% 视觉帧
  → 完成 onEndLoadingSceneEvent → AllowResponse() → cantOpenMenu=false
若加载卡死 / 未抛完成 → cantOpenMenu 永久 true + 画面像附图
```

### `ItemMap` 二次点击缝

若 `GetUIForm(MapPanel) != null`（地图仍开着）：**只关菜单、不再 Open 地图**。菜单没了、地图仍锁 `cantOpenMenu` → 体感「点了地图后系统没了」；须 ESC 关地图（靠 Map 的 `CloseFormOnEsc`，与 InputGSM 同事件多播——InputGSM 先 ignored，Map 仍可关）。

### 对拍表

| 检查项 | 现网 | 风险 |
|--------|------|------|
| Map OnOpen/OnClose Allow 成对？ | ✅ false/true | 异常关（Destroy 无 OnClose）会漏 |
| Menu Close → `isOpenMenu=false`？ | OnClose 有 `OnMenuActive(false)` | ItemMap **未显式**清；依赖 Close 回调 |
| Resume 对称？ | ❌ 要 `commonSfxCpn` | 移动锁死 ≠ 菜单锁 |
| 附图 35% | Loading 假进度 | **C 优先证伪** |
| 店内 ESC | `SetAllowOpenMenu(false)` 故意 | 勿误修回开菜单 |

---

## ④ 方案对比与推荐

| 方案 | 做法 | 何时选 | 裁决 |
|------|------|--------|------|
| **A** | 关地图路径保证 `SetAllowOpenMenu(true)`；关菜单后强制 `OnMenuActive(false)`；关图失败也兜底 | ESC ignored 坐实 | **P0** |
| **B** | `ItemMap`：关菜单后显式清 flag；`GetUIForm!=null` 时仍 `Refocus/Open` 地图，禁止「只关菜单」 | 竞态 / 二次点击 | **P0 同票** |
| **C** | 未真正换场则杀 Loading/Black；卡进度修完成回调 | 附图卡 xx% 坐实 | **条件 P0** |
| **D** | `OnClose` Resume 与 Pause 对称（去掉过严 null） | 仅移动锁、菜单能开 | **P1** |
| **E** | 菜单已 Open 但不可见：Group/排序/Alpha | Hierarchy 有 Form 看不见 | 末选 |

### 必答

1. **主因一句话**  
   ESC 开不出菜单 = `cantOpenMenu` 或 `isOpenMenu` 仍为 true；附图路径高度疑似 **Loading+CantResponse**；纯关图路径优先查 **flag 未清 / Map 未 OnClose**。

2. **最小文件**  
   `ItemMap.cs`、`MapFormLogic.cs`、必要时 `InputComponentGSM.cs`（诊断日志已有）；Loading 仅当 C 坐实。

3. **验收**  
   关图后再 ESC **必出** MenuPanel；店内 ESC 仍离店；章末开图不回潮；正当进村 Loading 走完后 ESC 可用。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 预期 |
|--------|------|
| `…/BagPack/ItemMap.cs` | 关菜单后显式 `OnMenuActive(false)`；地图已开则确保可见/不只关菜单 |
| `…/Map/MapFormLogic.cs` | OnClose 兜底 `AllowOpenMenu(true)`；Resume 对称（D） |
| `…/InputComponentGSM.cs` | 可选：关图后断言日志；勿改店内语义 |
| Loading / 换场 | 仅 C 坐实再动 |

施工说明：`Assets/Doc/施工说明/0922/背包点地图后系统UI不显示_施工说明.md`

---

## ⑥ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | ESC 开菜单 → 背包点地图 → **只关地图** → 再 ESC | **菜单显示且可操作**；无 `ESC ignored`（或仅关图当帧） |
| 2 | 重复开图关图 ESC ×3 | 稳定 |
| 3 | 开图 → 点肯姆尼正当进村 | Loading 走完；进村后 ESC 可用 |
| 4 | 未持地图时 ESC/背包 | 正常 |
| 5 | Village_Shop 店内 ESC | **仍离店**，不误开菜单 |
| 6 | 章末出图进村 | 不回潮 |

---

## ⑦ 风险与回滚

| 风险 | 处置 |
|------|------|
| 店内误开菜单 | 保持 Shop `SetAllowOpenMenu(false)`；勿在 AllowResponse 无条件冲掉店锁 |
| 章末地图 | 不改 Unlock/进村；只动 Allow 对称 |
| 换场中误开菜单 | 加载中 `CantResponse` 应保留 |
| 只修 D 不修 A | 能动了但仍 ESC ignored | 先 A/B |

**回滚**：还原 ItemMap / Map OnClose；保留商店 ESC 契约。
