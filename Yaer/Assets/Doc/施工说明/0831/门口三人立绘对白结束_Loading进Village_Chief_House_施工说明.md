# 门口三人立绘对白结束 → Loading 进 Village_Chief_House — 施工说明

**日期**：2026-08-31  
**依据**：`执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md`  
**方案**：L2 · `onStoryEnd` → `LoadSceneWithLoadingPanel`

---

## ① 结论

门口三人戏播完后，**自动**开 `LoadingPanel` 进 `Village_Chief_House`（`blackFade:false`）。  
`House_Chief` 保留且已勾 `ShowLoadingUI`；`House_Tree` 未动。

---

## ② 原因

产品要「快进屋」后进度条进屋，不能再点门；现网 `LoadSceneTaskAction` 无 Loading、默认黑幕，故不用 L1 裸挂图末。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 屋外三人立绘完整播完 | |
| 2 | 结束后自动 Loading → `Village_Chief_House` | |
| 3 | 进屋主表现=进度条，非纯黑幕 | |
| 4 | 落点室内 `EnterFrom_Village` | |
| 5 | 之后点 `House_Chief` 仍可进（Loading） | |
| 6 | `House_Tree` 仍只锁门对白 | |
| 7 | Console 有 `[SceneLoad] … blackFade=False` | |

---

## ④ 程序清单

| 路径 | 变更 |
|------|------|
| `LoadSceneComponentGSM.cs` | 新增 `LoadSceneWithLoadingPanel` |
| `SceneChangeDoor.cs` | `ShowLoadingUI` 改调助手 |
| `SimpleStoryTrigger.cs` | `OnStoryFinished` → `protected virtual` |
| `ChiefNearDoorStoryTrigger.cs` | override：门口戏结束 → Loading 进村长家 |
| `Village_KenMuNi1.unity` | `House_Chief` Override `ShowLoadingUI=1`（Q5） |

**不改**：`House_Tree` / `Village_TreeHouseLock`；`LoadSceneTaskAction`（仍无 Loading，勿裸用进村长家）。
