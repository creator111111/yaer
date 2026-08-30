# Village_Shop — 非首次进店 `Village_ShopRepeat` — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 接线拍板（**本阶段未改代码 / Prefab / 场景 / CSV / 存档**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Shop` · 正常换场进店（Door_Shop / LoadScene）  
**对白资产**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab`  
**产品目标**：  
- **第 1 次**进店：仍播 **`Village_ShopStart`**（存档只播一次）  
- **第 2 次及以后**：每次播 **`Village_ShopRepeat`**，结束后再买卖  

关联：`ShouldPlayShopStartStory` · `TryDeferBlackFadeForCover` · `OnEnterScene` · Head/Chest · Yes/No · `0827/...ShopStart联调`

---

## ① 结论一句话

**现网非首次进店因 `ShouldPlayShopStartStory()==false` 直接 Idle（无 Trigger），属 0827「二进宫不播」旧产品，0830 作废。推荐 R1：同一进店黑幕管线按旗标分支——Start 保持；否则 Trigger `Village_ShopRepeat`（每次都播、不写 used）。Repeat Prefab 根名可加载、仅 Merchant 合层脸 4 句短招呼，无需抄 Start 雅/古分层；进店黑幕 Defer 防闪要抄，结束黑幕对齐特殊对白（直接显 UI，不做 Start 那套慢黑幕）。**

---

## ② 原因（通俗）

第一次进店老板娘有长段开场，工程用存档记「播过了」，第二次再进就**故意不说话**，直接露出买卖栏——这是旧设定。

新产品要：第二次、第三次……每次进门老板娘都短短打个招呼（欢迎 / 来看看吧……），说完再让你点货。  
招呼稿 `Village_ShopRepeat` 已经做好了，**代码还没接到进店分支上**，所以二进宫仍静默。

---

## ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | **新档**第一次 Door_Shop 进店 | 仍播 **ShopStart**（长对白）；结束后买卖；买卖 UI 对白中不闪 |
| 2 | 同档**第二次**进店 | 播 **ShopRepeat**（欢迎~ → …）；对白中藏买卖 UI、热区关；结束回 Idle |
| 3 | 同档**第三次**进店 | **再播** ShopRepeat（不是静默） |
| 4 | Repeat 中点头 / ESC | 不叠 Head/Chest/Yes/No；ESC 对齐现网（`HasRunningStory` 忽略离店） |
| 5 | 读档后进店 | Start 已 used → Repeat；未 used → Start |
| 6 | Console | `TriggerStory Village_ShopRepeat`；无 Missing Prefab |
| 7 | （对照）点头 / 购买成败 | 仍走 Head / Yes / No，**不是** Repeat |

---

## ④ 给程序

### A. `Village_ShopRepeat` Prefab 内容（磁盘真源）

| 检查 | 结果 |
|------|------|
| 根 `m_Name` | ✅ `Village_ShopRepeat`（= 文件名 → `DialoguePath.GetPath` / `TriggerStory` 可加载） |
| Actor | **仅 Merchant（老板娘）**；无雅、无古 |
| 句数 | **4** 句（预扫写「3 句量级」；真源多第 4 句，本期**不改台本**，验收按 4 句） |
| 文案 | ①「欢迎~」②「来看看吧~」③「可能有你喜欢的~」④「出门在外没有补剂怎么行？」 |
| `UseShopkeeperPortrait` | ✅ 四句均为 true |
| ShopFace | ①② 默认；③④ **Face=1**；ShopBody 空 |
| 雅大立绘 / 分层 | BB 有孤儿 `GoOutStoryYaerPainting`，**`_objectReferences: []` 未绑**；图内无 CanvasGroup 淡入 → **不必**抄 Start 分层闸门 |
| 与 Start 互斥 | 同档：Start used 前只走 Start；used 后只走 Repeat → 不会同帧双开 |

**可播性**：✅ 故事名对齐即可 Trigger；店合层脸走现网 Portrait。

### B. 现网进店分支树（缺口钉死）

```
进 Village_Shop
├─ OnInit
│    ├─ CloseFightingPanel
│    └─ if ShouldPlayShopStartStory()     // !CheckStoryUsed("Village_ShopStart")
│         → Hide UI_Shop + 热区 OFF
│       else
│         → ❌ 不藏 UI（非首次现网：黑幕淡出后可能先露 Bar）
│
├─ TryDeferBlackFadeForCover
│    ├─ if ShouldPlayShopStart
│    │    → 黑幕内 Trigger Village_ShopStart + 分层准备 + onStoryEnd 慢黑幕显 UI  ✅
│    └─ else
│         → return false（默认换场淡出）          ❌ 无 Repeat
│
└─ OnEnterScene
     ├─ 非首次：对焦合层 + ResetDefault 脸
     ├─ TryTriggerShopStartStoryOnce()     // 仅首次兜底；非首次直接 return
     └─ 非首次且无 RunningStory → 热区 ON   // 直接 Idle 买卖
