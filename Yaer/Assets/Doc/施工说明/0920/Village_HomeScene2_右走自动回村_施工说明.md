# Village_HomeScene2 · 右走自动回村 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】只改这一家右门  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_HomeScene2_右走自动回村_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**村民家 2 走到屋子右缘会黑幕回到这家门外，进门时不会立刻弹回去。**

### ② 原因（通俗）

回村门原来关着，而且摆在场景最右边，人在屋里走不到。现在把门开到和右墙对齐，走进就换场。落点还是村里这家门口，不用新键。

### ③ 用户检查清单

1. 村里 `House_NPC2` 进屋：停在室内左侧（约 -24, -3.65），**不要**马上黑幕回村。
2. 往右走到屋子右缘：黑幕、不读条，回到门外（约 -124.7, 3.85）。
3. 左门仍关着。
4. 其它民居、村长家右门不变。

### ④ 程序补充

只改 `Village_HomeScene2.unity` 的 `RightDoor`。

| 字段 | 改为 |
|------|------|
| 物体 Active | 1 |
| SceneChangeDoor | 启用 |
| NextSceneName | `Village_KenMuNi1` |
| TriggerWhenMoveIn | 1 |
| ShowLoadingUI | 0 |
| EnterPosKey | 空（LastScene 已对上 `ExitFrom_HomeScene2`） |
| 本地 X | **-31.07**（与同父 `RightWall` 相同；世界盒约 X -4.3～-2.3） |

进门点 X=-24.12 在盒外。`LeftDoor` 保持 Inactive。未改 KenMuNi1 落点、`House_NPC2`、其它场景。
