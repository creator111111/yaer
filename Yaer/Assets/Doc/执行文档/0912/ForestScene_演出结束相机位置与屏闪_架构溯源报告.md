# ForestScene 演出结束：摄像机位置不对 + 屏闪 — 架构溯源报告

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**场景（钉死）**：`ForestScene`  
**复现钉死（首选）**：门口林恩链 — `HomeDoorStoryTrigger` → `ForestSceneLaiFlyStory` → `ForestSceneLinEnStory` →（`CameraMoveEnd`）→ `ForestSceneYaerAfterLinEnStory`  
**现象**：演出结束时摄像机位置不对 + 屏闪  
**产品期望**：结束后镜头立刻（或黑幕掩护下）落在跟玩家的正确机位；无错误机位闪帧、无可见乱滑  
**提示词**：`Assets/Doc/提示词/0912/ForestScene_演出结束相机位置与屏闪_架构侦探提示词.md`  
**对照**：`02_SYSTEM_SPEC.md` §3；`执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`（同源嫌疑，勿混场景施工）

---

## 沟通摘要

### ① 结论一句话

**门口林恩对白结束**时，`OnDialogueEnd` 用 `SetFollow(forceSnap)+smoothTime=0.3` **无黑幕手推**回玩家（过程露在画面上 =「机位不对再挪 / 闪」）；且 NodeCanvas 在等到 `CameraMoveEnd` 后又 **再调一次 `OnCameraMoveEnd`**，会 **二次 `forceSnap`**，加重顿挫/双闪。与龙宫 Stairs **机制同源**（SmoothDamp 手推），但 Forest **没有换场黑幕**，露景更直白。

### ② 原因（通俗）

门口大戏播完，镜头要从「看戏的机位」回到跟雅儿。现网不是「咔」一下定格，而是用 0.3 秒平滑挪过去，又没有黑幕挡着，所以你看见错机位再滑/闪一下。  
更糟的是：程序已经挪完并通知图继续了，图上又点了一次「相机挪完」收尾，等于再 snap 一遍。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 期望 |
|---|------|------|
| 1 | 清/新档使 `ForestSceneData.homeDoorStoryComplete=false`，进 Forest，走完门口演出到林恩对白结束 | 复现位置错 + 闪 |
| 2 | Console 过滤 `[CHAIN]` / `ForestSceneLinEnStory` | 见 `OnDialogueEnd` → `SetFollow` → `OnCameraMoveEnd`；注意是否 **两次** `OnCameraMoveEnd ENTER` |
| 3 | Hierarchy 看 `Camera`/`Cinemachine` 在对白结束瞬间 | 是否从演出位向玩家位多帧滑动 |
| 4 | （施工前自测）临时把 Forest `CameraComponent.smoothTime=0` 再跑门口链 | 若闪/滑明显减轻 → 坐实手推主因；**测完改回** |
| 5 | 确认不是其它 `ForestScene*` 对白 | 预扫：**仅 LinEn 链**挂 `OnDialogueEnd`/`CameraMoveEnd` 相机收束 |

### ④ 程序补充

见下文 §1～§7。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **哪段演出** | **A. 门口林恩链**（首选已钉死）。其它 Forest 对白 Prefab **无**同等相机收束钩子 |
| **主因 A** | **成立**：`ForestSceneLinEnStory.OnDialogueEnd` → `SetFollow(player, onComplete:OnCameraMoveEnd, forceSnap:true)`；场景 `smoothTime:0.3` → `_isSmoothingSnap` 手推；**无 BlackPanel** → 全过程可见 |
| **主因 C** | **成立（图设计）**：图序 `40 OnDialogueEnd` → `41 等待 CameraMoveEnd` → **`42 再 Execute OnCameraMoveEnd`**。第一次 `OnCameraMoveEnd` 已清 `_skipPlayerResnap`；第二次 **`!skipResnap` → 再 `SetFollow(forceSnap)`** → 二次手推/闪 |
| **嫌疑 B（UI 立绘闪）** | **次要/并存**：`id39` 对话 UI/立绘淡出；`StoryComponentGSM.OnStoryEnd` 延迟恢复战斗立绘防闪 —— 偏 UI，**不能单独解释机位错** |
| **嫌疑 D（Lock 吞 Follow）** | **已排除为主因**：`OnDialogueEnd` 先 `SetLock(false)` 再 `SetFollow`；日志亦打 `IsLock` |
| **嫌疑 E（Confiner）** | **未证实**：Confiner `m_Damping:0`；可能加剧边界弹，非主链 |
| **与龙宫 Stairs** | **机制同源**（`forceSnap`+`smoothTime` 手推）；**场景不同**：Stairs 有黑幕但仍揭得早；Forest **当场露景** + **图二次 OnCameraMoveEnd** |
| **推荐修复** | **方案组合 B′+A′**：① 删掉/旁路图节点 **id42** 对 `OnCameraMoveEnd` 的二次调用（或令第二次 no-op）；② 林恩收束路径 **瞬切**（临时 `smoothTime=0` 或等价当帧对齐），保留 `onComplete` 符合 SPEC §3 |

