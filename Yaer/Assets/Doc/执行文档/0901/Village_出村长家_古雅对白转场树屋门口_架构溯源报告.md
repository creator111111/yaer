# Village — 出村长家 · 古雅对白转场树屋门口 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探 → 施工已落地】ActionTask / CSV / G1 / Setup 菜单已就绪；**Prefab+Teleport 须开 Unity 跑 Setup**；依赖 E3′ 已施工  
**Unity**：2020.3.48f1  
**产品（台本钉死）**：从村长家 **1 楼出门** → 自动播段 A（女神质疑）→ **同场景黑幕**将主角+镜头转到 **雅尔树屋门口** → 段 B（道别+晚饭约定）  
**场景**：`Village_KenMuNi1`  
**提示词**：`提示词/0901/Village_出村长家_古雅对白转场树屋门口_架构侦探提示词.md`  
**关联**：楼梯换场 E3′ / W1 · 进屋续聊 C1 · `House_Tree`/`Village_TreeHouseLock` · BlackPanel 样板  

**不是**：进树屋室内 Scene；巨树 2 楼宝箱；晚宴整本替换；段 A 结束后还控自走树屋。

---

## 沟通摘要

### ① 结论一句话

**O1+G1+T1：1 楼门回村落在新建门前 `ExitFrom_HomeSceneChief`（与 2 楼 EnterPos 用 E3′ `enterPosKey` 拆开）→ KenMuNi1 `OnEnterScene` 在「门前键 ∧ 本戏未用」时 Trigger 一段 Prefab `Village_出村长家送树屋`；段 A 末用新建黑幕传送 Action 把玩家+镜头挪到 `House_Tree` 旁 Walk 内点再播段 B；2 楼回来不得误播。**

### ② 原因（通俗）

台本是「刚出村长家门口聊几句，再黑一下人就到树屋门口道别」，不是让玩家自己走过去，也不是上巨树二楼。  
现在凡从村长家回村都指到 2 楼落点，门前开场根本站错地方——必须先把 1 楼出门和楼梯上楼的落点拆开。  
对话图里也没有「黑幕传送玩家」现成节点，要补一个最小 Action，不能硬闪坐标穿帮。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 1 楼出门 → 落在村长家门前 → **自动**段 A（文案与提示词一致） | |
| 2 | 中段有黑幕遮罩；亮后人在树屋门口一带 | |
| 3 | 段 B 道别+晚饭句完整；结束后还控 | |
| 4 | 同档再出 1 楼门 **不**重播 | |
| 5 | 楼梯上 2 楼回来 **不**误播本戏 | |
| 6 | `House_Tree` 仍可点出 `Village_TreeHouseLock` | |
| 7 | Console 无对白 Prefab 加载失败 | |

### ④ 程序补充

见下文 §①～§⑩。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 出门 | **O1** · 1 楼 `LeftDoor` → 村门前（否 O2 巨树 2 楼；否 O3 室内直送） |
| EnterPos 拆分 | **强制依赖**楼梯案 **E3′**：默认 `Village_Chief_House`→2f；`LeftDoor` 用 **`enterPosKey=Village_Chief_House_Door`**（名可微调，三者一致）→ 新建 **`ExitFrom_HomeSceneChief`**（门前） |
| 触发 | **G1** · `Village_KenMuNiSceneManager.OnEnterScene`（或进场完成后的同级钩子）：门前键 ∧ `!CheckStoryUsed` ∧ 无 RunningStory → `TriggerStory` |
| 转场 | **T1** · **一段** Prefab 中段 **BlackPanel** 内 `SetPos` + 相机跟随；**非** Loading / **非** LoadScene |
| Story 名 | **`Village_出村长家送树屋`**（CSV / Generated / Prefab 同名） |
| 立绘 | 雅 + 古双人（无村长句）；Mask/壳对齐 `Village_KenMuNiStart` / 门口戏减村长 |
| 单次 | `StoryTriggerCountData`（TriggerStory 现网记次） |
| 晚宴 | **本期只对白**；不接晚宴触发旗（Q6） |
| 场景古莎 | **不强制**跟到树屋；转场只移玩家+镜头（Q5） |

