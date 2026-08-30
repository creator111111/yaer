# 获得道具 Tips 横幅 — 艾琳之剑溯源与老农复用 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`  
**范围**：本期落地报告 **A1**（对话内可弹 Item 横幅）；**未**接线老农发奖（Q1～Q3 未拍板）。

---

## ① 结论一句话

已新增 `OpenTipsFormActionTask`（默认 `ETipsType.Item`），对话图可与剑样板同路弹花边横幅；老农具体发什么道具/Tip 图/时机仍待产品，故未改 Prefab。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `…/UIPanel/OpenTipsFormActionTask.cs` | 新 Action：`TipKey` + `TipsType`（默认 **Item**）→ `TipsComponentGSM.OpenTipsForm` | 现网 `AddTipsInfoActionTask` 强制 Info，无获得物品音效；报告拍板 A1 |
| 同上 `.meta` | 新 guid | Unity 导入 |

**未改（门禁未齐）**

| 项 | 原因 |
|----|------|
| `Village_老农打水任务_完成结算.prefab` | 仅有 Generated.asset，无 Prefab；且 Q1 道具/金币、Q2 TipKey 图未定 |
| `EMainItemName` / Tip 三语图 / 图集 | 报告 P0 资源门槛；现网无 GetGold、无老农专用 GetXxx |
| TipsPanel 视觉 / 剑样板 `HomeScene2Box` | 报告明确不改 |
| 占位旧图（如 GetHpBall） | 须产品书面接受错字，本期未擅自占位 |

**用法（产品拍板后）**

```
… Statement「这是你的报酬」
  → GetItemActionTask（或发金 C#）     // 入包
  → OpenTipsFormActionTask(TipKey, Item) // 横幅+获得物品音效
```

缺图时 Console：`未找到Tips图片：{key}`，且不弹窗（与剑链路一致）。

---

## ③ 验收清单

### 本期可验（Task 落盘）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Unity 编译无错；NodeCanvas 可见「打开Tips横幅(可Item)」 | ✅ |
| 2 | （可选）临时图挂已知 Key `GetAiLinSword` 试播 | 同款花边 + 获得物品音效 |

### 老农全链路（待 Q1～Q3）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 触发发奖点 | 背包有对应物 + TipsPanel 同款横幅 |
| 2 | TipKey 缺图 | Error 且不弹 |
| 3 | 剑样板回归 | 卧室开箱仍弹 GetAiLinSword |

---

## ④ 给程序 · 待产品拍板再续跑

| ID | 需答复 | 默认倾向（报告） |
|----|--------|------------------|
| Q1 | 发道具还是金币？道具枚举名？ | 结算文案偏钱 |
| Q2 | TipKey + 三语 png 谁出？可否占位旧图？ | 新图 P0 |
| Q3 | 挂在接受 / **完成结算** / 其它？ | **完成结算句后** |

拍板后最小续跑：出图入图集 →（若道具）补枚举/库 → 建 `_完成结算` Prefab → GetItem/发金 + `OpenTipsFormActionTask`。
