# Village_村长家继续对话 — 开场分层淡入对齐门口 — 架构溯源报告

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【架构侦探】只读定根因（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**现象**：进屋自动播 `Village_村长家继续对话` **硬切齐活**（无黑屏→立绘→对话框依次淡入感）  
**对标真理源**：`Village_村长家门口初次对话`（黑屏压住 → 立绘淡入 → 对话框淡入 → 再说话）  
**不是**：改台本 / 摆位定稿 / 取消自动续聊 / 开蛋糕 LoadingPanel  
**提示词**：`提示词/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构侦探提示词.md`  

**关联**：
- `执行文档/0902/Village_门口进村长家_改黑屏切场_架构溯源报告.md`（换场黑幕 + F1′ defer）
- `执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md`（PrepareMask 关）
- `执行文档/0901/...三人大立绘摆位对齐门口_*`（布局已齐，本期不改 Pos）
- `ChiefNearDoorStoryTrigger` / `Village_KenMuNiStart` 分层样板

---

## 沟通摘要

### ① 结论一句话

**续聊 Prefab 前奏节点与门口基本同构（三路 CanvasGroup + UIAlpha），硬切主因不是「没做淡入」，而是揭黑时机早于立绘 Instantiated/StartAlpha 落地（H1b/H4），叠加续聊仍 `PrepareMaskAvatarOnFadeIn=true`。施工推荐 T1′：全黑内备好 alpha=0 再揭黑让玩家看见 0→1；并关掉续聊预亮；勿整壳覆盖丢针线包。**

### ② 原因（通俗）

门口和续聊图里都有「立绘淡入、对话框淡入」。  
问题是进屋续聊用换场黑幕盖着就开了戏，壳一报就绪就揭黑——但对话树还要等一帧才 Instantiate，玩家先看到空房间，再突然齐活，就像没淡入。  
另外续聊 Setup 默认仍勾「框出预亮小头像」，和门口「空框」决议也不一致。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进屋续聊：可见压黑 → 立绘淡入 → 对话框淡入，**非三件套瞬现** | |
| 2 | 时长手感接近门口初次（约立绘 1s → 框 Delay0.5+1s） | |
| 3 | 首句前框可空字；小头像不早于首句乱亮 | |
| 4 | 三人摆位/Scale 仍对齐门口定稿 | |
| 5 | 针线包 GetItem / Tips / Save 节点仍在 | |
| 6 | 门口初次分层回归未坏；无蛋糕读条 | |
| 7 | 续聊结束换古莎黑幕（0901）仍正常 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| Prefab 前奏同构？ | **基本是**（0 Fighting → 1 并行三路 CanvasGroupAlpha → 2 UIAlpha → Statement） |
| 硬切主因 | **H1b + H4**：`TryDeferBlackFadeForCover` 在 `onStoryTriggered` 后仅 hold **0.1s** 就 `CloseFormFade`；而 `StartDialogue`→`OnDialogueLoaded` 有 **`await UniTask.Yield()` + Instantiate**，揭黑时常尚无树/未执行 `cg.alpha=StartAlpha` |
| H2 StartAlpha | 两侧任务均为 `StartAlpha:{}`（运行时当 0）；Builder 会 `EnsureFloatParam(0)`；**非主因**，建议显式 `_value:0` 防回潮 |
| H3 图连接 | **否**：两边 `executionMode=1` 并行三立绘、Duration=1、UI Delay=0.5、均 `EndActionOnAnimationEnd/EndActon=true` |
| 续聊 PrepareMask | **仍 true**（门口已 false）→ 须关，对齐 0902 空框 |
| 门口结束淡出？ | 图内 **无** EndAlpha=0 UI/立绘淡出；结束直接切场。续聊结束另有 **GSM 换古莎 BlackPanel**（0901）≠ 开场分层 |
| 本期结束淡出？ | **不做**（门口开场真理源无「对白框淡出」义务；结束换人另案已有） |
| 方案 | **T1′（推荐）** 全黑内 Prepare alpha=0 + Instantiated 后再揭黑；辅 **T3** 显式 StartAlpha=0、PrepareMask=false；**禁止** T4 整壳覆盖、T5 Loading |

---

## ② 门口初次对话全链（任务 1 · 真理源）

```
Npc_Chief Enter
  → ChiefNearDoorStoryTrigger
       OpenSystemBlackFade（对话专用 BlackPanel）
       → 全黑：侧面涂层 + TriggerStory(门口初次对话)
       → onStoryTriggered → hold 0.1s → HideFade
       →（同拍）StartDialogue → Yield → Instantiate → 图开跑
  → Prefab 前奏：
       [0] FightingPanelVisible
       [1] ActionList Parallel：Yaer / Gusha / Chief CanvasGroupAlpha
            StartAlpha≈0 → EndAlpha=1，Duration=1，EndActionOnAnimationEnd=true
       [2] NormalDialogueUIAlpha
            Delay=0.5，Duration=1，EndActon=true
            PrepareMaskAvatarOnFadeIn = false（0902 已关）
       [3+] Statement…
  → 对白结束 → LoadScene(Chief_House) 黑幕进屋（0902 F1 已落地）
```

