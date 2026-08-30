# Village_KenMuNi1 — House_Tree 交互 Village_TreeHouseLock — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_架构溯源报告.md`  
**范围**：方案 A 物体交互三件套 + 锁对白 UIAlpha；**未**做换景 / Chest C6 / DepthZone 改动。

---

## ① 结论一句话

`Objects/House_Tree` 已落盘并可远程点击；播 `Village_TreeHouseLock`（「锁上了打不开」），对话框有 UIAlpha 壳。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_KenMuNi1.unity` | 新建 `House_Tree`（Layer21 + SceneEntity + Interactive + Collider + RaycastListener overlap=0 + SimpleStoryTrigger + Cursor View）；登记 `sceneObjs` | 磁盘原先无此物体；仿 StoneBrand/面包 |
| `Village_TreeHouseLock.prefab` | 首节点补 `NormalDialogueUIAlpha` EndA=1 Dur=1 → 再进 Statement | 防「播了看不见框」（ShopChest 同类） |

**摆位**：TreeDoor1/2 世界 XY 中点约 `(9.23, -7.5)`；门口小盒 Collider `2.4×3.2`。可在 Scene 微调。

**字段钉死**

- `StoryPrefabName = Village_TreeHouseLock`
- `triggerType = Click`；`SingleUseInArchive = 0`（可重复）
- `requirePlayerOverlap = 0`（远程）
- Cursor `View (2)` Priority=1（P1 一并做）

**未改**：`Door_Shop` / `House_Npc*` / TreeDoor DepthZone / 商店 Chest。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy 有 `Objects/House_Tree` 且已存盘 | 磁盘能搜到名 |
| 2 | 村内远处点门口小盒 | 播 **Village_TreeHouseLock** |
| 3 | 画面 | 「锁上了打不开」；**对话框可见** |
| 4 | 再点 | 可再播 |
| 5 | Console | 无 Missing Prefab |
| 6 | 回归 | Door_Shop / 进屋 / DepthZone 正常 |
| 7 | 边界 | **不**进树屋内部 |

若门口点偏：在 Scene 拖 `House_Tree` 对齐门视觉即可。

---

## ④ 给程序

- 勿接 `SceneChangeDoor`；勿混商店 Chest C6。  
- 钥匙任务改对白另开（本期永远可重复）。
