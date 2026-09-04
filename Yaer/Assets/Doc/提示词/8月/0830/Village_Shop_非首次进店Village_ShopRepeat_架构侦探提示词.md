# Cursor Agent Prompt · Village_Shop：非首次进店播 `Village_ShopRepeat`（老板娘日常招呼）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景**：`Village_Shop` · 从村 `Door_Shop` 正常进店  
> **对白资产（产品指定）**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab`  
> **产品目标（白话）**：  
> - **第一次**进店：仍播既有 **`Village_ShopStart`**（只播一次，存档旗标不变）  
> - **第一次之后**每次正常进店：播老板娘短招呼 **`Village_ShopRepeat`**，再进入买卖  
> **关联**：`ShouldPlayShopStartStory` · `TryDeferBlackFadeForCover` · `TryTriggerShopStartStoryOnce` · `OnShopStartStoryEnd` · Head/Chest 特殊对白 · Yes/No 购买对白  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV / 存档  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_Shop_非首次进店Village_ShopRepeat_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 进店次数（同档） | 对白 Prefab | 预扫文案（须用 Prefab 真源核实） |
|------------------|-------------|------------------------------|
| **第 1 次** | `Village_ShopStart` | 既有首次长对白（三角色）；**本期不改台本** |
| **第 2 次及以后** | `Village_ShopRepeat` | 店：「欢迎~」→「来看看吧~」→「可能有你喜欢的~」（Face 预扫末句 1） |

触发：玩家**正常进店**（换场进 `Village_Shop`），不是点头/点胸、不是购买成败 Yes/No。

### 现网缺口假说（0827 旧产品 → 0830 新产品）

| 项 | 0827 / 现网 | 新产品 |
|----|-------------|--------|
| 首次 | `ShopStart` + 存档只播一次 | **保持** |
| 非首次 | `ShouldPlayShopStartStory()==false` → **不播对白**，直接 UI + 热区 ON | → 播 **`Village_ShopRepeat`**，结束后再买卖 |
| 代码锚点 | `OnInit` 藏 UI（仅首次）；`TryDeferBlackFadeForCover`（仅首次）；`OnEnterScene` 非首次只对焦/开热区 | 非首次也要 Trigger Repeat + 藏 UI/热区 |

```
进 Village_Shop
  ├─ CheckStoryUsed("Village_ShopStart")==false  → 现网 ShopStart 黑幕路径（保持）
  └─ else（已播过 Start）
        → ❌ 现网：无 Trigger，直接 Idle 买卖
        → ✅ 新产品：Trigger "Village_ShopRepeat" → onStoryEnd → 显 UI_Shop + 热区
```

### 与其它对白的边界（防混接）

| Prefab | 用途 | 本期 |
|--------|------|------|
| `Village_ShopStart` | 同档**仅首次**进店 | 保持；勿改成 Repeat |
| `Village_ShopRepeat` | **非首次**每次进店招呼 | ✅ 新接线 |
| `Village_ShopHead` / Chest | 点头/点胸特殊 | ❌ 不改；对白中互斥 |
| `Village_ShopYes` / `No` | 购买成败 | ❌ 不改；≠进店招呼 |

### 触发 / 时序方案倾向（侦探拍板）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **R1 · 复用进店管线，分支故事名** | `ShouldPlayShopStart`→Start；否则→Repeat；藏 UI / 热区 / onStoryEnd 显 UI 对齐 Start（黑幕 Defer 是否复用见下） | **✅ 推荐** |
| R2 · 仅 OnEnterScene 补 Trigger Repeat | 不动 DeferCover | ⚠️ 可能闪一帧 Bar；须对照 Start 闪店修复 |
| R3 · ShopFormLogic Awake 播 | UI 层开剧情 | ❌ 易漏藏 UI / 与 GSM 状态机打架 |

**存档**：Repeat **不要** `CheckStoryUsed` 只播一次——应**每次**非首次进店都播（除非侦探发现产品要「每 N 次」；默认**每次**）。  
Start 仍用 `StoryTriggerCountData` 键 `Village_ShopStart`。

**黑幕**：Start 有 `TryDeferBlackFadeForCover`。Repeat 是否同样黑幕内 Trigger？倾向 **是**（防闪店），或拍板「短招呼可 OnEnter 后 Trigger + 先藏 UI」——侦探对照 Start 闪店报告裁定最小成本。

**结束**：对齐 Start 的 `onStoryEnd`（是否要结束慢黑幕 hold？短招呼可简化为直接显 UI；侦探写清与 Start 同/异）。

### Prefab 完备性（对照 ShopStart / ShopNo）

对 `Village_ShopRepeat` 出表：

| 检查 | 期望 / 预扫 |
|------|-------------|
| 根名 = 文件名 | `Village_ShopRepeat` → `DialoguePath.GetPath` 可加载 |
| Actor | 预扫仅 **Merchant（老板娘）**；有无雅/古？ |
| 店句 `UseShopkeeperPortrait` | ✅ 预扫 true |
| 句数 / 文案 | 短招呼 3 句量级 |
| 雅大立绘 / 分层闸门 | 预扫 BB 有 GoOut 变量？是否绑定？短句是否可不抄 Start 分层 |
| 与 Start 互斥 | 同档不会同帧既 Start 又 Repeat |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 非首次进店 → Repeat 接线设计 | ❌ 改写 ShopStart 台本 |
| ✅ Prefab 可播性 / 缺口表 | ❌ 购买 Yes/No、点头 Chest |
| ✅ 与 Start 旗标、黑幕、UI 藏显、热区互斥 | ❌ 新做第二套进店存档键「只播一次 Repeat」 |
| ✅ 最小施工清单 | ❌ 扩 DialogueFaceType |

### 严禁（本阶段）

- 改代码 / Prefab / CSV / 场景 / 存档  
- 把 Repeat 接到购买成功或点头出口  
- 用 `CheckStoryUsed("Village_ShopRepeat")` 导致第二次之后再也不打招呼（除非产品书面改口）  
- 非首次仍「直接买卖、无老板娘话」当已完成  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_ShopRepeat.prefab` | 对白真源 |
| `Village_ShopStart.prefab` | 首次对照 / 时序复用 |
| `Village_ShopSceneManager.cs` | 进店 Trigger / 旗标 / 黑幕 / UI |
| `0827/...ShopStart联调_架构溯源报告.md` | 旧「二进宫不播」作废点 |
| `StoryTriggerCountData` | 仅 Start 只播一次 |
| Head / Yes / No 常量 | 边界 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、存档。只读扫描 + 写「非首次进店 → Village_ShopRepeat」溯源报告。

