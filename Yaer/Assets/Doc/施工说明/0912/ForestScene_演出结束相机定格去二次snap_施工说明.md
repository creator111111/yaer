# ForestScene 演出结束 · 相机定格与去二次 snap — 施工说明

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【施工员】按旧侦探报告方案 **B′ + A′** 落地  
**⚠ 终态作废**：产品纠偏后 A′ 瞬切导致接话中断；请改看  
`施工说明/0912/ForestScene_保留相机移动_消闪并修复接话_施工说明.md`（**M3**）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0912/ForestScene_演出结束相机位置与屏闪_架构溯源报告.md`  
**根因**：林恩收束 `SetFollow(forceSnap)+smoothTime=0.3` 无黑幕手推可见；图 id42 再调 `OnCameraMoveEnd` → 二次 snap

---

## 沟通摘要

### ① 结论一句话

**（历史）** 门口林恩收束改为当帧瞬切…… → **已被 M3 回滚；勿再按「无移动」验收。**

### ② 原因（通俗）

戏播完镜头要从「看戏位」回到雅儿身上。以前用 0.3 秒慢慢挪、又没黑幕挡，所以看见错位再滑。  
图上还在「挪完」之后又点了一次收尾，等于再 snap 一遍。现在第一次直接定格，第二次直接忽略。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 清/新档使 `homeDoorStoryComplete=false`，走完门口林恩链 | 对白结束瞬间镜头已在玩家机位，**无可见滑动/闪帧** |
| 2 | Console 滤 `[CHAIN]` | `OnCameraMoveEnd ENTER` **每轮 1 次**；图再调应见 `SKIP(idempotent)`，**无**第二次真正收束 |
| 3 | 同次链路 | `YaerAfterLinEn` 仍正常接上 |
| 4 | Forest 日常走路 | 跟拍阻尼正常（场景 `smoothTime` 仍为 0.3，仅 LinEn 收束临时置 0） |
| 5 | 勿测成龙宫 Stairs | 本案只动 Forest LinEn；龙宫已另案 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A′** | `ForestSceneLinEnStory.cs` · `OnDialogueEnd` | `SetFollow` 前后临时 `CameraComponent.smoothTime=0` 再恢复；当帧定格 + 同步 `onComplete` |
| **B′** | 同文件 · `OnCameraMoveEnd` | 本轮已执行则立刻 return（幂等）；挡住图 id42 二次 snap |
| **附** | 同 · 安全兜底 | 仅当 `!_onCameraMoveEndInvokedThisChain` 才挂 2s 协程（A′ 同步成功后不再空挂） |

### 未改（本期禁止 / 不做）

- `ForestScene.unity` 整场景 `smoothTime`（OPEN Q2 备选，未采用）
- `ForestSceneLinEnStory.prefab` 删 id42（用代码幂等替代，改图风险更高）
- 黑幕方案 C、Framing-only 方案 D
- HomeScene / 龙宫 Stairs / ForestEast

---

## 为何 A′ 用「临时 0」而不是整场景 0

| 做法 | 取舍 |
|------|------|
| **仅 LinEn 路径临时 0** | **采用**：只修门口收束；Forest 其它进场/跟拍手感不变 |
| 整 Forest `smoothTime=0` | 备选；影响面更大 |
| 改 Prefab 删 id42 | 等价 B′；YAML 图难 diff，故用幂等 |

**替代说明**：若验收仍见软追（绑 Follow 后 Framing），再议降 `m_XDamping`（报告方案 D，不单独当主修）。

---

## 验收最短路径

新档进 Forest → 触发门口 → 林恩对白结束 → 看机位与 `[CHAIN]` → 确认 YaerAfterLinEn 接上。
