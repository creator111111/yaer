# Village_Shop — ESC 退出商店回村（保持进店落点）· 不再开菜单 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Shop` ↔ `Village_KenMuNi1`

关联提示词：`Assets/Doc/提示词/0829/Village_Shop_ESC退出商店回村_架构侦探提示词.md`  
关联旧口径：`0713/Village_Shop_ESC呼出菜单…`（**店内 ESC→菜单 本期作废**）· `0713/Door_Shop→Village_Shop…`  
技术通则：`Doc/技术文档/场景相关/场景切换.md`

---

## ① 结论一句话

**推荐方案 A（店 GSM 接管 ESC）+ 门卫关闭：`SetAllowOpenMenu(false)` 并改掉 `ShopFormLogic.OnOpen` 的放行，使 `InputComponentGSM` 忽略 ESC 开菜单；同帧另订 ESC → 复用 `ShopFormLogic.OnExitClick`（或抽到 GSM 的同一 `LoadScene(Village_KenMuNi1)` API）。回村落点 `EnterPosConfig[lastScene=Village_Shop]` → `EnterFrom_Shop`（约 -29.04, -6.5）已齐，施工免动 EnterPos。对白中 ESC 默认建议禁离店。**

---

## ② 原因（通俗）

### 2.1 现网为什么店内 ESC 会开菜单？

这是 **0713 有意施工**，不是残留：

| 点 | 现网 |
|----|------|
| 总线 | `InputComponentGM` Esc → `onEscPressed` |
| 门卫 | `InputComponentGSM.OnEscPressed` → `OpenUIForm(MenuPanel)`（`cantOpenMenu` / `isOpenMenu` 挡） |
| 店 GSM | `OnEnterScene` → **`SetAllowOpenMenu(true)`** + 日志「店内可 ESC 开菜单」 |
| Shop UI | `ShopFormLogic.OnOpen` → **`AllowOpenMenu(true)`**（再放行一次） |
| 无玩家 | 纯 UI 店不走 `PlayerLogic` 放行，故 0713 **显式** SetAllowOpenMenu |

**产品改口**：店内 ESC = **离店回村**，**不再**开 MenuPanel。村内 / 其它场景仍开菜单（勿改 `InputComponentGSM` 默认语义）。

### 2.2 「保持进店位置」不是存脚底坐标

纯 UI 商店 **没有玩家**，不会把进店前 XY 写进存档。

架构里「保持位置」=：

```
离店 LoadScene(Village_KenMuNi1)
  → LastSceneName = Village_Shop
  → 村里 SetPlayerPos 匹配 EnterPosConfig.lastScene == "Village_Shop"
  → 落到 EnterFrom_Shop
```

| 项 | 磁盘核实 |
|----|----------|
| EnterPos 条目 | ✅ `lastScene: Village_Shop` → `{fileID: 5601461779999999002}` |
| 物体名 | **`EnterFrom_Shop`** |
| 坐标 | local/world ≈ **(-29.04, -6.5, 0)**（父节点 Enter 根在原点） |
| Door_Shop | local ≈ **(-29.0394, 2.1124, 0)**（父 Object 根原点）；**同 X、Y 为纵深走位差** |
| 与民居模式 | 同构：`ExitFrom_HomeScene*` / `EnterFrom_Shop` 都是 EnterPos 表，不是「记住进门前像素」 |

白话：回村站在 **店门外固定出生点**（对准门的 X、走在可行走深度上），允许与「进店前脚底」差半步；**施工默认接受固定 EnterPos，免动**（开放问题 Q3）。

### 2.3 离店现成链路（按钮）

`ShopFormLogic.OnExitClick` 已是正规离店：

```
LoadSceneComponentGSM.LoadScene(Village_KenMuNi1, stayAction=CloseForm)
```

黑幕全黑时关 ShopPanel，再切场；`LastSceneName` 变 `Village_Shop` → 命中 `EnterFrom_Shop`。