---

## 背景（策划白话）

1. 第一次进店：老板娘首次长对白（`Village_ShopStart`）——已有，保持。  
2. **之后每次正常进店**：也要有老板娘招呼对白，用新 Prefab **`Village_ShopRepeat`**（欢迎 / 来看看吧 等短句）。  
3. 播完再进入正常买卖；点头、买成买败对白不动。  
4. 本阶段只摸清：挂在哪、和 Start 怎么分支、黑幕/藏 UI 要否照抄、Prefab 能否直接 Trigger。

---

## 侦探任务清单

### A. 钉死 Repeat Prefab 内容
出表：Actor、句数、文案、ShopFace/Body、UseShopkeeperPortrait、雅立绘/BB、根名是否可被 `TriggerStory("Village_ShopRepeat")` 加载。

### B. 钉死现网进店分支
画 `OnInit` / `TryDeferBlackFadeForCover` / `OnEnterScene` / `ShouldPlayShopStartStory` / `onStoryEnd` 树。  
标出：**首次 Start**、**非首次现网（无对白）**、**非首次应插 Repeat 的挂点**。

### C. 接线方案拍板
推荐 R1：同一进店管线按旗标选故事名（Start vs Repeat）。  
写清：
- Repeat **每次**播（不写 used 旗标）  
- 对白中藏 `UI_Shop`、关热区、`HasRunningStory` 挡特殊交互  
- 结束后显 UI + 热区 + 合层脸 Reset 是否复用 Start 结束黑幕  

### D. 黑幕 / 闪店
对照 Start 的 DeferCover：Repeat 是否必须黑幕内 Trigger？最小防闪方案写进施工清单。

### E. 状态机与互斥
与 Head/Chest、Yes/No、ESC 离店共存；Repeat 播放中禁止再开第二段进店对白。

### F. Prefab 最小补齐（若缺）
立绘/分层/Actor——对照 Start；短招呼能否只出合层脸+对话框。

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 常量 `ShopRepeatStoryName = "Village_ShopRepeat"` | **P0** |
| 2 | 非首次进店 Trigger Repeat（挂点按拍板） | **P0** |
| 3 | 对白中藏 UI / 关热区；结束恢复 | **P0** |
| 4 | 不把 Repeat 记入「只播一次」 | **P0** |
| 5 | 黑幕防闪（若需要） | P0/P1 按报告 |
| 6 | Prefab 缺口补齐 | P1 |

### H. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | **新档**第一次 Door_Shop 进店 | 仍播 **ShopStart**（长对白）；结束后买卖 |
| 2 | 同档**第二次**进店 | 播 **ShopRepeat**（欢迎~…）；藏买卖 UI；结束回 Idle |
| 3 | 同档**第三次**进店 | **再播** ShopRepeat（不是静默） |
| 4 | Repeat 中点头 / ESC | 不叠特殊对白；ESC 行为对齐现网对白中规则 |
| 5 | 读档后进店 | 若 Start 已 used → Repeat；未 used → Start |
| 6 | Console | `TriggerStory Village_ShopRepeat`；无 Missing Prefab |

### I. 开放问题
- Repeat 结束要否与 Start 同款慢黑幕 hold？  
- 纯 Debug 进店（非 Door_Shop）是否也播 Repeat？  
- 0629 旧文「二进宫不播」作废声明写哪？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_Shop_非首次进店Village_ShopRepeat_架构溯源报告.md`

MASTER 四段式：  
① 结论（分支：首次 Start / 非首次 Repeat + 挂点）  
② 原因（现网二进宫为何静默；新产品要每次招呼）  
③ 用户检查清单（一进 / 二进 / 三进怎么验）  
④ 给程序：分支树 + Prefab 表 + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_Shop_非首次进店Village_ShopRepeat_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs

你现在是【施工员】。按报告实现：非首次正常进店播 Village_ShopRepeat。

必须遵守：
- 首次仍只播 Village_ShopStart（存档只播一次）；Repeat 每次非首次进店都播，禁止误写成只播一次；
- 经 GSM 进店管线 Trigger，故事名与 Prefab 文件名一致；对白中藏 UI_Shop、关热区，结束恢复；
- 不要接到购买 Yes/No 或点头 Chest；不要改 Start 台本；
- 代码含详细注释；重要取舍写清原因（尤其与 Start 黑幕/结束流程的同异）。

提交说明：挂点、首次/非首次分支、如何验收、未做项。
```
