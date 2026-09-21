# ForestScene 门口 · 保留相机移动 + 修复接话 — 施工说明

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【施工员】按侦探报告方案 **M3** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0912/ForestScene_保留相机移动_消闪与接话中断_架构溯源报告.md`  
**前置（已纠偏）**：`施工说明/0912/ForestScene_演出结束相机定格去二次snap_施工说明.md`（A′ 瞬切 + B′ 整段 SKIP → **接话断**，本轮回滚/改语义）

---

## 沟通摘要

### ① 结论一句话

**已回滚门口收束瞬切，恢复平滑挪镜；`CameraMoveEnd` 等图注册后再发；主角应能接上 `YaerAfterLinEn`。**

### ② 原因（通俗）

上次修成「瞬间到位」后，程序在图还没走到「等待」就喊「好了」，门铃作废，后面又禁止再喊 → 雅儿不说话。  
现在改回慢慢挪过去，并且确认图已经站在门口等着，再按门铃。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 清 `homeDoorStoryComplete`，走完门口林恩链 | **可见**镜头从演出位平滑移回玩家（不是瞬切） |
| 2 | Console 滤 `[CHAIN]` / `ForestSceneLinEnStory` | `keepSmoothMove=M3`；若曾未注册应见「等待 Register」再 `FireNotify` |
| 3 | 同上 | 出现等待节点收到 `CameraMoveEnd`；**有** `ForestSceneYaerAfterLinEnStory`（主角接着说） |
| 4 | 图再调 `OnCameraMoveEnd` | 可见 `REENTER` / 跳过二次 snap；**不应**再整段吞掉 Notify |
| 5 | 闪一下是否可接受 | 若仍不可接受 → 另开 **M1 短黑幕**（本期未做）；**禁止**再 A′ `smoothTime=0` |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **M3-回滚 A′** | `ForestSceneLinEnStory.cs` · `OnDialogueEnd` | 去掉临时 `smoothTime=0`；`SetFollow` 用场景默认手推 |
| **M3-Notify 闸门** | 同 · `EnsureNotifyCameraMoveEndWhenRegistered` / `CoWaitRegisterThenNotify` | 未 Register 则实时轮询至注册或超时再发 |
| **B′ 改语义** | 同 · `OnCameraMoveEnd` | `_cameraMoveSnapHandledThisChain` 只挡二次 snap；未送达的 Notify 仍可 Ensure 补发 |
| **字段** | 同 | `waitRegisterCameraMoveEndRealSeconds`（默认 3）；去掉「整段 invoked 则 return」 |

### 未改（本期不做）

- 短黑幕 M1、Framing/M4 微调
- Prefab 删 id42（代码已可挡二次 snap）
- `ForestScene.unity` `smoothTime`、龙宫 Stairs
- 用固定 Wait 硬匹配机位时长

---

## 为何这样改（相对再瞬切）

| 做法 | 取舍 |
|------|------|
| **M3 回滚 + 等注册再发** | **采用**：接话 + 保留移动 |
| 再 A′ 瞬切 | **禁止**（产品钉死要移动；且不同步修时序会再断接话） |
| M1 黑幕 | 闪重时迭加；本期先验收移动+接话 |

**替代说明**：若超时仍打 Error 且接话断，查 id41 / `AnimationEventComponent` 挂点，而不是把 `smoothTime` 改回 0。

---

## 验收最短路径

新档进 Forest → 门口链 → 林恩结束 → 看镜头平滑移动 → 雅儿接话；Console 无长期「无人监听」后永久卡住。
