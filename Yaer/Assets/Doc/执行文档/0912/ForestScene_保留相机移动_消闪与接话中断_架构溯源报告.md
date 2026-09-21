# ForestScene 门口演出：保留相机移动 + 消闪 + 接话中断 — 架构溯源报告

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【架构侦探】只读回归核验；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestScene` 门口林恩链  
**产品纠偏（钉死 · 高于旧报告）**：  
1. **摄像机要移动过去**（保留平滑手感）—— **禁止**当帧瞬切作终态  
2. 旧问题只是移动时 **闪一下** —— 消闪但不能取消移动  
3. 移动结束后 **主角必须接着说话**（`ForestSceneYaerAfterLinEnStory`）  
4. **现网 Bug**：移动完了（或本该移动完）**主角不继续说话**  
**前置施工（嫌疑）**：`施工说明/0912/ForestScene_演出结束相机定格去二次snap_施工说明.md`（A′ 临时 `smoothTime=0` + B′ 幂等）  
**提示词**：`Assets/Doc/提示词/0912/ForestScene_保留相机移动_消闪与接话中断_修复提示词.md`  
**旧溯源**：`执行文档/0912/ForestScene_演出结束相机位置与屏闪_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**接话断在「门铃按早了」**：A′ 把收束改成 `smoothTime=0` → `SetFollow` **同步** `onComplete` → 在图还停在 id40、**尚未进入 id41 等待**时就 `TryNotify(CameraMoveEnd)`；事件无人听即丢；随后 id41 永久傻等；B′ 幂等又让之后的 `OnCameraMoveEnd`（含 2s 兜底）**SKIP 且不再发事件** → `YaerAfterLinEn` 永不 Trigger。消闪须在 **保留 `smoothTime>0` 移动** 前提下做（推荐 **M3 回滚 A′ + 等注册再 Notify**，闪仍重再加 **M1 短黑幕**）。

### ② 原因（通俗）

以前镜头慢慢挪回雅儿，挪完才喊「好了」，图刚好站在门口等这句话，所以下一句能接上——只是挪的时候会闪。  
最近修成「瞬间到位」后，程序在图还没走到「等待」就喊了「好了」；图再去门口等，门铃不会再响；而且「已经喊过」的标记还让后面补喊全部作废。于是雅儿下一句永远不来。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 期望（现网 A′/B′） |
|---|------|-------------------|
| 1 | 清 `homeDoorStoryComplete`，走完门口至林恩结束 | 镜头多半 **瞬切**（无缓慢移动） |
| 2 | Console 滤 `[CHAIN]` / `ForestSceneLinEnStory` / `未注册` | `OnCameraMoveEnd ENTER` 紧跟 `OnDialogueEnd`；`RegisterEvent: false` /「无人监听 CameraMoveEnd」Warning |
| 3 | 同上 | **没有** `[NodeCanvas/等待Animation事件] 收到事件: CameraMoveEnd`；**没有** `YaerAfterLinEn` |
| 4 | 若见 `OnCameraMoveEnd SKIP(idempotent)` | 证实 B′ 堵住了补发机会 |
| 5 | （对照）临时去掉 A′ 临时 `smoothTime=0` 再测 | 应恢复可见移动 + 接话（闪可能回来）——坐实因果 |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **接话中断根因** | **A′ 同步 Notify 过早**（主因，**已证实 by 代码路径**）+ **B′ SKIP 阻断补发**（加重，**已证实**） |
| **移动消失** | **A′ 故意行为**（临时 `smoothTime=0`）；与产品纠偏冲突，**须回滚作默认路径** |
| **闪** | 旧因：无黑幕 + `smoothTime=0.3` 手推露景；A′ 用取消移动换消闪 —— **产品否决作终态** |
| **龙宫 Stairs 套用** | **禁止**；本案不得整段收束再靠 `smoothTime=0` 同步 onComplete 且不修事件时序 |
| **推荐闭环** | **M3（必做）**：回滚 A′ 默认瞬切，恢复移动；`TryNotify` 改为「已 Register 才发 / 否则延至注册或下帧安全点」。**B′ 保留但改语义**：幂等只挡二次 **snap**，不得挡「唯一有效 Notify」或提供「未送达则补发」。闪仍明显再迭 **M1 短黑幕掩护手推**。 |

