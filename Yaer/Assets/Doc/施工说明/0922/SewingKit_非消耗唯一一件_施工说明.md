# SewingKit · 非消耗唯一一件 — 施工说明

**文档版本**：v1.0（2026-09-22）  
**文档性质**：【施工员】方案 A+B+C+D  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0922/SewingKit_非消耗唯一一件_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**针线包入包后永远只有 1 件，角标不再显示数字；旧档的「3」读档/刷包后会钳回 1。**

### ② 原因（通俗）

类型本来就是重要道具（不能当药喝）。角标「3」是账本真叠到 3——以前再发一次就 +1。本票给「一件家当」加白名单：不叠、旧档钳 1、UI 藏角标。空桶/药水不进白名单。

### ③ 用户检查清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 新档续聊首次获得 | 包内针线包；不能当药喝 |
| 2 | 角标 | 无「3」、无「1」数字 |
| 3 | 再 GetItem / 调试再加 | 仍为 1 |
| 4 | 旧档已有 3 | 读档或开背包后变为 1 |
| 5 | 空桶 1～4 | 不被误伤 |
| 6 | 血药/球堆叠 | 正常 |
| 7 | Tips「获得了针线包」 | 仍正常（二次发放仍可弹 Tips，包内仍 1） |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `PlayerBagData.cs` | `UniqueMainItemNames` + `IsUniqueMainItem`；`AddMainItem` 已有不叠、首次强制 1；`SetMainItemCount` 唯一件顶 1；`ClampAllItemStacks` / Refresh 修旧档 |
| `MenuFormMainItemBtn.cs` | 唯一件隐藏 `num` TMP（方案 D） |
| `MainItemConfig.json` | SewingKit `itemType` 2→0（方案 A；运行时真源仍是 Database） |

**唯一白名单**：SewingKit、AiLinSword、Map、GushaNacklace、XiaerPower。  
**排除**：EmptyWaterBucket、FullWaterBucket；全部 Cost/Material。

**未改**：续聊 Prefab / Tips；`MaxStackPerItem`；商店；Database（已是 TaskItem）。

---

## 为什么这样改

只藏角标不修账本会留下 num=3；全局 MaxStack=1 会毁药/球。白名单 + 入包门控 + 读档钳是最小闭环。
