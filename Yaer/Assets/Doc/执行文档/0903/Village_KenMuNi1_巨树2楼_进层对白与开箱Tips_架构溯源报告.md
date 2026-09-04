# Village_KenMuNi1 — 巨树 2 楼：进层对白 + 开箱对白/动画 + 生命体力球 Tips — 架构溯源报告

**文档版本**：v1.0（2026-09-03）  
**文档性质**：【架构侦探】只读定方案；**本文件不施工**  
**Unity**：2020.3.48f1 / C#  
**场景**：`Village_KenMuNi1` · `VillageWalkArea2`（巨树 2 楼）  
**产品（用户钉死）**：  
1. 进入树屋二层 → 播 **`Village_玩家上树屋二层`**  
2. 开宝箱 → 播 **`Village_玩家上树屋二层触发宝箱`**（含开箱动画）→ 入包 HpBall×3 / MpBall×3 → Tips **`GetHpBall`→`GetMpBall`**  
**资源**：CSV + Generated DialogueTree **已有**；Dialogue Prefab 壳 **缺失**；箱实体 `Tree2fHpMpBox` / `VillageKenMuNi1HpMpBox` **已有**（现网 `useStoryOnOpen=0` 直发奖）  
**样板**：进层单次 ≈ `LeaveChiefEscort` / `Village_KenMuNiStart`；开箱对白+Tips ≈ `HomeScene2Box`（ExecuteFunction）  
**提示词**：`Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构侦探提示词.md`  
**并行**：0903 卡住（DepthGap 已施工待重验）/ 宝箱可见性（磁盘已摆）——**本期主交付是对话接线**，不掩盖实体缺失

---

## 沟通摘要

### ① 结论一句话

**两份对白 Tree/CSV 已有，但缺 Dialogue Prefab 壳、缺进层 Trigger、开箱图缺动画/发奖节点，且箱仍 `useStoryOnOpen=0` 直发奖跳过对白——拍板 E1（楼梯落点后 GSM 单次播进层）+ B1（箱改走 Story + 图内 ExecuteFunction→`OnOpenBox`/`OnGetHpMp`），Tips 继续复用 `GetHpBall`/`GetMpBall`，不新做×3 图。**

### ② 原因（通俗）

台词文件已经写好了，但游戏真正播放要靠「对话壳 Prefab」；壳还没建，所以 `TriggerStory` 加载会空。  
一上 2 楼该谁喊开对白，场景里也还没接线。  
点箱子现在是程序直接给球，不等那三句「打开看看」——和产品要的「先对白再开箱再横幅」拧着。  
横幅图早就有了，不用再画「×3」专用图。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 同档首次：村长家楼梯 → 巨树 2 楼 | 播进层四句；还控 |
| 2 | 同档再经楼梯上 2 楼 | **不**重播进层 |
| 3 | 1 楼 `LeftDoor` / `Village_Chief_House_Door` 回村 | **不**播进层戏 |
| 4 | 点 `Tree2fHpMpBox` | 三句对白 → 箱 Open 动画 → 背包 +3/+3 |
| 5 | Tips | 依次 GetHpBall、GetMpBall 花边横幅 + 物品音效 |
| 6 | 同档再点箱 / 读档已开 | 不可再开；Open 态 |
| 7 | 资源 | `Prefabs/Dialogue/` 下两壳存在；WalkArea2 未改；**无双份球** |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| ID | 问题 | 裁定 |
|----|------|------|
| **Q1 进层触发** | E1 vs E2 | **E1**：`Village_KenMuNiSceneManager.OnEnterScene`（或等价进场后）在 `LastSceneName==Village_Chief_House` 且 `!CheckStoryUsed(进层名)` 时 `TriggerStory`；对齐 `TryTriggerLeaveChiefEscortOnce`。否 E3 合成一图 |
| **Q2 开箱** | B1 vs B2 | **B1**：`useStoryOnOpen=true`，`storyName=Village_玩家上树屋二层触发宝箱`；图内 `ExecuteFunction`→`OnOpenBox` / `OnGetHpMp`。否 B3/B4 |
| **Q3 Prefab** | 是否缺失 | **✅ 双壳皆无**；`DialoguePath`=`Assets/GameRes/Prefabs/Dialogue/{name}.prefab`；须新建两壳，名与 TriggerStory 字符串一致 |
| **Q4 单次键** | 进层 | **`Village_玩家上树屋二层`** → `StoryTriggerCountData`（`OnStoryEnd` 已记账）；开箱仍 **`tree2fHpMpBoxOpened`** |
| **Q5 非楼梯上 2 楼** | 是否也播 | **本期默认仅 Chief 楼梯路径**（与 W1/EnterPos 同门闩）；大门键禁播。同场景「爬楼进 WalkArea2」现网无独立产品入口——另案再加 Zone，勿本期扩成「凡进区就播」 |
| **Q6 Tips×3 图** | 要否 | **否**；复用 `GetHpBall`/`GetMpBall`（0830/0901） |