---

## 2. 时序图（期望 vs A′/B′ 现网）

### 2.1 期望（产品 + 旧版 `smoothTime>0`）

```
图 id40  OnDialogueEnd()
           SetLock(false)
           SetFollow(玩家, forceSnap, onComplete→OnCameraMoveEnd)
             smoothTime>0 → 多帧手推（可见移动）……
图 id41  进入「等待 CameraMoveEnd」→ RegisterEvent  ★ 已有人听
           ……手推到位……
         onComplete → OnCameraMoveEnd → TryNotify(CameraMoveEnd)
           → id41 EndAction
图 id42  （可选）再调 OnCameraMoveEnd → 应幂等跳过 snap，且不必再依赖事件
图 id43  清理
图 id44  TriggerStory(ForestSceneYaerAfterLinEnStory)  ★ 主角接着说
```

### 2.2 现网 A′ + B′（接话断）

```
图 id40  OnDialogueEnd()
           cam.smoothTime = 0          ★ A′
           SetFollow(...)
             smoothTime≈0 → ApplyFollow 当帧 + onComplete 同步调用
               → OnCameraMoveEnd ENTER
               → TryNotify(CameraMoveEnd)
                    IsEventRegistered == false   ★ 图还在 id40，id41 未 Register
                    AnimaEventTrigger → Warning「未注册」/ 事件丢弃
           恢复 smoothTime=0.3
           _onCameraMoveEndInvokedThisChain = true
           （因已 invoked，不挂 2s 兜底）

图 id41  才进入等待 → RegisterEvent
           ★ 永远等不到已错过的 CameraMoveEnd
           ★ 卡死，到不了 id44

图 id42  若永远到不了：无意义
         若其它路径再调 OnCameraMoveEnd → B′ SKIP，不再 TryNotify
```

**生活类比**：门铃在你走到门口前就按过了；你站门口一直等，不会再响；而且门卫说「已经按过了」拒绝再按。

---

## 3. 证据表

| # | 证据 | 状态 | 说明 |
|---|------|------|------|
| E1 | A′：`OnDialogueEnd` 临时 `smoothTime=0` 再 `SetFollow` | **已证实** | `ForestSceneLinEnStory.cs` 现网；施工说明 v1.0 |
| E2 | `CameraComponent.SetFollow`：`smoothTime≈0` 时 **同步** `onComplete?.Invoke()` | **已证实** | `CameraComponent.cs` |
| E3 | `smoothTime>0` 时 onComplete 在 LateUpdate 手推结束才调 | **已证实** | 旧链路能接话的原因 |
| E4 | 图序仍为 `40→41→42→43→44`；id41=`AnimationEventRegister(CameraMoveEnd)` | **已证实** | Prefab（施工未改图） |
| E5 | `AnimationEventRegisterTaskAction`：仅在 `OnExecute` 时 `RegisterEvent`；**错过的事件不会补投** | **已证实** | 无队列/无 sticky |
| E6 | `TryNotify` 在未注册时仍 `AnimaEventTrigger`，并打「无人监听」Warning | **已证实** | 脚本自带文案 |
| E7 | B′：`_onCameraMoveEndInvokedThisChain` 为真则 **整段 return**（含不再 `TryNotify`） | **已证实** | 堵死「等会儿再发一次」 |
| E8 | A′ 成功后 `_onCameraMoveEndInvokedThisChain==true` → **不挂** 2s 安全兜底 | **已证实** | 连兜底补发也被关掉 |
| E9 | 产品要保留移动 | **产品钉死** | 本提示词；否决 A′ 作终态 |
| E10 | Play 实机 Log | **未测（侦探无 Play）** | 验收用 §③ 清单坐实；代码路径已足够拍板回滚 |

### 3.1 与旧报告推荐的关系

| 旧推荐 | 现态 | 产品纠偏后 |
|--------|------|------------|
| A′ 瞬切消闪 | **已施工** | **否决作终态**；须回滚默认路径 |
| B′ 幂等挡二次 snap | **已施工** | **可保留思路**，但不得导致「唯一 Notify 发早 + 之后全 SKIP」 |
| 龙宫 Stairs `smoothTime=0` | 另案 | **禁止**套到本门口收束 |

---

## 4. 方案对比（约束：必须保留移动）

