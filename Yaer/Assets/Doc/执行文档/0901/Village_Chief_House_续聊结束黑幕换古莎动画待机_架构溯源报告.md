# Village_Chief_House — 续聊结束黑幕换古莎动画待机 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】只读定方案（**禁止改代码 / 场景 / Prefab**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Chief_House`  
**产品**：室内 **`Village_村长家继续对话` 结束** → 系统 **BlackPanel** 淡入淡出 → 关合层 **`古莎待机`** → 开 **`古莎动画合层`** 站在 **`村长`** 旁  
**资源**：`Assets/ArtRes/Animation/古莎动画合层.prefab`  
**提示词**：`提示词/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构侦探提示词.md`  
**上游**：0901 进屋自动续聊（`Village_Chief_HouseSceneManager` C1+F1）已合入  

---

## 沟通摘要

### ① 结论一句话

**挂点 G1：`Village_Chief_HouseSceneManager` 在续聊 `TriggerStory` 成功后订 `onStoryEnd` → BlackPanel 全黑内关 `古莎待机`、开预置的 `古莎动画合层`（关其子「背景」）再淡出；摆放 P1 进场景所用合层源 `Prefab/村长家合层.prefab`；同档用存档旗单次，读档已换则静默正确 Active、不再黑幕。**

### ② 原因（通俗）

续聊播完后，场景里那张静态「古莎待机」要换成动画合层古莎，且必须在黑屏里切，不然会闪两个古莎。  
动画 Prefab 自带一张「背景」图层，室内已有房间底，施工时要关掉，不然会盖住房间。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 续聊完整结束后 | 自动黑幕再亮 |
| 2 | 亮后 | `古莎待机` 关；`古莎动画合层` 在村长旁可见 |
| 3 | 无双古莎；房间底图不被动画「背景」盖住 | |
| 4 | 同档再进房 | 不重复黑幕换人；直接显示动画合层 |
| 5 | 续聊 / 针线包 Tips 回归正常 | |
| 6 | Console 无 BlackPanel / 空引用 Error | |

### ④ 程序补充

见下文 §①～§⑩。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 对话名 | 钉死 **`Village_村长家继续对话`**（勿绑晚宴） |
| 挂点 | **G1** · Chief_House GSM：`TriggerStory` 成功后订 `onStoryEnd` |
| 黑幕 | 系统 **BlackPanel** `FadeShow` → 全黑换人 → `CloseFormFade`；**非** Loading |
| 摆放 | **P1** 合层内预置动画实例，默认 **关**；运行时只 Active |
| 合层改哪份 | 场景引用 **`ArtRes/Scene/Village/Prefab/村长家合层.prefab`**（guid `5cad3431…`）；另有一份 `ArtRes/Scene/Village/村长家合层.prefab`（`f77da84d…`）场景 **未引用**，勿改错 |
| 「背景」子节点 | **关/删实例内「背景」**（Q4） |
| Animator | Prefab **无** Animator → 本期当 **分层 SR 合层待机**；真帧动画另案 |
| 读档 | 已换旗 → **静默**正确 Active，**跳过**黑幕（Q5） |

---

## ② 锚点（磁盘核实）

### Hierarchy（用户）

`Map → Design → 村长家合层 → 古莎待机`（旁近 `村长`）

### 合层源（场景真用）

| 路径 | guid | 场景引用 |
|------|------|----------|
| **`Assets/ArtRes/Scene/Village/Prefab/村长家合层.prefab`** | `5cad34316d506314a9d29868d5abcfb6` | ✅ `Village_Chief_House.unity`（多处 PrefabInstance） |
| `Assets/ArtRes/Scene/Village/村长家合层.prefab` | `f77da84d27b17b34bbf03796bb9c9c65` | ❌ 0 refs（内容同类，**勿当主改目标**） |

### 脚位 / Sorting（合层本地）

| 物体 | localPosition | SortingLayer | SortingOrder | Active |
|------|---------------|--------------|--------------|--------|
| **古莎待机** | `(33.415, 3.265, 2.667)` | Default(0) | **11** | 1 |
| **村长** | `(23.57, 4.20, 2.00)` | Default(0) | **12** | 1 |

