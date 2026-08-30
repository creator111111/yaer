# Village_Shop — Chest 热区安装 Village_ShopChest — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_Shop_Chest热区安装Village_ShopChest对话_架构溯源报告.md`  
**范围**：方案 A — 故事名常量对齐磁盘 Prefab；**未**改场景热区、未做 C6 树屋、未挂 Chest Catch。

---

## ① 结论一句话

点胸已指向 `Village_ShopChest`；Idle 点 Chest 可加载对白（不再喊空号 `…_ChestClick`）。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_ShopSceneManager.cs` | `ShopkeeperChestClickStoryName = "Village_ShopChest"` | 旧名无 Prefab，TriggerStory 必失败 |
| `ShopkeeperBodyHotspot.cs` | Chest 枚举注释 → ShopChest | 与常量一致 |
| `ShopkeeperSpecialClickSetupEditor.cs` | `ChestPrefabPath` → ShopChest；去掉「点胸不动」过时注释 | Setup 勿再生成空号 |

**未改**：热区已齐；`Village_ShopChest.prefab` / CSV；Head 线；Catch 光标；C6 树屋。

**替代（未采用）**：Rename Prefab 回 ChestClick；加别名映射。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 点 Chest | 播 **Village_ShopChest**；藏 UI；表情跟句 |
| 2 | 对白中再点头/胸 | **不叠** |
| 3 | 结束 | Idle；UI 显；热区开；脸复位 |
| 4 | Console | `story=Village_ShopChest started=true`；无 Missing / 旧名 |
| 5 | 再点 Chest | **可再播** |
| 6 | 点 Head | 仍播 ShopHead |
| 7 | 播完 | **不**切树屋 |

---

## ④ 给程序

- 0601「建议名 ChestClick」作废；真源 = `Village_ShopChest`。  
- 热区 / Special 管线未动，仅门牌对齐。