---

## ② 出门落点裁定（O1 vs 2 楼冲突）

### 磁盘事实

| 锚点 | 坐标（KenMuNi1） | 角色 |
|------|------------------|------|
| `House_Chief`（Prefab 实例） | ≈ `(-158.3, 1.9, 0)` | 村长家户外门视觉 |
| `Npc_Chief` | ≈ `(-157.5, -1.2, 0)` | 门口戏 NPC |
| `EnterPos` `Village_Chief_House` | → `ExitFrom_HomeSceneChief2f` ≈ `(-159.34, 41.66, 0)` | **现全部从 Chief 回村都落 2 楼** |
| `ExitFrom_HomeSceneChief`（1f） | ❌ **不存在** | 须新建 |
| `House_Tree` | ≈ **`(28.32, 5.45, 0)`** | 树屋锁对白挂点（提示词旧写 ≈(9.23,-7.5) **已过时**，以磁盘为准） |
| `TreeDoor1/2` | ≈ `(-17.5, -7.6)` | 巨树 DepthZone；**≠** 雅尔树屋门口 |

### 假说拍板

| 假说 | 判定 |
|------|------|
| **O1** 1 楼门 → 门前 → 本戏 → 转场树屋 | ✅ 台本地理通 |
| O2 楼梯 2 楼 | ❌ 与「树屋门口」距离/语义不符；2 楼另有宝箱线 |
| O3 续聊结束室内直送出村 | ❌ 产品写「出来」；且会绕过 LeftDoor |

### 与楼梯案绑定（必做，否则本戏无法开场）

对齐 `Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告` **E3′**：

| 出口 | lastScene / enterPosKey | 落点 | 本戏 |
|------|-------------------------|------|------|
| 楼梯顶门 | 默认 `Village_Chief_House` | `ExitFrom_HomeSceneChief2f` + WalkArea2 | **不播** |
| **`LeftDoor`** | **`Village_Chief_House_Door`**（覆盖键） | 新建 **`ExitFrom_HomeSceneChief`**（建议近 `Npc_Chief` / `House_Chief`，**Y 须落在 `VillageWalkArea` 合法带**，防 ClosestPoint 吸偏） | **播** |

**G1 门闩不得写** `lastScene == Village_Chief_House`（会误伤 2 楼）。须认 **门前键**（或等价：落点靠近门前 Transform / 非 2f 高度——优先认键，稳）。

若 E3′ 未先落地：1 楼出门仍落 2 楼 → **本戏阻塞**（OPEN Q7）。

---

## ③ 全时序（期望）

```
Village_Chief_House · LeftDoor（按 E）
  → enterPosKey = Village_Chief_House_Door
  → LoadScene(KenMuNi1, 黑幕)
  → SetPlayerPos → ExitFrom_HomeSceneChief（门前）
  → Village_KenMuNiSceneManager：G1 条件成立
  → TriggerStory("Village_出村长家送树屋")
  →【段 A】古/雅女神质疑台本（见提示词，勿改字）
  →【T1】BlackPanel ShowFade
        → 全黑：player.SetPos(树屋门口 Walk 点)；相机 Follow/对齐
        → 可选：Flush / WalkArea ClosestPoint 一次（防脚出区）
        → HideFade / CloseFormFade
  →【段 B】道别 + 晚饭约定
  → onStoryEnd 还控；StoryTriggerCount 已记本戏
```

**禁止**：段 A 结束还控 → 玩家自走 → 点古莎/树屋才续 B。

---

## ④ 转场方案（T1）

### 方案对比