**裁定**：ESC 离店必须 **调用同一 API**（直接 `OnExitClick()` 或抽 `ExitShopToVillage()` 两边共用），禁止第二套 LoadScene。

离开按钮：**保留**，与 ESC 同源。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村里走到 **Door_Shop** 附近记站位 → 进店 | 进店成功 |
| 2 | 店内按 **ESC**（施工后） | **回村**；**不**出 MenuPanel |
| 3 | 回村站位 | ≈ **Door_Shop 门外**（`EnterFrom_Shop`，X≈-29，走位 Y≈-6.5） |
| 4 | 点商店「离开」按钮 | 与 ESC 同等回村/落点 |
| 5 | 村内（非店）ESC | **仍开** MenuPanel |
| 6 | （建议）首次进店 / 点头对白中按 ESC | **不离店**（见开放问题；默认禁） |
| 7 | Console | 无「ESC→MenuPanel」与换场双开；无 NRE |

---

## ④ 给程序

### A. 现网「店内 ESC → 菜单」链路

```
InputComponentGM (Esc.performed)
  → onEscPressed
  → InputComponentGSM.OnEscPressed
       if isOpenMenu || cantOpenMenu → return
       else OpenUIForm(MenuPanel)
```

| 谁放行菜单 | 位置 |
|------------|------|
| `Village_ShopSceneManager.OnEnterScene` | `SetAllowOpenMenu(true)` |
| `ShopFormLogic.OnOpen` | `AllowOpenMenu(true)` |
| 换场结束 `AllowResponse` | `cantOpenMenu = false`（全局模块行为） |

**注意**：仅在 OnEnter 设 `false` 不够——`ShopFormLogic.OnOpen` 仍会再 `true`；两处都要改口。

### B. 离店回村链路

| 项 | 值 |
|----|-----|
| UI 离开按钮 | `btnExit` → `OnExitClick`（`ResolveExitButtonReference` 接线） |
| API | `LoadScene(SceneName.Village_KenMuNi1, CloseForm)` |
| LastScene | 离店后为 `Village_Shop` |
| 落点 | `EnterFrom_Shop` ✅ **免动** |

### C. EnterPos 核验结论

| 检查 | 结论 |
|------|------|
| 有无 `Village_Shop` 条目 | ✅ 有 |
| Transform | `EnterFrom_Shop` (-29.04, -6.5, 0) |
| 与 Door_Shop | 同 X；门精灵 Y≈2.11、落点 Y≈-6.5（2.5D 纵深，合理） |
| 施工 | **只改 ESC / 菜单门卫；EnterPos 不改** |

### D. ESC 改道方案拍板

| 方案 | 裁定 | 说明 |
|------|------|------|
| **A · 店 GSM 订阅 ESC + 调离店** | **✅ 主方案** | 改动集中在 `Village_ShopSceneManager`；仅本场景订阅 |
| **B · SetAllowOpenMenu(false)** | **✅ 必须配套** | 让现有 `InputComponentGSM` 对 ESC **早退**，避免开菜单；改 GSM + ShopFormLogic 两处 true |
| C · allowEscapeClose | ❌ | 场景常驻 `UI_Shop` / Form 关≠换场；且现 ShopForm 无此路径当离店 |
| D · 改 InputComponentGSM 按场景名分支 | ❌ | 污染全局 |

**推荐组合伪代码（挂点）**

```csharp
// Village_ShopSceneManager
OnEnterScene:
  SetAllowOpenMenu(false);           // 禁菜单（改口）
  inputGM.onEscPressed += OnShopEscPressed;

OnDestroy / OnShutDown:
  退订 OnShopEscPressed;

OnShopEscPressed:
  if (换场中 / 已在离店) return;
  if (StoryGsm.HasRunningStory) return;  // 默认：对白中禁 ESC 离店（Q1）
  ExitShopToVillage();               // → ShopFormLogic.OnExitClick() 同源

// ShopFormLogic.OnOpen:
  AllowOpenMenu(false);              // 或删除 true；注释写 0829 改口 vs 0713
  // OnClose 勿再 true 锁死下一场景；可保持 true 给回村，或依赖 AllowResponse
```

