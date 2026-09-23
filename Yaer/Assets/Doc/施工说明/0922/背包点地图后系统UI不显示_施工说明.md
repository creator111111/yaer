# 背包点地图后系统 UI 不显示 — 施工说明

**文档版本**：v1.2（2026-09-23 「只剩金币」回修）  
**文档性质**：【施工员】  
**Unity**：2020.3.48f1  

---

## 沟通摘要

### ① 结论一句话

**关地图后再 ESC，完整菜单（含按钮）必显示；店内 ESC 仍离店。**

### ② 原因（通俗）

两层问题叠在一起：

1. ESC 关界面回调曾漏退订（已修）。  
2. **只剩金币**：开背包时菜单中间按钮区被藏成透明；点地图若先关菜单再关道具页，恢复回调收不到，下次开菜单中间仍透明，只剩外侧金币。

### ③ 用户检查清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | ESC 开菜单 → 背包点地图 → 关地图 → 再 ESC | **按钮+金币都在**，可操作 |
| 2 | 仅开背包再关（不点地图）→ ESC | 菜单完整 |
| 3 | 开图关图 ESC ×3 | 稳定 |
| 4 | 店内 ESC | 仍离店 |

### ④ 程序补充

| 路径 | 改动 |
|------|------|
| `MenuCenterHideWhenItemShowPanel.cs` | OnEnable 按 ItemShow 是否在栈重算 Center 显隐 |
| `ItemMap.cs` | **先关 ItemShow 再关 Menu** |
| `MenuFormLogic.cs` | OnOpen 兜底恢复 Center |
| `BaseUIFormLogic.cs` | OnClose 退订 ESC（前次） |
| `MapFormLogic` / `InputComponentGSM` | 关图清门闩（前次） |
