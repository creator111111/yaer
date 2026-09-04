# Village — 出村长家 · 古雅对白转场树屋门口 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_出村长家_古雅对白转场树屋门口_架构溯源报告.md`  
**依赖**：楼梯案 **E3′**（`enterPosKey` + `ExitFrom_HomeSceneChief` + LeftDoor 填键）已落地

---

## 沟通摘要

### ① 结论一句话

**O1+G1+T1：1 楼出门落门前 → 自动播 `Village_出村长家送树屋`；段 A 末黑幕传送到 `TeleportTo_YaerTreeHouseDoor`（Walk 内近 House_Tree）→ 段 B；2 楼回来不播。**

### ② 原因（通俗）

台本是「刚出村长家门口聊几句，黑一下人就到树屋门口道别」。  
1 楼门和楼梯上楼的落点已用 E3′ 拆开；对话里补了黑幕传送节点，避免硬闪坐标。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 开 Unity 跑 `Tools / Dialogue / Setup Village 出村长家送树屋`（或等 request） | |
| 2 | 确认 Prefab `Village_出村长家送树屋` + 场景 `TeleportTo_YaerTreeHouseDoor` | |
| 3 | 1 楼 LeftDoor 出门 → 门前 → **自动**段 A（女神质疑文案） | |
| 4 | 中段黑幕；亮后人在树屋门口一带（可站） | |
| 5 | 段 B 道别+晚饭；还控 | |
| 6 | 同档再出 1 楼不重播；楼梯 2 楼回来不误播 | |
| 7 | `House_Tree` 仍可点 `Village_TreeHouseLock` | |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| `BlackFadeTeleportPlayerActionTask` | NodeCanvas ActionTask/Player | T1 黑幕+SetPos+相机 Snap |
| CSV | `Dialog/Village_出村长家送树屋.csv` | 段 A+B 台本（一字不改） |
| Setup | `VillageLeaveChiefEscortDialogueSetupEditor` | Prefab + 插传送 + 场景锚点 |
| G1 | `Village_KenMuNiSceneManager` | 认 `Village_Chief_House_Door` Trigger |
| E3′ | （上游已有） | LeftDoor 键 → `ExitFrom_HomeSceneChief` |

**未改**：晚宴旗；场景古莎跟到树屋；进树屋室内 Scene；WalkArea2。

---

## 时序

```
LeftDoor (enterPosKey=Village_Chief_House_Door)
  → KenMuNi1 落 ExitFrom_HomeSceneChief
  → TryTriggerLeaveChiefEscortOnce
  → 段 A → BlackFadeTeleport → TeleportTo_YaerTreeHouseDoor
  → 段 B → 还控；StoryTriggerCount 记次
```

---

## 菜单 / Request

- 菜单：`Tools / Dialogue / Setup Village 出村长家送树屋`
- Request：`Library/LeaveChiefEscortSetup.request`