| 方案 | 判定 |
|------|------|
| **T1** 同场景 BlackPanel + SetPos | ✅ 「屏幕转场」；已在村内 |
| T2 LoadScene 再落树屋 | ❌ 多余；LastScene 乱 |
| T3 两段 Story + GSM 串 | 次选；多一次 Trigger/记次风险 |
| `PlayerAutoMoveActionTask` | ❌ 仅跟 X、无遮罩；门前 x≈-158 → 树屋 x≈28 **不可用** |
| `LoadSceneTaskAction` | ❌ 换 Scene，不符合本期 |

### 现网缺口（侦探核实）

| 能力 | 状态 |
|------|------|
| 对话内 **黑幕传送玩家** ActionTask | ❌ **无**（NodeCanvas 无 SetPlayerPos/Teleport） |
| BlackPanel ShowFade 回调写坐标 | ✅ C# 样板：`ChiefNearDoorStoryTrigger` / 续聊换古莎 / `BlackFormLogic` |
| `CameraFollowPlayerActionTask` | ✅ 可在亮幕前后挂 |
| `playerLogic.SetPos` | ✅ 运行时 API 有；缺对话封装 |

### 施工最小 API（推荐）

新建 **`BlackFadeTeleportPlayerActionTask`**（名可微调）：

1. Open `BlackPanel`（System）→ `ShowFade`  
2. 回调内：`PlayerLogic.SetPos(Destination)`；必要时清速度 / 调一次 WalkArea 校正  
3. `CameraFollowPlayer` 或 `CameraComponentGSM` 对齐玩家  
4. `CloseFormFade` / HideFade → `EndAction`  

Destination：场景空物体 **`TeleportTo_YaerTreeHouseDoor`**（建议父级 `Objects` 或 `MapLimit`），**勿**直接用 `House_Tree` 的 `(28.32, 5.45)` 当脚点——Y≈5.45 易出 `VillageWalkArea` 被吸回（0822 ExitFrom 同类坑）。落点须 **OverlapPoint∈VillageWalkArea**，视觉靠近 `House_Tree` 门前（施工 Scene 微调；Q4）。

对话壳：转场时倾向 **保持对白 Form 打开**、仅黑幕盖屏（避免中段拆壳再 Open 闪）；亮后继续下一条 Statement。

---

## ⑤ 对话资源

### 命名（钉死）

| 资源 | 名 |
|------|----|
| Story / Prefab / CSV / Generated | **`Village_出村长家送树屋`** |
| Tip/键 | 同左；`StoryTriggerCountData.CheckStoryUsed` |

### 台本结构（文案以提示词为准，施工勿改字）

| 段 | 句数概要 | 说 |
|----|----------|-----|
| A | 古质疑女神 → 雅否认能力×3 → 古沉默 → 雅怕失望 → 古安慰×2 | 古 / 雅 |
| **Action** | 黑幕传送 | — |
| B | 古送到这里×2 → 雅「好。」 | 古 / 雅 |

无村长句 → Prefab **双人立绘**（雅+古）；可参考 `Village_KenMuNiStart` / `Village_村内雅古开场对白台本` 减村长。

### 流水线

```
新建 Assets/Dialog/Village_出村长家送树屋.csv
  → Import → Generated
  → Prefab Setup（壳 + UIAlpha + Actor）
  → 中段插入 BlackFadeTeleport Action（Destination 拖场景 Transform）
```

磁盘现状：该 CSV / Prefab **均不存在**（须新建；对齐续聊「缺 Prefab 必加载失败」教训）。

---

## ⑥ 与 2 楼 / 树屋锁 / 上游解耦

| 系统 | 关系 |
|------|------|
| 楼梯 → 2 楼 / WalkArea2 / 宝箱 | **正交**；G1 只认门前键 |
| `House_Tree` → `Village_TreeHouseLock` | 本戏 **不**改锁对白；转场后仍可点锁；**不**进室内 Scene |
| 门口初次 / 室内续聊 / 针线包 / 换古莎 | **上游已完成**后才出屋播本戏；门闩不依赖续聊名，只依赖本戏未用 + 门前键 |
| `GushaSidePortrait`（村内有） | 不强制同步到树屋（Q5） |
| 晚宴台本 | **≠** 本期；「同样时间来吃饭」仅台词（Q6） |

