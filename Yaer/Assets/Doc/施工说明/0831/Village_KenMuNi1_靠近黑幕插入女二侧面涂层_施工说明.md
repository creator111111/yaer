# Village_KenMuNi1 — 靠近黑幕插入女二侧面涂层 — 施工说明

**日期**：2026-08-31  
**依据**：`执行文档/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_架构溯源报告.md`  
**强关联**：靠近村长黑幕（同一 `Npc_Chief` / 同一 BlackPanel）

---

## ① 结论

与 `Npc_Chief` **同一次黑幕**：全黑时启用 `GushaSidePortrait`（世界 SR、钉 **SceneObject**）再播门口对白；对白结束关侧面；**无二次连闪**。

---

## ② 原因

侧面是场内氛围涂层（压玩家），不是 UI `GushaPainting`；必须在全黑后启用，避免亮屏蹦出。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走近村长旁 | **一次**黑幕；亮后场上有女二 |
| 2 | 重叠区 | 女二在玩家 **上面**（SR=SceneObject） |
| 3 | 对白 | `Village_村长家门口初次对话` |
| 4 | 对白结束 | 侧面关掉（无二次黑） |
| 5 | 同档再走近 | 不再触发 |
| 6 | UI / Mask 古莎 | 仍正常 |
| 7 | `House_Chief` | 仍只进屋 |

---

## ④ 程序

| 路径 | 变更 |
|------|------|
| `ChiefNearDoorStoryTrigger.cs` | `onShowEnd`：EnableSide → TriggerStory；`onStoryEnd` 关侧面 |
| `Village_KenMuNi1.unity` | `Objects/GushaSidePortrait` 默认关；绑到 Trigger |
| `ArtRes/Scene/Village/GushaSide/古莎_侧面全身_占位.png` | **占位**（拷自 `古莎站立`）；正侧全身美术到位后替换同路径或改 SR 引用 |

**GushaSidePortrait**

- 位 ≈ `(-157, -1.55, 0)`；Z=0；无 Interactive  
- `sortingLayerName=SceneObject`，`sortingOrder=0`  
- **未**挂 DepthSort / DepthComponent  

**美术替换**：把正式侧面图导入建议路径，改 Sprite 引用；Pivot 脚底；PPU 跟村合层人物对齐后 Scene 微调 Scale/XY。