- 待机在村长 **右侧**（Δx≈+9.85，Δy≈−0.94）。  
- 动画合层预摆脚位：**优先对齐现网 `古莎待机` 世界/合层坐标**（本就是「站村长旁」），再肉眼微调。  
- Sorting：动画实例宜对齐待机量级（order≈11，且 **&lt;** 村长 12，或按美术再调）；合层现网多为 Default——若与玩家穿帮，再钉 `SceneObject`（对齐村外侧面教训，**勿**搬侧面整套逻辑）。

### 解耦（严禁混用）

| 资源 | 职责 | 本期 |
|------|------|------|
| 合层 `古莎待机` / `古莎动画合层` | **室内场景 NPC 涂层** | ✅ 换人对象 |
| `GushaSidePortrait` | 村外门口侧面 | ❌ 勿搬 |
| UI `GushaPainting` | 对话框立绘 | ❌ 勿当待机 |
| 玩家 Controllable | 可操作角色 | ❌ 合层古莎不替代玩家 |

---

## ③ 动画合层结构

路径：`Assets/ArtRes/Animation/古莎动画合层.prefab`（guid `4271a266…`）  
**场景现网 0 引用**（须预置）。

```
古莎动画合层          (root, 0,0,0)
├── 背景              (1.47, 3.2, 10)   SR order 0   ← 室内应关
└── 古莎正面          (0,0,0)
    ├── 图层 1        (1.51, 3.09, 8.33) SR order 1
    ├── 组 8          (1.515, 5.01, 6.67) SR order 2
    └── 图层95        (1.52, 5.01, 5)    SR order 3
```

| 项 | 核实 |
|----|------|
| Animator / Animation Clip | ❌ **无** |
| 表现 | 多层 **SpriteRenderer** 合层（静态分层） |
| Q2 | 先按 SR 合层待机落地；真 Animator 帧动画 **另案** |

**「背景」**：室内合层已有房间 `背景`/`背景_2`；再开动画 Prefab 的「背景」易盖景 → **实例上 SetActive(false) 或删除该子节点**（改源动画 Prefab 亦可，但会影响其它潜在引用；优先实例关）。

---

## ④ 时序与黑幕

### 期望全链

```
OnEnterScene
  →（既有）F1 满足则 TriggerStory("Village_村长家继续对话")
  → 续聊播完（含针线包等）
  → onStoryEnd（须校验 Current/刚结束名为续聊）
  → Open BlackPanel FadeShow
  → onShowEnd（全黑）:
       古莎待机.SetActive(false)
       古莎动画合层.SetActive(true)   // 背景子已关
       // Sorting/脚位预摆
       记档「已换」
  → CloseFormFade
  → 还控
```

**禁止**：亮屏硬切；不关待机只加新实例（双古莎）；用 LoadingPanel 当这次表现。

### 黑幕样板

对齐 `ChiefNearDoorStoryTrigger`：

- `UIPrefabPath` → `BlackPanel` · `EUIGroup.System`  
- `ShowBlackFormArgs { showType=FadeShow, onShowEnd }`  
- 换人只在 **onShowEnd 全黑** 内做  
- `CloseFormFade` 淡出  

---

## ⑤ 挂点 / 摆放

### 挂点

| 方案 | 做法 | 裁定 |
|------|------|------|
| **G1 · GSM onStoryEnd** | `TryTriggerChiefContinueOnce` 成功后 `+= OnContinueStoryEnd`；结束里开黑换人 | ✅ |
| G2 · 对话图末 Action | 扩 Action + 难绑场景引用 | ❌ 本期不优先 |
| G3 · Collider 再触发 | 要玩家再走 | ❌ |

**注意**：`StoryComponentGSM.onStoryEnd` 为场景级广播——回调内须确认刚结束的是 **`Village_村长家继续对话`**（用结束前缓存的 `CurrentRunningStoryName`，因 `OnStoryEnd` 会清名）。  
仅订一次 / `OnDestroy` 解绑，防泄漏。

### 摆放

| 方案 | 做法 | 裁定 |
|------|------|------|
| **P1 · 预置** | 在 **`Prefab/村长家合层`**（或场景 Design 下合层实例）拖入 `古莎动画合层`，默认关，脚位≈待机，关「背景」 | ✅ |
| P2 · Instantiate | 运行时生成 | ⚠️ 备选；重复进房要防双实例 |
| P3 · 改待机 Sprite | — | ❌ |