---

## 2. 调用链（错误机位可能露出的窗口）

### 2.1 整段门口链

```
进场 homeDoorStoryComplete==false
  ForestSceneManager.OnEnterScene
    → CancelFollow() + SetLock(true)          ← 不跟玩家，锁在演出机位侧

HomeDoorStoryTrigger（走进 Trigger）
  → TriggerStory("ForestSceneLaiFlyStory")
  → … Timeline/国王莱演出 …
  → TriggerStory("ForestSceneLinEnStory")

林恩对白末（图）
  id38  林恩句「是。」
  id39  NormalDialogueUI 淡出 + 立绘 CanvasGroup 淡出
  id40  ExecuteFunction OnDialogueEnd()     ★ 开始手推回玩家（可见）
  id41  等待 AnimationEvent「CameraMoveEnd」
  id42  ExecuteFunction OnCameraMoveEnd()   ★★ 二次收束（易二次 snap）
  id43  SetObjectActive 清理
  id44  TriggerStory("ForestSceneYaerAfterLinEnStory")
```

### 2.2 `OnDialogueEnd` → 机位（代码）

```
OnDialogueEnd
  SetLock(false)
  SetFollow(player, onComplete: OnCameraMoveEnd, forceSnap: true)
       │
       ├─ smoothTime=0.3：Follow=null；LateUpdate SmoothDamp 多帧
       │     ★★ 错误机位窗口：从演出末 VCam 位 → 玩家位，无黑幕 ★★
       └─ onComplete → OnCameraMoveEnd
            → TryNotify("CameraMoveEnd")  → 解开 id41
            → skipResnap=true → 不在此处二次 SetFollow
  homeDoorStoryComplete = true
  （2s 实时兜底：若 onComplete 未到则强制 OnCameraMoveEnd）
```

### 2.3 二次 `OnCameraMoveEnd`（图 id42）

```
id41 收到 CameraMoveEnd 后继续
  → id42 再次 OnCameraMoveEnd()
       _skipPlayerResnap 已在第一次被清为 false
       → SetFollow(player, forceSnap:true) 再次手推/瞬切
       → ★ 第二段可见顿挫 / 双闪窗口 ★
  → id44 开 YaerAfterLinEn
```

---

## 3. 证据表

| # | 证据 | 状态 | 说明 |
|---|------|------|------|
| E1 | 触发：`HomeDoorStoryTriggerLogic` → `ForestSceneLaiFlyStory` | **已证实** | `homeDoorStoryComplete==false` |
| E2 | 仅 `ForestSceneLinEnStory.prefab` 挂 `OnDialogueEnd`/`CameraMoveEnd`/`OnCameraMoveEnd` | **已证实** | 其它 `ForestScene*.prefab` 无同等相机收束 |
| E3 | 图连接 `38→39→40→41→42→43→44` | **已证实** | 磁盘 connections |
| E4 | id40=`OnDialogueEnd`；id41=`AnimationEventRegister(CameraMoveEnd)`；id42=`OnCameraMoveEnd`；id44=`YaerAfterLinEn` | **已证实** | Prefab 序列化 |
| E5 | `OnDialogueEnd`：`SetFollow(..., forceSnap:true, onComplete:OnCameraMoveEnd)` | **已证实** | `ForestSceneLinEnStory.cs` |
| E6 | Forest `CameraComponent.smoothTime: 0.3`；`maxHandSnapRealSeconds: 2.5` | **已证实** | `ForestScene.unity` |
| E7 | Framing `m_XDamping: 0.7`；`m_DeadZoneHeight: 1`；`m_YDamping: 0` | **已证实** | 绑 Follow 后仍可能横追 |
| E8 | 进场 `CancelFollow`+`SetLock(true)` | **已证实** | `ForestSceneManager.OnEnterScene` |
| E9 | `_skipPlayerResnap` 防的是 **同一次** onComplete 路径内二次 snap；**挡不住图 id42 再调** | **已证实** | 第一次 `OnCameraMoveEnd` 末尾把 flag 清掉 |
| E10 | `safeTriggerYaerAfterRealSeconds=2` 兜底 | **高度可疑（边缘）** | 正常 0.3 手推应先于 2s；若卡住可能「未到位就发 CameraMoveEnd」 |
| E11 | `StoryComponentGSM` 立绘延迟防闪 | **已证实为 UI 向** | 不解释机位位移 |
| E12 | LaiFly Prefab **无** CameraMove/SetFollow 任务 | **已证实** | 演出中机位多半 Timeline/锁镜残留，收束仍靠 LinEn |
| E13 | Play 帧日志二次 `OnCameraMoveEnd ENTER` | **未测（侦探无 Play）** | 验收用 `[CHAIN]` 坐实 |

