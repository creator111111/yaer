# Village · 暂时关闭出村长家送树屋戏 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】临时闸  
**Unity**：2020.3.48f1

---

## 沟通摘要

### ① 结论一句话

**从村长家出来，现在直接回村，不再播送树屋对白。**

### ② 原因（通俗）

以前 1 楼出门落门前会自动播「出村长家送树屋」。产品暂时只要静默回村，所以加了总闸关掉这场戏。左右门都受影响。

### ③ 用户检查清单

1. 左门按 E 或右门走进出屋 → 黑幕回村门前，**不要**出对白。
2. Console **不应**出现 `[LeaveChiefEscort] OnEnterScene TriggerStory`。
3. 回村后能正常走；2 楼楼梯回程仍不播这场（本来就不播）。

### ④ 程序补充

`Village_KenMuNiSceneManager`：`DisableLeaveChiefEscortTemporarily = true`，`ShouldPlayLeaveChiefEscort` 直接 false。  
要恢复戏：把该常量改回 `false`。Prefab / CSV / 落点键都没删。