```

| 路径 | 现网 | 新产品 |
|------|------|--------|
| 首次 | Start + DeferCover | **保持** |
| 非首次 | **无对白**，直接 Idle | → **`Village_ShopRepeat`**，结束后 Idle |
| 挂点 | — | **`TryDeferBlackFadeForCover` 非首次分支**（主）；`OnEnterScene` 兜底（Defer 未跑时） |

**0827 / 0629 作废点**：「二进宫 `CheckStoryUsed` → 直接买卖、无重复 Trigger」——对 **Start** 仍成立；对「进店完全无话」**作废**，改由 Repeat 承接。

### C. 接线方案拍板（R1）

| 方案 | 裁定 |
|------|------|
| **R1 · 同一进店管线，分支故事名** | ✅ **推荐** |
| R2 · 仅 OnEnterScene 补 Trigger | ⚠️ 易闪 Bar（0827 F2）；须另藏 UI，不如直接扩 Defer |
| R3 · ShopFormLogic Awake 播 | ❌ UI 开剧情，与 GSM 打架 |

**R1 行为钉死**

| 项 | 决议 |
|----|------|
| 故事名常量 | `ShopRepeatStoryName = "Village_ShopRepeat"` |
| Start 旗标 | 仍仅 `CheckStoryUsed("Village_ShopStart")`；**不变** |
| Repeat 存档 | **禁止** `CheckStoryUsed("Village_ShopRepeat")` → **每次**非首次进店都播 |
| 对白中 | Hide `UI_Shop` + 热区 OFF；`HasRunningStory` 挡 Head/Chest/Yes/No/ESC 离店 |
| 结束 | **对齐特殊对白** `OnShopkeeperSpecialStoryEnd`：`ResetDefault` + Show UI + 热区 ON；**不做** Start 结束慢黑幕（2s+hold+2s） |
| 分层闸门 | Repeat **跳过** `PrepareShopStartLayeredReveal` / `ShopStartLayerRevealGate`（无大立绘） |

**伪代码（施工意图）**

```
TryDeferBlackFadeForCover(close):
  if ShouldPlayShopStart → 现网 Start 路径（不动）
  else:
    // 非首次：Repeat 进店招呼
    Hide UI + 热区 OFF + 锁相机对焦
    TriggerStory(ShopRepeatStoryName)
    onStoryTriggered →（短 hold）CloseFormFade   // 无分层 Prepare
    onStoryEnd → OnShopRepeatStoryEnd（= Special 结束语义）
    return true

OnInit:
  // 首次或非首次：进店招呼前都藏 UI（防闪）
  Hide UI + 热区 OFF   // 或：Start||将播 Repeat 时藏（正规进店即总藏）

OnEnterScene:
  非首次勿立刻热区 ON；若 Defer 已起 Repeat / HasRunningStory → 等 onStoryEnd
  兜底：Defer 未跑且 Start 已 used → Trigger Repeat + 订 onStoryEnd
```

### D. 黑幕 / 闪店

| 阶段 | Start | Repeat（拍板） |
|------|-------|----------------|
| 进店换场黑幕内 Trigger | ✅ DeferCover | ✅ **同样**（最小防闪，对齐 0827） |
| 黑幕下藏 UI / 关热区 / 对焦 | ✅ | ✅ |
| 雅/古分层显现 | ✅ | ❌ 不需要 |
| 对白结束慢黑幕 | ✅ 2s+0.4hold+2s | ❌ **不要**（短招呼体感；对齐 Head/Yes/No） |

### E. 状态机与互斥

| 场景 | 行为 |
|------|------|
| Repeat 进行中 | UI 藏；热区 OFF；`HasRunningStory` → Head/Chest/Yes/No 忽略；ESC 离店忽略 |
| 仍处首次 Start 窗口 | 现网 Special 已拒；Repeat 也不会与 Start 同开 |
| Start 与 Repeat | 互斥：`ShouldPlayShopStart` 二选一 |
| 购买 Yes/No、点头 Chest | **不改**；勿把 Repeat 接到这些出口 |
| Debug / 非 Door 进店 | **同样**走 GSM 进店管线：Start used → Repeat（不绑死 Door_Shop） |

### F. Prefab 最小补齐

| 项 | 优先级 | 说明 |
|----|--------|------|
| 根名 / 文案 / Portrait 店句 | 已齐 | P0 只接线 |
| 删第 4 句 / 改成 3 句 | ❌ 除非产品改口 | 真源是 4 句 |
| 绑 GoOut 雅立绘 + 分层 | **本期不做** | 短招呼合层脸+对话框即可 |
| 删孤儿 BB 变量 | P2 | 不影响 Trigger |

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | GSM 常量 `ShopRepeatStoryName = "Village_ShopRepeat"` | **P0** |
| 2 | `TryDeferBlackFadeForCover`：非首次分支 Trigger Repeat + 订结束 | **P0** |
| 3 | `OnInit`：非首次进店也先藏 UI / 关热区（防闪） | **P0** |
| 4 | `OnEnterScene`：非首次勿抢先热区 ON；补 Repeat 兜底 Trigger | **P0** |
| 5 | `OnShopRepeatStoryEnd`：ResetDefault + Show UI + 热区（可与 Special 共用实现） | **P0** |
| 6 | **不**写 Repeat 的 `CheckStoryUsed` / AddCount 只播一次 | **P0** |
| 7 | 注释：0827「二进宫静默」作废；Repeat ≠ Start/Yes/No/Head | **P0** |
| 8 | Prefab 大立绘 / 改台本 | ❌ / 产品另开 |

**预期 diff**

- 主改：`Village_ShopSceneManager.cs`  
- **一般不改** Repeat/Start Prefab、ShopFormLogic、购买/点头常量  

### H. 验收清单

同 §③。

### I. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | Repeat 结束要否与 Start 同款慢黑幕？ | **否**，对齐 Special 直接显 UI | ✅ 本报告 |
| Q2 | 纯 Debug 进店是否也播 Repeat？ | **是**（凡正规进 `Village_Shop` 且 Start 已 used） | ✅ |
| Q3 | 0629/0827「二进宫不播」作废写哪？ | **本报告 + OPEN_QUESTIONS**；施工注释再钉一句 | ✅ |
| Q4 | Prefab 第 4 句是否保留？ | **保留**（真源 4 句）；产品要砍再另开 | ✅ 倾向 |
| Q5 | Repeat 是否每 N 次才播？ | **否**，每次非首次 | ✅ 产品默认 |

（已追加 `OPEN_QUESTIONS.md`。）
