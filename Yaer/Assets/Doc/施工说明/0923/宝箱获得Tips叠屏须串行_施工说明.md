# 宝箱获得 Tips 叠屏须串行 — 施工说明

**文档版本**：v1.0（2026-09-23）  
**文档性质**：【施工员】  
**Unity**：2020.3.48f1  
**报告**：`Assets/Doc/执行文档/0923/宝箱获得Tips叠屏须串行_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**开箱连发 Tips 只开一个面板，横幅一个接一个出，不再叠屏。**

### ② 原因（通俗）

同帧连调两次 `OpenTipsForm` 时，第一次面板还没开完，第二次又开了一个 TipsPanel，两张横幅叠在一起。面板里本来有队列能串行，但第二次从没进到队列里。

### ③ 用户检查清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 村 Tree2f 宝箱开箱 | GetHpBall 播完（或间隔）再 GetMpBall；**不叠** |
| 2 | Hierarchy | 同时最多 **一个** TipsPanel |
| 3 | 背包 | Hp+3、Mp+3 仍对 |
| 4 | 西境同款箱 / 夏尔连发（抽测） | 同样串行 |
| 5 | 单条 Tips（剑等） | 不回潮 |

### ④ 程序补充

| 路径 | 改动 |
|------|------|
| `TipsComponentGSM.cs` | 方案 A：`_openingTipsPanel` + `_pendingTips`；Open 中禁止再开；callBack 后逐条 `AddTipsInfo` |

**未改**：箱脚本发奖顺序/数量、`Box.prefab`、`TipsFormLogic` 队列本体。
