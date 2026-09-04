# Cursor Agent Prompt · Village_Shop：购买成功 / 失败 → 播 `Village_ShopYes` / `Village_ShopNo` 对话

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **场景**：`Village_Shop` · 点「决定」购买  
> **对白资产（产品指定）**：  
> - **成功**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab`  
> - **失败**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab`  
> **产品目标（白话）**：买成了老板娘（及对白）要有反应；买失败（尤其钱不够）也要有对话反应——不是只打 Console  
> **关联**：`OnConfirmClick` 扣款/入包 · `TryTriggerShopkeeperSpecial` · Head/点头特殊对白 · 货币旁路 `bypassGoldCheckForBagJoint` · 0829 购买成功失败检测提示词  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 结果 | 对白 Prefab | 预扫文案（须用图内真源核实） |
|------|-------------|------------------------------|
| **购买成功** | `Village_ShopYes` | 店：「谢谢惠顾~~」（Face 预扫 2） |
| **购买失败** | `Village_ShopNo` | 店：「哎哟，你好像没什么钱呢。」（Face 预扫 3） |

触发：玩家在商店点 **「决定」** 走完校验后，按成败 **播对应短对白**。

### 现网购买链路假说（缺口 = 无对白）

```
OnConfirmClick
  → 出售 Tab？ → 仅 Log，本期是否播 No？（开放）
  → 数量 0 / 堆叠超 / 存档空 → 失败 Log，return  ← 是否播 No？
  → bypass 关：TrySpend 失败 → LogInsufficientGold，return  ← 【应对 ShopNo】
  → 入包 + Save → Log 成功，清数量  ← 【应对 ShopYes】
  → ❌ 现网无 TriggerStory(Yes/No)
```

### 与特殊对白架构的关系（复用，勿另起炉灶）

现网点头/点胸已有：

```
Village_ShopSceneManager.TryTriggerShopkeeperSpecial(storyName)
  → HasRunningStory 守卫
  → HideShopUiRoot
  → TriggerStory(name)   // 名 = Prefab 文件名
  → onStoryEnd → Show UI + 热区 + ResetDefault…
```

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · 成败也走 `TryTriggerShopkeeperSpecial`** | 成功调 `"Village_ShopYes"`；钱不够调 `"Village_ShopNo"` | **✅ 推荐**（Hide/Show/互斥现成） |
| **B · ShopFormLogic 直接 TriggerStory** | UI 调 StoryGSM | ❌ 易漏藏 UI / 叠对白 |
| **C · 仅 TipsPanel 文字** | — | ❌ 产品已指定对话 Prefab |

`ShopFormLogic` 应通知 GSM（事件/接口/Find SceneManager），**不要** UI 直接开剧情。

### 哪些失败播 `ShopNo`（侦探必拍板）

| 失败原因 | 是否播 No | 助手倾向 |
|----------|-----------|----------|
| **金币不足** | ✅ | **必须**（文案对得上） |
| 数量全 0 | ❌ 或仅 Warning | 倾向 **不播**（还没形成交易意图） |
| 堆叠将超 | ？ | 倾向 **不播 No** 或另文案；文案是「没钱」→ 不宜 |
| 存档不可用 | ❌ | 开发错误 |
| 出售未实现 | ❌ | |
| bypass=true 时「钱不够」 | **测不到 No** | 正式成败对白验收须 **关旁路** |

成功：仅 **真正入包成功** 后播 Yes（旁路成功入包也算成功？倾向 **是**，仍播 Yes；但货币验收关旁路）。

### Prefab 完备性（对照 ShopHead / ShopStart）

对 `Village_ShopYes` / `Village_ShopNo` 各出表：

| 检查 | 期望 |
|------|------|
| 根名 = 文件名 | `Village_ShopYes` / `Village_ShopNo` |
| Actor | Merchant + 是否需要 Yaer |
| 店句 `UseShopkeeperPortrait` | ✅ |
| 雅大立绘 | 有无嵌 GoOut；alpha/淡入是否对齐「先立绘后框」（若短句可简化，侦探裁定） |
| 句数 | 短反应即可；是否含雅句 |
| 与 Head 互斥 | HasRunningStory 已挡 |

### 时序 / UX（必查）

```
点决定
  →（先）扣款+入包 或 判定失败   // 钱/包已定结果
  → Hide UI + TriggerStory Yes/No
  → 对白中禁止再点决定 / 热区
  → onStoryEnd → Show UI；成功则数量已清；失败未改包
```