**双发防护（钉死）**：Story 路径开启后，`OpenBox` **禁止**再走 `else { OnOpenBox(); OnGetHpMp(); }`；发奖只许图回调一次。

---

## ② 资源核实

### CSV（对齐产品）

| 戏 | 句数 | 内容摘要 |
|----|------|----------|
| `Village_玩家上树屋二层` | 4 | 好高 / 远方漂亮 / …… / 不能往下看要小心（Surprised→Smile→ChiBie） |
| `Village_玩家上树屋二层触发宝箱` | 3 | 有箱子？ / 嗯…… / 打开看看吧~（Surprised→Laugh） |

### Generated DialogueTree

| 项 | 进层 | 开箱 |
|----|------|------|
| 路径 | `DialogueTrees/Generated/Village_玩家上树屋二层.asset` | `…触发宝箱.asset` |
| 现有节点 | `UIAlpha` Action + **StatementNodeEx×4** | `UIAlpha` + **StatementNodeEx×3** |
| ExecuteFunction / GetItem / OpenTips | **无** | **无** |

→ 开箱图施工必须 **补** Action（至少 `OnOpenBox`、`OnGetHpMp`）；不能假设 CSV Import 已含发奖。

### Dialogue Prefab 壳

| 名 | `GameRes/Prefabs/Dialogue/` |
|----|------------------------------|
| `Village_玩家上树屋二层.prefab` | **❌ 不存在** |
| `Village_玩家上树屋二层触发宝箱.prefab` | **❌ 不存在** |
| 样板可复制 | `Village_KenMuNiStart` / 短自语壳；开箱可对标 `HomeScene2Box.prefab` 的 ExecuteFunction 链 |

`TriggerStory` → `DialoguePath.GetPath(name)` → 上表路径；缺壳则加载失败、进层/开箱戏起不来。

### 箱实体（0901）

| 项 | 现状 |
|----|------|
| `Objects/Tree2fHpMpBox` | ✅ 场景有；约 `(-152,41.2)` ∈ WalkArea2 |
| `VillageKenMuNi1HpMpBox` | ✅；公开 `OnOpenBox` / `OnGetHpMp` |
| 序列化 | `useStoryOnOpen: 0` → 点箱 **直** `OnOpenBox+OnGetHpMp`，**跳过对白** |
| 存档 | `VillageKenMuNi1Data.tree2fHpMpBoxOpened` — **继续用** |

---

## ③ 期望时序（定稿）

```
【A · 进 2 楼 · E1】
  StairsDoor → LastScene = Village_Chief_House
  → SetPlayerPos / W1 落 ExitFrom + WalkArea2
  → OnEnterScene：!CheckStoryUsed("Village_玩家上树屋二层")
       && !HasRunningStory
       → TriggerStory("Village_玩家上树屋二层")
  → 对白完 → OnStoryEnd 记账 → 还控
  （大门键 Village_Chief_House_Door：不进此分支）

【B · 开箱 · B1】
  点 Tree2fHpMpBox（未开）
  → opened=true；关 canTouch（防连点）
  → TriggerStory("Village_玩家上树屋二层触发宝箱")
  → 图：三句 Statement
       → ExecuteFunction OnOpenBox   // Open 动画 + tree2fHpMpBoxOpened=true + SFX
       → ExecuteFunction OnGetHpMp   // AddMainItem×2 + OpenTipsForm GetHpBall→GetMpBall
  → 同档不可再开；读档 Open 态
```

**推荐图内顺序**：三句对白播完（或末句后）再 `OnOpenBox`，紧接 `OnGetHpMp`（对齐 HomeScene2：先开箱回调再 Get 道具回调）。替代：末句前开箱——可，但勿在首句就发奖。

---

## ④ 方案对比

### 进层

| 方案 | 做法 | 判定 |
|------|------|------|
| **E1** | GSM：`last==Village_Chief_House` + StoryTriggerCount 单次 | ✅ **主选**；不依赖脚碰；与 W1 门闩一致 |
| E2 | `SimpleStoryTrigger` Enter 罩 ExitFrom | ⚠️ 落点漂移/DepthGap 曾漏播风险；可作 E1 失败兜底，非主 |
| E3 | 与开箱一图 | ❌ 产品已拆两资源 |

### 开箱 + Tips

| 方案 | 做法 | 判定 |
|------|------|------|
| **B1** | `useStoryOnOpen=true` + 图 ExecuteFunction→已有 C# API | ✅ **最贴现网**；动画/存档/Tips 不丢 |
| B2 | 图内 GetItem×2 + OpenTipsForm×2 + 另写 Open/存档 | ⚠️ 可；须关 C# 直发且补动画节点，面更大 |
| B3 | 只对白不发奖 | ❌ |
| B4 | 只改 CSV 盼 Import 自带发奖 | ❌ Generated 现无发奖节点 |

