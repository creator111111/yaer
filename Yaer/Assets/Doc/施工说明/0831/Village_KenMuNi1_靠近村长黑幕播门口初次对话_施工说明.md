# Village_KenMuNi1 — 靠近村长黑幕播门口初次对话 — 施工说明

**日期**：2026-08-31  
**依据**：`执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md`  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`

---

## ① 结论

已新建 **`Objects/Npc_Chief`**：Enter → 系统 BlackPanel 全黑 → `TriggerStory("Village_村长家门口初次对话")` → 壳就绪 HideFade；单次存档；与 `House_Chief` 解耦。

---

## ② 原因

合层「村长」仅 SR（Z≠0），不能挂物理。对齐老农：Objects 交互体 + 店式主动开黑（非换场 Defer）。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走近村长身旁（**不点 E**） | 先黑幕，再出门口对白 |
| 2 | 黑幕全黑前 | 不露三大立绘 |
| 3 | 对白名 | `Village_村长家门口初次对话` |
| 4 | 同档再走近 | **不再**触发 |
| 5 | 点 `House_Chief` | **只进屋** |
| 6 | 合层 `村长` | 美术仍在 |

**依赖**：门口对话 Prefab 须已用 Setup 菜单生成；未就绪时 Enter 仍开黑并尝试 Trigger（Console 可有 Missing，不崩）。

---

## ④ 程序

| 路径 | 变更 |
|------|------|
| `ChiefNearDoorStoryTrigger.cs` | 覆写 `TriggerStory`：ShowFade→`TryStartBoundStory`→`onStoryTriggered` HideFade；超时兜底 |
| `SimpleStoryTrigger.cs` | `TryStartBoundStory` / `CanStartStoryNow` / `StoryGsm` 供子类黑幕编排 |
| `Village_KenMuNi1.unity` | `Npc_Chief`：位≈(-157.5,-1.2,0)、Layer21、Collider 2×2（偏右避门）、`CldInteractiveListener`、Enter+SingleUse、`sceneObjs` |

**Npc_Chief 要点**

- `StoryPrefabName` = `Village_村长家门口初次对话`
- `triggerType` = Enter；`SingleUseInArchive` = true
- `requirePlayerOverlap` = 1；`canTouchWithPlayer` = 0（自动触发，无 E 提示）
- CursorChange 禁用（不抢 Chat 光标）
- 合层 `村长` / `House_Chief` **未改**

**微调**：若误触门，在 Scene 缩小 Collider 或再右移 `Npc_Chief`。