| 方案 | 做法 | 接话 | 移动 | 消闪 | 改动量 | 裁定 |
|------|------|------|------|------|--------|------|
| **M1 黑幕掩护** | 收束前淡黑 → 手推 → onComplete 揭幕 + Notify（须仍满足「先等待后发」或 M2） | 需叠加时序 | ✅ | ✅ 强 | 中 | 闪重时迭加 |
| **M2 延迟发事件** | 保持平滑；**Register 后再 TryNotify**；或 onComplete 只置位、等 id41 注册后补发 | ✅ | ✅ | 弱 | 小～中 | **时序核心** |
| **M3 回滚 A′ + 时序** | 去掉临时 `smoothTime=0`；加「未注册则延后 Notify」；B′ 只挡二次 snap | ✅ | ✅ | 可能回旧闪 | **最小纠偏** | **推荐首发** |
| **M4 降阻尼/缩短手推** | 略降 `smoothTime`/XDamping（**不为 0**） | 靠 M2/M3 | ✅ 缩短 | 弱 | 小 | 辅助 |
| ~~再 A′ 瞬切~~ | 同步 onComplete 不修时序 | ❌ | ❌ | ✅ | — | **禁止** |

### 推荐落地顺序

1. **M3（必做）**  
   - 回滚 A′：收束使用场景默认 `smoothTime`（约 0.3），恢复可见移动。  
   - **Notify 闸门**：`TryNotify` 前若 `!IsEventRegistered`，则挂协程/`yield null` 或短轮询直到已注册（或超时再发并打 Error）；保证 id41 已听。  
   - **B′ 收紧**：幂等只跳过「二次 SetFollow / 重复业务」；若第一次 Notify 时未注册，允许「补发 Notify」而不算二次 snap；或图删 id42，幂等仅防兜底重入。  
2. **验收闪**仍不可接受 → 迭 **M1**（短黑幕掩护手推，揭幕与 onComplete 对齐）。  
3. 可选 **M4** 微调手推时长，**禁止**再置 0。

**SPEC §3**：仍走 `SetFollow` + `onComplete`；延迟 Notify 是等「等待节点就绪」，不是用固定 Wait 硬匹配机位。

---

## 5. id42 / B′ 怎么处理

| 选项 | 利弊 |
|------|------|
| **保留 B′ 幂等 + 修 Notify 时序** | 不改 Prefab；须保证第一次有效 Notify 发生在 Register 之后；第二次只 SKIP snap |
| **Prefab 删 id42** | 从图上消灭二次调用；YAML 难 review；可与代码幂等二选一或并存 |
| **错误做法** | 第一次过早 Notify + B′ 整段 return → **接话永久断**（现网） |

**建议**：先改代码时序（M3）；id42 可暂留，靠修正后的幂等（第二次不 snap、也不需要再发事件）。若想图更干净，另案删 id42。

---

## 6. OPEN_QUESTIONS

已更新 `Assets/Doc/OPEN_QUESTIONS.md`（本条摘要）：

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 产品是否钉死「保留移动」？ | **是**（纠偏） | ✅ |
| Q2 | 是否回滚 A′ 瞬切默认路径？ | **是** | 待施工 |
| Q3 | 首发 M3（回滚+延迟 Notify），闪重再 M1？ | **是** | 待确认 |
| Q4 | B′ 是否保留？ | **保留但改语义**（不得挡唯一有效 Notify） | 待施工 |
| Q5 | 是否删 Prefab id42？ | 可选；代码修好后非必须 | 待确认 |

---

## 7. 给施工员的一句话

**回滚 A′ 默认瞬切；修好「先等待、后 Notify」时序；保留可见移动；B′ 不得再吞唯一一次有效 `CameraMoveEnd`；消闪优先短黑幕（M1）而不是再 `smoothTime=0`。**

施工说明落盘：`Assets/Doc/施工说明/0912/ForestScene_保留相机移动_消闪并修复接话_施工说明.md`（本侦探轮未写施工）。

---

## 8. 限制

- 未改代码 / Prefab / 场景 / Git。  
- 未把龙宫 Stairs 定格方案套回 Forest。  
- 未 Play；接话中断以代码时序 **已证实**，验收用 Console Warning + YaerAfter 是否出现。