GSM 用 **SerializeField** 绑 `古莎待机` / `古莎动画合层`（或按名在合层下 Find），避免 `GameObject.Find` 全局脆。

---

## ⑥ 读档 / 单次

| 时机 | 行为 |
|------|------|
| 续聊未播 | 待机 **开**；动画 **关**（默认） |
| 续聊结束黑幕换人成功 | 记档旗；亮后动画可见 |
| 再进房 / 读档且旗已立 | **静默** ApplyVisual（待机关、动画开），**不再** BlackPanel |
| 续聊已播但旗未立（异常中断） | 倾向：进房静默 Apply 或补一次黑幕——施工默认 **静默 Apply**（少打扰）；OPEN 可改 |

**存档键倾向**（与 `StoryTriggerCountData` 同路，免新字段）：

- 键名建议：`Village_ChiefHouse_GushaAnimStandby`（或报告等价常量）  
- `OnStoryTriggered` 在换人成功后调用（语义=「已换过」）  
- **不要**仅用 `Village_村长家继续对话` 已用替代换人旗（续聊记档在 `OnStoryEnd` 早于黑幕换人完成，中断时会不同步）

`OnEnterScene` 伪序：

```
ApplyGushaVisualFromArchive()  // 旗已立 → 静默对；未立 → 待机开/动画关
TryTriggerChiefContinueOnce()  // 成功则订 onStoryEnd → 黑幕换人
```

---

## ⑦ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 在 **`Prefab/村长家合层.prefab`**（场景真源）预置 `古莎动画合层` 实例：默认关、脚位≈`古莎待机`、关「背景」、Sorting 对齐 | **P0** |
| 2 | `Village_Chief_HouseSceneManager`：绑引用；续聊 Trigger 成功订 `onStoryEnd`；黑幕换人 + 记旗；`OnEnterScene` 先静默 Apply | **P0** |
| 3 | 解绑 / 防双订；校验续聊名 | **P0** |
| 4 | **不改**村外侧面、UI 立绘、续聊台本、Loading 进屋 | — |
| 5 | Animator 真动画 | **另案**（本期无 Clip） |

施工说明由【施工员】写：`施工说明/0901/…`。

---

## ⑧ 验收清单

同沟通摘要 §③；另：

- [ ] Hierarchy：换人前仅待机；换人后仅动画合层（背景子关）  
- [ ] 同档第二次进房无第二次换人黑幕  
- [ ] 晚宴台本结束 **不**触发本逻辑  

---

## ⑨ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 对话名钉死续聊？ | **`Village_村长家继续对话`** | ✅ |
| Q2 | 要不要 Animator？ | **先 SR 合层**；帧动画另案 | ✅ |
| Q3 | 改合层源 vs 场景 Override？ | 改 **`Prefab/村长家合层`**（场景引用份）预置 | ✅ |
| Q4 | 关「背景」？ | **是** | ✅ |
| Q5 | 读档已换跳过黑幕？ | **是**（静默 Active） | ✅ |
| Q6 | 与玩家关系？ | 场景 NPC 涂层，不替 Controllable | ✅ |
| Q7 | 续聊已用但换人旗未立？ | 默认静默 Apply | ⏳ |

---

## ⑩ 程序补充（速查）

| 锚点 | 用途 |
|------|------|
| `Village_Chief_HouseSceneManager` | G1 挂点；已有续聊 Trigger |
| `StoryComponentGSM.onStoryEnd` / `TriggerStory` | 结束信号 |
| `ChiefNearDoorStoryTrigger` | BlackPanel Show/Hide 样板 |
| `BlackFormLogic` / `ShowBlackFormArgs` | FadeShow + onShowEnd |
| `StoryTriggerCountData` | 换人单次旗 |
| `Prefab/村长家合层` · `古莎待机`/`村长` | 关旧 / 脚位参考 |
| `ArtRes/Animation/古莎动画合层.prefab` | 新资源；无 Animator |

**一句话**：续聊结束用黑幕在合层里「关待机、开动画合层」；预置好、剥背景、记旗，读档别再闪一次黑。
