# VerdantCorridor 史莱姆吃羊 · 运镜抖闪不丝滑 — 架构溯源报告

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**现象（用户）**：走廊 `SlimeEatSheepStoryTrigger` 运镜 **又抖又闪、不丝滑**；要求慢慢推、好好做完  
**产品期望**：去程 / 停顿 / 拉回全程可感知平滑；对白↔镜头顺序保持东城郊金标准；黑幕刷怪不穿帮  
**不是**：取消运镜改瞬切；重做 Cinemachine；改东城郊图当实验；改村庄相机；全局 `smoothTime=0`  
**对照**：`ForestEastSceneSlimeEatSheep`（时序金标准，Duration 同样是 1s）；Forest 门口消闪经验可参考机制，**勿照搬林恩链脚本**  
**前案（时序）**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_架构溯源报告.md`  
**时序施工**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序对齐_施工说明.md`（连线已对齐）  
**提示词**：`Assets/Doc/提示词/0913/VerdantCorridor_史莱姆吃羊_运镜抖闪不丝滑_架构侦探提示词.md`  
**施工说明**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_运镜丝滑_施工说明.md`（2026-09-13 已按 B+A 落地）

---

## 沟通摘要

### ① 结论一句话

**顺序已经对了，难看是因为「甩太快 + 开推还先手推对齐」**：约 20 单位路程只用 1 秒，DOMove 默认 OutQuad 起手猛；`SetFollow(临时物体)` 默认 `forceSnap=true` 又用 0.3s SmoothDamp 去追一个已经在飞的目标，起止就会顿/闪。要丝滑必须 **加长走廊 Duration**，并且 **改共用的 `CameraMoveTaskAction`（Ease + 开推不要 forceSnap）**；不要改成瞬切。

### ② 原因（通俗）

镜头要横推快一屏多，却只给 1 秒，看起来像甩。更糟的是：每次开推会先「伸手把机位拽到临时点」，而临时点已经开始往前跑，两套运动抢方向，开头就会抖一下。到位后又换跟拍目标、毁掉临时物体，再跟一次玩家，结尾再顿一下。黑幕节点本身会等淡满再刷怪，**闪内容**不是主因，主因是运镜交接。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网预期（未修） |
|---|------|------------------|
| 1 | 新档走进触发器，看第一句后的推镜 | 约 1s 甩到 CameraPos2；开头可能顿/闪 |
| 2 | 停 1s 后拉回 | 同样偏急；到位后再跟玩家可能再顿 |
| 3 | 对照东城郊 | **同样 Duration=1**、同一套 `CameraMoveTaskAction`；走廊因距离≈20.22 更显急 |
| 4 | 黑幕刷怪 | id5 `EndActonOnAnimationEnd=true`，一般应先黑再刷；若仍露怪再加本案 D |
| 5 | 拍板：走廊 Duration **2.2s**（可 2.0～2.5）+ 改 CameraMove Ease/开推 | 两边共用脚本，东城郊会一起少抖，但东城郊时长仍 1s |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **时序** | 现网连线已是 `0→8→1→2→3→4→9…13→5→6→7`（时序前案已落地）；本案 **不是** 先后问题 |
| **「不慢慢」主因 A** | 去程/回程 `Duration=1`，CameraPos2 local x≈**20.22** → ≈20u/s 甩镜 |
| **「抖/闪」主因 C** | 开推 `SetFollow(go)` **默认 forceSnap=true** + `smoothTime=0.3` 手推，与立刻 `DOMove` **抢位** |
| **加重 B** | `DOMove` **未 SetEase**，DOTween 默认 **OutQuad**（起手快、到位刹） |
| **加重 D** | DOMove 完 `SetFollow(EndPos, false)` 后 **同帧 Destroy(go)** + `EndAction`，不手稳定 |
| **加重 E** | 回程 EndPos 已是玩家（CameraPos1）；id4 `CameraFollowPlayer` **再 SetFollow 一次** |
| **F 黑幕刷怪** | id5 黑幕 Duration=1 且 **等动画完** 再 Action2 → **非主因**；黑幕节点丢引用才会闪 |
| **G Damping** | 跟拍移动中的临时 GO 时 CM 滞后，次要橡皮筋 |
| **只改 Prefab Duration？** | 能改善「急」，**治不好开推手推抢位** |
| **推荐** | **A + B**：走廊 Duration 加到 **2.2s** + `CameraMoveTaskAction`：`InOutSine`、开推 `forceSnap:false`、Destroy 延一帧；可选跳过重复 FollowPlayer |

---

## 2. 运镜调用链（逐步标抖闪点）

现网图序（已核实 connections）：

```
0 Setup + UI 淡入 0.7s
→ 8 「那是……」（镜头仍在玩家）
→ 1 CameraMove Pos1→Pos2  Duration=1     ★ 去程
→ 2 Wait 1s
→ 3 CameraMove Pos2→Pos1  Duration=1     ★ 回程
→ 4 CameraFollowPlayer（Prefab 未写 forceSnap → 默认 false）
→ 9…13 后续台词
→ 5 BlackMask 0→1 Duration=1 EndOnAnim=true
→ 6 Action2("start") 刷怪
→ 7 UI/黑幕收
```

### 2.1 `CameraMoveTaskAction.Move`（去程/回程各跑一次）

```
WaitUntil !IsLock
new GO @ StartPos
SetFollow(go)                          ★ 默认 forceSnap=true
  → smoothTime=0.3：清 VCam.Follow，LateUpdate SmoothDamp 追 GO
