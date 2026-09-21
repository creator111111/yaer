# HomeScene1 ↔ HomeScene2 · Stairs 换场相机定格 — 施工说明

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【施工员】按侦探报告方案 **A** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`  
**根因**：进场 `SetFollow(forceSnap)` 在 `smoothTime≈0.3` 下手推未收束，黑幕 hold 0.3s 即揭幕 → 1→2 偏闪、2→1 左→右滑

---

## 沟通摘要

### ① 结论一句话

**龙宫两侧 `CameraComponent.smoothTime` 已改为 0：Stairs 揭幕时镜头当帧定格在落点，不再手推闪/滑。**

### ② 原因（通俗）

换场后相机不是立刻贴人，而是从场景默认机位慢慢挪过去；黑幕又揭得早，所以你看见闪一下或从左滑到右。  
把进场平滑时间关掉，镜头直接卡在人身上再揭幕。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy：`HomeScene1/Camera` → `CameraComponent` | `smoothTime = 0`（已序列化，不是空着吃默认） |
| 2 | Hierarchy：`HomeScene2/Camera` → `CameraComponent` | `smoothTime = 0`（原 0.3） |
| 3 | HS1 Stairs → HS2（上楼）≥2 次 | 揭幕**无闪帧**、无短促跳变 |
| 4 | HS2 Stairs → HS1（下楼）≥2 次 | 揭幕**无左→右滑动** |
| 5 | 同场景只走路（不换场） | 日常 Follow 阻尼仍正常（本案只影响进场 forceSnap 手推） |
| 6 | （回归抽测）Forest→HS1、换装→HS2 | 进场同样瞬切定格；若某条剧情要平滑追镜再另案 |
| 7 | 若 A 后 1→2 **仍闪** | 转验收：再议 HS2 Framing `DeadZoneHeight`（OPEN Q3），本期未改 |

### ④ 程序补充

见下文改动清单。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A1** | `Assets/GameRes/Scenes/HomeScene1.unity` | `CameraComponent` 补序列化 `smoothTime: 0`（及 followSnap / 手推兜底字段，与 HS2 对齐） |
| **A2** | `Assets/GameRes/Scenes/HomeScene2.unity` | `smoothTime: 0.3` → **`0`** |

### 未改（报告禁止 / 本期不做）

- `CameraComponent.cs` / `LoadSceneComponentGSM.cs` / `BaseGameSceneManager`（不上方案 B「对齐再揭幕」）
- Stairs Prefab / `EnterPosConfig` / 默认 VCam 世界坐标（方案 C）
- HS2 Framing `m_DeadZoneHeight` / `YDamping`（OPEN Q3，A 后仍闪再开）
- 村庄 / 村长家 Stairs / 战斗镜头

---

## 为何用方案 A（相对 B/C）

| 方案 | 本案取舍 |
|------|----------|
| **A 两侧 smoothTime=0** | **采用**：改动面最小、产品语「定格」、符合 SPEC §3（仍走 SetFollow，瞬切是 API 已有分支） |
| B 黑幕等 onComplete | 改公共换场契约，影响面过大 |
| C 只挪默认 VCam | 缩小位移但不消灭手推契约缝 |

**替代说明**：若日后某条龙宫剧情必须「进场平滑追镜」，应对该 Story 显式 `SetFollow(..., forceSnap:false)` 或临时抬高 smoothTime，而不是把默认进场改回 0.3。

---

## 验收最短路径

Init/读档进龙宫可走 → Stairs 上楼 → 立刻 Stairs 下楼；Console 可滤 `SceneChangeDoor` / `SceneLoad`（应仍为 `blackFade=true`）。
