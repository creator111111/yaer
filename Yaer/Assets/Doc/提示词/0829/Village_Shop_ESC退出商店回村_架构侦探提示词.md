# Cursor Agent Prompt · Village_Shop：ESC 退出商店回村（保持进店位置）· 不再开菜单

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **场景**：`Village_Shop`（纯 UI 商店）↔ 回 `Village_KenMuNi1`  
> **产品目标（白话）**：  
> 1. **进店后按 ESC** → **直接退出商店、回到村庄**  
> 2. 回村后玩家位置 = **进入商店前的位置**（门口落点，勿丢到别处）  
> 3. **店内 ESC 不再打开菜单**（MenuPanel）——与现网「店内可 ESC 开菜单」**产品改口**  
> **本阶段**：只读；禁止改代码 / Prefab / 场景  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_Shop_ESC退出商店回村_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 店内按 ESC | **离店回村**，不是开 MenuPanel |
| 回村落点 | **保持进店位置**（Door_Shop 门外 / 进店前站位；勿随机重生） |
| 离店方式 | ESC 与现有「离开」按钮应殊途同归（同一套 LoadScene 回村） |
| 其它场景 ESC | **仍开菜单**（勿改坏村内 / 室内 InputComponentGSM 默认行为） |
| 店内还要不要菜单？ | 产品本期：**店内 ESC 不开菜单**；若需存档等，写入开放问题（是否另入口） |

### 与历史文档的「改口」（必写进报告）

| 文档 / 现网 | 旧口径 | **本期新产品** |
|-------------|--------|----------------|
| `0713/Village_Shop_ESC呼出菜单…` | 店内 ESC → MenuPanel | ❌ **作废（仅店内）** |
| `0713/Door_Shop→Village_Shop…` | 离店靠 UI 离开按钮；ESC 仍开菜单 | ESC = 离店；按钮可保留 |
| `Village_ShopSceneManager` / `ShopFormLogic` | 显式 `SetAllowOpenMenu(true)` + Debug「店内可 ESC 开菜单」 | 须改为 **禁菜单 + ESC 离店** |
| `ShopFormLogic.OnExitClick` | 已有 `LoadScene(Village_KenMuNi1)` + CloseForm | **复用**，勿另写第二套换场 |

### 落点假说（「保持进店位置」）

现网换场通则（须证伪）：

```
进店：KenMuNi1 · Door_Shop → LoadScene(Village_Shop)
      LastSceneName = Village_KenMuNi1（店侧一般不落玩家）

离店：Village_Shop → LoadScene(Village_KenMuNi1)
      LastSceneName = Village_Shop
      村里 EnterPosConfig 匹配 lastScene=Village_Shop → 门外 Transform
```

预扫：`Village_KenMuNi1` 的 `EnterPosConfig` **似已有** `lastScene: Village_Shop` 条目。  
侦探必须：

1. 钉死该 Transform 坐标 / 物体名（是否 Door_Shop 旁 ExitFrom / Born）  
2. 说明：纯 UI 店 **不存玩家坐标**；「保持进店位置」= **回村用进店门对应的 EnterPos**，不是把店内鼠标坐标带回村  
3. 若 EnterPos 缺失或指错 → 施工清单写校正；若已正确 → 施工只改 ESC，落点免动  
4. 对比民居出门 `ExitFrom_HomeScene*` 模式，确认商店是否同构

### ESC 改道候选（侦探必拍板）

现网：`InputComponentGM.onEscPressed` → `InputComponentGSM.OnEscPressed` → `OpenUIForm(MenuPanel)`（受 `cantOpenMenu` / `isOpenMenu` 控制）。

| 方案 | 做法 | 优点 | 风险 | 助手倾向 |
|------|------|------|------|----------|
| **A · 店场景覆盖 ESC** | `Village_ShopSceneManager` 订阅 ESC（或重写/替换 Input 行为）：禁菜单 + 调离店 | 改动集中在店 GSM | 须避免与 InputComponentGSM 双开 | **优先** |
| **B · SetAllowOpenMenu(false) + 另订 ESC 离店** | 关菜单门卫，另挂离店监听 | 清晰互斥 | 两处订阅时序 | 可行 |
| **C · ShopPanel allowEscapeClose → OnExitClick** | ESC 关商店 UI 并顺带换场 | 复用 UI ESC | Shop 是场景常驻 UI 还是 Form？须核实；关 Form≠换场 | 仅当商店是可关 Form |
| **D · 改 InputComponentGSM 全局** | ESC 按场景名分支 | 一处改 | 易污染全项目 | ❌ 除非极薄且报告论证 |

**禁止**：在 `ShopFormLogic.Update` 里 `GetKeyDown(Escape)` 野路子绕开输入总线（0713 已否决同类写法）。

### 对白 / UI 互斥（必查）

| 状态 | ESC 应怎样？ |
|------|----------------|
| Idle 买卖 | 离店回村 |
| 首次进店 / 点头胸对白进行中 | **禁离店？** 还是允许 ESC 强退？（产品未说 → **开放问题**，侦探给推荐默认） |
| MenuPanel 若仍被其它路径打开 | 不应再出现；若出现，ESC 关菜单还是离店？ |
| 黑幕换场中 | 忽略重复 ESC |

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `InputComponentGSM` / `InputComponentGM` | ESC → 菜单真源 |
| `Village_ShopSceneManager` | 现网 `SetAllowOpenMenu(true)` 改口点 |
| `ShopFormLogic.OnExitClick` | 离店换场复用 |
| `Village_KenMuNi1` · `EnterPosConfig` · `Village_Shop` | 回村落点 |
| `Door_Shop` · `SceneChangeDoor` | 进店入口 |
| `0713` 两篇商店文档 | 旧口径对照 |
| 民居出门 EnterPos 样例 | 落点模式对照 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景  
- 把「全游戏 ESC 改成退出当前场景」  
- 店内继续保留「ESC 开菜单」当默认（与产品冲突）  
- 离店不走 `LoadSceneComponentGSM`、不配 EnterPos 就指望「自动停在原坐标」却不核实机制

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/7月/0713/Village_Shop_ESC呼出菜单_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/7月/0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_架构溯源与施工执行说明.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/InputComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、配置。只读扫描 + 写「ESC 退出商店回村」溯源报告。

