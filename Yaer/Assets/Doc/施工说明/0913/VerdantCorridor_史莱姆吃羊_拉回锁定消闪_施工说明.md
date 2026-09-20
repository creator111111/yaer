# VerdantCorridor 史莱姆吃羊 · 拉回锁定消闪 — 施工说明

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【施工员】用户验收反馈「摄像机回来锁定玩家时屏闪」  
**Unity**：2020.3.48f1  
**前置**：`VerdantCorridor_史莱姆吃羊_镜头对白时序对齐_施工说明.md`（连线已对齐；本案修物理交接）

---

## 沟通摘要

### ① 结论一句话

**拉回后不再「销毁临时跟拍点 → 再 forceSnap 手推玩家」**：移动结束软绑落点，跟随主角默认不二次手推，屏闪应消失。

### ② 原因（通俗）

镜头是先绑在一个临时空物体上滑回来的。滑完把空物体删掉，再突然「吸」回玩家，中间会晃/闪一下。现在滑完直接交给落点/玩家，不再吸第二次。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 新档走廊吃羊：推镜 → Wait → **拉回 + 锁定玩家** | **无明显屏闪/二次滑动** |
| 2 | 同上 | 时序仍正确：第一句在推镜前；其余在 Follow 后 |
| 3 | 黑幕后 | Action2 刷怪正常 |
| 4 | 抽测东城郊同款 / 其它「CamMove→Follow」图 | 拉回锁定也不应更闪；若某图要远处瞬切，在 Follow 节点勾 `forceSnapToTarget` |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A** | `CameraMoveTaskAction.cs` | DOMove 完 → `SetLock(false)` → `SetFollow(EndPos, forceSnap:false)` → 再 `Destroy(go)` |
| **B** | `CameraFollowPlayerActionTask.cs` | 新增 `forceSnapToTarget`（**默认 false**）；`SetFollow(player, forceSnap:…)` |

### 未改

- 走廊 / 东城郊 Dialogue Prefab 连线与文案  
- `CameraComponent.smoothTime` / 短黑幕 M1  
- Action2 / Mgr2  

### 替代说明

| 做法 | 取舍 |
|------|------|
| **A+B 软交接 + Follow 不手推** | **采用**：改动面小、所有 Move→Follow 链受益 |
| 短黑幕掩护手推（Forest M1） | 过重，本案不需要 |
| Follow 仍 forceSnap + smoothTime=0 瞬切 | 可能变成硬切闪，产品要「定格」时再用节点勾 forceSnap |

---

## 验收最短路径

新档 `VerdantCorridor` → 走进吃羊触发器 → 盯「拉回锁定玩家」那一帧；应无二次晃闪，对白时序保持金标准。
