# ForestEast 洞口爬完 · 对白不触发卡死 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **A**（命中停爬 + Sign 超时强制进洞 + 修正 Out Right + AutoEnter 略加宽）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/ForestEast_洞口爬完对白不触发卡死_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**爬到洞口会先停掉强制爬行锁，再黑幕送进树洞并播原来的进洞对白；右口出洞盒也会跟着显隐。**

### ② 原因（通俗）

洞口有两套机关叠在一起：一套逼你蹲着爬、爬的时候锁走和菜单；另一套碰到窄门才黑屏送进去，进去后才说话。以前爬行区从洞口铺到洞里，人没爬出这块地锁就不会解开；进洞门又太窄，人经常站在洞外进不去，对白盒还在树里面。镜头贴底只在黑屏之后才跑，跟这张「洞外站着卡住」无关。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Console | 有 `[CanNotSomeActionArea] StartAutoCrawl` 后，进洞或离开爬区必有 `StopAutoCrawl`；命中门有 `[TreeEnter] find player` |
| 2 | 左口从外侧 S+方向爬进 | 黑幕进洞；存档未用过时播 `ForestEastSceneEnterTreeBridge` |
| 3 | 爬完 / 进洞后 | 可走；可 ESC 菜单（洞内仍不能站直，这是原设计） |
| 4 | 进洞镜头 | 仍贴 `CameraTreeInArea` 底边（afterY≈−2.9） |
| 5 | 右口进出 | 出洞盒 Right 会随进洞关掉、出洞后再亮 |
| 6 | Sign 卡住（Hurt/跳等） | 约 2 秒后仍会强制进黑幕，不会永久不能动 |

走近树（x≈248）仍是 **BeforeEnter**（SingleUse）；洞口爬完缺的是进洞后的 **Enter**，不是 `PassTreeBridge`。

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **命中停爬** | `ForestEastTreeEnterTrigger.OnPlayerDetected` | `StopAutoCrawlForPlayer` + `SetBlockAutoCrawl(true)`，再 `DisablePlayerMove` |
| **Sign 超时** | 同上 `Update` | 等 Idle/Run/Squat，**2s** 仍不是则强制走黑幕（OPEN Q4） |
| **防再开爬** | `CanNotSomeActionArea.blockAutoCrawl` | 黑幕流程中 Update 不再 `StartAutoCrawl` |
| **Out Right** | `ForestEastTreeBridgeStoryMgr` | `ChangeEnterAndOutNodeActive` / `AwakeAllStoryNodeActive` 第二处改为 `storyTriggerOutNodeRight` |
| **门加宽** | `ForestEastScene` AutoEnter Left/Right | 左口盖住与爬区之间约 1.7 空档；底边略降；不碰到 AutoOut |

### 未改

- `CameraTreeInArea` / 贴底公式  
- 对白 CSV / Prefab 文案  
- `CanNotSomeActionArea` 盒宽 80（方案 B 不用）  
- `GroundCenter` / `GroundLeft_1` 地板（OPEN Q5）  
- `PassTreeBridge` 触发器  
- 存档 SingleUse 逻辑（OPEN Q2）

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 以 A 为默认修 | **是**（C 用超时兜住；H 无证据不改壳） |
| Q2 改存档 SingleUse | **否** |
| Q3 右口双写 | **已改正** |
| Q4 Sign 超时 | **强制走完进洞黑幕** |
| Q5 改地板 | **否** |

---

## 替代方案（未采用）

- **B** 缩短爬行盒：洞外不再锁，但进洞门仍可能擦边；可与 A 以后组合  
- **C** 只加宽盒不加超时：不解 Disable 死等  
- **D** 回滚 0914 贴底：与洞外截图无因果  

---

## 剩余风险

- BeforeEnter / Enter 若存档已 SingleUse，再爬无词但应能走、能菜单。  
- 人在门上死亡时 `hasFindPlayer` 仍可能卡住 Pause（原有路径，本期未扩）。  
- 本机未 Play 左/右口；请按清单验收。