### 3.1 场景相机字段（磁盘）

| 字段 | ForestScene |
|------|-------------|
| `CameraComponent.smoothTime` | **0.3** |
| `maxHandSnapRealSeconds` | 2.5 |
| VCam OrthoSize | 7.9 |
| Framing XDamping | 0.7 |
| Framing DeadZoneHeight | 1 |
| Confiner Damping | 0 |

---

## 4. 与龙宫 Stairs 案对照

| | 龙宫 Stairs（0912） | Forest 门口林恩（本案） |
|--|---------------------|-------------------------|
| 触发 | 换场 `InitPlayer`→`SetFollow` | 演出结束 `OnDialogueEnd`→`SetFollow` |
| `smoothTime` | 曾为 0.3（已方案 A 改 0） | **仍为 0.3** |
| 黑幕 | 有；hold 0.3 后揭开仍可能露未对齐 | **无**；手推全程可见 |
| 二次 snap | 无图二次调用 | **有** 图 id42 再调 `OnCameraMoveEnd` |
| 施工边界 | 只动 HomeScene1/2 | **只动 Forest / LinEn**；勿回改龙宫 |

**一句话**：都是「forceSnap + smoothTime 手推」；Forest 更糟在 **当场露景 + 图多打一次收束**。

---

## 5. 修复方案对比（只方案，不施工）

### 方案 A′ — 林恩收束瞬切（推荐核心之一）

| 项 | 内容 |
|----|------|
| 做法 | `OnDialogueEnd` 的 `SetFollow` 当帧对齐：临时 `smoothTime=0` 再恢复，或增加「instantSnap」路径 |
| 效果 | 第一次回玩家无可见滑动 |
| 改动量 | 小（`ForestSceneLinEnStory` 或仅本场景 `smoothTime`） |
| SPEC §3 | **符合**（仍 `SetFollow`+`onComplete`，禁死 Wait 硬匹配） |
| 风险 | 若把整个 Forest `smoothTime=0`：进场/其它跟拍也瞬切（通常可接受） |

### 方案 B′ — 去掉图上二次 `OnCameraMoveEnd`（推荐必做）

| 项 | 内容 |
|----|------|
| 做法 | NodeCanvas：删除 id42 的 `ExecuteFunction OnCameraMoveEnd`；`41 等待` 结束后直接 id43/44。或代码：`OnCameraMoveEnd` 若本轮已执行则直接 return |
| 效果 | 消灭二次 `forceSnap` |
| 改动量 | 改对话 Prefab 或加一行幂等守卫 |
| 风险 | 极低；第一次 onComplete 已 `TryNotify` |

### 方案 C — 收束期间黑幕掩护

| 项 | 内容 |
|----|------|
| 做法 | `OnDialogueEnd` 先 FadeShow，对齐/`onComplete` 后再揭 |
| 效果 | 遮住手推；手感更「过场」 |
| 风险 | 与门口演出节奏叠加；改动面大于 A′B′；非必须 |

### 方案 D — 只改 Framing Damping

| 项 | 内容 |
|----|------|
| 做法 | 降 `m_XDamping` |
| 效果 | 减轻绑 Follow 后软追；**不消灭**手推主窗 |
| 建议 | **不单独采用** |

**推荐**：**B′ + A′**（先消二次 snap，再让第一次瞬切）。  
**验收**：门口链 ≥2 次；`[CHAIN] OnCameraMoveEnd ENTER` **每轮仅 1 次**（若保留幂等则可 2 次但第二次无 SetFollow）；结束瞬间无错误机位闪/滑；日常 Forest 走路跟拍不回归；YaerAfterLinEn 正常接上。

---

## 6. OPEN_QUESTIONS

已写入 `Assets/Doc/OPEN_QUESTIONS.md`：

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 是否采用 B′（去/幂等二次 OnCameraMoveEnd）+ A′（收束瞬切）？ | **是** | 待确认 |
| Q2 | A′ 用「仅 LinEn 临时 smoothTime=0」还是「整场景 Forest smoothTime=0」？ | **优先仅 Lin恩路径**；场景级作备选 | 待确认 |
| Q3 | 是否上黑幕方案 C？ | **本期否** | 待确认 |
| Q4 | 用户现象是否确认为门口林恩链（非其它 Forest 对白）？ | **默认是**；若否改写复现名 | 待 Play 确认 |

---

## 7. 限制

- 未改代码 / Prefab / 场景 / Git。  
- 未扩大到 ForestEast 树桥 / 村庄 Part3 双 VCam / 龙宫 Stairs（仅对照）。  
- 未 Play；验收依赖 `[CHAIN]` 与门口链复测。
