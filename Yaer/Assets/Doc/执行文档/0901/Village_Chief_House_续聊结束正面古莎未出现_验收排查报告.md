# Village_Chief_House — 续聊结束正面古莎未出现 — 验收排查报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【验收员 / 架构侦探 → 施工已落地 H2】Setup 已扩写**场景拆包合层**；须开 Unity 跑一次预置菜单；**勿改无关场景设置**  
**Unity**：2020.3.48f1  
**现象**：`Village_村长家继续对话` 结束后，**新的正面古莎（古莎动画合层）未出现**  
**期望**：BlackPanel 全黑内关 **`古莎待机`** → 开 **`古莎动画合层`**（含 **`古莎正面`**）  
**依据**：`执行文档/0901/…续聊结束黑幕换古莎动画待机_架构溯源报告.md` · `施工说明/0901/…施工说明.md`  
**提示词**：`提示词/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查提示词.md`

---

## 沟通摘要

### ① 结论一句话

**主因 H2：运行时场景里的「村长家合层」是已拆包的场景物体，没有「古莎动画合层」；Setup 只写进了未挂进场景的 `Prefab/村长家合层.prefab`。换人一跑就关掉待机，却找不到动画实例 → 正面古莎空白。**

### ② 原因（通俗）

换人代码是好的，但「新古莎」从没摆进**正在玩的那间房**。  
菜单预置改的是合层 Prefab 资产，场景里却是一份断开的合层拷贝——里面只有旧的「古莎待机」，没有动画合层。  
所以一换人：旧的关了，新的不存在，看起来就像「正面古莎没出现」。

### ③ 用户检查清单（修复后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy：`Design/村长家合层` 下有 **`古莎动画合层`**（可先灰） | |
| 2 | 续聊未播档完整续聊结束 → 黑幕 → 待机关、动画合层（正面）在村长旁可见 | |
| 3 | Console：`[ChiefGushaSwap] 全黑内已切换`；**无**「未找到古莎动画合层」 | |
| 4 | 同档再进：静默动画合层，不双古莎 | |
| 5 | 续聊已用、曾换人失败的旧档：进房也能看到正面古莎（Q7） | |
| 6 | 针线包 Tips / 续聊台本回归 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H2**（强相关 **H8**）：场景合层 **无** `古莎动画合层` 实例；`FindDeepInactive` 失败 |
| **次因（测档）** | **H1** 可能叠加：续聊已用则不订 `onStoryEnd`；但 Q7 进房仍 `Apply(true)` → **同一 H2 空白** |
| GSM 逻辑 | ✅ 订约 / 黑幕 / Apply / 记旗链路已落地；SerializeField 为空属设计（按名解析） |
| Prefab 资产 | ✅ `Prefab/村长家合层`（guid `5cad…`）内 **已嵌** `4271a266…`，默认 `m_IsActive:0` |
| 场景真源 | ❌ `Village_Chief_House` 的 `Design/村长家合层` **不是** `5cad` PrefabInstance，而是 **拆包 GO**（`CorrespondingSourceObject=0`），子节点 **无**动画合层 |
| 最小修复 | 把动画合层预置进 **场景正在用的那份合层**（或把场景合层重新挂回 `5cad` Prefab）；再绑/验收 Find；补短日志 |

---

## ② 复现

### 推荐复现（首次换人黑幕）

1. 存档：`Village_村长家门口初次对话` **已用** ∧ `Village_村长家继续对话` **未用** ∧ `Village_ChiefHouse_GushaAnimStandby` **未用**  
2. 进 `Village_Chief_House` → 自动续聊 → 完整播完  
3. 期望：黑幕 → 正面动画古莎；实际：无新古莎（常见：待机也消失 → 空白）

### 次要复现（Q7 静默）

1. 存档：续聊 **已用** ∧ 换人旗 **未用**（曾中断 / 曾 Apply 失败）  
2. 再进房 → 无黑幕、无订约；`ApplyGushaVisualFromArchive` 仍 `showAnim=true`  
3. 同样因 H2：**待机被关、动画找不到** → 空白  

### 测档记录表（验收时填）