| 磁盘项 | 门口 Prefab |
|--------|-------------|
| 三路 CanvasGroupAlpha | 有；StartAlpha `{}`；End=1；Dur=1 |
| UIAlpha | 1 个；Delay=0.5；PrepareMask=**false** |
| 图内结束淡出 EndAlpha=0 | **0 处** |
| 立绘 m_Alpha Override | 现网多为 **1**（靠任务强制打到 StartAlpha 再淡） |

---

## ③ 进屋续聊全链（任务 2 · 被改方）

```
门口戏结束 → LoadScene(Chief_House, blackFade:true)
  → GSM Ready → TryDeferBlackFadeForCover（0902 F1′ 已在代码）
       若门闩应播续聊：
         TriggerStory(继续对话)
         onStoryTriggered → hold 0.1s → CloseFormFade（换场黑幕揭开）
         StartDialogue → Yield → Instantiate → 前奏图
  → OnEnterScene：TryTriggerChiefContinueOnce 仅兜底（已有剧情则跳过）
  → Prefab 前奏：与门口同序同参（见下表）
  → …中段 GetItem/Tips/Save…
  → onStoryEnd →（0901）另开 BlackPanel 换古莎待机→动画合层
```

### 玩家「看得见的淡入」窗口（现网问题）

| 时刻 | 黑幕 | 对话树 | 玩家所见 |
|------|------|--------|----------|
| t0 `onStoryTriggered` | 仍全黑 | 尚未 Instantiate | 黑 |
| t0+0.1s 开始 CloseFormFade | 淡出中 | 可能仍在 Yield | **空室内渐显** |
| t0+1frame+ Instantiate | 可能已较亮/已亮 | 才 `cg.alpha=0` 并 DOFade | 立绘/框突然接上 → **体感硬切** |

对比门口：同样有 Yield 缝，但有**对话专用黑幕 + 侧面涂层 staging**；续聊揭的是**换场黑幕**，下面是空客厅，缝更刺眼。

---

## ④ 两 Prefab 图头对比（任务 3 · 勿只看「有没有节点」）

| 项 | 门口初次 | 继续对话 | 同构？ |
|----|----------|----------|--------|
| 节点 0 | FightingPanelVisible | 同 | ✅ |
| 节点 1 | Parallel×3 CanvasGroupAlpha（雅/古/村） | 同；BB 名同 | ✅ |
| Duration / End wait | 1 / true | 1 / true | ✅ |
| 节点 2 UIAlpha Delay+Dur | 0.5 + 1 | 0.5 + 1 | ✅ |
| PrepareMaskAvatarOnFadeIn | **false** | **true** | ❌ 须改续聊 |
| MaskAvatarRole/Face | Yaer / Smug | Yaer / Smug | （预亮关则无关） |
| 中段专有 | 无 | GetItem / OpenTips / Save | 续聊保留 |
| 图内开场淡出 | 无 | 无 | ✅ |
| 立绘默认 alpha Override | 多为 1 | Yaer/Gusha 为 **0**（Setup 写入） | 异；任务都会打 StartAlpha |

**H3 证伪**：不是「续聊缺前奏节点」或串并行不一致。

---

## ⑤ 假说证伪（H1～H5）

| ID | 假说 | 结果 | 证据 |
|----|------|------|------|
| **H1** | 淡入在遮罩下播完再揭开 | ⚠ **历史主因（Loading 时代）**；现网已 F1 黑幕+F1′ | Loading 路径会 classic H1；当前黑幕路径改为 H1b |
| **H1b** | 揭黑早于 Instantiate / StartAlpha | ✅ **现网主因** | `onStoryTriggered` 先于 `StartDialogue`；`OnDialogueLoaded` 含 `Yield`；hold 仅 0.1s |
| **H2** | StartAlpha 空导致 1→1 | ❌ 非主因 | `StartAlpha != null ? value : 0`；空 BB 默认 0；Continue 磁盘立绘已 0 |
| **H3** | 图连接/阻塞不同 | ❌ | 连接 0→1→2→Statement；ActionList mode=1 同 |
| **H4** | 缺「壳就绪再揭黑」成对 / 与门口门控不等价 | ✅ **成立（体验差）** | 门口=对话专用 BlackPanel；续聊=换场黑幕在壳回调就揭，未等树就绪+alpha 备好 |
| **H5** | 改黑幕后更糟/更好 | 更好于 Loading 幕下播完；**仍不够**对齐门口体感 | F1′ 已落地仍可能空房闪后硬切 |

---

## ⑥ 门口结束淡出？本期范围（任务 4）

