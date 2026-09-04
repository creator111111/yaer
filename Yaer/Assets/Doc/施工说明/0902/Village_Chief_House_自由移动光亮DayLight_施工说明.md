# Village_Chief_House — 自由移动光亮 DayLight — 施工说明

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【施工员】按侦探报告 F1 落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0902/Village_Chief_House_自由移动光亮DayLight_架构溯源报告.md`  
**产品**：村长家自由移动与其它 NPC 家里一致 → Home `Idle`/`Walk`/`Bink` 槽播 `*_DayLight`

---

## 沟通摘要

### ① 结论一句话

**已在 `VillageHomeDayLightAnimApplier` 白名单加入 `Village_Chief_House`；进房自动换白天片子。未动龙宫/村街/状态机名。**

### ② 原因（通俗）

村民家进门会把暗版家居动画换成白天光亮片，抽屉名还叫 Idle/Walk/Bink。  
村长家以前没写进这份名单，所以一直暗版。补上名字就行。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村长家站立/走路/眨眼：光亮，对齐 `HomeScene23` | |
| 2 | Console：`[VillageHomeDayLight] 已换白天` 且 `scene=Village_Chief_House` | |
| 3 | 出屋回村街：非 DayLight | |
| 4 | 龙宫 `HomeScene1/2`：仍非 DayLight | |
| 5 | 自由移动不是 Combat `Run`；续聊还控后仍是光亮 Home | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `VillageHomeDayLightAnimApplier.cs` | `VillageHomeSceneNames` 追加 `SceneName.Village_Chief_House` + 注释 |

**未改**：Animator 状态名、`RegisterState`、磁盘 Override、`isFightingScene`、龙宫/村街名单、续聊战斗涂层。

---

## ② 若 Console 出现「找不到白天 Clip」

查该套 Home Override 是否缺 `Idle_DayLight` / `Walk_DayLight` / `Bink_DayLight` 行（同 0818/0822）。  
**禁止**改 C# 状态名、禁止方案 E 改磁盘共用槽。

---

## ③ OPEN

| ID | 项 | 状态 |
|----|----|------|
| Q1～Q4 | 白名单 / 龙宫勿进 / 不改状态名 | ✅ 已施工 |