| 键 | 本测档 |
|----|--------|
| `Village_村长家门口初次对话` | |
| `Village_村长家继续对话` | |
| `Village_ChiefHouse_GushaAnimStandby` | |

---

## ③ 假说表（H1～H8）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | 未 Trigger 续聊 → 未订 `onStoryEnd` | **条件成立可能**；**非唯一主因** | `ShouldPlayChiefContinue` 要门口已用∧续聊未用；已用档不订约。但 Q7 仍会 `Apply(true)`，空白仍由 H2 造成 |
| **H2** | 找不到「古莎动画合层」 | **✅ 主因成立** | 场景 YAML：**无** `古莎动画合层` 名、**无** guid `4271a266`；合层 15 子含待机/村长等，**无动画**；GSM `gushaAnimComposite={fileID:0}` 依赖 Find → 必 Warning |
| **H3** | BlackPanel `onShowEnd` 未到 | **否（非主因）** | 即便黑幕失败，进房 Q7/`Apply` 仍会关待机；磁盘主缺是实例缺失。若 Console 全无 `ChiefGushaSwap` 再查 H3 |
| **H4** | Active 开了但看不见 | **否（对象不存在）** | 无实例可谈 Sorting/镜头；资产侧 Sorting 已 +8（9～11）属次要 |
| **H5** | Find 命中错误实例 | **否** | 场景内 **0** 个该名；找不到比找错更贴 |
| **H6** | SerializeField 绑空 | **现象有、非根因** | `gushaStandby/Anim={fileID:0}` 故意可空；待机按名可找到；动画按名失败因场景无物体 |
| **H7** | 测的是晚宴等未订约对白 | **待测档确认** | 仅续聊 Trigger 成功才 `+= onStoryEnd`；用户口述「村长家对话」偏续聊 |
| **H8** | 改错合层源 | **✅ 成立（与 H2 同构）** | Setup 写 `Prefab/村长家合层`（`5cad`）✅ 有动画；**场景未引用 `5cad`**（SourcePrefab 列表无此 guid），玩的是拆包合层 → 预置未进运行时 |

---

## ④ 证据（磁盘）

### A. GSM（逻辑已通）

```74:88:Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
        public override void OnEnterScene()
        {
            // ...
            ApplyGushaVisualFromArchive();
            TryTriggerChiefContinueOnce();
        }
```

- Trigger 成功 → `SubscribeContinueStoryEnd` → `OnChiefContinueStoryEnd` → BlackPanel → `ApplyGushaVisual(true)` + 记旗  
- `ApplyGushaVisual`：找不到动画 → **`[ChiefGushaSwap] 未找到「古莎动画合层」…Setup…`**  
- Q7：`continueDone || flag` → `showAnim=true`（旧档再进仍踩 H2）

### B. 场景合层（运行时真源）

| 项 | 磁盘 |
|----|------|
| 路径 | `Map → Design → 村长家合层` |
| 链接 | **拆包**：`m_CorrespondingSourceObject/PrefabInstance = 0`；**非** `5cad` PrefabInstance |
| 子节点 | 背景/楼梯/栏杆/…/ **`古莎待机`** / **`村长`** / 光…（15 个） |
| **`古莎动画合层`** | ❌ **不存在** |
| guid `4271a266` / `5cad3431` | ❌ **场景 0 命中** |
| GSM 引用 | `gushaStandby: {fileID:0}` · `gushaAnimComposite: {fileID:0}` |

### C. Prefab 资产（Setup 写入处 · 未进本场景）

| 项 | 磁盘 |
|----|------|
| `ArtRes/Scene/Village/Prefab/村长家合层.prefab` | guid **`5cad3431…`** |
| 内嵌 | PrefabInstance **`4271a266…`**，改名「古莎动画合层」，**`m_IsActive: 0`**，Sorting 9/11，背景关 |
| 场景是否实例化该资产 | ❌ **否** |

### D. Console / Hierarchy（验收员 Play 时核对）