---

## ⑦ 最小施工清单

1. **E3′ 先行（或同 PR）**：`enterPosKey` + `ExitFrom_HomeSceneChief` 门前 + LeftDoor 填键；保持 2f EnterPos。  
2. 新建 CSV → Generated → Prefab `Village_出村长家送树屋`（台本一字不改）。  
3. 新建 `BlackFadeTeleportPlayerActionTask`；场景摆 `TeleportTo_YaerTreeHouseDoor`（Walk 内近 House_Tree）。  
4. Prefab 段 A 末插传送 Action → 段 B。  
5. `Village_KenMuNiSceneManager`：G1 `TryTriggerLeaveChiefEscortOnce`（门前键 ∧ 未用 ∧ 无 Running）。  
6. 回归：2 楼回来无本戏；`House_Tree` 锁对白；开场/续聊/宝箱。  
7. 施工说明 + OPEN。

---

## ⑧ 验收

- [ ] 1 楼出门 → 门前 → 自动段 A（文案正确）  
- [ ] 中段黑幕；亮后在树屋门口一带（Walk 内可站）  
- [ ] 段 B 完整；还控  
- [ ] 同档再出 1 楼不重播  
- [ ] 楼梯 2 楼回来不误播  
- [ ] `House_Tree` → 仍 `Village_TreeHouseLock`  
- [ ] 无 Prefab 加载失败；未进树屋室内 Scene  

---

## ⑨ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 出门落点 O1？ | **是**；补 `ExitFrom_HomeSceneChief` + E3′ | ✅ |
| Q2 | 转场黑幕？ | **BlackPanel**；非 Loading 换场 | ✅ |
| Q3 | 一段还是两段 Prefab？ | **一段 + 中段传送 Action** | ✅ |
| Q4 | 树屋落点精确坐标？ | 新建 Teleport Transform；近 `House_Tree`(28.32,5.45)，**Y 进 WalkArea**；Scene 微调 | ⏳ 施工摆点 |
| Q5 | 场景古莎跟到树屋？ | **不强制**；只移玩家+镜头 | ✅ |
| Q6 | 接晚宴旗？ | **本期否**；只对白 | ✅ |
| Q7 | E3′ / 门前落点是否已施工？ | **本戏依赖**；未做则阻塞 | ⏳ |
| Q8 | 转场时对白壳是否保持打开？ | **倾向保持**；避免拆壳闪一下 | ⏳ 施工手感 |

---

## ⑩ 程序补充

### 关键锚点

| 符号 | 路径 / 说明 |
|------|-------------|
| 进场自动戏样板 | `Village_KenMuNiSceneManager` Start；`Village_Chief_HouseSceneManager` Continue |
| LastScene / EnterPos | `ChangeSceneComponentGM`；`BaseGameSceneManager.SetPlayerPos` |
| E3′ 依赖 | 0901 楼梯上楼换场报告 |
| BlackPanel | `UIPrefabPath` · `BlackPanel` · `ShowBlackFormArgs` / `BlackFadeComponent` |
| 树屋锁 | `House_Tree` · `SimpleStoryTrigger` · `Village_TreeHouseLock` |
| 相机 | `CameraFollowPlayerActionTask` / `CameraComponentGSM` |

### 台本原文位置

提示词正文「产品台本」段 A / 转场硬要求 / 段 B——**施工禁止擅自改中文台词**。

### 硬禁止

- 段 A 后还控自走树屋再点人续 B  
- 2 楼回来误播本戏（`lastScene==Village_Chief_House` 裸判）  
- 转场无遮罩硬闪坐标  
- Story 名与 Prefab 不一致  
- 进树屋室内 Scene；改 WalkArea2；改晚宴整本  
