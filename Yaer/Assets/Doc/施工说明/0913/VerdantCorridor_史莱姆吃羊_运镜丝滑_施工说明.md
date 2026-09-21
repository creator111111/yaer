# VerdantCorridor 史莱姆吃羊 · 运镜丝滑 — 施工说明

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【施工员】按侦探报告 **B + A** 落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_运镜抖闪不丝滑_架构溯源报告.md`  
**根因**：Duration=1 甩镜 + 开推 forceSnap 手推与 DOMove 抢位 + OutQuad 起停硬 + 重复 FollowPlayer

---

## 沟通摘要

### ① 结论一句话

**走廊推拉改成约 2.2 秒；共用 CameraMove 用 InOutSine、开推不 forceSnap、Destroy 晚一帧；跳过重复跟玩家。**

### ② 原因（通俗）

1 秒推一屏多像甩；开推还先「手拽」一下再跑，两套运动抢方向就抖。现在慢慢推、起停柔和、到位稳一下再换目标。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 新档走进触发器，看去程推镜 | **慢慢**推到羊尸处；开推/到位无明显跳闪 |
| 2 | Wait 后拉回 | 同样丝滑，约 2.2s |
| 3 | 对白时序 | 仍：先「那是…」→ 推镜 → 拉回 → 后续句 |
| 4 | 黑幕后 | Action2 刷怪不露闪 |
| 5 | 抽测东城郊吃羊 | 时序不变；开推应更稳（时长仍 1s） |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **B** | `CameraMoveTaskAction.cs` | 开推 `forceSnap:false`；`SetEase(InOutSine)`；软绑后 `Yield(LastPostLateUpdate)` 再 Destroy |
| **A** | `VerdantCorridorSlimeEatSheep.prefab` | id1/id3 `Duration=2.2` |
| **跳过 id4** | 同上 Prefab connections | `3→9`（去掉重复 `CameraFollowPlayer`） |

### 未改

- 时序主干 `0→8→1→2→3→…`  
- Action2 / 台词 / 东城郊 Duration（本期仍 1s）  
- `CameraComponent.smoothTime` 全局 / 瞬切  

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 | 走廊 Duration **2.2s**（去/回相同） |
| Q2 | 东城郊 **不加长** |
| Q3 | **跳过** id4 FollowPlayer |
| Q4 | 开推 **forceSnap=false** |

---

## 验收标准

1. 去/回程可感知慢慢，开推/到位无跳变  
2. 对白↔镜头顺序不回退  
3. 黑幕刷怪不露闪  
4. 东城郊抽测开推更稳  