| 问 | 答 |
|----|-----|
| 门口图有立绘/对话框结束淡出吗？ | **无**（无 EndAlpha=0 任务） |
| 续聊结束黑幕？ | **有**——GSM 换古莎（0901），属结束演出，非开场分层 |
| 本期是否必须做续聊结束淡出？ | **否**；写入 OPEN：开场对齐即可 |

---

## ⑦ 方案与最小改动清单（任务 5）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **T1′ · 全黑内 Prepare 后再揭** | Defer 黑幕：`onStoryTriggered` **不要立刻 Close**；等对话树 Instantiate 后强制三立绘+字幕条 `alpha=0`（白名单，禁广扫 Mask），再 `CloseFormFade`；让玩家看见图内 0→1 | ✅ **最对齐门口精神** |
| **T1 · 进屋后再垫对话专用 BlackPanel** | 仿 NearDoor：Show→Trigger→壳就绪 HideFade（可与换场黑幕衔接） | ✅ 备选；多一次黑 |
| **T2 · 换场黑幕淡完再 Trigger** | 易露空房；需另遮 | ⚠️ 不如 T1′ |
| **T3 · Prefab 显式 StartAlpha=0 + PrepareMask=false** | 修续聊 BB；Setup 默认对齐门口 | ✅ **必做辅修** |
| T4 整 Prefab 覆盖门口 | 丢 GetItem/Tips | ❌ |
| T5 Loading 拖时间 | 产品否 | ❌ |

### 推荐组合

**T1′ + T3**（主）；若不愿动 GSM 时序，至少 T3 + 加长 `ContinueShellReadyHoldSeconds` 仅作弱缓解（仍不治本）。

### 施工最小清单

1. **`Village_Chief_HouseSceneManager`（T1′）**
   - `OnChiefContinueStoryTriggeredForCover`：延迟 Close，直到续聊树已 Instantiate 且三立绘/对话框 alpha 已置 0（可挂 FormLogic 回调 / 短轮询 / 去掉关键路径上多余 Yield 风险须谨慎）
   - 注释写清：对齐 NearDoor「揭黑时玩家才能看见淡入」；禁止幕下播完再揭
   - **禁止**广扫 `Painting` 名误伤 Mask（KenMuNi 教训）
2. **`Village_村长家继续对话.prefab`（T3）**
   - UIAlpha：`PrepareMaskAvatarOnFadeIn=false`
   - 三路 CanvasGroup + UIAlpha：显式 `StartAlpha._value=0`
3. **`VillageChiefContinueDialogueSetupEditor`**
   - `DialoguePreludeOptions.PrepareMaskAvatarOnFadeIn = false`（对齐 Door Setup）
   - 防 Setup 回潮预亮
4. **保留** 针线包节点；**勿改**门口 Prefab；**勿开** LoadingPanel
5. 回归门口初次 + 续聊结束换古莎

### stayAction？

换场 `stayAction` 在旧场景全黑时触发，**不能**挂新场景续聊 Prepare。继续用 **DeferCover**，改的是「何时 CloseFormFade」。

---

## ⑧ 与 0902 空头像衔接（任务 6）

| 项 | 要求 |
|----|------|
| 门口 | PrepareMask 已 false |
| 续聊对齐分层时 | **必须保持 PrepareMask=false**；禁止为「框+头像同拍」重开预亮 |
| Setup 默认 | Continue 现默认 `PrepareMaskAvatarOnFadeIn` 属性默认 **true** → 须显式 false |

---

## ⑨ OPEN / 残留风险

| ID | 项 | 决议 | 状态 |
|----|----|------|------|
| Q1 | 硬切主因？ | H1b/H4 揭黑早于树就绪；非缺前奏节点 | ✅ 侦探拍板 |
| Q2 | 方案？ | T1′ + T3 | ✅ 施工默认 |
| Q3 | 本期做结束淡出？ | **否** | ✅ |
| Q4 | 续聊 PrepareMask？ | **关** | ✅ |
| Q5 | 是否允许二次对话专用黑幕（T1）？ | T1′ 优先；不够再 T1 | ⏳ 验收决定 |

---

## ⑩ 代码锚点速查

| 主题 | 路径 |
|------|------|
| 门口黑幕成对 | `ChiefNearDoorStoryTrigger.cs` |
| 续聊 defer | `Village_Chief_HouseSceneManager.TryDeferBlackFadeForCover` |
| 壳信号早于树 | `StoryComponentGSM.OnStoryPrefabLoad`：`onStoryTriggered` → `StartDialogue` |
| Yield 缝 | `NormalDialogueFormNewLogic.OnDialogueLoaded`：`await UniTask.Yield()` |
| 淡入任务 | `CanvasGroupAlphaActionTask` / `NormalDialogueUIAlphaAnimationTaskAction` |
| Setup | `VillageChiefContinueDialogueSetupEditor` / `DialoguePreludeOptions` |

---

## ⑪ 给施工员的一句话

**图里已有淡入；要让玩家在揭黑之后才开跑/才看得见 0→1。先关续聊 PrepareMask，再把 CloseFormFade 挪到 alpha 备好之后。**