| 过滤 | 期望（当前坏档） |
|------|------------------|
| `[ChiefContinue] OnEnterScene TriggerStory` | 仅「续聊未用」档有 |
| `[ChiefGushaSwap] 未找到「古莎动画合层」` | **应出现**（主因指纹） |
| `[ChiefGushaSwap] 全黑内已切换` | 可能仍打印（Apply 不抛错），但动画 GO 为空则无效 |
| Hierarchy Pause | `古莎待机` 或已关；**无** `古莎动画合层` 或始终灰且从未被找到 |

---

## ⑤ 主因

**运行时场景拆包合层缺少「古莎动画合层」实例；0901 Setup 只更新了未挂接的 `Prefab/村长家合层`，导致 `ApplyGushaVisual(true)` 关待机、开空引用 → 正面古莎不出现。**

时序后果：

```
Apply(true)
  → 古莎待机.SetActive(false)     // 找得到
  → 古莎动画合层 == null          // 场景没有
  → 玩家看到：无人 / 无「新正面古莎」
```

---

## ⑥ 最小修复清单（给施工员）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | **在场景** `Design/村长家合层` 下预置 `古莎动画合层`（实例化 `ArtRes/Animation/古莎动画合层.prefab`）：默认关、local≈`古莎待机`、关子「背景」、Sorting 对齐 Setup（+8） | **P0** |
| 2 | **二选一结构**：**A** 将场景合层重新 Prefab 链接/`Apply` 到 `Prefab/村长家合层`（`5cad`，已含动画）；或 **B** 扩展 Setup：同时 patch **场景实例**（勿只改资产） | **P0** |
| 3 | GSM Inspector 拖好 `gushaStandby` / `gushaAnimComposite`（减少 Find 脆弱） | P1 |
| 4 | 诊断日志（可开关）：Resolve 后打印 path / instanceId / activeSelf；Find 失败保持现 Warning | P1 |
| 5 | 勿改：WalkArea2、出屋送树屋、UI `GushaPainting`、另一份 `Village/村长家合层`（`f77da…`）除非确认要统一 | — |

**不建议**：仅「亮屏硬切」当修复却不补场景实例。

---

## ⑦ 验收

- [ ] 场景 Hierarchy 合层下有 `古莎动画合层`  
- [ ] 首次续聊结束 → 黑幕 → 待机关、正面合层可见在村长旁  
- [ ] Console 有全黑切换成功；**无**「未找到古莎动画合层」  
- [ ] 同档再进：静默动画，不双古莎  
- [ ] 续聊已用、旗未立旧档：进房可见正面古莎  
- [ ] 针线包 / 续聊回归  

---

## ⑧ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 主因？ | **H2+H8：场景无动画实例 / Setup 未进运行时合层** | ✅ |
| Q2 | 修复走场景补实例还是重挂 `5cad` Prefab？ | 施工任选；**推荐重挂/Apply Prefab** 免资产与场景 | ⏳ |
| Q3 | 旧档已关待机且无动画？ | 修场景后进房 Q7 即可恢复；无需清档 | ✅ |
| Q4 | 是否缺 Animator 真帧？ | **否**（本期 SR 合层）；「没出现」≠ 缺 Animator | ✅ |
| Q5 | 测档是否曾 H1？ | 验收时填 Story 三键；不改变 H2 主因 | ⏳ |

---

## ⑨ 程序补充

### 关键锚点

| 符号 | 说明 |
|------|------|
| `ApplyGushaVisual` / `FindDeepInactive` | 只搜 **已加载场景**；改 Prefab 资产不等于场景有实例 |
| `ChiefHouseGushaAnimStandbySetupEditor` | 只写 `Prefab/村长家合层`；**未写** `Village_Chief_House.unity` |
| 拆包合层 | 场景 `村长家合层` fileID `5211136240839023578` / Transform `8912268226331786818` |

### 建议日志插点（短、可开关）

1. `ResolveGushaRefsIfNeeded` 末：standby/anim 是否 null、`HierarchyPath`  
2. `TryTriggerChiefContinueOnce`：F1 三布尔（门口/续聊/是否 started）  
3. `ApplyGushaVisualFromArchive`：flag / continueDone / showAnim  

### 硬禁止

- 未核 Hierarchy 就改脚位猜修  
- 把 UI `GushaPainting` 当场景正面古莎  
- 只改 `f77da` / 只改资产不碰场景拆包合层  