### Tips×3

| 方案 | 判定 |
|------|------|
| 复用 GetHpBall / GetMpBall | ✅（Q6） |
| 新「×3」专图 | ❌ 本期不做 |

---

## ⑤ 最小施工清单

1. **Prefab×2**  
   - `Assets/GameRes/Prefabs/Dialogue/Village_玩家上树屋二层.prefab`  
   - `…/Village_玩家上树屋二层触发宝箱.prefab`  
   - Duplicate 相近村壳 → 挂对应 Generated Tree；开箱壳补 ExecuteFunction（目标类型 `VillageKenMuNi1HpMpBox`，方法 `OnOpenBox` / `OnGetHpMp`；Agent 绑场景 `Tree2fHpMpBox` 或运行时查找约定对齐 HomeScene2）  
2. **进层 E1**：`Village_KenMuNiSceneManager` 增 `TryTriggerTree2fEnterOnce`（常量名=进层戏；门闩=Chief 楼梯键；`HasRunningStory` 跳过策略对齐 LeaveChief 注释风险——建议若占场则 **延迟到 onStoryEnd 再试一次** 或黑幕后再 Trigger，施工写清，避免「跳过且本档永不播」）  
3. **开箱 B1**：场景/Setup 将 `Tree2fHpMpBox` 的 `useStoryOnOpen=1`，`storyName=Village_玩家上树屋二层触发宝箱`；确认 OpenBox 在 Story 分支 **不再** else 直发  
4. **回归**：大门送树屋戏、开场戏、WalkArea2 多边形、已开档箱态、无双份球  
5. **文档**：施工说明 `0903/…施工说明.md`；同步 OPEN  
6. **不改**：WalkArea2 形状；西境箱；Tips UI；存档键名

---

## ⑥ 与相关案边界

| 案 | 关系 |
|----|------|
| 0901 宝箱 HpMp×3 | 产品改口：由「纯 C# 直发」改为「对白+回调」；**保留** `OnOpenBox`/`OnGetHpMp`/`tree2fHpMpBoxOpened` |
| 0903 DepthGap 卡住 | 进层戏依赖能站稳 2 楼；DepthGap 已施工则先重验移动再验对白 |
| 0903 宝箱看不见 | 实体磁盘已有；接线前 Hierarchy 确认箱仍在；Sorting 另案 |
| 0830 Tips 横幅 | TipKey/入包双步契约不变 |

---

## ⑦ 验收

- [ ] 首次楼梯上 2 楼播进层四句；同档再上不重播  
- [ ] 大门路径不播进层  
- [ ] 点箱播三句；Open 动画；+3/+3；GetHpBall→GetMpBall  
- [ ] 已开不可再开；读档 Open  
- [ ] 两 Dialogue Prefab 存在且 TriggerStory 成功  
- [ ] WalkArea2 未改；无双份球；无相关 Error  

---

## ⑧ OPEN 建议

| ID | 问题 | 决议 | 状态建议 |
|----|------|------|----------|
| Q1 | 进层触发？ | **E1** Chief 楼梯 + StoryTriggerCount | ✅ 本报告 |
| Q2 | 开箱方案？ | **B1** useStoryOnOpen + ExecuteFunction | ✅ |
| Q3 | Prefab？ | **须新建两壳** | 待施工 |
| Q4 | 单次键？ | 进层用戏名；开箱用 `tree2fHpMpBoxOpened` | ✅ |
| Q5 | 非楼梯进 2 楼？ | **本期仅楼梯**；同场景爬楼另案 | ✅ 默认 |
| Q6 | ×3 专图？ | **否** | ✅ |
| Q7 | HasRunningStory 占场跳过？ | 施工选：延迟重试 / 黑幕后播，避免永跳 | 待施工写死 |

---

## ⑨ 程序索引

| 符号 | 路径 |
|------|------|
| TriggerStory / 路径 | `StoryComponentGSM` · `DialoguePath.GetPath` |
| 进层挂点样板 | `Village_KenMuNiSceneManager.TryTriggerLeaveChiefEscortOnce` |
| 箱逻辑 | `VillageKenMuNi1HpMpBox` |
| 开箱对白样板 | `HomeScene2Box` + `Prefabs/Dialogue/HomeScene2Box.prefab` |
| Tips | `OnGetHpMp` → `OpenTipsForm("GetHpBall"/"GetMpBall")` |
| 单次 | `StoryTriggerCountData.CheckStoryUsed` / `OnStoryTriggered` |

**硬禁止**：双发奖；TipKey 中文；新 TipsPanel；改 WalkArea2；挂西境箱；脚本私锁 `isTalking`；只改 CSV 不建 Prefab 当完成。