---

## 背景（策划白话）

1. 玩家从村里进商店后，按 **ESC** 应该 **马上回村**，站在 **进店前那个位置**。  
2. 在商店里按 ESC **不要再弹出系统菜单**。  
3. 本阶段只摸清：现在 ESC 为什么会开菜单、离店回村现成链路在哪、落点靠什么保证，以及最小改哪几处。

---

## 侦探任务清单

### A. 钉死现网「店内 ESC → 菜单」链路

| 项 | 填 |
|----|-----|
| 谁订阅 `onEscPressed`？ | |
| `Village_Shop` 何处 `SetAllowOpenMenu(true)`？ | GSM？ShopPanel OnOpen？ |
| 无玩家时菜单如何解锁？ | |
| 结论：店内 ESC 开菜单是 **有意施工** 还是残留？ | （对照 0713） |

### B. 钉死现网「离店回村」链路

| 项 | 填 |
|----|-----|
| `ShopFormLogic.OnExitClick` 是否唯一离店入口？ | UI 是否有离开按钮？ |
| `LoadScene(Village_KenMuNi1)` + `CloseForm` 时序 | |
| `LastSceneName` 离店后是否为 `Village_Shop`？ | |
| 黑幕 / FightingPanel / UI_Shop 关闭顺序 | |

**裁定**：ESC 离店应 **调用同一离店 API**（提取公共方法 vs 直接调 OnExitClick），禁止复制第二套 LoadScene。

### C. 钉死「保持进店位置」

| 项 | 填 |
|----|-----|
| `KenMuNi1.EnterPosConfig` 中 `lastScene=Village_Shop` 的目标 Transform / 坐标 | |
| 与 Door_Shop 世界坐标关系（是否门外原位） | |
| 进店时玩家坐标是否写入别处？ | 纯 UI 店通常不写 |
| 若 EnterPos 已正确 | 施工 **只改 ESC**，落点标 ✅ 免动 |
| 若偏移 / 缺失 | 给出应绑物体与建议坐标（参考门旁） |

白话向用户解释：「保持位置」在本架构里 = **回村出生点表对准店门**，不是把商店场景里的坐标存档。

### D. ESC 改道方案拍板（必选 A/B/C…）

回答：

1. 推荐方案与伪代码挂点（类 / 方法）  
2. 如何保证 **仅 Village_Shop** 生效，村内 ESC 仍开菜单  
3. 如何避免「既开菜单又离店」双响应  
4. 对白进行中 ESC 默认策略（推荐 + 开放问题）  
5. 离开按钮是否保留（产品未禁则 **保留**，与 ESC 同 API）

### E. 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | 店内禁止 ESC 开菜单 | 去掉/反转现网 AllowOpenMenu(true) 等 | P0 |
| 2 | 店内 ESC → 离店 API | | P0 |
| 3 | EnterPos 核对/校正 | | P0 核验 |
| 4 | 对白中 ESC 策略 | | P1 |
| 5 | 0713 文档口径备注 / 短技术说明 | | P2 |
| 6 | 调试日志 | 如 `[ShopEscExit]` | P2 |

**排除**：改全局 InputComponentGSM 默认语义；重做 Door_Shop 进店；店内仍默认开菜单。

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村里走到 Door_Shop 记位置 → 进店 → ESC | 回村，站位 ≈ 进店前（门外 EnterPos） |
| 2 | 店内 ESC | **不**出现 MenuPanel |
| 3 | 村内（非店）ESC | 仍可开菜单 |
| 4 | 点商店离开按钮（若有） | 与 ESC 同等回村/落点 |
| 5 | （按报告）对白中 ESC | 符合拍板策略 |
| 6 | Console | 无双开菜单+换场；无 NRE |

### G. 开放问题

- 对白中 ESC 是否允许强退离店？  
- 店内是否完全不要菜单（存档入口是否可接受「回村再 ESC」）？  
- EnterPos 与「进店前脚底坐标」若差半步，是否接受门外固定点？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_Shop_ESC退出商店回村_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（ESC 改道方案 + 落点是否已齐 + 菜单如何禁）  
② 原因（通俗：现网为何会开菜单、回村靠 EnterPos 不是存坐标）  
③ 用户检查清单（进店前站哪、ESC 后站哪、村里 ESC 是否仍开菜单）  
④ 给程序：链路表 + 方案对比 + 最小文件 diff + 开放问题

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_ESC退出商店回村_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/InputComponentGSM.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。只按报告实现：店内 ESC = 退出商店回村（保持 EnterPos 进店落点），店内不再 ESC 开菜单。

必须遵守：
- 仅 Village_Shop 改 ESC 语义；其它场景仍开菜单；
- 离店复用现有 LoadScene 回村 API（与 OnExitClick 同源），禁止 Update 野监听；
- EnterPos 仅当报告要求校正时改；
- 对白中 ESC 按报告拍板；
- 代码含详细注释；重要取舍写清原因（含与 0713「店内可开菜单」改口说明）。

提交说明：改了哪些文件、ESC 如何验收、落点如何确认、未做项。
```