**仅 Village_Shop 生效**：订阅挂在店 GSM；村内无此订阅，仍走 `InputComponentGSM` → 菜单。

**避免双响应**：`cantOpenMenu=true` → InputComponentGSM 不开菜单；店订阅只离店。禁止在 `ShopFormLogic.Update` 里 `GetKeyDown(Escape)`。

**离开按钮**：保留，继续 `OnExitClick`。

**对白中 ESC（推荐默认）**：`HasRunningStory` 时 **忽略 ESC 离店**（首次进店 / 点头胸不被强断）。写入开放问题；若产品要强退，再改一行。

**黑幕换场中**：`CantResponse` 已 `cantOpenMenu=true`；离店 API 内再加「已在 LoadScene」防抖（若 LoadScene 无自带互斥则补）。

### E. 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | `Village_ShopSceneManager.OnEnterScene` | `SetAllowOpenMenu(false)`；注释改口 | **P0** |
| 2 | 同 GSM | 订阅 `onEscPressed` → 离店；OnDestroy 退订 | **P0** |
| 3 | `ShopFormLogic.OnOpen` | 去掉/反转 `AllowOpenMenu(true)`；改日志文案 | **P0** |
| 4 | 离店 API | ESC 调 `OnExitClick` 或抽 `ExitShopToVillage` 共用 | **P0** |
| 5 | EnterPos | **核对通过，免动** | — |
| 6 | 对白中 ESC | 默认 `HasRunningStory` 则 return | P1 |
| 7 | 0713 文档备注「店内 ESC 菜单已作废」 | | P2 |
| 8 | 日志 `[ShopEscExit]` | | P2 |

**排除**：改全局 InputComponentGSM 默认；重做 Door_Shop；店内仍默认开菜单；Update 野监听 ESC。

**预期 diff 文件**

- `Village_ShopSceneManager.cs`
- `ShopFormLogic.cs`（OnOpen 门卫 + 可选抽公共 Exit）
- （不改）`Village_KenMuNi1.unity` EnterPos
- （不改）`InputComponentGSM.cs` 核心逻辑

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Door_Shop 进店 → ESC | 回村，站 `EnterFrom_Shop` 附近；无 MenuPanel |
| 2 | 店内 ESC | Console 无 `ESC → OpenUIForm MenuPanel` |
| 3 | 村内 ESC | 仍开菜单 |
| 4 | 离开按钮 | 同落点 |
| 5 | 对白中 ESC | 符合拍板（默认不离店） |
| 6 | 无双开 / NRE | |

### G. 开放问题

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 对白中 ESC 是否允许强退离店？ | **否**（`HasRunningStory` 忽略） | 待确认 |
| Q2 | 店内是否完全不要菜单（存档须回村再 ESC）？ | **是**（本期产品） | ✅ 产品已偏此 |
| Q3 | EnterPos 与进店前脚底差半步是否接受？ | **接受门外固定点** | 待确认 |
| Q4 | `ShopFormLogic.OnClose` 是否仍 `AllowOpenMenu(true)`？ | 可保留（利回村）；勿在 OnOpen 再 true | 待施工注意 |

（已追加 `OPEN_QUESTIONS.md`。）

---

## 附录 · 与 0713 改口对照

| 0713 / 现网 | 0829 产品 |
|-------------|-----------|
| 店内 ESC → MenuPanel（贵重物品验收） | ❌ 作废（仅店内） |
| `SetAllowOpenMenu(true)` + Debug「可开菜单」 | → `false` + ESC 离店 |
| 离店靠离开按钮 | ESC **与按钮同源**；按钮保留 |
| 回村靠 EnterPos | ✅ 机制不变；`EnterFrom_Shop` 已齐 |