| 问 | 倾向 |
|----|------|
| 先播对白再扣款？ | ❌ **先结算再对白**（避免对白中途失败不一致） |
| 对白中 ESC 离店？ | 现网 HasRunningStory 已忽略 ESC；保持 |
| Yes/No 要否 Reset 合层脸？ | 对齐特殊对白结束 ResetDefault |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 成败 → Yes/No Trigger 链路设计 | ❌ 重做首次进店 / 点头台本 |
| ✅ Prefab 缺口表（立绘/时序） | ❌ 出售成交对白（除非同挂） |
| ✅ 旁路与验收关系 | ❌ 新 Tips 替代对话 |
| ✅ 最小施工清单 | ❌ 扩 DialogueFaceType |

### 严禁（本阶段）

- 改代码 / Prefab / CSV  
- UI 直接 `TriggerStory` 绕过 GSM  
- 旁路开启时宣称「失败对白已验收」  
- 把堆叠失败硬播「没钱」No（文案不符）  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_ShopYes.prefab` / `Village_ShopNo.prefab` | 对白真源 |
| `ShopFormLogic.OnConfirmClick` | 挂接点 |
| `Village_ShopSceneManager.TryTriggerShopkeeperSpecial` | 复用 |
| `ShopDebugLogger` | 现网仅 Log |
| Head / ShopStart 互斥 | 状态机 |
| 货币旁路字段 | 失败路径前提 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/Doc/提示词/0829/Village_Shop_购买成功失败与剩余货币检测_架构侦探提示词.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「购买成败 → ShopYes/ShopNo」溯源报告。

---

## 背景（策划白话）

1. 商店买成了要播 **`Village_ShopYes`**；买失败（钱不够等）要播 **`Village_ShopNo`**。  
2. Prefab 已在工程里；缺的是点「决定」之后怎么接到 TriggerStory。  
3. 本阶段只摸清：挂在哪、和点头对白怎么互斥、Prefab 是否可播、旁路怎么影响失败验收。

---

## 侦探任务清单

### A. 钉死两 Prefab 内容
各出表：Actor、句数、文案、Face/Body、雅立绘/淡入、根名是否可被 `DialoguePath.GetPath` 加载。

### B. 钉死 OnConfirmClick 成败出口
画分支树；标出 **应播 Yes / 应播 No / 只 Log 不播**。

### C. 接线方案拍板
推荐 A（经 GSM `TryTriggerShopkeeperSpecial` 或专用薄封装 `TryTriggerPurchaseResult(bool success)`）。  
`ShopFormLogic` → GSM 的调用方式（强制转换 Village_ShopSceneManager / 接口）。

### D. 状态机与互斥
与 ShopStart / Head / Chest / ESC 离店共存；对白中决定按钮是否需额外禁用。

### E. Prefab 最小补齐（若缺）
立绘 alpha、BB、先立绘后框——对照 ShopHead 报告，成败短句是否可简化。

### F. 最小施工清单（本阶段不执行）
| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 成功入包后 Trigger `Village_ShopYes` | **P0** |
| 2 | 金币不足 Trigger `Village_ShopNo` | **P0** |
| 3 | 关旁路说明 / 验收开关 | **P0** |
| 4 | 其它失败不误播 No | P0 |
| 5 | Prefab 立绘/时序缺口 | P1 |
| 6 | 结束 Reset 合层脸 | P1（若特殊对白已统一则复用） |

### G. 验收清单
| # | 操作 | 通过 |
|---|------|------|
| 1 | 钱够买 → 决定 | 入包扣款后播 **Yes**；藏买卖 UI；结束回 Idle |
| 2 | 钱不够（旁路关）→ 决定 | 不入包；播 **No** |
| 3 | 数量 0 → 决定 | 不播 No（按拍板） |
| 4 | 对白中再点决定/头 | 不叠第二段 |
| 5 | Console | 故事名=Prefab 名；无 Missing Prefab |

### H. 开放问题
- 堆叠失败播什么？  
- bypass 成功入包是否仍播 Yes？  
- Yes/No 要否雅大立绘完整分层？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构溯源报告.md`

MASTER 四段式：  
① 结论（接线方案 + 哪些出口播 Yes/No）  
② 原因（现网只有 Log；复用特殊对白 GSM）  
③ 用户检查清单（够钱/不够钱怎么验）  
④ 给程序：分支表 + Prefab 表 + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs

你现在是【施工员】。按报告实现：购买成功播 Village_ShopYes，金币不足等拍板失败播 Village_ShopNo。

必须遵守：
- 先结算再对白；经 GSM 触发，禁止 ShopFormLogic 直接乱 TriggerStory；
- 故事名与 Prefab 文件名一致；复用 Hide/Show UI 与 HasRunningStory；
- 不要把数量 0 / 堆叠失败误播成「没钱」No（除非报告写明）；
- 正式验失败对白须关货币旁路；
- 代码含详细注释；重要取舍写清原因。

提交说明：挂点、Yes/No 各自触发条件、如何验收、未做项。
```