SetLock(true)
go.DOMove(EndPos, Duration)            ★ Duration=1；无 Ease → OutQuad
  ★ 与上面手推同时进行：GO 已在飞，手推去追飞靶 = 开推抖/闪
await DOMove complete
SetLock(false)
SetFollow(EndPos, forceSnap:false)     // 0913 已加：避免跟临时 GO
Destroy(go)                            ★ 同帧销毁；CM 刚换目标
EndAction()                            ★ 不等 Follow 稳定
```

**标点**

| 时刻 | 体感 | 嫌疑 |
|------|------|------|
| 开推 0～0.3s | 顿挫、小幅反向/闪 | **C** 手推抢 DOMove |
| 推镜全程 | 太快、像甩 | **A** Duration=1 / 20u |
| 起停 | 生硬 | **B** OutQuad |
| 到位瞬间 | 跳一下 | **D** 换 Follow + Destroy |
| 拉回后再跟玩家 | 二次顿 | **E** FollowPlayer 重复 |
| 黑幕→刷怪 | 若露怪 | **F** 弱（节点已等满黑） |

`02_SYSTEM_SPEC`：镜头用完成回调衔接。本案 Duration/Ease 是 **任务参数与实现**，不是用乱 Delay 去硬配台词。Wait 1s 是图内看点，对齐东城郊保留即可。

### 2.2 距离对照

| | CameraPos2 local x | Duration | 平均速度 |
|--|-------------------|----------|----------|
| 走廊 | **20.22** | 1 | ≈20.2 u/s |
| 东城郊 | **22** | 1 | ≈22 u/s |

东城郊同一套脚本、同样偏急；走廊用户在时序修好后更明显看到这段推拉。正交尺寸约 7.9，1 秒横跨超过一屏。

---

## 3. 证据表

| # | 证据 | 结论 | 状态 |
|---|------|------|------|
| E1 | 连线 `0→8→1→2→3→4→9…→5→6→7` | 时序已对齐金标准 | ✅ |
| E2 | id1/id3 `Duration._value: 1`；Pos1↔Pos2 | 推拉各 1s | ✅ |
| E3 | 场景 CameraPos2 `{x:20.22,y:0}` | 横移约 20u | ✅ |
| E4 | `SetFollow(go)` 无第三参 → `forceSnapToTarget=true` | 开推必选手推 | ✅ |
| E5 | `CameraComponent.smoothTime=0.3`；手推时 `Follow=null` | 与 Forest 门口闪同源机制 | ✅ |
| E6 | `DOMove(..., Duration)` 无 `SetEase` | 默认 OutQuad | ✅ |
| E7 | 现网已有 `SetFollow(EndPos, false)` 再 Destroy | 交接做了一半，仍同帧 Destroy、仍开推 forceSnap | ✅ 部分修复 |
| E8 | id4 FollowPlayer 默认 `forceSnap=false`；CameraPos1=玩家 | 回程结束已跟玩家，id4 **多余** | ✅ |
| E9 | id5 `EndActonOnAnimationEnd: true`，再 Action2 | 黑幕满再刷怪 | ✅ F 非主因 |
| E10 | `CameraComponentGSM.SetFollow` 在 `IsLock` 时直接 return | 运镜途中别人切 Follow 无效；手推是 Move **开锁前** 自己调的 | ✅ |
| E11 | Forest 门口案：产品否决 `smoothTime=0` 瞬切 | 本案禁止方案 C | ✅ 约束 |
| E12 | Play 标「抖发生在哪一段」 | 侦探未实机；链路足够定性主因 | ⚠️ 待验收 |

### 嫌疑复核

| 嫌疑 | 裁定 |
|------|------|
| **A Duration 太短** | **已证实**（不「慢慢」） |
| **B 无 InOut 缓动** | **已证实**（起停生硬） |
| **C 开推 forceSnap 手推** | **主抖/闪 · 已证实** |
| **D Destroy/换 Follow 同帧** | **加重 · 已证实** |
| **E 重复 FollowPlayer** | **加重 · 已证实** |
| **F 黑幕未盖住刷怪** | **排除为主因**（已等满）；验收仍露再 D |
| **G CM Damping** | **次要**；加长+InOut 后会减轻 |

---

## 4. 与时序前案关系

| 案 | 解决什么 | 现网 |
|----|----------|------|
| 镜头↔对白时序 | 谁先谁后 | **已对齐**：先「那是……」再推镜；拉回后再后续句 |
| **本案** | 运动质感 | 顺序对了仍甩、仍抖 |

时序施工 **未改** Duration / Ease / `CameraMoveTaskAction` 开推参数。不要把本案修回去成瞬切，也不要再改 connections 顺序。

---

## 5. 修复方案对比

### 建议参数（「慢慢」）

| 项 | 建议 | 说明 |
|----|------|------|
| 去程/回程 Duration | **2.2s**（可接受 2.0～2.5） | 20.22u / 2.2s ≈ **9.2 u/s**，约 1s 半屏，能看出在挪 |
| Ease | **InOutSine** | 两端柔；禁止靠 OutQuad 甩 |
| Wait 看点 | **保持 1s** | 对齐东城郊节奏，不是硬配台词 |
| 按距离算速度 | 可选后续 | 本期固定 2.2s 足够；东城郊 22u 若以后要慢慢再单独加时长 |

### 方案 A（只改走廊 Prefab）

把 id1/id3 Duration 改为 2.2；可选 connections `3→9` 跳过 id4 FollowPlayer。

| 利 | 弊 |
|----|-----|
| 不动 C#；东城郊时长不变 | **开推手推抢位仍在**，抖可能还在 |
| 跳过 Follow 少一次切换 | 「慢慢」能改善，丝滑不够 |

### 方案 B（改 `CameraMoveTaskAction` · 必须）

1. 开推 `SetFollow(go, forceSnapToTarget: **false**)`，避免 0.3s 手推与 DOMove 抢位。  
2. `DOMove(...).SetEase(Ease.InOutSine)`。  
3. `SetFollow(EndPos, false)` 后 **下一帧再 Destroy(go)**（或等 1 帧），再 `EndAction`。  
4. **不要** `smoothTime=0`，**不要** 开推 forceSnap 当帧瞬切机位。

影响：所有用该任务的图（含东城郊吃羊）开推更稳；东城郊仍是 1s，只减抖不自动变慢。

| 利 | 弊 |
|----|-----|
| 治主抖；东城郊同受益 | 须抽测其它 CameraMove 演出 |
| 符合「保留移动」 | 不能单靠它达到「慢慢」（时长仍看 Prefab） |

### 方案 C（禁止）

`smoothTime=0` / forceSnap 瞬切消闪。产品已否决「不要运镜」。

### 方案 D（可选 · 仅本图）

若验收黑幕仍露刷怪：保证 StartAlpha=0、EndAlpha=1、`EndActonOnAnimationEnd=true`（现网已是）；必要时略加长黑幕。对齐 Forest M1「先盖住再做事」，**只动本图节点**。

### 推荐组合

**B + A**（主）+ **跳过 id4**（顺手）+ **D 仅当验收露怪**。

| 项 | 内容 |
|----|------|
| **走廊 Prefab** | Duration 2.2 / 2.2；可选 `3→9` 去掉重复 Follow |
| **CameraMoveTaskAction** | false snap 开推 + InOutSine + Destroy 延帧 |
| **不要动** | 时序 connections 主干；Action2；东城郊台词序；`CameraComponent.smoothTime` 全局；村庄相机；CameraPos2 摆位；林恩链脚本 |

### 回归与验收

| 风险 | 验收 |
|------|------|
| 时序回退 | 第一句仍在推镜前；后续句在拉回后 |
| 东城郊 | 抽测推拉仍在；开推应更稳；时长仍 1s（除非另案加长） |
| 其它 CameraMove 图 | 抽 1～2 条常用运镜 |
| Action2 / SingleUse | 黑幕后刷怪；同存档不重播 |
| 瞬切回归 | 肉眼必须看到「慢慢挪」，不是跳切 |

**验收标准**

1. Play ≥2：去程可感知慢慢，开推/到位无明显跳变。  
2. 拉回同样丝滑。  
3. Follow 后台词正常。  
4. 黑幕期间刷怪不露闪。  
5. 对白↔镜头顺序不回退。  
6. 若改了共用 CameraMove：东城郊同触发器过一遍。

---

## 6. OPEN_QUESTIONS

已记入 `Assets/Doc/OPEN_QUESTIONS.md`：

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 走廊 Duration 取 2.2s 还是 2.5s？ | **2.2s**（去程/回程相同） | 待确认 |
| Q2 | 东城郊是否一并加长 Duration？ | **本期否**（只吃 B 的减抖） | 待确认 |
| Q3 | 是否删除/跳过 id4 FollowPlayer？ | **建议跳过**（`3→9`） | 待确认 |
| Q4 | 开推 forceSnap=false 后若偶发「没贴上起点」？ | **先 false**；勿改回 true 手推抢 DOMove | 待 Play |

---

## 7. 给施工员的一句话

**走廊 Prefab 把推拉改成约 2.2 秒；`CameraMoveTaskAction` 用 InOutSine、开推不要 forceSnap、Destroy 晚一帧；不要瞬切，也不要动已经对齐的台词顺序。**
